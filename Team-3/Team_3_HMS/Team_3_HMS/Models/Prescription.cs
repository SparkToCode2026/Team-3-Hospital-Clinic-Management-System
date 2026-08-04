using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Team_3_HMS.Models
{
    public class Prescription
    {
        public int PrescriptionID { get; set; }

        public DateTime IssuedDate { get; set; }

        public string Instructions { get; set; } = string.Empty;

        public string Notes { get; set; } = string.Empty;

        public int MedicalRecordID { get; set; }

        //contains relationship 1:N with medical record
        [ForeignKey("Medical")]
        [Required]
        public int MedicalRecordId { get; set; }

        [JsonIgnore]
        public MedicalRecord? Medical { get; set; }

        //lists relation with medication M:N
        [ForeignKey("Medications")]
        [Required]
        public int MedicationID { get; set; }

        [JsonIgnore]
        public List<Medication> Medications { get; set; }
    }
}
