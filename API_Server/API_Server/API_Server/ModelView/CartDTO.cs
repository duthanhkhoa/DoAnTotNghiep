namespace API_Server.Models
{
    public class CartDTO
    {
        public string ProductId { get; set; } // ID của sản phẩm trong giỏ hàng
        public int QuantityCart { get; set; } // Số lượng sản phẩm
        public string UserId { get; set; } // ID của người dùng đang thao tác
        public string Status { get; set; } // Trạng thái của sản phẩm trong giỏ hàng (ví dụ: "pending")
    }
}
