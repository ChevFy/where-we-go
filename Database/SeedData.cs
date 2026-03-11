using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using where_we_go.Models;
using where_we_go.Models.Enums;

namespace where_we_go.Database;

public static class SeedData
{
    public static void Initialize(IServiceProvider serviceProvider)
    {
        using var context = new AppDbContext(serviceProvider.GetRequiredService<DbContextOptions<AppDbContext>>());

        Console.WriteLine("Seeding initial data...");

        var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        SeedRoles(roleManager);
        SeedUsers(userManager);
        SeedCategories(context);
        SeedPosts(context);
        SeedParticipants(context);
        SeedNotifications(context);

        Console.WriteLine("Seeding completed.");
    }

    private static void SeedRoles(RoleManager<IdentityRole> roleManager)
    {
        var roles = new[] { "Admin", "User" };

        foreach (var role in roles)
        {
            if (!roleManager.RoleExistsAsync(role).Result)
            {
                roleManager.CreateAsync(new IdentityRole(role)).Wait();
            }
        }
    }

    private static void SeedUsers(UserManager<User> userManager)
    {
        if (userManager.Users.Any()) return;

        foreach (var (userName, email, name, password, role, profileImageKey) in SeedDataModels.Users.Data)
        {
            var user = new User
            {
                UserName = userName,
                Email = email,
                Name = name,
                EmailConfirmed = true,
                ProfileImageKey = profileImageKey,
                DateCreated = DateTime.UtcNow,
                DateUpdated = DateTime.UtcNow
            };

            var result = userManager.CreateAsync(user, password).Result;
            if (!result.Succeeded)
            {
                Console.WriteLine($"Failed to create user {userName}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
            else
            {
                userManager.AddToRoleAsync(user, role).Wait();
            }
        }
    }

    private static void SeedCategories(AppDbContext context)
    {
        if (context.Categories.Any()) return;

        foreach (var (name, description) in SeedDataModels.Categories.Data)
        {
            context.Categories.Add(new Category
            {
                CategoryId = Guid.NewGuid(),
                Name = name,
                Description = description
            });
        }

        context.SaveChanges();
    }

    private static void SeedPosts(AppDbContext context)
    {
        if (context.Posts.Count() >= 10) return;

        var users = context.Users.ToList();
        if (!users.Any()) return;

        var categories = context.Categories.ToList();

        foreach (var (title, description, location, daysDeadline, minPart, maxPart, inviteCode, categoryNames, ownerEmail, daysAgo, lat, lon, picture, minutesDeadline) in SeedDataModels.Posts.Data)
        {
            // Find the specific owner by email
            var author = users.FirstOrDefault(u => u.Email == ownerEmail);
            if (author == null) continue;

            var postCategories = categories.Where(c => categoryNames.Contains(c.Name)).ToList();

            var dateCreated = DateTime.UtcNow.AddDays(-daysAgo);
            var dateDeadline = minutesDeadline.HasValue
                ? dateCreated.AddMinutes(minutesDeadline.Value)
                : dateCreated.AddDays(daysDeadline);
            var eventDate = dateDeadline.AddDays(1);

            context.Posts.Add(new Post
            {
                PostId = Guid.NewGuid(),
                UserId = author.Id,
                Title = title,
                Description = description,
                LocationName = location,
                DateCreated = dateCreated,
                DateDeadline = dateDeadline,
                EventDate = eventDate,
                MinParticipants = minPart,
                MaxParticipants = maxPart,
                Status = PostStatus.Open,
                InviteCode = inviteCode,
                Categories = postCategories,
                LocationLat = (float)lat,
                LocationLon = (float)lon,
                PostImageKey = picture
            });
        }

        context.SaveChanges();
    }

    private static void SeedParticipants(AppDbContext context)
    {
        if (context.Participants.Any()) return;

        var posts = context.Posts.ToList();
        var users = context.Users.ToList();

        foreach (var (inviteCode, userEmail, status) in SeedDataModels.Participants.Data)
        {
            var post = posts.FirstOrDefault(p => p.InviteCode == inviteCode);
            if (post == null) continue;

            var user = users.FirstOrDefault(u => u.Email == userEmail);
            if (user == null) continue;

            context.Participants.Add(new Participant
            {
                ParticipantId = Guid.NewGuid(),
                PostId = post.PostId,
                UserId = user.Id,
                Status = status,
                DateJoin = DateTime.UtcNow
            });
        }

        context.SaveChanges();
    }

    private static void SeedNotifications(AppDbContext context)
    {
        if (context.Notifications.Any()) return;

        var posts = context.Posts.OrderBy(p => p.DateCreated).ToList();
        var users = context.Users.ToList();

        foreach (var (postIndex, userEmail, content, isRead, type, link, daysAgo) in SeedDataModels.Notifications.Data)
        {
            if (postIndex >= posts.Count) continue;

            var post = posts[postIndex];
            var user = users.FirstOrDefault(u => u.Email == userEmail);
            if (user == null) continue;

            context.Notifications.Add(new Notification
            {
                NotificationId = Guid.NewGuid(),
                UserId = user.Id,
                PostId = post.PostId,
                Content = content,
                IsRead = isRead,
                Type = type,
                Link = $"{link}/{post.PostId}",
                DateCreated = DateTime.UtcNow.AddDays(-daysAgo)
            });
        }

        context.SaveChanges();
    }
}