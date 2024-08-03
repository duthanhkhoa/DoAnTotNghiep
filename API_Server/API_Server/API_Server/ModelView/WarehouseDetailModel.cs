namespace API_Server.ModelView
{
    public class WarehouseDetailModel
    {
        public int Id { get; set; }
        public int WarehouseId { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime UpdateDay { get; set; }
        public string Status { get; set; }
        public int QuantityAvailable { get; set; }
        public string Image { get; set; }
    }
}
