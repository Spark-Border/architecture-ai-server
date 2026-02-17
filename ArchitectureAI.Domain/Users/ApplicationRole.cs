namespace ArchitectureAI.Domain.Users;

public class ApplicationRole
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string NormalizedName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();
    public bool IsSystemRole { get; set; } = false; // System roles cannot be deleted

    // GCP-style Permissions: "compute.instances.create", "users.view", etc.
    public List<string> Permissions { get; set; } = new();
}
