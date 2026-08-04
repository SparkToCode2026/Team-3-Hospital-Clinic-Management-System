using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Team_3_HMS.Models
{
    public class Medication
    {
        [Key]
        public int MedicationID { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string GenericName { get; set; }
        
        [Required]
        public string DosageForm { get; set; }

        [Required]
        public double UnitPrice { get; set; }

        [JsonIgnore]
        public List<Prescription>? Prescriptions { get; set; }
    }
}
