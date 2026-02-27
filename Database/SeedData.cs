using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using where_we_go.Models;

namespace where_we_go.Database;

public static class SeedData
{
    public static void Initialize(IServiceProvider serviceProvider)
    {
        using (var context = new AppDbContext(
            serviceProvider.GetRequiredService<
                DbContextOptions<AppDbContext>>()))
        {
            Console.WriteLine("Seeding initial data...");

            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            var roles = new[] { "Admin", "User" };

            foreach (var role in roles)
            {
                var roleName = role.ToString();
                if (!roleManager.RoleExistsAsync(roleName).Result)
                {
                    roleManager.CreateAsync(new IdentityRole(roleName)).Wait();
                }
            }

            if (!context.Users.Any())
            {
                var users = new[]
                {
                    new User { UserName = "admin@example.com", Email = "admin@example.com", Name = "Admin", EmailConfirmed = true, DateCreated = DateTime.UtcNow, DateUpdated = DateTime.UtcNow },
                    new User { UserName = "john@example.com", Email = "john@example.com", Name = "John Doe", EmailConfirmed = true, DateCreated = DateTime.UtcNow, DateUpdated = DateTime.UtcNow },
                    new User { UserName = "jane@example.com", Email = "jane@example.com", Name = "Jane Smith", EmailConfirmed = true, DateCreated = DateTime.UtcNow, DateUpdated = DateTime.UtcNow },
                    new User { UserName = "alice@example.com", Email = "alice@example.com", Name = "Alice Johnson", EmailConfirmed = true, DateCreated = DateTime.UtcNow, DateUpdated = DateTime.UtcNow },
                    new User { UserName = "bob@example.com", Email = "bob@example.com", Name = "Bob Williams", EmailConfirmed = true, DateCreated = DateTime.UtcNow, DateUpdated = DateTime.UtcNow },
                    new User { UserName = "charlie@example.com", Email = "charlie@example.com", Name = "Charlie Brown", EmailConfirmed = true, DateCreated = DateTime.UtcNow, DateUpdated = DateTime.UtcNow },
                    new User { UserName = "diana@example.com", Email = "diana@example.com", Name = "Diana Prince", EmailConfirmed = true, DateCreated = DateTime.UtcNow, DateUpdated = DateTime.UtcNow },
                    new User { UserName = "evan@example.com", Email = "evan@example.com", Name = "Evan Martinez", EmailConfirmed = true, DateCreated = DateTime.UtcNow, DateUpdated = DateTime.UtcNow },
                    new User { UserName = "fiona@example.com", Email = "fiona@example.com", Name = "Fiona Garcia", EmailConfirmed = true, DateCreated = DateTime.UtcNow, DateUpdated = DateTime.UtcNow },
                    new User { UserName = "george@example.com", Email = "george@example.com", Name = "George Wilson", EmailConfirmed = true, DateCreated = DateTime.UtcNow, DateUpdated = DateTime.UtcNow },
                    new User { UserName = "hannah@example.com", Email = "hannah@example.com", Name = "Hannah Lee", EmailConfirmed = true, DateCreated = DateTime.UtcNow, DateUpdated = DateTime.UtcNow },
                    new User { UserName = "ivan@example.com", Email = "ivan@example.com", Name = "Ivan Petrov", EmailConfirmed = true, DateCreated = DateTime.UtcNow, DateUpdated = DateTime.UtcNow },
                    new User { UserName = "julia@example.com", Email = "julia@example.com", Name = "Julia Roberts", EmailConfirmed = true, DateCreated = DateTime.UtcNow, DateUpdated = DateTime.UtcNow },
                    new User { UserName = "kevin@example.com", Email = "kevin@example.com", Name = "Kevin Hart", EmailConfirmed = true, DateCreated = DateTime.UtcNow, DateUpdated = DateTime.UtcNow },
                    new User { UserName = "laura@example.com", Email = "laura@example.com", Name = "Laura White", EmailConfirmed = true, DateCreated = DateTime.UtcNow, DateUpdated = DateTime.UtcNow },
                    new User { UserName = "michael@example.com", Email = "michael@example.com", Name = "Michael Scott", EmailConfirmed = true, DateCreated = DateTime.UtcNow, DateUpdated = DateTime.UtcNow },
                    new User { UserName = "nancy@example.com", Email = "nancy@example.com", Name = "Nancy Drew", EmailConfirmed = true, DateCreated = DateTime.UtcNow, DateUpdated = DateTime.UtcNow },
                    new User { UserName = "oliver@example.com", Email = "oliver@example.com", Name = "Oliver Twist", EmailConfirmed = true, DateCreated = DateTime.UtcNow, DateUpdated = DateTime.UtcNow },
                    new User { UserName = "paula@example.com", Email = "paula@example.com", Name = "Paula Abdul", EmailConfirmed = true, DateCreated = DateTime.UtcNow, DateUpdated = DateTime.UtcNow },
                    new User { UserName = "quincy@example.com", Email = "quincy@example.com", Name = "Quincy Jones", EmailConfirmed = true, DateCreated = DateTime.UtcNow, DateUpdated = DateTime.UtcNow },
                    new User { UserName = "rachel@example.com", Email = "rachel@example.com", Name = "Rachel Green", EmailConfirmed = true, DateCreated = DateTime.UtcNow, DateUpdated = DateTime.UtcNow },
                    new User { UserName = "steve@example.com", Email = "steve@example.com", Name = "Steve Rogers", EmailConfirmed = true, DateCreated = DateTime.UtcNow, DateUpdated = DateTime.UtcNow },
                    new User { UserName = "tina@example.com", Email = "tina@example.com", Name = "Tina Turner", EmailConfirmed = true, DateCreated = DateTime.UtcNow, DateUpdated = DateTime.UtcNow },
                    new User { UserName = "ursula@example.com", Email = "ursula@example.com", Name = "Ursula Burns", EmailConfirmed = true, DateCreated = DateTime.UtcNow, DateUpdated = DateTime.UtcNow },
                    new User { UserName = "victor@example.com", Email = "victor@example.com", Name = "Victor Hugo", EmailConfirmed = true, DateCreated = DateTime.UtcNow, DateUpdated = DateTime.UtcNow },
                    new User { UserName = "wendy@example.com", Email = "wendy@example.com", Name = "Wendy Williams", EmailConfirmed = true, DateCreated = DateTime.UtcNow, DateUpdated = DateTime.UtcNow },
                    new User { UserName = "xavier@example.com", Email = "xavier@example.com", Name = "Xavier Cugat", EmailConfirmed = true, DateCreated = DateTime.UtcNow, DateUpdated = DateTime.UtcNow },
                    new User { UserName = "yvonne@example.com", Email = "yvonne@example.com", Name = "Yvonne Craig", EmailConfirmed = true, DateCreated = DateTime.UtcNow, DateUpdated = DateTime.UtcNow },
                    new User { UserName = "zack@example.com", Email = "zack@example.com", Name = "Zack Morris", EmailConfirmed = true, DateCreated = DateTime.UtcNow, DateUpdated = DateTime.UtcNow }
                };

                var passwords = new[] {
                    "AdminPassword123!", "JohnPassword123!", "JanePassword123!",
                    "Password123!", "Password123!", "Password123!", "Password123!", "Password123!", "Password123!", "Password123!",
                    "Password123!", "Password123!", "Password123!", "Password123!", "Password123!", "Password123!", "Password123!", "Password123!", "Password123!", "Password123!",
                    "Password123!", "Password123!", "Password123!", "Password123!", "Password123!", "Password123!", "Password123!", "Password123!", "Password123!"
                };
                var roleNames = new[] { "Admin", "User", "User", "User", "User", "User", "User", "User", "User", "User", "User", "User", "User", "User", "User", "User", "User", "User", "User", "User", "User", "User", "User", "User", "User", "User", "User", "User", "User" };

                for (int i = 0; i < users.Length; i++)
                {
                    var result = userManager.CreateAsync(users[i], passwords[i]).Result;
                    if (!result.Succeeded)
                    {
                        Console.WriteLine($"Failed to create user {users[i].UserName}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                    else
                    {
                        userManager.AddToRoleAsync(users[i], roleNames[i]).Wait();
                    }
                }
            }

            // Seed Categories
            if (!context.Categories.Any())
            {
                context.Categories.AddRange(
                    new Category { CategoryId = Guid.NewGuid(), Name = "Travel", Description = "Travel destinations and experiences" },
                    new Category { CategoryId = Guid.NewGuid(), Name = "Food", Description = "Dining and food recommendations" },
                    new Category { CategoryId = Guid.NewGuid(), Name = "Entertainment", Description = "Movies, music, and entertainment venues" },
                    new Category { CategoryId = Guid.NewGuid(), Name = "Shopping", Description = "Shopping malls and stores" },
                    new Category { CategoryId = Guid.NewGuid(), Name = "Nature", Description = "Parks, hiking, and outdoor activities" }
                );
                context.SaveChanges();
            }

            if (context.Posts.Count() < 3)
            {
                var author = context.Users.FirstOrDefault();
                if (author != null)
                {
                    // Clear existing if you want a fresh start, or just add more
                    context.Posts.AddRange(
                        new Post
                        {
                            PostId = Guid.NewGuid(),
                            UserId = author.Id,
                            Title = "Mountain Trip",
                            Description = "Hiking adventure in the north.",
                            LocationName = "Chiang Mai",
                            DateDeadline = DateTime.UtcNow.AddDays(7),
                            MinParticipants = 2,
                            MaxParticipants = 5,
                            DateCreated = DateTime.UtcNow,
                            Status = "Active",
                            CurrentParticipants = 1,
                            InviteCode = "TRIP01"
                        },
                        new Post
                        {
                            PostId = Guid.NewGuid(),
                            UserId = author.Id,
                            Title = "Cafe Hopping",
                            Description = "Exploring aesthetic cafes.",
                            LocationName = "Bangkok",
                            DateDeadline = DateTime.UtcNow.AddDays(3),
                            MinParticipants = 2,
                            MaxParticipants = 3,
                            DateCreated = DateTime.UtcNow,
                            Status = "Active",
                            CurrentParticipants = 1,
                            InviteCode = "CAFE02"
                        },
                        new Post
                        {
                            PostId = Guid.NewGuid(),
                            UserId = author.Id,
                            Title = "Beach Day",
                            Description = "Sun, sand, and relaxing vibes.",
                            LocationName = "Phuket",
                            DateDeadline = DateTime.UtcNow.AddDays(10),
                            MinParticipants = 4,
                            MaxParticipants = 10,
                            DateCreated = DateTime.UtcNow,
                            Status = "Active",
                            CurrentParticipants = 1,
                            InviteCode = "BEACH3"
                        }
                    );
                    context.SaveChanges();
                }
            }
        }
    }
}
