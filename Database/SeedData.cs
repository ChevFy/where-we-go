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

            if (context.Users.Any())
            {
                return;
            }

            var users = new[]
            {
                new User
                {
                    UserName = "admin@example.com",
                    Email = "admin@example.com",
                    Name = "Admin",
                    EmailConfirmed = true,
                    DateCreated = DateTime.UtcNow,
                    DateUpdated = DateTime.UtcNow
                },
                new User
                {
                    UserName = "john@example.com",
                    Email = "john@example.com",
                    Name = "John Doe",
                    EmailConfirmed = true,
                    DateCreated = DateTime.UtcNow,
                    DateUpdated = DateTime.UtcNow
                },
                new User
                {
                    UserName = "jane@example.com",
                    Email = "jane@example.com",
                    Name = "Jane Smith",
                    EmailConfirmed = true,
                    DateCreated = DateTime.UtcNow,
                    DateUpdated = DateTime.UtcNow
                }
            };

            var passwords = new[] { "AdminPassword123!", "JohnPassword123!", "JanePassword123!" };
            var roleNames = new[] { "Admin", "User", "User" };

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

            // Seed Categories
            context.Categories.AddRange(
                new Category
                {
                    CategoryId = Guid.NewGuid(),
                    Name = "Travel",
                    Description = "Travel destinations and experiences"
                },
                new Category
                {
                    CategoryId = Guid.NewGuid(),
                    Name = "Food",
                    Description = "Dining and food recommendations"
                },
                new Category
                {
                    CategoryId = Guid.NewGuid(),
                    Name = "Entertainment",
                    Description = "Movies, music, and entertainment venues"
                },
                new Category
                {
                    CategoryId = Guid.NewGuid(),
                    Name = "Shopping",
                    Description = "Shopping malls and stores"
                },
                new Category
                {
                    CategoryId = Guid.NewGuid(),
                    Name = "Nature",
                    Description = "Parks, hiking, and outdoor activities"
                }
            );
            context.SaveChanges();
        }
    }
}