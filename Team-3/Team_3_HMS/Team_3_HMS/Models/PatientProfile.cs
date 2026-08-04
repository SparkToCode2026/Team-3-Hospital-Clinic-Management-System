using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Team_3_HMS.Models
{
    public class PatientProfile
    {
        [Key]
        public int PatientProfileID { get; set; }

        [Required]
        public string gender { get; set; }

        [Required]
        public string DateOfBirth { get; set; }

        [Required]
        public string BloodGroup { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string emergencyContact { get; set; }

        //owns

        [ForeignKey("user")]
        [Required]
        public int userID { get; set; }

        [JsonIgnore]
        public user? user { get; set; }
    }
}
