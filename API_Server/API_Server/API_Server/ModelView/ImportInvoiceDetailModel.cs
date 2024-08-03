namespace API_Server.ModelView
{
    public class ImportInvoiceDetailModel
    {
        public int Id { get; set; }
        public string ImportInvoiceId { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; }
    }
}
