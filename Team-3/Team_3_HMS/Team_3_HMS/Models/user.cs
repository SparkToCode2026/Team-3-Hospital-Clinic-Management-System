using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Team_3_HMS.Models
{
    public class user
    {
        [Key]
        public int userID { get; set; }

        [Required]
        public string Fullname { get; set; }

        [Required]
        public string email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        public string role { get; set; }

        [Required]
        public string Phone { get; set; }

        //owns relationship 1:1

        [InverseProperty("user")]
        [JsonIgnore]
        public PatientProfile? Profile { get; set; }
    }
}
