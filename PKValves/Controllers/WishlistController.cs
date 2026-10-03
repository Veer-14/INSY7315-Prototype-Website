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

        public async Task<IActionResult> Index()
        {
            // User must be logged in
            if (User.Identity == null ||
                !User.Identity.IsAuthenticated)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            // Get Firebase UID from the logged-in user's claims
            string? uid =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(uid))
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            // Get the user's saved wishlist products
            List<PKValves.Models.Product> wishlistProducts =
                await _wishlistApi.GetWishlistAsync(uid);

            // Get ALL products for the existing Compare feature
            List<PKValves.Models.Product> allProducts =
                await _productApi.GetProductsAsync();

            // The Wishlist view uses ViewBag.AllProducts
            // for the Compare section.
            ViewBag.AllProducts = allProducts;

            return View(wishlistProducts);
        }


        // =====================================================
        // ADD PRODUCT TO WISHLIST
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Add(int id)
        {
            // Wishlist requires the user to be logged in
            if (User.Identity == null ||
                !User.Identity.IsAuthenticated)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            // Get Firebase UID
            string? uid =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(uid))
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            // Add product to this user's Firestore wishlist
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

        [HttpGet]
        public async Task<IActionResult> Remove(int id)
        {
            // Wishlist requires the user to be logged in
            if (User.Identity == null ||
                !User.Identity.IsAuthenticated)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            // Get Firebase UID
            string? uid =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(uid))
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            // Remove product from this user's Firestore wishlist
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