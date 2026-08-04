using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Team_3_HMS.Models
{
    public class Room
    {
        [Key]
        public int RoomId { get; set; }

        public string RoomNumber { get; set; }

        public string Type { get; set; }

        public bool IsAvailable { get; set; }

        //hosts relationship 1:N with Appointment

        [InverseProperty("room")]
        [JsonIgnore]
        public List<Appointment>? appointments { get; set; }
    }
}