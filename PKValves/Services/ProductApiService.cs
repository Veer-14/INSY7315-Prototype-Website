using System.Net;
using System.Net.Http.Json;
using PKValves.Models;

namespace PKValves.Services
{
    public class ProductApiService
    {
        private readonly HttpClient _httpClient;

        public ProductApiService(
            HttpClient httpClient)
        {
            _httpClient = httpClient;
        }


        // =====================================================
        // GET ALL PRODUCTS
        // =====================================================

        public async Task<List<Product>>
            GetProductsAsync()
        {
            return await _httpClient
                .GetFromJsonAsync<List<Product>>(
                    "api/products")
                ?? new List<Product>();
        }


        // =====================================================
        // GET SINGLE PRODUCT
        // =====================================================

        public async Task<Product?>
            GetProductAsync(int id)
        {
            HttpResponseMessage response =
                await _httpClient.GetAsync(
                    $"api/products/{id}");

            if (response.StatusCode ==
                HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            return await response.Content
                .ReadFromJsonAsync<Product>();
        }
    }
}