namespace ArchitectureAI.Persistence.Context;

public class FirestoreDbContext
{
    private readonly FirestoreDb _firestoreDb;

    public FirestoreDbContext(IConfiguration configuration)
    {
        var projectId = configuration["Firebase:ProjectId"];
        if (string.IsNullOrEmpty(projectId))
        {
            throw new ArgumentNullException(nameof(projectId), "Firebase ProjectId is not configured.");
        }
        
        // Ensure GOOGLE_APPLICATION_CREDENTIALS is set in environment or handled via default auth
        _firestoreDb = FirestoreDb.Create(projectId);
    }

    public CollectionReference Collection(string collectionName)
    {
        return _firestoreDb.Collection(collectionName);
    }
    
    public FirestoreDb Db => _firestoreDb;
}
