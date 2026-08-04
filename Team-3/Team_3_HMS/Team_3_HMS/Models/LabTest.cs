using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Team_3_HMS.Models
{
    public class LabTest
    {
        [Key]
        public int LabTestId { get; set; }

        [Required]
        public string TestName { get; set; }

        [Required]
        public string Category { get; set; }

        [Required]
        public string TestDate { get; set; }

        [Required]
        public decimal Cost { get; set; }

        [Required]
        public string Result { get; set; }

        //orders relationship 1:N with medical record 10
        [ForeignKey("record")]
        [Required]
        public int MedicalRecordId { get; set; }

        public MedicalRecord? record { get; set; }
    }
}