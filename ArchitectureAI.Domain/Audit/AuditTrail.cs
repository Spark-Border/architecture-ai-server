using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ArchitectureAI.Domain.Audit
{
    public class AuditTrail
    {
        [Key]
        public int Id;
        public string ActionName { get; set; } = String.Empty;
        public string ActionDescription { get; set; } = String.Empty;
        public string Module { get; set; } = String.Empty;
        public string LoggedInUser { get; set; } = String.Empty;
        public string CreatedBy { get; set; } = String.Empty;
        public string Origin { get; set; } = String.Empty;
        public DateTime ActionTime { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
