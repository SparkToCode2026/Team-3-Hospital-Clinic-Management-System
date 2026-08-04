using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Team_3_HMS.Models
{
    public class Specialization
    {
        [Key]
        public int SpecializationId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        //practiced relationship M:N with DoctorProfile
        [ForeignKey("Doctor_specialization")]
        [Required]
        public int DoctorProfileId { get; set; }

        [JsonIgnore]
        public List<DoctorProfile>? Doctor_specialization { get; set; }
    }
}
