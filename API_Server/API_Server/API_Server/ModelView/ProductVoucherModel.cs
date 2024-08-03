namespace API_Server.ModelView
{
    public class ProductVoucherModel
    {
        public string Id { get; set; }
        public decimal DiscountPercentage { get; set; }
        public string UserName { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime UpdateDay { get; set; }
        public string Status { get; set; }
        public List<ProductVModel> Products { get; set; }
    }

    public class ProductVModel
    {
        public string ProductId { get; set; }
    }
}
