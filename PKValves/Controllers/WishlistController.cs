using Microsoft.AspNetCore.Mvc;
using PKValves.Models;

namespace PKValves.Controllers
{
    public class WishlistController : Controller
    {
        // Prototype only.
        // No database is used.
        private static readonly List<int> WishlistIds = new();


        // =====================================================
        // WISHLIST PAGE
        // =====================================================

        public IActionResult Index()
        {
            var products = ProductsController
                .GetProducts()
                .Where(p => WishlistIds.Contains(p.Id))
                .ToList();

            return View(products);
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