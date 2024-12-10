using Microsoft.AspNetCore.Identity;
using BevoBnB.Models;
using BevoBnB.DAL;

namespace BevoBnB.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(AppDbContext context,
            UserManager<Users> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            context.Database.EnsureCreated();

            // Create roles
            string[] roles = { "Admin", "Host", "Customer" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Create admin user
            if (await userManager.FindByEmailAsync("admin@bevobnb.com") == null)
            {
                var admin = new Users
                {
                    UserName = "admin@bevobnb.com", // Add this line
                    Email = "admin@bevobnb.com",
                    FirstName = "System",
                    LastName = "Admin",
                    Birthday = new DateTime(1990, 1, 1),
                    Address = "123 Admin St",
                    Phone = "555-0123",
                    HireStatus = true,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true, // Add this for completeness
                    LockoutEnabled = false // Prevent accidental lockout
                };

                var result = await userManager.CreateAsync(admin, "Admin123!");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
                else
                {
                    // Log or handle the error
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new Exception($"Failed to create admin user: {errors}");
                }
            }

            await context.SaveChangesAsync();
        }
    }
}