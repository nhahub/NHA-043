using depi_real_state_management_system.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace depi_real_state_management_system.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _UserManager;
        private readonly SignInManager<ApplicationUser> _SignInManager;
        private readonly RoleManager<IdentityRole> _RoleManager;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _UserManager = userManager;
            _SignInManager = signInManager;
            _RoleManager = roleManager;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }
        //-----------------------------------------Authentication---------------------------------------
        public async Task<IActionResult> Register()
        {
            ViewBag.Roles = await _RoleManager.Roles
                .Where(r => r.Name != "Admin")
                .Select(r => new { Value = r.Id, Text = r.Name })
                .ToListAsync();
                
            return View(new RegisterViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmRegister(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber
                };

                if (model.ProfileImage != null && model.ProfileImage.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "profilesimg");
                    Directory.CreateDirectory(uploadsFolder); // Ensure the folder exists
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.ProfileImage.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ProfileImage.CopyToAsync(fileStream);
                    }

                    user.ProfileImage = uniqueFileName;
                }

                var result = await _UserManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    var role = await _RoleManager.FindByIdAsync(model.RoleId);
                    if (role != null)
                    {
                        await _UserManager.AddToRoleAsync(user, role.Name);
                        return RedirectToAction("Login");
                    }
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If ModelState is invalid, re-render the view with errors
            ViewBag.Roles = await _RoleManager.Roles
                .Select(role => new SelectListItem
                {
                    Value = role.Id,
                    Text = role.Name
                }).ToListAsync();
            return View("Register", model);
        }

        public IActionResult Login()
        {
			ApplicationUser user = new ApplicationUser();
			return View(user);
        }


        [HttpPost]
        public async Task<IActionResult> ConfirmLogin(ApplicationUser user)
        {
            var result = await _SignInManager.PasswordSignInAsync(user.UserName, user.PasswordHash, user.RememberMe, false);

            if (result.Succeeded)
            {
                return Redirect("/property/index");
            }
            ViewBag.Status = 0;
            ViewBag.ErrorMessage = "Username or Password is incorrect";
            return View("Login");
        }

        public async Task<IActionResult> Logout()
        {
            await _SignInManager.SignOutAsync();
            return RedirectToAction("Login");
        }
        //-----------------------------------------Authorization---------------------------------------

        public async Task<IActionResult> AddRole(string role)
        {
            IdentityRole roleModel = new IdentityRole()
            {
                Name = role,
            };
            
            await _RoleManager.CreateAsync(roleModel);
            return View();
        }
        //----------------------------------------------------------------------------------------------
        public async Task<IActionResult> Profile(string id)
        {
            // Find the user by their ID
            var user = await _UserManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            // Check if the user is a manager (can own properties)
            var roles = await _UserManager.GetRolesAsync(user);
            bool isManager = roles.Contains("Manager");

            // Create lists for owned and booked properties
            List<Property> ownedProperties = new List<Property>();
            List<Property> bookedProperties = new List<Property>();

            // Fetch properties owned by the manager
            if (isManager)
            {
                ownedProperties = await _context.Properties
                                                .Where(p => p.OwnerId == id)
                                                .ToListAsync();
            }

            // Fetch properties booked by the user (manager or tenant)
            bookedProperties = await (from lease in _context.Leases
                                      join property in _context.Properties on lease.PropertyID equals property.PropertyID
                                      where lease.TenantID == id
                                      select property).ToListAsync();

            var leases = await _context.Leases.Where(l => l.TenantID == user.Id).ToListAsync();

            // Pass the user, owned properties, and booked properties to the view using ViewBag
            ViewBag.User = user;
            ViewBag.OwnedProperties = ownedProperties;
            ViewBag.BookedProperties = bookedProperties;
            ViewBag.IsManager = isManager;
            ViewBag.Leases = leases;

            // Assuming you have a field in ApplicationUser for ProfileImage
            ViewBag.ProfileImagePath = $"/profilesimg/{user.ProfileImage ?? "default.jpg"}"; // default.png is a placeholder image

            return View();
        }



        public async Task<IActionResult> EditProfile(string id)
        {
            var user = await _UserManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var model = new EditProfileViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                ProfileImage = null // Ensure it's nullable in the view model
            };

            return View(model);
        }

        // POST: EditProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _UserManager.FindByIdAsync(model.Id);
                if (user != null)
                {
                    user.UserName = model.UserName;
                    user.Email = model.Email;
                    user.PhoneNumber = model.PhoneNumber;

                    // Handle profile image upload
                    if (model.ProfileImage != null && model.ProfileImage.Length > 0)
                    {
                        // Define the path to save the uploaded image
                        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "profilesimg");
                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.ProfileImage.FileName);
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        // Delete old image if exists
                        if (!string.IsNullOrEmpty(user.ProfileImage))
                        {
                            var oldImagePath = Path.Combine(uploadsFolder, user.ProfileImage);
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        // Save the new image
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.ProfileImage.CopyToAsync(fileStream);
                        }

                        // Update user's profile image property
                        user.ProfileImage = uniqueFileName;
                    }
                    else
                    {
                        user.ProfileImage = null; // Explicitly set to null if no image is uploaded
                    }

                    var result = await _UserManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        // Redirect to the profile page after success
                        return RedirectToAction("Profile", new { id = user.Id });
                    }
                }
            }

            // If we got this far, something failed, redisplay the form
            return View(model);
        }



        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
