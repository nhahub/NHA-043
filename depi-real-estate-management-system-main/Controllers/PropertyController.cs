using depi_real_state_management_system.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace depi_real_state_management_system.Controllers
{
    [Authorize]
    public class PropertyController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public PropertyController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            var properties = _context.Properties.ToList();
            return View(properties);
        }

        // Display property details
        public IActionResult Details(int id)
        {
            var property = _context.Properties
                                   .Include(p => p.Owner)
                                   .FirstOrDefault(p => p.PropertyID == id);
            if (property == null)
            {
                return NotFound();
            }
            return View(property);
        }


        // Display form to create a new property
        [Authorize(Roles ="Manager")]
        public IActionResult Create()
        {
            return View();
        }

        // Add new property to the database
        [HttpPost]
        public async Task<IActionResult> Create(PropertyViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Save the uploaded image if it exists
                string uniqueFileName = null!;
                if (model.Image != null && model.Image.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Image.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.Image.CopyToAsync(fileStream);
                    }
                }

                // Create a new property instance with the new fields
                var property = new Property
                {
                    Location = model.Location,
                    Size = model.Size,
                    PricePerNight = model.Price,
                    Description = model.Description,
                    IsAvailable = model.IsAvailable,
                    DateAdded = model.DateAdded,
                    ImageUrl = uniqueFileName!, // Save the image path or null
                    OwnerId = model.OwnerId,
                    // Offers
                    HasKitchen = model.HasKitchen,
                    HasDedicatedWorkspace = model.HasDedicatedWorkspace,
                    HasDryer = model.HasDryer,
                    HasIndoorFireplace = model.HasIndoorFireplace,
                    HasHairDryer = model.HasHairDryer,
                    HasWifi = model.HasWifi,
                    HasWasher = model.HasWasher,
                    HasBackyard = model.HasBackyard,
                    AllowsLuggageDropoff = model.AllowsLuggageDropoff,
                    HasLockOnBedroomDoor = model.HasLockOnBedroomDoor
                };

                _context.Properties.Add(property);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(model);
        }



        public async Task<IActionResult> Edit(int id)
        {
            var property = await _context.Properties.FindAsync(id);

            if (property == null)
            {
                return NotFound();
            }

            // Map the existing property to the ViewModel
            var propertyViewModel = new PropertyViewModel
            {
                Location = property.Location,
                Size = property.Size,
                Price = property.PricePerNight,
                Description = property.Description,
                IsAvailable = property.IsAvailable,
                OwnerId = property.OwnerId,
                Image = null,
                HasKitchen = property.HasKitchen,
                HasDedicatedWorkspace = property.HasDedicatedWorkspace,
                HasDryer = property.HasDryer,
                HasIndoorFireplace = property.HasIndoorFireplace,
                HasHairDryer = property.HasHairDryer,
                HasWifi = property.HasWifi,
                HasWasher = property.HasWasher,
                HasBackyard = property.HasBackyard,
                AllowsLuggageDropoff = property.AllowsLuggageDropoff,
                HasLockOnBedroomDoor = property.HasLockOnBedroomDoor,
            };

            return View(propertyViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, PropertyViewModel model)
        {
            var existingProperty = await _context.Properties.FindAsync(id);
            var user = await _userManager.FindByIdAsync(model.OwnerId);

            if (existingProperty == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Process the uploaded image
                if (model.Image != null)
                {
                    var fileName = Path.GetFileName(model.Image.FileName);
                    var filePath = Path.Combine("wwwroot/images/", fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.Image.CopyToAsync(fileStream);
                    }

                    existingProperty.ImageUrl = fileName; // Update ImageUrl with the new file
                }
                else
                {
                    existingProperty.ImageUrl = existingProperty.ImageUrl;
                }

                // Update the rest of the property details
                existingProperty.Location = model.Location;
                existingProperty.Size = model.Size;
                existingProperty.PricePerNight = model.Price;
                existingProperty.Description = model.Description;
                existingProperty.IsAvailable = model.IsAvailable;
                existingProperty.HasKitchen = model.HasKitchen;
                existingProperty.HasDedicatedWorkspace = model.HasDedicatedWorkspace;
                existingProperty.HasDryer = model.HasDryer;
                existingProperty.HasIndoorFireplace = model.HasIndoorFireplace;
                existingProperty.HasHairDryer = model.HasHairDryer;
                existingProperty.HasWifi = model.HasWifi;
                existingProperty.HasWasher = model.HasWasher;
                existingProperty.HasBackyard = model.HasBackyard;
                existingProperty.AllowsLuggageDropoff = model.AllowsLuggageDropoff;
                existingProperty.HasLockOnBedroomDoor = model.HasLockOnBedroomDoor;

                await _context.SaveChangesAsync();
                return Redirect($"/Account/Profile/{user.Id}");
 
            }
            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> Delete(string propertyId)
        {
            // Convert the propertyId from string to int
            if (int.TryParse(propertyId, out int id))
            {
                var property = await _context.Properties.FindAsync(id);
                if (property != null)
                {
                    _context.Properties.Remove(property);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index"); // Redirect to the appropriate page after deletion
                }
            }

            return NotFound();
        }
    }
}
