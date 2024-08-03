using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace API_Server.Models
{
    public class InvoiceDetail
    {
        [Key]
        public int Id { get; set; }

        [DisplayName("Mã hóa đơn")]
        public string InvoiceId { get; set; }
        public Invoice Invoice { get; set; }

        [DisplayName("Sản phẩm")]
        public string ProductId { get; set; }
        public Product Product { get; set; }

        [DisplayName("Số lượng")]
        public int Quantity { get; set; }

        [DisplayName("Giá")]
        public decimal Price { get; set; }

        [DisplayName("Tổng giá chi tiết")]
        public decimal total_price { get; set; }

        [DisplayName("Trạng thái")]
        public string Status { get; set; }
    }
}
