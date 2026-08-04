using System.ComponentModel.DataAnnotations;

namespace Team_3_HMS.Models
{
    public class Room
    {
        [Key]
        public int RoomId { get; set; }

        public string RoomNumber { get; set; }

        public string Type { get; set; }

        public bool IsAvailable { get; set; }
    }
}