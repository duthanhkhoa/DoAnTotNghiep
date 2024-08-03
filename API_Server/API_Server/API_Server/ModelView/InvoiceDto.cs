namespace API_Server.ModelView
{
    public class InvoiceDto
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public DateTime Date { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public int PaymentMethod { get; set; }
        public string ConsigneeName { get; set; }
        public string ShippingAddress { get; set; }
        public string ShippingPhone { get; set; }
        public int? VoucherId { get; set; }
        public string PaymentStatus { get; set; }
        public string Status { get; set; }
        public List<InvoiceDetailDto> InvoiceDetails { get; set; }
    }

    public class InvoiceDetailDto
    {
        public int Id { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }
    }

}
