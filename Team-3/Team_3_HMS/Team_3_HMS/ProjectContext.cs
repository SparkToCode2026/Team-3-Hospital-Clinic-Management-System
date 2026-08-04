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

            //registering models

            public DbSet<user> Users { get; set; }
            public DbSet<PatientProfile> PatientProfiles { get; set; }
            public DbSet<Invoice> Invoices { get; set; }
        }

    }
}
