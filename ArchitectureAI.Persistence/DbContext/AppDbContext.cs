using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArchitectureAI.Domain.Audit;

namespace ArchitectureAI.Persistence.DbContext
{
    public class AppDbContext
    {
        public AppDbContext() { }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public static AppDbContext Create()
        {
            return new AppDbContext();
        }

        public DbSet<AuditTrail> AuditTrail { get; set; }
    }
}
