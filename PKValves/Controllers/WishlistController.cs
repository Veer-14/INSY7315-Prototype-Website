using Microsoft.AspNetCore.Mvc;
using PKValves.Models;
using PKValves.Services;

namespace PKValves.Controllers
{
    public class WishlistController : Controller
    {
        // Temporary wishlist storage.
        // This will be replaced with Firestore
        // in the next part.
        private static readonly List<int> WishlistIds = new();

        private readonly ProductApiService _productApi;

        public WishlistController(ProductApiService productApi)
        {
            _productApi = productApi;
        }

        // =====================================================
        // WISHLIST PAGE
        // =====================================================

        public async Task<IActionResult> Index()
        {
            List<Product> allProducts =
                await _productApi.GetProductsAsync();

            List<Product> wishlistProducts =
                allProducts
                    .Where(p => WishlistIds.Contains(p.Id))
                    .ToList();

            ViewBag.AllProducts = allProducts;

            return View(wishlistProducts);
        }

        // =====================================================
        // ADD
        // =====================================================

        public IActionResult Add(int id)
        {
            if (!WishlistIds.Contains(id))
            {
                WishlistIds.Add(id);
            }

            return RedirectToAction("Index");
        }

        // =====================================================
        // REMOVE
        // =====================================================

        public IActionResult Remove(int id)
        {
            WishlistIds.Remove(id);

            return RedirectToAction("Index");
        }
    }
}