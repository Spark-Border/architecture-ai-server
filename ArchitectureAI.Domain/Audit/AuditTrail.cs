using Google.Cloud.Firestore;
using ArchitectureAI.Domain.Common;

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
