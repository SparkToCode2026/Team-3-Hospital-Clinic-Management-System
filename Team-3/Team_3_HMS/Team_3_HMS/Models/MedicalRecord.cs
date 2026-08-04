namespace Team_3_HMS.Models
{
    public class MedicalRecord
    {
        public int MedicalRecordID { get; set; }

        public string Diagnosis { get; set; } = string.Empty;

        public string TreatmentPlan { get; set; } = string.Empty;

        public string Symptom { get; set; } = string.Empty;

        public DateTime RecordDate { get; set; }

        public int AppointmentId { get; set; }
    }
}
