using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace API_Server.Models
{
    public class Voucher
    {
        [Key]
        public int Id { get; set; }

        [StringLength(50)]
        [DisplayName("Mã phiếu giảm giá")]
        public string VoucherCode { get; set; }

        [DisplayName("Tên phiếu giảm giá")]
        public string VoucherName { get; set; }

        [DisplayName("Tỉ lệ chiết khấu")]
        public decimal DiscountPercentage { get; set; }

        [DisplayName("Số lượng")]
        public int Quantity { get; set; }

        [DisplayName("Ngày hết hạn")]
        public DateTime ExpiryDate { get; set; }

        [DisplayName("Ngày bắt đầu")]
        public DateTime StartDate { get; set; }

        [DisplayName("Trạng thái")]
        public string Status { get; set; }
    }
}
