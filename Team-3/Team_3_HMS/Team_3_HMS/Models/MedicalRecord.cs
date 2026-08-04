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
        public string RecordDate { get; set; }

        [Required]
        public int AppointmentId { get; set; }


        //provides relationship 1:1 with appointment
        [InverseProperty("MedicalRecord")]
        [JsonIgnore]
        public Appointment? AppointmentID { get; set; }

        //contains relationship 1:N with prescription 12

        [InverseProperty("Medical")]
        [JsonIgnore]
        public List<Prescription>? Prescriptions { get; set; }

        //orders relationship 1:N with lab test
        [InverseProperty("record")]
        [JsonIgnore]
        public List<LabTest>? LabTests { get; set; }
    }
}
