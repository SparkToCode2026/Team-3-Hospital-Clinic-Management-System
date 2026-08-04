using System.ComponentModel.DataAnnotations;

namespace Team_3_HMS.Models
{
    public class Specialization
    {
        [Key]
        public int SpecializationId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
    }
}
