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
            public DbSet<DoctorProfile> DoctorProfiles { get; set; }

            public DbSet<Department> Departments { get; set; }

            public DbSet<Specialization> Specializations { get; set; }

            public DbSet<DoctorSpecialization> DoctorSpecializations { get; set; }


        }

    }
}
