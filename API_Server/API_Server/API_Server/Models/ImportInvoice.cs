using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_Server.Models
{
    public class ImportInvoice
    {
        public string Id { get; set; }

        [DisplayName("Người Dùng")]
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }

        [DisplayName("Nhà cung cấp")]
        public int SupplierId { get; set; }
        public Supplier Supplier { get; set; }

        [DisplayName("Ngày lập hóa đơn")]
        public DateTime Date { get; set; }

        [Required]
        [DisplayName("Tổng số tiền")]
        public decimal TotalAmount { get; set; }

        [DisplayName("Phương thức thanh toán")]
        public int PaymentMethod { get; set; }
        
        public string Status { get; set; }
    }
}
