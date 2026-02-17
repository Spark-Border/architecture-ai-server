using ArchitectureAI.Application.Interfaces.Repositories;
using ArchitectureAI.Domain.Users;
using Microsoft.AspNetCore.Identity;

namespace ArchitectureAI.Infrastructure.Identity;

public class FirestoreUserStore(IGenericRepository<ApplicationUser> userRepository)
    : IUserStore<ApplicationUser>,
        IUserPasswordStore<ApplicationUser>,
        IUserEmailStore<ApplicationUser>,
        IUserRoleStore<ApplicationUser>,
        IUserSecurityStampStore<ApplicationUser>
{
    private readonly IGenericRepository<ApplicationUser> _userRepository = userRepository;

    public async Task<IdentityResult> CreateAsync(
        ApplicationUser user,
        CancellationToken cancellationToken
    )
    {
        await _userRepository.AddAsync(user);
        return IdentityResult.Success;
    }

    public async Task<IdentityResult> DeleteAsync(
        ApplicationUser user,
        CancellationToken cancellationToken
    )
    {
        await _userRepository.RemoveAsync(user);
        return IdentityResult.Success;
    }

    public void Dispose()
    {
        // Nothing to dispose
    }

    public async Task<ApplicationUser?> FindByIdAsync(
        string userId,
        CancellationToken cancellationToken
    )
    {
        return await _userRepository.GetByIdAsync(userId);
    }

    public async Task<ApplicationUser?> FindByNameAsync(
        string normalizedUserName,
        CancellationToken cancellationToken
    )
    {
        return await _userRepository.FindAsync(u => u.NormalizedUserName == normalizedUserName);
    }

    public Task<string?> GetNormalizedUserNameAsync(
        ApplicationUser user,
        CancellationToken cancellationToken
    )
    {
        return Task.FromResult(user.NormalizedUserName);
    }

    public Task<string> GetUserIdAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.Id.ToString());
    }

    public Task<string?> GetUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.UserName);
    }

    public Task SetNormalizedUserNameAsync(
        ApplicationUser user,
        string? normalizedName,
        CancellationToken cancellationToken
    )
    {
        user.NormalizedUserName = normalizedName ?? string.Empty;
        return Task.CompletedTask;
    }

    public Task SetUserNameAsync(
        ApplicationUser user,
        string? userName,
        CancellationToken cancellationToken
    )
    {
        user.UserName = userName ?? string.Empty;
        return Task.CompletedTask;
    }

    public async Task<IdentityResult> UpdateAsync(
        ApplicationUser user,
        CancellationToken cancellationToken
    )
    {
        await _userRepository.UpdateAsync(user);
        return IdentityResult.Success;
    }

    // IUserPasswordStore
    public Task<string?> GetPasswordHashAsync(
        ApplicationUser user,
        CancellationToken cancellationToken
    )
    {
        return Task.FromResult<string?>(user.PasswordHash);
    }

    public Task<bool> HasPasswordAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(!string.IsNullOrEmpty(user.PasswordHash));
    }

    public Task SetPasswordHashAsync(
        ApplicationUser user,
        string? passwordHash,
        CancellationToken cancellationToken
    )
    {
        user.PasswordHash = passwordHash ?? string.Empty;
        return Task.CompletedTask;
    }

    // IUserEmailStore
    public async Task<ApplicationUser?> FindByEmailAsync(
        string normalizedEmail,
        CancellationToken cancellationToken
    )
    {
        return await _userRepository.FindAsync(u => u.NormalizedEmail == normalizedEmail);
    }

    public Task<string?> GetEmailAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult<string?>(user.Email);
    }

    public Task<bool> GetEmailConfirmedAsync(
        ApplicationUser user,
        CancellationToken cancellationToken
    )
    {
        return Task.FromResult(user.EmailConfirmed);
    }

    public Task<string?> GetNormalizedEmailAsync(
        ApplicationUser user,
        CancellationToken cancellationToken
    )
    {
        return Task.FromResult<string?>(user.NormalizedEmail);
    }

    public Task SetEmailAsync(
        ApplicationUser user,
        string? email,
        CancellationToken cancellationToken
    )
    {
        user.Email = email ?? string.Empty;
        return Task.CompletedTask;
    }

    public Task SetEmailConfirmedAsync(
        ApplicationUser user,
        bool confirmed,
        CancellationToken cancellationToken
    )
    {
        user.EmailConfirmed = confirmed;
        return Task.CompletedTask;
    }

    public Task SetNormalizedEmailAsync(
        ApplicationUser user,
        string? normalizedEmail,
        CancellationToken cancellationToken
    )
    {
        user.NormalizedEmail = normalizedEmail ?? string.Empty;
        return Task.CompletedTask;
    }

    // IUserRoleStore
    public Task AddToRoleAsync(
        ApplicationUser user,
        string roleName,
        CancellationToken cancellationToken
    )
    {
        // For Firestore, simpler to store role names directly if ID lookup is expensive,
        // OR store IDs. Let's store Names for simplicity in this NoSQL implementation
        // as looking up RoleID by Name every time is costly without a join.
        // However, standard is usually ID.
        // Let's assume we store Role Normalized Names in the user for fast lookup.

        if (!user.Roles.Contains(roleName))
        {
            user.Roles.Add(roleName);
        }
        return Task.CompletedTask;
    }

    public Task RemoveFromRoleAsync(
        ApplicationUser user,
        string roleName,
        CancellationToken cancellationToken
    )
    {
        user.Roles.Remove(roleName);
        return Task.CompletedTask;
    }

    public Task<IList<string>> GetRolesAsync(
        ApplicationUser user,
        CancellationToken cancellationToken
    )
    {
        return Task.FromResult<IList<string>>(user.Roles);
    }

    public Task<bool> IsInRoleAsync(
        ApplicationUser user,
        string roleName,
        CancellationToken cancellationToken
    )
    {
        return Task.FromResult(user.Roles.Contains(roleName));
    }

    public Task<IList<ApplicationUser>> GetUsersInRoleAsync(
        string roleName,
        CancellationToken cancellationToken
    )
    {
        // This would require a specific query in Firestore: "Roles array-contains roleName"
        // IGenericRepository might not support array-contains directly with strict expressions unless mapped.
        // For now, throw NotSupported or implement inefficiently (not recommended).
        // Ideally, extend IGenericRepository or cast to specific repository.
        throw new NotImplementedException(
            "GetUsersInRoleAsync not implemented for FirestoreUserStore yet."
        );
    }

    // IUserSecurityStampStore
    public Task SetSecurityStampAsync(
        ApplicationUser user,
        string stamp,
        CancellationToken cancellationToken
    )
    {
        user.SecurityStamp = stamp;
        return Task.CompletedTask;
    }

    public Task<string?> GetSecurityStampAsync(
        ApplicationUser user,
        CancellationToken cancellationToken
    )
    {
        return Task.FromResult<string?>(user.SecurityStamp);
    }
}
