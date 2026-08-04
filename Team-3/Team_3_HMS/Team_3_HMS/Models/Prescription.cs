namespace Team_3_HMS.Models
{
    public class Prescription
    {
        public int PrescriptionID { get; set; }

        public DateTime IssuedDate { get; set; }

        public string Instructions { get; set; } = string.Empty;

        public string Notes { get; set; } = string.Empty;

        public int MedicalRecordID { get; set; }
    }
}
