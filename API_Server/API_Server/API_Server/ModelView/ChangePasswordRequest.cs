namespace API_Server.ModelView
{
    public class ChangePasswordRequest
    {
        public string UserId { get; set; }
        public string NewPassword { get; set; }
    }
}
