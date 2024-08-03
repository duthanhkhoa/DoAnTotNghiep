using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace API_Server.Models
{
    public class Image
    {
        [Key]
        public int Id { get; set; }

        [DisplayName("Sản phẩm")]
        public string ProductId { get; set; }
        public Product Product { get; set; }


        [DisplayName("Tên hình ảnh")]
        public string NameImage { get; set; }


        [Required]
        public string Image_url { get; set; }

        public string Status { get; set; }
        

    }
}
