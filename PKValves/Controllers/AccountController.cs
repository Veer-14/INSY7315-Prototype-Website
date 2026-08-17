using Microsoft.AspNetCore.Mvc;

namespace PKValves.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            // Prototype only:
            // simulate a successful login

            TempData["Success"] =
                "Welcome back! You have successfully logged in.";

            return RedirectToAction("Index", "Products");
        }


        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(
            string fullName,
            string email,
            string phone,
            string password,
            string confirmPassword)
        {
            // Prototype only:
            // simulate successful registration

            TempData["Success"] =
                "Account created successfully. Please log in.";

            return RedirectToAction("Login");
        }


        public IActionResult Logout()
        {
            TempData["Success"] =
                "You have been logged out.";

            return RedirectToAction("Index", "Home");
        }
    }
}