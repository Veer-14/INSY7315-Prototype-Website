using Microsoft.AspNetCore.Mvc;
using PKValves.Models;
using PKValves.Services;
using Microsoft.AspNetCore.Authorization;

namespace PKValves.Controllers
{
    public class ContactController : Controller
    {
        private readonly IContactEmailService _emailService;
        private readonly ILogger<ContactController> _logger;

        public ContactController(
       IContactEmailService emailService,
       ILogger<ContactController> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new ContactViewModel());
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(
            ContactViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            try
            {
                await _emailService.SendEnquiryAsync(model);

                TempData["ContactSuccess"] =
                    "Thank you. Your enquiry has been submitted successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Record the failure type without logging enquiry contents.
                _logger.LogError(
                    "Contact email submission failed: {ErrorType}",
                    ex.GetType().Name);

                ModelState.AddModelError(
                    string.Empty,
                    "We could not confirm that your enquiry was sent. " +
                    "Please try again later or contact us directly.");

                return View("Index", model);
            }
        }
    }
}