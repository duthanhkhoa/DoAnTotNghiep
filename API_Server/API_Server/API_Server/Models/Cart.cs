using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_Server.Models
{
    public class Cart
    {
        public int Id { get; set; }
        [DisplayName("Sản Phẩm")]

        public string ProductId { get; set; }
        public Product Product { get; set; }



        [DisplayName("Số Lượng")]
        public int QuantityCart { get; set; }



        [DisplayName("Người Dùng")]
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }



        [DisplayName("Trạng thái")]
        public string Status { get; set; }

        
        //public User User { get;set; }
    }
}
