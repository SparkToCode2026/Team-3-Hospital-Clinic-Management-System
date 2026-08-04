using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Team_3_HMS.Models
{
    public class MedicalRecord
    {
        [Key]
        public int MedicalRecordID { get; set; }

        public string Diagnosis { get; set; } = string.Empty;

        public string TreatmentPlan { get; set; } = string.Empty;

        public string Symptom { get; set; } = string.Empty;

        public DateTime RecordDate { get; set; }

        public int AppointmentId { get; set; }


        //provides relationship 1:1 with appointment
        [InverseProperty("MedicalRecord")]
        [JsonIgnore]
        public int AppointmentID { get; set; }

        //contains relationship 1:N with prescription

        [InverseProperty("Medical")]
        [JsonIgnore]
        public List<Prescription>? Prescriptions { get; set; }

        //orders relationship 1:N with lab test
        [InverseProperty("record")]
        [JsonIgnore]
        public List<LabTest>? LabTests { get; set; }
    }
}
