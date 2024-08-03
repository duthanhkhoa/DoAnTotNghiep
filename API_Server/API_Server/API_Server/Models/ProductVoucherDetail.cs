using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace API_Server.Models
{
    public class ProductVoucherDetail
    {
        [Key]
        public int Id { get; set; }

        [DisplayName("Sản phẩm")]
        public string ProductId { get; set; }
        public Product Product { get; set; }

        public string ProductVoucherId { get; set; }
        public ProductVoucher ProductVoucher { get; set; }

        [DisplayName("Trạng thái")]
        public string Status { get; set; }
    }
}
