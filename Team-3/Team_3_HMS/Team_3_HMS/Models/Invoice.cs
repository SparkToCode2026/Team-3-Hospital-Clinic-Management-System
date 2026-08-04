using System.ComponentModel.DataAnnotations;
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


    }
}
