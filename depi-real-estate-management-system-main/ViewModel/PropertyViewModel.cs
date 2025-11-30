using System.ComponentModel.DataAnnotations;

namespace depi_real_state_management_system.Models
{
    public class PropertyViewModel
    {
        [Required(ErrorMessage = "Location is required.")]
        public string Location { get; set; }

        [Required(ErrorMessage = "Size is required.")]
        public string Size { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Price must be a positive number.")]
        public int Price { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; }

        public bool IsAvailable { get; set; } = true;

        public DateTime DateAdded { get; set; } = DateTime.Now;

        public IFormFile? Image { get; set; }

        public string OwnerId { get; set; }

        // Property Offers
        public bool HasKitchen { get; set; }
        public bool HasDedicatedWorkspace { get; set; }
        public bool HasDryer { get; set; }
        public bool HasIndoorFireplace { get; set; }
        public bool HasHairDryer { get; set; }
        public bool HasWifi { get; set; }
        public bool HasWasher { get; set; }
        public bool HasBackyard { get; set; }
        public bool AllowsLuggageDropoff { get; set; }
        public bool HasLockOnBedroomDoor { get; set; }
    }
}
