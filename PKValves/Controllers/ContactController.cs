using Microsoft.AspNetCore.Mvc;

namespace PKValves.Controllers
{
    public class ContactController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Submit(
            string name,
            string email,
            string phone,
            string subject,
            string message)
        {
            // Prototype only.
            // No database or email service is used.

            TempData["ContactSuccess"] =
                "Thank you for contacting PK Valves. Your enquiry has been received.";

            return RedirectToAction("Index");
        }
    }
}