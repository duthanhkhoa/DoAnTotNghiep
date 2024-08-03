using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace API_Server.Models
{
    public class ProductType
    {
        [Key]
        public int Id { get; set; }

        [DisplayName("loại sản phẩm")]
        public string TypeName { get; set; }

        [DisplayName("Trạng thái")]
        public string Status { get; set; }

    }
}
