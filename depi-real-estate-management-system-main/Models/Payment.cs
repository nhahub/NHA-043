namespace depi_real_state_management_system.Models
{
    public class Payment
    {
        public int PaymentID { get; set; }
        public string TenantID { get; set; }  // Foreign Key
        public int LeaseID { get; set; }   // Foreign Key
        public int AmountPaid { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentStatus { get; set; }

        
        public ApplicationUser Tenant { get; set; }  // One-to-one relationship
        public Lease Lease { get; set; }    // Many-to-one relationship
    }
}
