namespace API_Server.ModelView
{
    public class ImportInvoiceModel
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public int SupplierId { get; set; }
        public int PaymentMethod { get; set; }
        public string status { get; set; }
        public List<ProductModel> Products { get; set; }

        public decimal TotalAmount { get; set; }
        public DateTime Date { get; set; }
        public string UserFullName { get; set; }
        public string SupplierName { get; set; }
        public string PaymentMethodName { get; set; }
    }

    public class ProductModel
    {
        public string ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
