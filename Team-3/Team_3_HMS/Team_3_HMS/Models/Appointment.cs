using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Team_3_HMS.Models
{
    public class Appointment
    {
        [Key]
        public int AppointmentId { get; set; }

        public string Status { get; set; }

        public DateTime AppointmentDateTime { get; set; }

        public string ReasonForVisit { get; set; }

        //books relationship 1:N with PatientProfile
        [ForeignKey("PatientProfile")]
        [Required]
        public int PatientProfileID { get; set; }

        [JsonIgnore]
        public PatientProfile? PatientProfile { get; set; }

        //conducts relationship 1:N with DoctorProfile

        [ForeignKey("DoctorProfile")]
        public int DoctorProfileId { get; set; }

        [JsonIgnore]
        public DoctorProfile? DoctorProfile { get; set; }

        //hosts relationship 1:N with Room

        [ForeignKey("Room")]
        public int RoomId { get; set; }

        [JsonIgnore]
        public Room? room { get; set; }

    }
}