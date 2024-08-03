using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace API_Server.Models
{
    public class ProductTypeDetail
    {
        public string Id { get; set; }

        [DisplayName("Chi tiết loại sản phẩm")]
        public int ProductTypeId { get; set; }
        public ProductType ProductType { get; set; }

        [DisplayName("Chi tiết tên loại")]
        public string DetailName { get; set; }

        [DisplayName("Trạng thái")]
        public string Status { get; set; }

    }
}
