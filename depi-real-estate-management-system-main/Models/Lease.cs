using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace depi_real_state_management_system.Models
{
    public class Lease
    {
        public int LeaseID { get; set; }
        public int PropertyID { get; set; }  // Foreign Key
        public string TenantID { get; set; }  // Foreign Key

        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        public string Status { get; set; }

        public int TotalRentAmount { get; set; }

        // Navigation properties
        public Property? Property { get; set; }

        public ApplicationUser? Tenant { get; set; }

        // Method to calculate the status of the lease
        public void UpdateStatus()
        {
            if (DateTime.Now < StartDate.AddDays(-2))
            {
                Status = "Terminated";  // Terminated two days before the start date
            }
            else if (DateTime.Now < EndDate && DateTime.Now >= StartDate)
            {
                Status = "Ongoing";  // Ongoing lease
            }
            else if (DateTime.Now >= EndDate)
            {
                Status = "Completed";  // Lease completed
            }
        }
    }
}
