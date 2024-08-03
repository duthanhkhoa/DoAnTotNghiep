using Microsoft.AspNetCore.Identity;

namespace API_Server.Models
{
    public class User : IdentityUser
    {
        public string FullName { get; set; }
        public string Address { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool Status { get; set; }
    }
}
