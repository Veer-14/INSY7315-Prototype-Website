using System.Net.Http.Json;
using PKValves.Models;

namespace PKValves.Services
{
    public class AccountApiService
    {
        private readonly HttpClient _httpClient;

        public AccountApiService(
            HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ApiAuthResponse?> RegisterAsync(
            RegisterViewModel model)
        {
            var request = new
            {
                fullName = model.FullName,
                email = model.Email,
                phone = model.Phone,
                password = model.Password
            };

            HttpResponseMessage response =
                await _httpClient.PostAsJsonAsync(
                    "api/auth/register",
                    request);

            if (!response.IsSuccessStatusCode)
            {
                return await response.Content
                    .ReadFromJsonAsync<ApiAuthResponse>();
            }

            return await response.Content
                .ReadFromJsonAsync<ApiAuthResponse>();
        }

        public async Task<ApiAuthResponse?> LoginAsync(
            LoginViewModel model)
        {
            var request = new
            {
                email = model.Email,
                password = model.Password
            };

            HttpResponseMessage response =
                await _httpClient.PostAsJsonAsync(
                    "api/auth/login",
                    request);

            if (!response.IsSuccessStatusCode)
            {
                return await response.Content
                    .ReadFromJsonAsync<ApiAuthResponse>();
            }

            return await response.Content
                .ReadFromJsonAsync<ApiAuthResponse>();
        }
    }

    public class ApiAuthResponse
    {
        public bool Success { get; set; }

        public string Message { get; set; } = "";

        public string Uid { get; set; } = "";

        public string Email { get; set; } = "";

        public string IdToken { get; set; } = "";
    }
}