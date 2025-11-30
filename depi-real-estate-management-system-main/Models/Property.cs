using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace depi_real_state_management_system.Models
{
    public class Property
    {
        public int PropertyID { get; set; } // Auto-incremented by the database

        [Required(ErrorMessage = "Location is required.")]
        public string Location { get; set; }

        [Required(ErrorMessage = "Size is required.")]
        public string Size { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Price must be a positive number.")]
        public int PricePerNight { get; set; } // Renamed to clarify this is price per night

        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; }

        public bool IsAvailable { get; set; } = true;

        [DataType(DataType.Date)]
        public DateTime DateAdded { get; set; } = DateTime.Now;

        [Url(ErrorMessage = "Invalid Image URL.")]
        public string? ImageUrl { get; set; }
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


        [ForeignKey("Owner")]
        public string OwnerId { get; set; }

        public virtual ApplicationUser? Owner { get; set; }

        public ICollection<Lease> Leases { get; set; }
    }
}
