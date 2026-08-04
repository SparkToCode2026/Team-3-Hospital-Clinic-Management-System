using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Team_3_HMS.Models
{
    public class MedicalRecord
    {
        [Key]
        public int MedicalRecordID { get; set; }

        [Required]
        public string Diagnosis { get; set; }

        [Required]
        public string TreatmentPlan { get; set; }

        [Required]
        public string Symptom { get; set; }

        [Required]
        public string RecordDate { get; set; }

        // Foreign key matching Appointment
        [ForeignKey("Appointment")]
        public int AppointmentId { get; set; }

        [JsonIgnore]
        public Appointment? Appointment { get; set; }

        // 1:N relationship with Prescription
        [InverseProperty("MedicalRecord")]
        [JsonIgnore]
        public List<Prescription>? Prescriptions { get; set; }

        // 1:N relationship with LabTest
        [InverseProperty("record")]
        [JsonIgnore]
        public List<LabTest>? LabTests { get; set; }
    }
}