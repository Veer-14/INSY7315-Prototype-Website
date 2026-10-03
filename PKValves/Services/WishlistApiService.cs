using System.Net.Http.Json;
using PKValves.Models;

namespace PKValves.Services
{
    public class WishlistApiService
    {
        private readonly HttpClient _httpClient;

        public WishlistApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // =====================================================
        // GET USER WISHLIST
        // =====================================================

        public async Task<List<Product>> GetWishlistAsync(
            string uid)
        {
            return await _httpClient
                .GetFromJsonAsync<List<Product>>(
                    $"api/wishlist/{uid}")
                ?? new List<Product>();
        }


        // =====================================================
        // ADD PRODUCT TO WISHLIST
        // =====================================================

        public async Task<bool> AddToWishlistAsync(
            string uid,
            int productId)
        {
            HttpResponseMessage response =
                await _httpClient.PostAsync(
                    $"api/wishlist/{uid}/{productId}",
                    null);

            return response.IsSuccessStatusCode;
        }


        // =====================================================
        // REMOVE PRODUCT FROM WISHLIST
        // =====================================================

        public async Task<bool> RemoveFromWishlistAsync(
            string uid,
            int productId)
        {
            HttpResponseMessage response =
                await _httpClient.DeleteAsync(
                    $"api/wishlist/{uid}/{productId}");

            return response.IsSuccessStatusCode;
        }
    }
}