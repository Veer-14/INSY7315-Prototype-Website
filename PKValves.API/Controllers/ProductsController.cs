using Microsoft.AspNetCore.Mvc;
using PKValves.API.Models;
using PKValves.API.Services;

namespace PKValves.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly FirestoreService _firestore;

        public ProductsController(
            FirestoreService firestore)
        {
            _firestore = firestore;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            List<Product> products =
                await _firestore.GetProductsAsync();

            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            Product? product =
                await _firestore.GetProductAsync(id);

            if (product == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Product not found."
                });
            }

            return Ok(product);
        }
    }
}