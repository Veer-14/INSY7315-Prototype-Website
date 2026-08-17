using Microsoft.AspNetCore.Mvc;
using PKValves.Models;

namespace PKValves.Controllers
{
    public class CompareController : Controller
    {
        public IActionResult Index(
            int product1,
            int product2)
        {
            var products =
                ProductsController.GetProducts();


            var first =
                products.FirstOrDefault(
                    p => p.Id == product1);


            var second =
                products.FirstOrDefault(
                    p => p.Id == product2);


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