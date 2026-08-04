using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Team_3_HMS.Models
{
    public class Medication
    {
        public int MedicationID { get; set; }

        public string Name { get; set; } = string.Empty;

        public string GenericName { get; set; } = string.Empty;

        public string DosageForm { get; set; } = string.Empty;

        public decimal UnitPrice { get; set; }

        //lists relation with prescription M:N
        [ForeignKey("Prescriptions")]
        [Required]
        public int PrescriptionID { get; set; }

        [JsonIgnore]
        public List<Prescription> Prescriptions { get; set; }
    }
}
