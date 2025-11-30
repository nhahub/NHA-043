using depi_real_state_management_system.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using MailKit.Net.Smtp;
using MimeKit;

namespace depi_real_state_management_system.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;
        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return Redirect("/Property/Index");
        }

        // GET: /Home/ContactUs
        public IActionResult ContactUs()
        {
            return View();
        }

        // POST: /Home/ContactUs
        [HttpPost]
        public IActionResult ContactUs(string name, string email, string message)
        {
            // Prepare the email message
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(name, email));
            emailMessage.To.Add(new MailboxAddress("Admin", "minanader0000@gmail.com"));
            emailMessage.Subject = "Contact Us Form Submission";
            emailMessage.Body = new TextPart("plain")
            {
                Text = $"Name: {name}\nEmail: {email}\nMessage: {message}"
            };

            // Send the email using SMTP
            try
            {
                using (var client = new SmtpClient())
                {
                    // SMTP settings from appsettings.json
                    var smtpHost = _configuration["Smtp:Host"];
                    var smtpPort = int.Parse(_configuration["Smtp:Port"]);
                    var smtpUser = _configuration["Smtp:Username"];
                    var smtpPass = _configuration["Smtp:Password"];

                    // Connect to the SMTP server
                    client.Connect(smtpHost, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                    client.Authenticate(smtpUser, smtpPass);

                    // Send the email
                    client.Send(emailMessage);
                    client.Disconnect(true);
                }

                // Success message
                ViewBag.Message = "Thank you for contacting us! We will get back to you soon.";
            }
            catch (Exception ex)
            {
                // Handle email sending errors
                ViewBag.Message = $"Sorry, something went wrong: {ex.Message}";
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
