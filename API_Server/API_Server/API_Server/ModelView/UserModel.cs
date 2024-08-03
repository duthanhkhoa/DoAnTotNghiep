namespace API_Server.ModelView
{
    public class UserModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool Status { get; set; }
    }
}
