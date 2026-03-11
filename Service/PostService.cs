using Microsoft.EntityFrameworkCore;

using where_we_go.Database;
using where_we_go.DTO;
using where_we_go.Models.Enums;
using where_we_go.Models;

namespace where_we_go.Service
{
    public class PostService(AppDbContext _dbContext, IFileService _fileService, INotificationService _notificationService) : BaseService, IPostService
    {
        private PostStatus GetPostStatus(Post post)
        {
            // 1. Check for manual/explicit Cancelled
            if (post.Status == PostStatus.Cancelled)
                return PostStatus.Cancelled;

            if (post.Status == PostStatus.Closed)
                return PostStatus.Closed;

            var now = DateTime.UtcNow;

            // 2. Check EventDate - Closed and Full become Completed
            if (now > post.EventDate)
                return PostStatus.Completed;

            // 3. Check DateDeadline logic
            if (now > post.DateDeadline)
            {
                // If Open: close immediately, then cancel 1+ hour after deadline
                if (post.Status == PostStatus.Open)
                {
                    var timeSinceDeadline = now - post.DateDeadline;
                    if (timeSinceDeadline.TotalHours >= 1)
                        return PostStatus.Cancelled;
                    else
                        return PostStatus.Closed;
                }

                // If already Closed, stay Closed (will become Completed when EventDate passes)
                if (post.Status == PostStatus.Closed)
                    return PostStatus.Closed;

                // Ignore Full status
                if (post.Status == PostStatus.Full)
                    return PostStatus.Full;
            }

            // 4. Check capacity-based states (before deadline)
            if (now <= post.DateDeadline)
            {
                var participantCount = _dbContext.Participants.Count(part => part.PostId == post.PostId && part.Status == ParticipantStatus.Approved);
                if (participantCount >= post.MaxParticipants)
                    return PostStatus.Full;
            }

            // 5. Default state
            return PostStatus.Open;
        }

        private async Task<PaginatedResponseDto<PostDto>> ApplyFiltersAndGetPaginatedPostsAsync(IQueryable<Post> posts, PostQueryDto query, string? currentUserId = null)
        {
            // Filter by name
            if (!string.IsNullOrWhiteSpace(query.NameFilter))
            {
                var keyword = query.NameFilter.Trim();
                posts = posts.Where(p => EF.Functions.Like(p.Title.ToLower(), $"%{keyword}%"));
            }

            // Filter by categories
            if (query.Categories != null && query.Categories.Count > 0)
            {
                posts = posts.Where(p => query.Categories.Any(catId => p.Categories.Any(c => c.CategoryId == catId)));
            }

            // Filter by status
            var now = DateTime.UtcNow;
            if (!string.IsNullOrWhiteSpace(query.StatusFilter))
            {
                posts = query.StatusFilter.ToLower() switch
                {
                    "open" => posts.Where(p => p.Status == PostStatus.Open),
                    "pending" => posts.Where(p => p.Status != PostStatus.Cancelled && p.Participants.Any(part => part.UserId == currentUserId && part.Status == ParticipantStatus.Pending)),
                    "cancelled" => posts.Where(p => p.Status == PostStatus.Cancelled && p.UserId == currentUserId), // only owner can view
                    "completed" => posts.Where(p => p.EventDate <= now && p.Status != PostStatus.Cancelled && (p.UserId == currentUserId
                    || p.Participants.Any(part => part.UserId == currentUserId && part.Status == ParticipantStatus.Approved))), // owner and participants can view
                    "upcoming" => posts.Where(p => p.EventDate > now && p.Status != PostStatus.Cancelled && (p.UserId == currentUserId
                    || p.Participants.Any(part => part.UserId == currentUserId && part.Status == ParticipantStatus.Approved))), // owner and participants can view
                    _ => posts.Where(p => p.Status == PostStatus.Open)
                };
            }
            else
            {
                // Exclude cancelled posts by default
                posts = posts.Where(p => p.Status != PostStatus.Cancelled);
            }

            // Sort by
            posts = (query.SortBy ?? "").ToLower() switch
            {
                "title" => posts.OrderBy(p => p.Title),
                "title_desc" => posts.OrderByDescending(p => p.Title),
                "latest" => posts.OrderByDescending(p => p.DateCreated),
                "oldest" => posts.OrderBy(p => p.DateCreated),
                "soonest" => posts.OrderBy(p => p.EventDate),
                _ => posts.OrderBy(p => p.PostId)
            };

            // Map to PostDto and paginate
            var result = await ToPaginatedResponseAsync(posts, query, p => new PostDto
            {
                PostId = p.PostId,
                Title = p.Title,
                Description = p.Description,
                LocationName = p.LocationName,
                DateDeadline = p.DateDeadline,
                EventDate = p.EventDate,
                PostImgURL = p.PostImageKey,
                Status = GetPostStatus(p).ToString(),
                MaxParticipants = p.MaxParticipants,
                CurrentParticipants = _dbContext.Participants.Count(part => part.PostId == p.PostId && part.Status == ParticipantStatus.Approved),
                Categories = [.. p.Categories.Select(c => new CategorySimpleDto
                {
                    CategoryId = c.CategoryId,
                    Name = c.Name
                })]
            });

            // Generate presigned URLs for post images
            foreach (var p in result.Data)
            {
                p.PostImgURL = await _fileService.GeneratePresignedPostUrlAsync(p.PostImgURL);
            }

            return result;
        }

