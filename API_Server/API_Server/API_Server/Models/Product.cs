using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_Server.Models
{
    public class Product
    {
        public string Id { get; set; }

        [DisplayName("Tên sản phẩm")]
        public string ProductName { get; set; }

        [DisplayName("Mã số tiêu chuẩn quốc tế")]
        public string ISBN { get; set; }

        [DisplayName("Loại bìa")]
        public string CoverType { get; set; }

        [DisplayName("Mô tả")]
        public string Description { get; set; }
       
        [DisplayName("Giá Cũ")]
        public decimal Oldprice { get; set; }

        [DisplayName("Giá mới")]
        public decimal Price { get; set; }

        [DisplayName("Số lượng có sẵn")]
        public int QuantityAvailable { get; set; }

        [DisplayName("Tác giả")]
        public string Author { get; set; }

        [DisplayName("Nhà xuất bản")]
        public string Publisher { get; set; }

        [DisplayName("Ngày xuất bản")]
        public DateTime? PublishedDate { get; set; }

        public string ProductTypeDetailId { get; set; }
        public ProductTypeDetail ProductTypeDetail { get; set; }

        [DisplayName("Hình Ảnh")]
        public string Image { get; set; }

        [DisplayName("Trạng thái")]
        public string Status { get; set; }


        [NotMapped]
        public decimal DiscountPercentage
        {
            get
            {
                if (Oldprice > 0)
                {
                    // Tính toán phần trăm giảm giá
                    return Math.Round((Oldprice - Price) / Oldprice * 100, 2);
                }
                return 0;
            }
        }
    }
}
