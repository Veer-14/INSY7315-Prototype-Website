using Microsoft.AspNetCore.Mvc;
using PKValves.Models;
using PKValves.Services;

namespace PKValves.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ProductApiService _productApi;

        public ProductsController(ProductApiService productApi)
        {
            _productApi = productApi;
        }

        public async Task<IActionResult> Index()
        {
            List<Product> products =
                await _productApi.GetProductsAsync();

            return View(products);
        }

        public async Task<IActionResult> Details(int id)
        {
            Product? product =
                await _productApi.GetProductAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }
    }
}