using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Team_3_HMS.Models
{
    public class Invoice
    {
        [Key]
        public int InvoiceID { get; set; }

        [Required]
        public double TotalAmount { get; set; }

        [Required]
        public string Paymentmethod { get; set; }

        [Required]
        public string PaymentStatus { get; set; } = "Pending";

        [Required]
        public string IssuedDate { get; set; }

        //generates relationship 1:1 with Appointment
        [ForeignKey("Appointment")]
        [Required]
        public int AppointmentID { get; set; }

        [JsonIgnore]
        public Appointment? Appointment { get; set; }
    }
}
