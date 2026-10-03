using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using PKValves.Services;

namespace PKValves.Controllers
{
    public class WishlistController : Controller
    {
        private readonly WishlistApiService _wishlistApi;
        private readonly ProductApiService _productApi;

        public WishlistController(
            WishlistApiService wishlistApi,
            ProductApiService productApi)
        {
            _wishlistApi = wishlistApi;
            _productApi = productApi;
        }


        // =====================================================
        // DISPLAY USER WISHLIST
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (User.Identity == null ||
                !User.Identity.IsAuthenticated)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            string? uid =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(uid))
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            List<PKValves.Models.Product> wishlistProducts =
                await _wishlistApi.GetWishlistAsync(uid);

            // Needed by the existing Compare feature
            List<PKValves.Models.Product> allProducts =
                await _productApi.GetProductsAsync();

            ViewBag.AllProducts = allProducts;

            return View(wishlistProducts);
        }


        // =====================================================
        // ADD PRODUCT TO WISHLIST
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int id)
        {
            if (User.Identity == null ||
                !User.Identity.IsAuthenticated)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            string? uid =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(uid))
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            bool success =
                await _wishlistApi.AddToWishlistAsync(
                    uid,
                    id);

            if (success)
            {
                TempData["Success"] =
                    "Product added to your wishlist.";
            }
            else
            {
                TempData["Error"] =
                    "Unable to add product to your wishlist.";
            }

            return RedirectToAction(
                "Index",
                "Wishlist");
        }


        // =====================================================
        // REMOVE PRODUCT FROM WISHLIST
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int id)
        {
            if (User.Identity == null ||
                !User.Identity.IsAuthenticated)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            string? uid =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(uid))
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            bool success =
                await _wishlistApi.RemoveFromWishlistAsync(
                    uid,
                    id);

            if (success)
            {
                TempData["Success"] =
                    "Product removed from your wishlist.";
            }
            else
            {
                TempData["Error"] =
                    "Unable to remove product from your wishlist.";
            }

            return RedirectToAction(
                "Index",
                "Wishlist");
        }
    }
}