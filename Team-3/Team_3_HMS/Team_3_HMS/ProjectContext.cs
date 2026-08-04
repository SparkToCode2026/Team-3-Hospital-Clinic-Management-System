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
            public DbSet<DoctorProfile> DoctorProfiles { get; set; }

            public DbSet<Department> Departments { get; set; }

            public DbSet<Specialization> Specializations { get; set; }

            public DbSet<DoctorSpecialization> DoctorSpecializations { get; set; }

            public DbSet<Appointment> Appointments { get; set; }

            public DbSet<Room> Rooms { get; set; }

            public DbSet<LabTest> LabTests { get; set; }


        }

    }
}
