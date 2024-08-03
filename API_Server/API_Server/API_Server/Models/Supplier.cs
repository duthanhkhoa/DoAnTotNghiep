using System.ComponentModel.DataAnnotations;

namespace API_Server.Models
{
    public class Supplier
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Address { get; set; }

        public string Phone { get; set; }

        public string Status { get; set; }
    }
}