        public async Task<PaginatedResponseDto<PostDto>> GetAllPostsAsync(PostQueryDto query, string? userId = null)
        {
            var posts = _dbContext.Posts
                .Include(p => p.Categories)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(userId) && query.StatusFilter == "open")
            {
                posts = posts.Where(p => p.UserId != userId); // Exclude user's own posts from the general listing
            }

            return await ApplyFiltersAndGetPaginatedPostsAsync(posts, query, userId);
        }

        public async Task<PostDetailDto?> GetPostDetailAsync(Guid id, string? currentUserId = null)
        {
            var post = await _dbContext.Posts
                .Include(p => p.Categories)
                .Where(p => p.PostId == id)
                .FirstOrDefaultAsync();

            if (post == null)
                return null;

            // For public display, "current participants" should mean APPROVED participants only.
            // Owner-side management uses GetPostApplicantsAsync for other statuses.
            var approvedParticipants = await _dbContext.Participants
                .Include(part => part.User)
                .Where(part => part.PostId == post.PostId && part.Status == ParticipantStatus.Approved)
                .ToListAsync();

            var participantDetails = new List<ParticipantDetailDto>();
            foreach (var part in approvedParticipants)
            {
                participantDetails.Add(new ParticipantDetailDto
                {
                    UserId = part.UserId,
                    UserName = part.User.UserName ?? "",
                    ProfileImgURL = await _fileService.GeneratePresignedProfileUrlAsync(part.User.ProfileImageKey)
                });
            }

            // Load post owner info
            var owner = await _dbContext.Users.FindAsync(post.UserId);

            var result = new PostDetailDto
            {
                PostId = post.PostId,
                Title = post.Title,
                Description = post.Description,
                LocationName = post.LocationName,
                DateDeadline = post.DateDeadline,
                EventDate = post.EventDate,
                Status = GetPostStatus(post).ToString(),
                Locationlat = post.LocationLat ?? 0f,
                Locationlon = post.LocationLon ?? 0f,
                CurrentParticipants = approvedParticipants.Count,
                MaxParticipants = post.MaxParticipants,
                CurrentParticipantsDetail = participantDetails,
                Categories = post.Categories.Select(c => new CategoryDetailDto
                {
                    CategoryId = c.CategoryId,
                    Name = c.Name,
                    Description = c.Description
                }).ToList(),
                PostImgURL = await _fileService.GeneratePresignedPostUrlAsync(post.PostImageKey),
                UserId = post.UserId,
                Owner = new ParticipantDetailDto
                {
                    UserId = owner?.Id ?? "",
                    UserName = owner?.UserName ?? "",
                    ProfileImgURL = owner != null ? await _fileService.GeneratePresignedProfileUrlAsync(owner.ProfileImageKey) : null
                },
                IsJoined = currentUserId != null && _dbContext.Participants.Any(part => part.PostId == post.PostId && part.UserId == currentUserId && part.Status == ParticipantStatus.Approved),
                IsPending = currentUserId != null && _dbContext.Participants.Any(part => part.PostId == post.PostId && part.UserId == currentUserId && part.Status == ParticipantStatus.Pending),
                IsRejected = currentUserId != null && _dbContext.Participants.Any(part => part.PostId == post.PostId && part.UserId == currentUserId && part.Status == ParticipantStatus.Rejected),
                IsWithdrawn = currentUserId != null && _dbContext.Participants.Any(part => part.PostId == post.PostId && part.UserId == currentUserId && part.Status == ParticipantStatus.Withdrawn),
                ChatId = await _dbContext.GroupChats
                    .Where(g => g.PostId == post.PostId)
                    .Select(g => (Guid?)g.GroupChatId)
                    .FirstOrDefaultAsync()
            };

            // if the current user is joined OR is the owner, but there is no chat yet, create one lazily
            if ((result.IsJoined || (currentUserId != null && currentUserId == post.UserId)) && result.ChatId == null)
            {
                var newChat = new GroupChat
                {
                    GroupChatId = Guid.NewGuid(),
                    PostId = post.PostId,
                    GroupChatName = post.Title
                };
                _dbContext.GroupChats.Add(newChat);
                await _dbContext.SaveChangesAsync();
                result.ChatId = newChat.GroupChatId;
            }

            return result;
        }

