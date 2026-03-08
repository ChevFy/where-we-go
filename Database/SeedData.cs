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

        foreach (var (userName, email, name, password, role) in SeedDataModels.Users.Data)
        {
            var user = new User
            {
                UserName = userName,
                Email = email,
                Name = name,
                EmailConfirmed = true,
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
        var random = new Random();
        var postIndex = 0;

        foreach (var (title, description, location, daysDeadline, minPart, maxPart, inviteCode, categoryNames) in SeedDataModels.Posts.Data)
        {
            // Select a random user or cycle through users
            var author = users[postIndex % users.Count];

            var postCategories = categories.Where(c => categoryNames.Contains(c.Name)).ToList();

            var dateCreated = DateTime.UtcNow;
            var dateDeadline = dateCreated.AddDays(daysDeadline);

            context.Posts.Add(new Post
            {
                PostId = Guid.NewGuid(),
                UserId = author.Id,
                Title = title,
                Description = description,
                LocationName = location,
                DateCreated = dateCreated,
                DateDeadline = dateDeadline,
                EventDate = dateDeadline.AddDays(1),
                MinParticipants = minPart,
                MaxParticipants = maxPart,
                Status = PostStatus.Active,
                InviteCode = inviteCode,
                Categories = postCategories
            });

            postIndex++;
        }

        context.SaveChanges();
    }

    private static void SeedParticipants(AppDbContext context)
    {
        if (context.Participants.Any()) return;

        var posts = context.Posts.OrderBy(p => p.DateCreated).ToList();
        var users = context.Users.ToList();

        foreach (var (postIndex, userEmails) in SeedDataModels.Participants.Data)
        {
            if (postIndex >= posts.Count) continue;

            var post = posts[postIndex];

            foreach (var email in userEmails)
            {
                var user = users.FirstOrDefault(u => u.Email == email);
                if (user == null) continue;

                context.Participants.Add(new Participant
                {
                    ParticipantId = Guid.NewGuid(),
                    PostId = post.PostId,
                    UserId = user.Id,
                    Status = ParticipantStatus.Approved,
                    DateJoin = DateTime.UtcNow.AddDays(-1)
                });
            }
        }

        context.SaveChanges();
    }
}