using System.ComponentModel.DataAnnotations;

namespace API_Server.Models
{
    public class Slider
    {
        [Key]
        public int SlidersId { get; set; }

        public string ImageName { get; set; }

        public string Name { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime ExpiryDate { get; set; }

        public string Status { get; set; }
    }
}
