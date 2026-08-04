using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Team_3_HMS.Models
{
    public class Department
    {
        [Key]
        public int DepartmentId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string BuildingLocation { get; set; }


        //employs relationship 1:N with DoctorProfile

        [ForeignKey("Profile")]
        [Required]
        public int DoctorProfileId { get; set; }

        [JsonIgnore]
        public DoctorProfile? Profile { get; set; }

     
    }
}
