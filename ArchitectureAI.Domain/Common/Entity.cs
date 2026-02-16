using Google.Cloud.Firestore;

namespace ArchitectureAI.Domain.Common;

[FirestoreData]
public class Entity
{
    [FirestoreDocumentId]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [FirestoreProperty]
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;

    [FirestoreProperty]
    public DateTime? DateModified { get; set; }
}
