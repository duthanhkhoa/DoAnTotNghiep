using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace API_Server.Models
{
    public class ProductVoucher
    {
        public string Id { get; set; }

        [DisplayName("Tỉ lệ chiết khấu")]
        public decimal DiscountPercentage { get; set; }

        [DisplayName("Người Cập Nhật")]
        public string UserName { get; set; }

        [DisplayName("Ngày thêm")]
        public DateTime DateAdded { get; set; }

        [DisplayName("Ngày cập nhật")]
        public DateTime UpdateDay { get; set; }

        [DisplayName("Trạng thái")]
        public string Status { get; set; }
    }
}
