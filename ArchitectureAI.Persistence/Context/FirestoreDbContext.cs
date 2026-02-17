using Google.Cloud.Firestore;
using Microsoft.Extensions.Configuration;

namespace ArchitectureAI.Persistence.Context;

public class FirestoreDbContext
{
    private readonly FirestoreDb _firestoreDb;

    public FirestoreDbContext(IConfiguration configuration)
    {
        // Enforce Environment Variable only
        var projectId = Environment.GetEnvironmentVariable("FIREBASE_PROJECT_ID");

        if (string.IsNullOrEmpty(projectId))
        {
            throw new InvalidOperationException(
                "FIREBASE_PROJECT_ID environment variable is missing. Please set it in your .env file or system environment."
            );
        }

        // Ensure GOOGLE_APPLICATION_CREDENTIALS is set in environment or handled via default auth
        var emulatorHost = Environment.GetEnvironmentVariable("FIRESTORE_EMULATOR_HOST");
        if (!string.IsNullOrEmpty(emulatorHost))
        {
            _firestoreDb = new FirestoreDbBuilder
            {
                ProjectId = projectId,
                EmulatorDetection = Google.Api.Gax.EmulatorDetection.EmulatorOnly
            }.Build();
        }
        else
        {
            _firestoreDb = FirestoreDb.Create(projectId);
        }
    }

    public CollectionReference Collection(string collectionName)
    {
        return _firestoreDb.Collection(collectionName);
    }

    public FirestoreDb Db => _firestoreDb;
}
