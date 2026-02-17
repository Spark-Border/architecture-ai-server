using ArchitectureAI.Application.Constants;
using ArchitectureAI.Application.Interfaces.Services;
using ArchitectureAI.Domain.Users;
using Microsoft.AspNetCore.Identity;

namespace ArchitectureAI.Infrastructure.Data;

public class DataSeeder(
    RoleManager<ApplicationRole> roleManager,
    UserManager<ApplicationUser> userManager,
    ITenantService tenantService
)
{
    private readonly RoleManager<ApplicationRole> _roleManager = roleManager;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly ITenantService _tenantService = tenantService;

    public async Task SeedAsync()
    {
        // Set System Tenant Context for Seeding
        _tenantService.SetTenant("012345678901");

        await SeedRolesAsync();
        await SeedUsersAsync();
    }

    private async Task SeedUsersAsync()
    {
        var adminEmail = "superadmin@architectureai.com";
        var adminPassword = "P@ssw0rd!";

        if (await _userManager.FindByEmailAsync(adminEmail) == null)
        {
            var adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                Name = "Super Admin",
                EmailConfirmed = true,
            };

            var result = await _userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(adminUser, "Root Admin");
            }
        }
    }

    private async Task SeedRolesAsync()
    {
        // Define default roles
        var roles = new List<ApplicationRole>
        {
            new()
            {
                Name = "Root Admin",
                Description = "System Administrator with full access",
                IsSystemRole = true,
                Permissions = Permissions.All, // Grant all permissions
            },
            new()
            {
                Name = "Manager",
                Description = "Project Manager",
                IsSystemRole = false,
                Permissions = new List<string>
                {
                    Permissions.Users.View,
                    Permissions.Projects.View,
                    Permissions.Projects.Create,
                    Permissions.Projects.Edit,
                    Permissions.Roles.View,
                },
            },
            new()
            {
                Name = "Viewer",
                Description = "Read-only access",
                IsSystemRole = true,
                Permissions = new List<string>
                {
                    Permissions.Users.View,
                    Permissions.Projects.View,
                },
            },
        };

        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role.Name))
            {
                await _roleManager.CreateAsync(role);
            }
            else
            {
                // Optional: Update permissions for existing system roles to ensure they are up to date with code constants
                if (role.IsSystemRole)
                {
                    var existingRole = await _roleManager.FindByNameAsync(role.Name);
                    if (existingRole != null)
                    {
                        // Merge or overwrite permissions? Let's overwrite for system roles to match code
                        existingRole.Permissions = role.Permissions;
                        await _roleManager.UpdateAsync(existingRole);
                    }
                }
            }
        }
    }
}
