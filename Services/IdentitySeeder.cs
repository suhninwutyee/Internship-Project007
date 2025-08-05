using Microsoft.AspNetCore.Identity;
using ProjectManagementSystem.Models;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

public static class IdentitySeeder
{
    public static async Task SeedRolesAndAdminsAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        string[] roles = { "Admin", "User" };

        // Create roles if they don't exist
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Admin list
        var admins = new[]
        {
            new { Email = "admin1@example.com", Password = "Admin@1", FullName = "Admin One" },
            new { Email = "admin2@example.com", Password = "Admin@2", FullName = "Admin Two" }
        };

        // Seed each admin
        foreach (var admin in admins)
        {
            var user = await userManager.FindByEmailAsync(admin.Email);

            if (user == null)
            {
                var newAdmin = new ApplicationUser
                {
                    FullName = admin.FullName,
                    UserName = admin.Email,
                    Email = admin.Email,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(newAdmin, admin.Password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newAdmin, "Admin");
                    Console.WriteLine($"Admin created: {admin.Email}");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine($"Error creating admin {admin.Email}: {error.Description}");
                    }
                }
            }
            else
            {
                // Update FullName if needed
                if (user.FullName != admin.FullName)
                {
                    user.FullName = admin.FullName;
                    await userManager.UpdateAsync(user);
                }

                // Add to Admin role if not already
                if (!await userManager.IsInRoleAsync(user, "Admin"))
                {
                    await userManager.AddToRoleAsync(user, "Admin");
                }

                // ✅ Reset password
                var token = await userManager.GeneratePasswordResetTokenAsync(user);
                var result = await userManager.ResetPasswordAsync(user, token, admin.Password);

                if (result.Succeeded)
                {
                    Console.WriteLine($"Password reset for {admin.Email}");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine($"Error resetting password for {admin.Email}: {error.Description}");
                    }
                }
            }
        }
    }
}
