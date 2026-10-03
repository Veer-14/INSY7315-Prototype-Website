using System.Net.Http.Json;
using PKValves.Models;

namespace PKValves.Services
{
    public class ProductApiService
    {
        private readonly HttpClient _httpClient;

        public ProductApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Product>> GetProductsAsync()
        {
            return await _httpClient
                .GetFromJsonAsync<List<Product>>("api/products")
                ?? new List<Product>();
        }

        public async Task<Product?> GetProductAsync(int id)
        {
            return await _httpClient
                .GetFromJsonAsync<Product>($"api/products/{id}");
        }
    }
}