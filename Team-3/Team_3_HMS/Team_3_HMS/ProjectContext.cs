using Microsoft.EntityFrameworkCore;
using Team_3_HMS.Models;
namespace Team_3_HMS
{
    public class ProjectContext 
    {
        public class DBContext : DbContext
        {

          
            public DBContext(DbContextOptions<DBContext> options) : base(options)
            {

            }
            public DbSet<MedicalRecord> MedicalRecords { get; set; } = null!;

            public DbSet<Prescription> Prescriptions { get; set; } = null!;

            public DbSet<Medication> Medications { get; set; } = null!;

            public DbSet<PrescriptionItem> PrescriptionItems { get; set; } = null!;
        }

    }
}
