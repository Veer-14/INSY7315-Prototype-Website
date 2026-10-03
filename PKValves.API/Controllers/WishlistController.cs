using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PKValves.API.Models;
using PKValves.API.Services;

namespace PKValves.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WishlistController : ControllerBase
    {
        private readonly FirestoreService _firestore;

        public WishlistController(
            FirestoreService firestore)
        {
            _firestore = firestore;
        }

        // GET USER WISHLIST

        [HttpGet("{uid}")]
        public async Task<IActionResult> GetWishlist(
            string uid)
        {
            if (!UserOwnsUid(uid))
            {
                return Forbid();
            }

            List<int> productIds =
                await _firestore
                    .GetWishlistProductIdsAsync(uid);

            List<Product> products = new();

            foreach (int productId in productIds)
            {
                Product? product =
                    await _firestore
                        .GetProductAsync(productId);

                if (product != null)
                {
                    products.Add(product);
                }
            }

            return Ok(products);
        }

        // ADD PRODUCT TO WISHLIST

        [HttpPost("{uid}/{productId}")]
        public async Task<IActionResult> AddToWishlist(
            string uid,
            int productId)
        {
            if (!UserOwnsUid(uid))
            {
                return Forbid();
            }

            Product? product =
                await _firestore
                    .GetProductAsync(productId);

            if (product == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Product not found."
                });
            }

            await _firestore.AddToWishlistAsync(
                uid,
                productId);

            return Ok(new
            {
                success = true,
                message = "Product added to wishlist.",
                productId = productId
            });
        }
        // REMOVE PRODUCT FROM WISHLIST
       

        [HttpDelete("{uid}/{productId}")]
        public async Task<IActionResult>
            RemoveFromWishlist(
                string uid,
                int productId)
        {
            if (!UserOwnsUid(uid))
            {
                return Forbid();
            }

            await _firestore.RemoveFromWishlistAsync(
                uid,
                productId);

            return Ok(new
            {
                success = true,
                message = "Product removed from wishlist.",
                productId = productId
            });
        }


        private bool UserOwnsUid(string uid)
        {
            string? authenticatedUid =
                User.FindFirst("user_id")?.Value
                ?? User.FindFirst(
                    ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value;

            return
                !string.IsNullOrWhiteSpace(
                    authenticatedUid)
                &&
                string.Equals(
                    authenticatedUid,
                    uid,
                    StringComparison.Ordinal);
        }
    }
}