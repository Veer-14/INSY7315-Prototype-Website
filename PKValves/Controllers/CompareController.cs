using Microsoft.AspNetCore.Mvc;
using PKValves.Models;
using PKValves.Services;

namespace PKValves.Controllers
{
    public class CompareController : Controller
    {
        private readonly ProductApiService _productApi;

        public CompareController(ProductApiService productApi)
        {
            _productApi = productApi;
        }

        public async Task<IActionResult> Index(
            int product1,
            int product2)
        {
            Product? first =
                await _productApi.GetProductAsync(product1);

            Product? second =
                await _productApi.GetProductAsync(product2);

            if (first == null || second == null)
            {
                return RedirectToAction(
                    "Index",
                    "Wishlist");
            }

            ViewBag.Product1 = first;
            ViewBag.Product2 = second;

            return View();
        }
    }
}