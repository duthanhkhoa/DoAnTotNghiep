using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace API_Server.Models
{
    public class ImportInvoiceDetail
    {
        [Key]
        public int Id { get; set; }

        [DisplayName("Hóa đơn nhập")]
        public string ImportInvoiceId { get; set; }
        public ImportInvoice ImportInvoice { get; set; }

        [DisplayName("Sản phẩm")]
        public string ProductId { get; set; }
        public Product Product { get; set; }

        [Required]
        [DisplayName("Số lượng")]
        public int Quantity { get; set; }

        [Required]
        [DisplayName("Giá")]
        public decimal Price { get; set; }

        [DisplayName("Trạng thái")]
        public string Status { get; set; }
    }
}
