using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Team_3_HMS.Models
{
    public class DoctorProfile
    {
        [Key]
        public int DoctorProfileId { get; set; }

        public string LicenseNumber { get; set; }

        public int YearsOfExperience { get; set; }

        public decimal ConsultationFee { get; set; }

        //has relationship 1:1 with user
        [ForeignKey("userid")]
        [Required]
        public int userID { get; set; }

        [JsonIgnore]
        public user? userid { get; set; }


        //employs relationship 1:N with Department
        [InverseProperty("Profile")]
        [JsonIgnore]
        public List<Department>? Departments { get; set; }

        //conducts relationship 1:N with Appointment
        [InverseProperty("DoctorProfile")]
        [JsonIgnore]
        public List<Appointment>? Appointments { get; set; }

        //practices relationship M:N with Specialization
        [ForeignKey("specializations")]
        [Required]
        public int SpecializationId { get; set; }

        [JsonIgnore]
        public List<Specialization>? specializations { get; set; }
    }
}
