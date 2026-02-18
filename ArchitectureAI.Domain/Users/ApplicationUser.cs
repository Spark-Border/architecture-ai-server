using ArchitectureAI.Domain.Common;
using Google.Cloud.Firestore;

namespace ArchitectureAI.Domain.Users
{
    [FirestoreData]
    public class ApplicationUser : Entity
    {
        [FirestoreProperty]
        public string UserName { get; set; } = string.Empty;

        [FirestoreProperty]
        public string NormalizedUserName { get; set; } = string.Empty;

        [FirestoreProperty]
        public string Email { get; set; } = string.Empty;

        [FirestoreProperty]
        public string NormalizedEmail { get; set; } = string.Empty;

        [FirestoreProperty]
        public bool EmailConfirmed { get; set; }

        [FirestoreProperty]
        public string PasswordHash { get; set; } = string.Empty;

        [FirestoreProperty]
        public string SecurityStamp { get; set; } = Guid.NewGuid().ToString();

        [FirestoreProperty]
        public string ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();

        [FirestoreProperty]
        public string Name { get; set; } = string.Empty;

        [FirestoreProperty]
        public string? RefreshToken { get; set; }

        [FirestoreProperty]
        public DateTime? RefreshTokenExpiryTime { get; set; }

        // RBAC: List of Role IDs assigned to this user
        [FirestoreProperty]
        public List<string> Roles { get; set; } = [];
    }
}
