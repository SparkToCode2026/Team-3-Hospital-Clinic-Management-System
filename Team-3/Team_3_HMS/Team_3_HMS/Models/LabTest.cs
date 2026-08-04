using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Team_3_HMS.Models
{
    public class LabTest
    {
        [Key]
        public int LabTestId { get; set; }

        public string TestName { get; set; }

        public string Category { get; set; }

        public DateTime TestDate { get; set; }

        public decimal Cost { get; set; }

        public string Result { get; set; }

        //orders relationship 1:N with medical record
        [ForeignKey("record")]
        [Required]
        public int MedicalRecordId { get; set; }

        public MedicalRecord record { get; set; }
    }
}