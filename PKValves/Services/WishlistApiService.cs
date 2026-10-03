using System.Net.Http.Headers;
using System.Net.Http.Json;
using PKValves.Models;

namespace PKValves.Services
{
    public class WishlistApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor
            _httpContextAccessor;

        public WishlistApiService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor =
                httpContextAccessor;
        }


        // =====================================================
        // GET USER WISHLIST
        // =====================================================

        public async Task<List<Product>>
            GetWishlistAsync(string uid)
        {
            string? token =
                GetFirebaseIdToken();

            if (string.IsNullOrWhiteSpace(token))
            {
                return new List<Product>();
            }

            using HttpRequestMessage request =
                CreateAuthenticatedRequest(
                    HttpMethod.Get,
                    $"api/wishlist/{uid}",
                    token);

            HttpResponseMessage response =
                await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                return new List<Product>();
            }

            return await response.Content
                .ReadFromJsonAsync<List<Product>>()
                ?? new List<Product>();
        }


        // =====================================================
        // ADD PRODUCT TO WISHLIST
        // =====================================================

        public async Task<bool>
            AddToWishlistAsync(
                string uid,
                int productId)
        {
            string? token =
                GetFirebaseIdToken();

            if (string.IsNullOrWhiteSpace(token))
            {
                return false;
            }

            using HttpRequestMessage request =
                CreateAuthenticatedRequest(
                    HttpMethod.Post,
                    $"api/wishlist/{uid}/{productId}",
                    token);

            HttpResponseMessage response =
                await _httpClient.SendAsync(request);

            return response.IsSuccessStatusCode;
        }


        // =====================================================
        // REMOVE PRODUCT FROM WISHLIST
        // =====================================================

        public async Task<bool>
            RemoveFromWishlistAsync(
                string uid,
                int productId)
        {
            string? token =
                GetFirebaseIdToken();

            if (string.IsNullOrWhiteSpace(token))
            {
                return false;
            }

            using HttpRequestMessage request =
                CreateAuthenticatedRequest(
                    HttpMethod.Delete,
                    $"api/wishlist/{uid}/{productId}",
                    token);

            HttpResponseMessage response =
                await _httpClient.SendAsync(request);

            return response.IsSuccessStatusCode;
        }


        // =====================================================
        // GET FIREBASE ID TOKEN
        // =====================================================

        private string? GetFirebaseIdToken()
        {
            return _httpContextAccessor
                .HttpContext?
                .Session
                .GetString("FirebaseIdToken");
        }


        // =====================================================
        // CREATE AUTHENTICATED API REQUEST
        // =====================================================

        private static HttpRequestMessage
            CreateAuthenticatedRequest(
                HttpMethod method,
                string url,
                string token)
        {
            HttpRequestMessage request =
                new HttpRequestMessage(
                    method,
                    url);

            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token);

            return request;
        }
    }
}