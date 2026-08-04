using System.ComponentModel.DataAnnotations;

namespace Team_3_HMS.Models
{
    public class DoctorProfile
    {
        [Key]
        public int DoctorProfileId { get; set; }

        public string LicenseNumber { get; set; }

        public int YearsOfExperience { get; set; }

        public decimal ConsultationFee { get; set; }

    }
}
