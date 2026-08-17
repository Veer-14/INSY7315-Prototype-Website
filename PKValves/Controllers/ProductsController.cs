using Microsoft.AspNetCore.Mvc;
using PKValves.Models;

namespace PKValves.Controllers
{
    public class ProductsController : Controller
    {
        // Prototype product list
        // No database is being used.

        private static readonly List<Product> Products = new()
        {
            new Product
{
    Id = 1,
    Name = "Plassion 90° Equal Elbow",
    Category = "Fittings",
    Description = "A durable 90° equal elbow fitting designed to connect two pipes of the same diameter and create a 90-degree change in direction.",
    Image = "/images/product1.jpeg"
},

new Product
{
    Id = 2,
    Name = "Plassion Reducing Coupling",
    Category = "Fittings",
    Description = "A reliable reducing coupling designed to connect pipes of different diameters and provide a secure transition between pipe sizes.",
    Image = "/images/product2.jpeg"
},

new Product
{
    Id = 3,
    Name = "Plassion Male Tee",
    Category = "Fittings",
    Description = "A versatile male tee fitting designed to connect pipe sections and provide a branch connection for plumbing and fluid distribution systems.",
    Image = "/images/product3.jpeg"
},

new Product
{
    Id = 4,
    Name = "Resilient Gate Valve",
    Category = "Valves",
    Description = "A robust resilient gate valve designed to control and shut off water and fluid flow in industrial, commercial and plumbing applications.",
    Image = "/images/valve.jpeg"
},

new Product
{
    Id = 5,
    Name = "Water Transfer Pump",
    Category = "Pumps",
    Description = "An efficient water transfer pump designed to move water between tanks, systems and other locations for commercial, industrial and agricultural applications.",
    Image = "/images/pumps.jpeg"
},

new Product
{
    Id = 6,
    Name = "Industrial Storage Tank",
    Category = "Tanks",
    Description = "A high-capacity industrial storage tank designed for the safe and reliable storage of water and other suitable liquids.",
    Image = "/images/tanks.jpeg"
},

new Product
{
    Id = 7,
    Name = "Plumbing Fittings",
    Category = "Plumbing",
    Description = "A range of reliable plumbing fittings designed to provide secure connections between pipes and support a variety of plumbing installations.",
    Image = "/images/plumbing.jpeg"
}
        };


        // Products page
        public IActionResult Index()
        {
            return View(Products);
        }


        // Product details
        public IActionResult Details(int id)
        {
            var product = Products.FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }


        // Allows other prototype controllers
        // such as Wishlist and Compare to access
        // the dummy product list.
        public static List<Product> GetProducts()
        {
            return Products;
        }
    }
}