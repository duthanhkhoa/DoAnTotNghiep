using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace API_Server.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        [DisplayName("Người Dùng")]
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }


        [DisplayName("Sản phẩm")]
        public string ProductId { get; set; }
        public Product Product { get; set; }

        [DisplayName("Nội Dung")]
        public string Content { get; set; }

        [DisplayName("Ngày")]
        public DateTime Date { get; set; }


        [DisplayName("Trạng thái")]
        public string Status { get; set; }

        [NotMapped]
        public string UserFullName => User?.FullName;
    }
}
