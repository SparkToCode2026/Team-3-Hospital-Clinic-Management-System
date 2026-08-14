using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Team_3_HMS.Models
{
    public class DoctorProfile
    {
        [Key]
        public int DoctorProfileId { get; set; }

        [Required]
        public string LicenseNumber { get; set; }

        [Required]
        public int YearsOfExperience { get; set; }

        [Required]
        public double ConsultationFee { get; set; }

        [ForeignKey("userid")]
        [Required]
        public int userID { get; set; }

        [JsonPropertyName("user")]
        public user? userid { get; set; }


        //employs relationship 1:N with Department 3
        [InverseProperty("Profile")]
        [JsonIgnore]
        public List<Department>? Departments { get; set; }

        //conducts relationship 1:N with Appointment 4
        [InverseProperty("DoctorProfile")]
        [JsonIgnore]
        public List<Appointment>? Appointments { get; set; }

        //practices relationship M:N with Specialization 5
        [ForeignKey("specializations")]
        [Required]
        public int SpecializationId { get; set; }

        [JsonIgnore]
        public List<Specialization>? specializations { get; set; }
    }
}
