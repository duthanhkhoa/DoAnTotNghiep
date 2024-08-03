using System.ComponentModel.DataAnnotations;
namespace API_Server.Models
{
    public class UserVouchers
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; }
        public User User { get; set; }

        public string VoucherCode { get; set; }

        public DateTime UsedDate { get; set; }
    }
}
