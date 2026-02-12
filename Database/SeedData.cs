using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using WhereWeGo.Models;

namespace WhereWeGo.Database;

public static class SeedData
{
    public static void Initialize(IServiceProvider serviceProvider)
    {
        using (var context = new AppDbContext(
            serviceProvider.GetRequiredService<
                DbContextOptions<AppDbContext>>()))
        {
            Console.WriteLine("Seeding initial data...");
            if (context.Users.Any())
            {
                return;
            }
            context.Users.AddRange(
                new User
                {
                    UserName = "admin@example.com",
                    Email = "admin@example.com",
                    Firstname = "Admin",
                    Lastname = "User",
                    Role = UserRoleEnum.Admin,
                    DateCreated = DateTime.UtcNow,
                    DateUpdated = DateTime.UtcNow
                },
                new User
                {
                    UserName = "john@example.com",
                    Email = "john@example.com",
                    Firstname = "John",
                    Lastname = "Doe",
                    Role = UserRoleEnum.User,
                    DateCreated = DateTime.UtcNow,
                    DateUpdated = DateTime.UtcNow
                },
                new User
                {
                    UserName = "jane@example.com",
                    Email = "jane@example.com",
                    Firstname = "Jane",
                    Lastname = "Smith",
                    Role = UserRoleEnum.User,
                    DateCreated = DateTime.UtcNow,
                    DateUpdated = DateTime.UtcNow
                }
            );
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