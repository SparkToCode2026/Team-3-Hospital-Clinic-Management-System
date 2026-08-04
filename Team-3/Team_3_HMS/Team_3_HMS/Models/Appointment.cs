using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Team_3_HMS.Models
{
    public class Appointment
    {
        [Key]
        public int AppointmentId { get; set; }
        [Required]
        public string Status { get; set; }

        [Required]
        public DateTime AppointmentDateTime { get; set; }

        [Required]
        public string ReasonForVisit { get; set; }

        //books relationship 1:N with PatientProfile
        [ForeignKey("PatientProfile")]
        [Required]
        public int PatientProfileID { get; set; }

        [JsonIgnore]
        public PatientProfile? PatientProfile { get; set; }

        //conducts relationship 1:N with DoctorProfile

        [ForeignKey("DoctorProfile")]
        [Required]
        public int DoctorProfileId { get; set; }

        [JsonIgnore]
        public DoctorProfile? DoctorProfile { get; set; }

        //hosts relationship 1:N with Room

        [ForeignKey("Room")]
        [Required]
        public int RoomId { get; set; }

        [JsonIgnore]
        public Room? room { get; set; }

        //provides relationship 1:1 with medical record
        [ForeignKey("MedicalRecord")]
        [Required]
        public int MedicalRecordID { get; set; }

        [JsonIgnore]
        public MedicalRecord? MedicalRecord { get; set; }

        //generates relationship 1:1 with Invoice 
        [InverseProperty("Appointment")]
        [JsonIgnore]
        public Invoice? invoice { get; set; }

    }
}