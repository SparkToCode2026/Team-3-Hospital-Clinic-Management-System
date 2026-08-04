namespace Team_3_HMS.Models
{
    public class Medication
    {
        public int MedicationID { get; set; }

        public string Name { get; set; } = string.Empty;

        public string GenericName { get; set; } = string.Empty;

        public string DosageForm { get; set; } = string.Empty;

        public decimal UnitPrice { get; set; }
    }
}
