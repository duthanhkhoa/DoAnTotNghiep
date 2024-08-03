using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_Server.Models
{
    public class Invoice
    {
        public string Id { get; set; }

        [DisplayName("Người Dùng")]
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }

        [DisplayName("Ngày")]
        public DateTime Date { get; set; }

        [DisplayName("Tổng số tiền")]
        public decimal TotalAmount { get; set; }

        [DisplayName("Số tiền giảm giá từ voucher (nếu có)")]
        public decimal discount_amountt { get; set; }

        [DisplayName("Tổng số tiền cuối cùng sau khi áp dụng phiếu giảm giá")]
        public decimal final_amount { get; set; }

        [DisplayName("Phương thức thanh toán")]
        public int PaymentMethod { get; set; }

        [DisplayName("Tên người nhận hàng")]
        public string ConsigneeName { get; set; }

        [DisplayName("Địa chỉ")]
        public string ShippingAddress { get; set; }

        [DisplayName("Số điện thoại")]
        public string ShippingPhone { get; set; }

        [DisplayName("Mã giảm giá")]
        public int? VoucherId { get; set; }
        public Voucher Voucher { get; set; }

        [DisplayName("Trạng thái thanh toán")]
        public string payment_status { get; set; }

        [DisplayName("Trạng thái")]
        public string Status { get; set; }

        public ICollection<InvoiceDetail> InvoiceDetails { get; set; }
    }
}
