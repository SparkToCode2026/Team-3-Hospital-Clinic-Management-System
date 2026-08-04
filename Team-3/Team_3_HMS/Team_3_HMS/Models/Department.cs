using System.ComponentModel.DataAnnotations;

namespace Team_3_HMS.Models
{
    public class Department
    {
        [Key]
        public int DepartmentId { get; set; }
        public string Name { get; set; }

        public string Description { get; set; }

        public string BuildingLocation { get; set; }

    }
}
