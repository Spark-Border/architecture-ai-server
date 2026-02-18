using ArchitectureAI.Domain.Common;
using Google.Cloud.Firestore;

namespace ArchitectureAI.Domain.Users;

[FirestoreData]
public class ApplicationRole : Entity
{
    [FirestoreProperty]
    public string Name { get; set; } = string.Empty;

    [FirestoreProperty]
    public string NormalizedName { get; set; } = string.Empty;

    [FirestoreProperty]
    public string Description { get; set; } = string.Empty;

    [FirestoreProperty]
    public string ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();

    [FirestoreProperty]
    public bool IsSystemRole { get; set; } = false; // System roles cannot be deleted

    // GCP-style Permissions: "compute.instances.create", "users.view", etc.
    [FirestoreProperty]
    public List<string> Permissions { get; set; } = [];
}
