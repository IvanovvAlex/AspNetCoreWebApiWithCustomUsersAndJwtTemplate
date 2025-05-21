using Template.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Template.Core.StaticClasses;

namespace Template.Data.Seed;
public static class UserSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (context.Users.Any())
        {
            return;
        }

        PasswordHasher<User> hasher = new();

        List<User> users = [];

        User admin = new()
        {
            Email = "admin@template.com",
            Names = "Admin",
            Phone = "0872123199",
            Role = Roles.Admin,
        };
        admin.PasswordHash = hasher.HashPassword(admin, "Admin123!");
        users.Add(admin);

        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();
    }
}