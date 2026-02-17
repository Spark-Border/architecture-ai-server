using ArchitectureAI.Application.Interfaces.Repositories;
using ArchitectureAI.Domain.Users;
using Microsoft.AspNetCore.Identity;

namespace ArchitectureAI.Infrastructure.Identity;

public class FirestoreRoleStore(IGenericRepository<ApplicationRole> roleRepository)
    : IRoleStore<ApplicationRole>
{
    private readonly IGenericRepository<ApplicationRole> _roleRepository = roleRepository;

    public async Task<IdentityResult> CreateAsync(
        ApplicationRole role,
        CancellationToken cancellationToken
    )
    {
        await _roleRepository.AddAsync(role);
        return IdentityResult.Success;
    }

    public async Task<IdentityResult> DeleteAsync(
        ApplicationRole role,
        CancellationToken cancellationToken
    )
    {
        await _roleRepository.RemoveAsync(role);
        return IdentityResult.Success;
    }

    public void Dispose() { }

    public async Task<ApplicationRole?> FindByIdAsync(
        string roleId,
        CancellationToken cancellationToken
    )
    {
        return await _roleRepository.GetByIdAsync(roleId);
    }

    public async Task<ApplicationRole?> FindByNameAsync(
        string normalizedRoleName,
        CancellationToken cancellationToken
    )
    {
        return await _roleRepository.FindAsync(r => r.NormalizedName == normalizedRoleName);
    }

    public Task<string?> GetNormalizedRoleNameAsync(
        ApplicationRole role,
        CancellationToken cancellationToken
    )
    {
        return Task.FromResult<string?>(role.NormalizedName);
    }

    public Task<string> GetRoleIdAsync(ApplicationRole role, CancellationToken cancellationToken)
    {
        return Task.FromResult(role.Id.ToString());
    }

    public Task<string?> GetRoleNameAsync(ApplicationRole role, CancellationToken cancellationToken)
    {
        return Task.FromResult<string?>(role.Name);
    }

    public Task SetNormalizedRoleNameAsync(
        ApplicationRole role,
        string? normalizedName,
        CancellationToken cancellationToken
    )
    {
        role.NormalizedName = normalizedName ?? string.Empty;
        return Task.CompletedTask;
    }

    public Task SetRoleNameAsync(
        ApplicationRole role,
        string? roleName,
        CancellationToken cancellationToken
    )
    {
        role.Name = roleName ?? string.Empty;
        return Task.CompletedTask;
    }

    public async Task<IdentityResult> UpdateAsync(
        ApplicationRole role,
        CancellationToken cancellationToken
    )
    {
        await _roleRepository.UpdateAsync(role);
        return IdentityResult.Success;
    }
}
