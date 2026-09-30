using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using PKValves.Models;
using PKValves.Services;

namespace PKValves.Controllers
{
    public class AccountController : Controller
    {
        private readonly AccountApiService _accountApi;

        public AccountController(
            AccountApiService accountApi)
        {
            _accountApi = accountApi;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            ApiAuthResponse? result =
                await _accountApi.LoginAsync(model);

            if (result == null ||
                !result.Success)
            {
                ViewBag.Error =
                    result?.Message
                    ?? "Unable to log in.";

                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    result.Uid),

                new Claim(
                    ClaimTypes.Email,
                    result.Email)
            };

            var identity =
                new ClaimsIdentity(
                    claims,
                    CookieAuthenticationDefaults
                        .AuthenticationScheme);

            var principal =
                new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults
                    .AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent =
                        model.RememberMe
                });

            HttpContext.Session.SetString(
                "FirebaseIdToken",
                result.IdToken);

            TempData["Success"] =
                "Welcome back! You have successfully logged in.";

            return RedirectToAction(
                "Index",
                "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            ApiAuthResponse? result =
                await _accountApi.RegisterAsync(model);

            if (result == null ||
                !result.Success)
            {
                ViewBag.Error =
                    result?.Message
                    ?? "Unable to create your account.";

                return View(model);
            }

            TempData["Success"] =
                "Account created successfully. Please log in.";

            return RedirectToAction("Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Remove(
                "FirebaseIdToken");

            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults
                    .AuthenticationScheme);

            TempData["Success"] =
                "You have been logged out.";

            return RedirectToAction(
                "Index",
                "Home");
        }
    }
}