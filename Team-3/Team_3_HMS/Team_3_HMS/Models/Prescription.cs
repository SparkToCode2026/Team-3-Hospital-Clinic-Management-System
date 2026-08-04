using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Team_3_HMS.Models
{
    public class Prescription
    {
        [Key]
        public int PrescriptionID { get; set; }

        [Required]
        public string IssuedDate { get; set; }

        [Required]
        public string Instructions { get; set; } = string.Empty;
        
        [Required]
        public string Notes { get; set; } = string.Empty;

        //contains relationship 1:N with medical record
        [ForeignKey("Medical")]
        [Required]
        public int MedicalRecordId { get; set; }

        [JsonIgnore]
        public MedicalRecord? Medical { get; set; }

        [JsonIgnore]
        public List<Medication>? Medications { get; set; }
    }
}
