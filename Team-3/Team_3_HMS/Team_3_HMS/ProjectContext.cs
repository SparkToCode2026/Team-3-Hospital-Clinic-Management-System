using Microsoft.EntityFrameworkCore;
using Team_3_HMS.Models;
namespace Team_3_HMS
{
    public class ProjectContext : DbContext
    {
       
            

            public ProjectContext(DbContextOptions<ProjectContext> options) : base(options)
            {

            }

            public DbSet<user> Users { get; set; }

            public DbSet<PatientProfile> PatientProfiles { get; set; }

            public DbSet<Invoice> Invoices { get; set; }

            public DbSet<MedicalRecord> MedicalRecords { get; set; }

            public DbSet<Prescription> Prescriptions { get; set; }

            public DbSet<Medication> Medications { get; set; }

            public DbSet<DoctorProfile> DoctorProfiles { get; set; }

            public DbSet<Department> Departments { get; set; }

            public DbSet<Specialization> Specializations { get; set; }

            public DbSet<Appointment> Appointments { get; set; }

            public DbSet<Room> Rooms { get; set; }

            public DbSet<LabTest> LabTests { get; set; }


        

    }
}
