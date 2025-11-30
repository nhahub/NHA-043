using depi_real_state_management_system.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace depi_real_state_management_system.Controllers
{
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PaymentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Payment
        public async Task<IActionResult> Index()
        {
            var payments = await _context.Payments
                .Include(p => p.Lease)
                .Include(p => p.Tenant)
                .ToListAsync();
            return View(payments);
        }

        // GET: Payment/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var payment = await _context.Payments
                .Include(p => p.Lease)
                .Include(p => p.Tenant)
                .FirstOrDefaultAsync(p => p.PaymentID == id);

            if (payment == null)
            {
                return NotFound();
            }
            return View(payment);
        }

        // GET: Payment/Create
        [HttpGet]
        public async Task<IActionResult> CreatePayment(int? leaseId)
        {
            if (leaseId == null)
            {
                // Redirect to Lease index to select a lease
                return RedirectToAction("Index", "Lease");
            }

            var lease = await _context.Leases
                .Include(l => l.Tenant)
                .FirstOrDefaultAsync(l => l.LeaseID == leaseId.Value);

            if (lease == null)
            {
                return NotFound();
            }

            var payment = new Payment
            {
                LeaseID = lease.LeaseID,
                TenantID = lease.Tenant.Id,
                PaymentDate = DateTime.Now
            };

            return View(payment);
        }

        // POST: Payment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Payment payment)
        {
            if (ModelState.IsValid)
            {
                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(payment);
        }
    }
}
