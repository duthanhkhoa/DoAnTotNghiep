using System.ComponentModel.DataAnnotations;

namespace API_Server.Models
{
    public class News
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Content { get; set; }

        public string Image { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime ExpiryDate { get; set; }

        public string Status { get; set; }
    }
}
