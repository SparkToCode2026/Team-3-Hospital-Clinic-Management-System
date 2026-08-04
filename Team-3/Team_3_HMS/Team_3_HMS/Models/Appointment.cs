using System.ComponentModel.DataAnnotations;

namespace Team_3_HMS.Models
{
    public class Appointment
    {
        [Key]
        public int AppointmentId { get; set; }

        public string Status { get; set; }

        public DateTime AppointmentDateTime { get; set; }

        public string ReasonForVisit { get; set; }
    }
}