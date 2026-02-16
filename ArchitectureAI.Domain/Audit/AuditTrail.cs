using ArchitectureAI.Domain.Common;
using Google.Cloud.Firestore;

namespace ArchitectureAI.Domain.Audit
{
    [FirestoreData]
    public class AuditTrail : Entity
    {
        [FirestoreProperty]
        public string ActionName { get; set; } = String.Empty;

        [FirestoreProperty]
        public string ActionDescription { get; set; } = String.Empty;

        [FirestoreProperty]
        public string Type { get; set; } = "General"; // Pipeline vs Data

        [FirestoreProperty]
        public string Module { get; set; } = String.Empty;

        [FirestoreProperty]
        public string LoggedInUser { get; set; } = String.Empty;

        [FirestoreProperty]
        public string CreatedBy { get; set; } = String.Empty;

        [FirestoreProperty]
        public string Origin { get; set; } = String.Empty;

        [FirestoreProperty]
        public DateTime ActionTime { get; set; }
    }
}
