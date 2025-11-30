namespace depi_real_state_management_system.Models
{
    public class EditProfileViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }

        public IFormFile? ProfileImage { get; set; }
    }
}
