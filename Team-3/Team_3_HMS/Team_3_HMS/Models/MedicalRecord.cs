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
        public string Diagnosis { get; set; } = string.Empty;

        [Required]
        public string TreatmentPlan { get; set; } = string.Empty;

        [Required]
        public string Symptom { get; set; } = string.Empty;

        [Required]
        public string RecordDate { get; set; } = string.Empty;

        [ForeignKey("Appointment")]
        public int AppointmentId { get; set; }

        [JsonIgnore]
        public Appointment? Appointment { get; set; }

        [InverseProperty("Medical")]
        [JsonIgnore]
        public List<Prescription>? p { get; set; }

        // 1:N relationship with LabTest
        [InverseProperty("record")]
        [JsonIgnore]
        public List<LabTest>? LabTests { get; set; }
    }
}