        public async Task CreatePostAsync(PostCreateDto dto, string userId)
        {
            // Combine date and time into a single DateTime
            var combinedDateTime = dto.DateDeadline.Add(dto.TimeDeadline.ToTimeSpan());
            var combinedEventDateTime = dto.EventDate.Add(dto.EventTime.ToTimeSpan());

            var dateDeadline = combinedDateTime.ToUniversalTime();
            var eventDate = combinedEventDateTime.ToUniversalTime();

            var post = new Post
            {
                PostId = Guid.NewGuid(),
                UserId = userId,
                Title = dto.Title,
                Description = dto.Description,
                LocationName = dto.LocationName,
                LocationLat = !string.IsNullOrEmpty(dto.LocationLat) ? float.Parse(dto.LocationLat) : null,
                LocationLon = !string.IsNullOrEmpty(dto.LocationLon) ? float.Parse(dto.LocationLon) : null,
                PostImageKey = string.IsNullOrWhiteSpace(dto.PostImgkey) ? null : dto.PostImgkey,

                DateDeadline = dateDeadline,
                EventDate = eventDate,

                MinParticipants = dto.MinParticipants,
                MaxParticipants = Math.Max(1, dto.MaxParticipants - 1), // -1 because owner counts; stored value = participant slots

                DateCreated = DateTime.UtcNow,

                Status = PostStatus.Open, // Changed from Active to Open
                InviteCode = Guid.NewGuid().ToString().Substring(0, 8).ToUpper()
            };

            _dbContext.Posts.Add(post);
            await _dbContext.SaveChangesAsync();

            // Associate categories if provided
            if (dto.CategoryIds?.Count > 0)
            {
                var categories = await _dbContext.Categories
                    .Where(c => dto.CategoryIds.Contains(c.CategoryId))
                    .ToListAsync();

                foreach (var category in categories)
                {
                    post.Categories.Add(category);
                }

                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<bool> UpdatePostAsync(Guid postId, PostUpdateDto dto, string currentUserId)
        {
            var post = await _dbContext.Posts
                .Include(p => p.Categories)
                .FirstOrDefaultAsync(p => p.PostId == postId);

            if (post == null || post.UserId != currentUserId)
            {
                return false; // Post not found or user is not the owner
            }

            // Check current approved participant count
            var approvedCount = await _dbContext.Participants
                .CountAsync(p => p.PostId == postId && p.Status == ParticipantStatus.Approved);

            // Validate max participants against current approved count (stored max = dto.MaxParticipants - 1 because owner counts)
            var storedMax = Math.Max(1, dto.MaxParticipants - 1);
            if (storedMax < approvedCount)
            {
                throw new InvalidOperationException($"Cannot set maximum participants below current approved count ({approvedCount}).");
            }

            // Merge date and time
            var dateDeadline = dto.DateDeadline.Date.Add(dto.TimeDeadline.ToTimeSpan()).ToUniversalTime();
            var eventDate = dto.EventDate.Date.Add(dto.EventTime.ToTimeSpan()).ToUniversalTime();

            // Update post fields
            post.Title = dto.Title;
            post.Description = dto.Description;
            post.LocationName = dto.LocationName;
            post.LocationLat = !string.IsNullOrEmpty(dto.LocationLat) ? float.Parse(dto.LocationLat) : null;
            post.LocationLon = !string.IsNullOrEmpty(dto.LocationLon) ? float.Parse(dto.LocationLon) : null;
            post.PostImageKey = string.IsNullOrWhiteSpace(dto.PostImgkey) ? post.PostImageKey : dto.PostImgkey;
            post.DateDeadline = dateDeadline;
            post.EventDate = eventDate;
            post.MinParticipants = dto.MinParticipants;
            post.MaxParticipants = storedMax; // -1 because owner counts

            // Update categories
            post.Categories.Clear();
            if (dto.CategoryIds?.Count > 0)
            {
                var categories = await _dbContext.Categories
                    .Where(c => dto.CategoryIds.Contains(c.CategoryId))
                    .ToListAsync();

                foreach (var category in categories)
                {
                    post.Categories.Add(category);
                }
            }

            _dbContext.Posts.Update(post);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeletePostAsync(Guid id, string userId)
        {
            var post = await _dbContext.Posts.FirstOrDefaultAsync(p => p.PostId == id && p.UserId == userId);
            if (post == null)
            {
                return false;
            }

            post.Status = PostStatus.Cancelled; // Changed from Delete to Cancelled
            _dbContext.Posts.Update(post);

            // TODO: Notify every participant (except status == reject, withdrawn)
            var participantsToNotify = await _dbContext.Participants
               .Where(p => p.PostId == id &&
                           p.Status != ParticipantStatus.Rejected &&
                           p.Status != ParticipantStatus.Withdrawn)
               .ToListAsync();

            foreach (var participant in participantsToNotify)
            {
                await _notificationService.CreateNotificationAsync(new NotificationCreateDto
                {
                    UserId = participant.UserId,
                    PostId = post.PostId,
                    Content = $"The activity '{post.Title}' you joined has been cancelled.",
                    Link = $"/Post/PostDetail/{post.PostId}",
                    Type = NotificationType.ActivityCancelled
                });
            }

            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<string> JoinPostAsync(Guid postId, string userId)
        {
            var post = await _dbContext.Posts.FindAsync(postId);
            if (post == null) return "Activity not found.";

            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null) return "User not found.";

            var currentStatus = GetPostStatus(post);
            if (currentStatus == PostStatus.Cancelled) return "This activity has been cancelled.";
            if (currentStatus == PostStatus.Closed) return "This activity is closed for new participants.";
            if (currentStatus == PostStatus.Completed) return "This activity is already completed.";
            if (currentStatus == PostStatus.Full) return "This activity is currently full.";

            var existingParticipant = await _dbContext.Participants
                .FirstOrDefaultAsync(p => p.PostId == postId && p.UserId == userId);

            if (existingParticipant != null)
            {
                if (existingParticipant.Status == ParticipantStatus.Approved) return "You have already joined this activity.";
                if (existingParticipant.Status == ParticipantStatus.Pending) return "Your request is already pending.";
                if (existingParticipant.Status == ParticipantStatus.Rejected) return "Your previous request was rejected.";
                if (existingParticipant.Status == ParticipantStatus.Withdrawn) return "You cannot rejoin this activity after withdrawing.";
            }

            // New participant ALWAYS goes to pending
            var participant = new Participant
            {
                ParticipantId = Guid.NewGuid(),
                PostId = postId,
                UserId = userId,
                DateJoin = DateTime.UtcNow,
                Status = ParticipantStatus.Pending
            };

            _dbContext.Participants.Add(participant);
            await _dbContext.SaveChangesAsync();

            // TODO: Notify owner here
            await _notificationService.CreateNotificationAsync(new NotificationCreateDto
            {
                UserId = post.UserId,
                PostId = post.PostId,
                Content = $"{user.UserName ?? user.Name ?? "A user"} requested to join your activity.",
                Link = $"/Post/PostDetail/{post.PostId}",
                Type = NotificationType.ParticipantRequested
            });

            // if approved and there is no group chat yet, create one now
            if (participant.Status == ParticipantStatus.Approved)
            {
                var existingChat = await _dbContext.GroupChats
                    .FirstOrDefaultAsync(g => g.PostId == postId);
                if (existingChat == null)
                {
                    var newChat = new GroupChat
                    {
                        GroupChatId = Guid.NewGuid(),
                        PostId = postId,
                        GroupChatName = post.Title
                    };
                    _dbContext.GroupChats.Add(newChat);
                    await _dbContext.SaveChangesAsync();
                }
            }

            return "Pending";
        }

        public async Task<string> LeavePostAsync(Guid postId, string userId)
        {
            var participant = await _dbContext.Participants
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.PostId == postId &&
                                          p.UserId == userId &&
                                          (p.Status == ParticipantStatus.Approved || p.Status == ParticipantStatus.Pending));

            if (participant == null) return "You are not a member of this activity.";

            var wasApproved = participant.Status == ParticipantStatus.Approved;
            participant.Status = ParticipantStatus.Withdrawn;

            await _dbContext.SaveChangesAsync();

            var post = await _dbContext.Posts.FindAsync(postId);
            if (post != null)
            {
                var displayName = participant.User?.Name ?? participant.User?.UserName ?? "A user";
                var content = wasApproved
                    ? $"{displayName} left your activity."
                    : $"{displayName} withdrew their request to join.";

                await _notificationService.CreateNotificationAsync(new NotificationCreateDto
                {
                    UserId = post.UserId,
                    PostId = post.PostId,
                    Content = content,
                    Link = $"/Post/PostDetail/{post.PostId}",
                    Type = NotificationType.ParticipantWithdrawn
                });
            }
            return "Success";
        }

        public async Task<string> ApproveJoinAsync(Guid postId, string[] participantUserIds, string currentUserId)
        {
            var post = await _dbContext.Posts.FindAsync(postId);
            if (post == null || post.UserId != currentUserId) return "Unauthorized or Post Not Found.";

            if (participantUserIds == null || participantUserIds.Length == 0)
                return "No participants selected.";

            var targetUserIds = participantUserIds
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            if (targetUserIds.Count == 0)
                return "No participants selected.";

            var pendingParticipants = await _dbContext.Participants
                .Where(p => p.PostId == postId &&
                            p.Status == ParticipantStatus.Pending &&
                            targetUserIds.Contains(p.UserId))
                .OrderBy(p => p.DateJoin)
                .ToListAsync();

            if (pendingParticipants.Count == 0) return "Participant request not found.";

            // Approve as many pending users as remaining capacity allows.
            var approvedCount = await _dbContext.Participants
                .CountAsync(p => p.PostId == postId && p.Status == ParticipantStatus.Approved);

            var availableSlots = post.MaxParticipants - approvedCount;
            if (availableSlots <= 0) return "Cannot approve: Activity is already full.";

            if (pendingParticipants.Count > availableSlots)
            {
                return $"Cannot approve selected participants: only {availableSlots} slot(s) remaining.";
            }

            var participantsToApprove = pendingParticipants;

            foreach (var participant in participantsToApprove)
            {
                participant.Status = ParticipantStatus.Approved;
            }

            await _dbContext.SaveChangesAsync();

            foreach (var participant in participantsToApprove)
            {
                await _notificationService.CreateNotificationAsync(new NotificationCreateDto
                {
                    UserId = participant.UserId,
                    PostId = postId,
                    Content = "Your request to join was approved!",
                    Link = $"/Post/PostDetail/{post.PostId}",
                    Type = NotificationType.ParticipantApproved
                });
            }

            return "Success";
        }

        public async Task<string> RejectJoinAsync(Guid postId, string[] participantUserIds, string currentUserId)
        {
            var post = await _dbContext.Posts.FindAsync(postId);
            if (post == null || post.UserId != currentUserId) return "Unauthorized or Post Not Found.";

            if (participantUserIds == null || participantUserIds.Length == 0)
                return "No participants selected.";

            var targetUserIds = participantUserIds
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            if (targetUserIds.Count == 0)
                return "No participants selected.";

            var participantsToReject = await _dbContext.Participants
                .Where(p => p.PostId == postId &&
                            p.Status == ParticipantStatus.Pending &&
                            targetUserIds.Contains(p.UserId))
                .ToListAsync();

            if (participantsToReject.Count == 0) return "Participant request not found.";

            foreach (var participant in participantsToReject)
            {
                participant.Status = ParticipantStatus.Rejected;
            }

            await _dbContext.SaveChangesAsync();

            foreach (var participant in participantsToReject)
            {
                await _notificationService.CreateNotificationAsync(new NotificationCreateDto
                {
                    UserId = participant.UserId,
                    PostId = postId,
                    Content = "Your request to join was declined.",
                    Link = $"/Post/PostDetail/{post.PostId}",
                    Type = NotificationType.ParticipantRejected
                });
            }

            return "Success";
        }

        public async Task<string> RemoveParticipantAsync(Guid postId, string participantUserId, string currentUserId)
        {
            // Verify post exists and current user is the owner
            var post = await _dbContext.Posts.FindAsync(postId);
            if (post == null || post.UserId != currentUserId)
            {
                return "Unauthorized or Post Not Found.";
            }

            // Find the approved participant
            var participant = await _dbContext.Participants
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.PostId == postId &&
                                          p.UserId == participantUserId &&
                                          p.Status == ParticipantStatus.Approved);

            if (participant == null)
            {
                return "Participant not found or not approved.";
            }

            // Set status to Rejected (user was removed by owner, not voluntary withdrawal)
            participant.Status = ParticipantStatus.Rejected;
            await _dbContext.SaveChangesAsync();

            // Notify the removed participant
            await _notificationService.CreateNotificationAsync(new NotificationCreateDto
            {
                UserId = participantUserId,
                PostId = postId,
                Content = $"You have been removed from the activity '{post.Title}'.",
                Link = $"/Post/PostDetail/{post.PostId}",
                Type = NotificationType.ParticipantRejected
            });

            return "Success";
        }

        public async Task<List<ApplicantDto>> GetPostApplicantsAsync(Guid postId, string currentUserId, string? statusFilter = null)
        {
            // 1. Verify the post exists and the current user is actually the owner
            var post = await _dbContext.Posts.FirstOrDefaultAsync(p => p.PostId == postId);
            if (post == null || post.UserId != currentUserId)
            {
                return new List<ApplicantDto>(); // Return empty if unauthorized
            }

            var participantsQuery = _dbContext.Participants
                .Include(p => p.User)
                .Where(p => p.PostId == postId);

            if (!string.IsNullOrWhiteSpace(statusFilter) &&
                !statusFilter.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                if (!Enum.TryParse<ParticipantStatus>(statusFilter, true, out var parsedStatus))
                {
                    return new List<ApplicantDto>();
                }

                participantsQuery = participantsQuery.Where(p => p.Status == parsedStatus);
            }

            // 2. Fetch the raw entities from the database FIRST (this prevents the EF translation error)
            var participants = await participantsQuery
                .OrderBy(p => p.DateJoin)
                .ToListAsync();

            // 3. Map to DTO in memory (just like your old GetPostDetailAsync code)
            var applicants = new List<ApplicantDto>();
            foreach (var p in participants)
            {
                applicants.Add(new ApplicantDto
                {
                    UserId = p.UserId,
                    Name = p.User?.UserName ?? p.User?.Name ?? "Unknown User",
                    ProfileImageKey = await _fileService.GeneratePresignedProfileUrlAsync(p.User?.ProfileImageKey),
                    Status = p.Status.ToString(),
                    DateJoin = p.DateJoin
                });
            }

            return applicants;
        }

        public async Task<PaginatedResponseDto<PostDto>> GetPostsByUserIdAsync(string userId, PostQueryDto query)
        {
            var posts = _dbContext.Posts
                .Include(p => p.Categories)
                .Where(p => p.UserId == userId)
                .AsNoTracking();

            return await ApplyFiltersAndGetPaginatedPostsAsync(posts, query);
        }

        public async Task<PaginatedResponseDto<PostDto>> GetPostsJoinedByUserIdAsync(string userId, PostQueryDto query)
        {
            var posts = _dbContext.Posts
                .Include(p => p.Categories)
                .Where(p => _dbContext.Participants.Any(part => part.PostId == p.PostId &&
                                                               part.UserId == userId &&
                                                               (part.Status == ParticipantStatus.Approved || part.Status == ParticipantStatus.Pending)))
                .AsNoTracking();

            return await ApplyFiltersAndGetPaginatedPostsAsync(posts, query);
        }
    }
}