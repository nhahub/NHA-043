using Microsoft.AspNetCore.Identity;

namespace depi_real_state_management_system.Models
{
    public class ApplicationUser : IdentityUser
    {
        public bool RememberMe { get; set; }
        public string? ProfileImage { get; set; }
    }
}
