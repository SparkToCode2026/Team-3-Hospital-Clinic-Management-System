using Microsoft.EntityFrameworkCore;

namespace Team_3_HMS
{
    public class ProjectContext 
    {
        public class DBContext : DbContext
        {

          
            public DBContext(DbContextOptions<DBContext> options) : base(options)
            {

            }
        }

    }
}
