using depi_real_state_management_system.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace depi_real_state_management_system.Controllers
{
    public class LeaseController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public LeaseController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var leases = await _context.Leases
                .Include(l => l.Property) // Include related property details
                .Include(l => l.Tenant)   // Include related tenant details
                .ToListAsync();

            return View(leases);
        }

        public async Task<IActionResult> Details(int id)
        {
            var lease = await _context.Leases
                .Include(l => l.Property) // Include related property
                .Include(l => l.Tenant)   // Include related tenant
                .FirstOrDefaultAsync(m => m.LeaseID == id);

            if (lease == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            ViewBag.currentUserID = currentUser.Id;

            return View(lease);
        }

        // GET: Lease/CreateBook
        [HttpGet]
        public async Task<IActionResult> CreateBooking(int id)
        {
            var property = await _context.Properties.FindAsync(id);
            var tenant = await _userManager.GetUserAsync(User);
            if (property == null)
            {
                return NotFound();
            }

            var lease = new Lease
            {
                PropertyID = id,
                Property = property,
                Tenant = tenant,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(1), // Default to 1-night booking
            };

         
            return View(lease); // CreateBooking view
        }

        // POST: Lease/CreateBooking
        [HttpPost]
        public async Task<IActionResult> CreateBooking(Lease lease)
        {
            if (ModelState.IsValid)
            {
                // Validate the start date
                if (lease.StartDate.Date < DateTime.Now.Date)
                {
                    return Json(new { success = false, message = "Start date cannot be in the past." });
                }

                // Validate that the end date is after the start date
                if (lease.EndDate.Date <= lease.StartDate.Date)
                {
                    return Json(new { success = false, message = "End date must be after the start date." });
                }

                // Check if the property is already booked for the selected date range
                var conflictingLease = await _context.Leases
                    .Where(l => l.PropertyID == lease.PropertyID &&
                                l.StartDate < lease.EndDate &&
                                l.EndDate > lease.StartDate) // Check for overlapping dates
                    .FirstOrDefaultAsync();

                if (conflictingLease != null)
                {
                    return Json(new { success = false, message = "The property is already booked for the selected dates." });
                }

                // If no conflict, proceed with creating the booking
                _context.Leases.Add(lease);  // Add the lease to the context
                await _context.SaveChangesAsync();  // Save changes to the database

                return Json(new { success = true, redirectUrl = Url.Action("Details", new { id = lease.LeaseID }) });
            }

            // If ModelState is not valid, return the form with validation errors
            return Json(new { success = false, message = "Invalid data submitted." });
        }





        // Optional: Logic to terminate the lease before 2 days
        [HttpPost]
        public async Task<IActionResult> TerminateLease(int leaseId)
        {
            var lease = await _context.Leases.FindAsync(leaseId);
            if (lease == null)
            {
                return NotFound();
            }

            // Check if the termination is allowed (two days before the start date)
            if (DateTime.Now < lease.StartDate.AddDays(-2))
            {
                lease.Status = "Terminated";
                _context.Update(lease);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index)); // Redirect to lease listing page
            }

            // If not allowed, show an error message
            ModelState.AddModelError("", "Lease cannot be terminated less than 2 days before the start date.");
            return View("Details", lease);
        }
    }
}
