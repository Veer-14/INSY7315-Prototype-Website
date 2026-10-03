using System.Net.Http.Json;
using System.Text.Json;
using PKValves.API.Models;

namespace PKValves.API.Services
{
    public class FirebaseAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public FirebaseAuthService(
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<AuthResponse> RegisterAsync(
            string email,
            string password)
        {
            string apiKey =
                _configuration["Firebase:WebApiKey"]
                ?? throw new InvalidOperationException(
                    "Firebase Web API key is missing.");

            string url =
                $"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={apiKey}";

            var request = new
            {
                email = email,
                password = password,
                returnSecureToken = true
            };

            HttpResponseMessage response =
                await _httpClient.PostAsJsonAsync(
                    url,
                    request);

            string responseBody =
                await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = GetFirebaseError(responseBody)
                };
            }

            FirebaseResponse? firebaseResponse =
                JsonSerializer.Deserialize<FirebaseResponse>(
                    responseBody);

            if (firebaseResponse == null)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Firebase returned an invalid response."
                };
            }

            return new AuthResponse
            {
                Success = true,
                Message = "Account created successfully.",
                Uid = firebaseResponse.localId,
                Email = firebaseResponse.email,
                IdToken = firebaseResponse.idToken
            };
        }

        public async Task<AuthResponse> LoginAsync(
            string email,
            string password)
        {
            string apiKey =
                _configuration["Firebase:WebApiKey"]
                ?? throw new InvalidOperationException(
                    "Firebase Web API key is missing.");

            string url =
                $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={apiKey}";

            var request = new
            {
                email = email,
                password = password,
                returnSecureToken = true
            };

            HttpResponseMessage response =
                await _httpClient.PostAsJsonAsync(
                    url,
                    request);

            string responseBody =
                await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = GetFirebaseError(responseBody)
                };
            }

            FirebaseResponse? firebaseResponse =
                JsonSerializer.Deserialize<FirebaseResponse>(
                    responseBody);

            if (firebaseResponse == null)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Firebase returned an invalid response."
                };
            }

            return new AuthResponse
            {
                Success = true,
                Message = "Login successful.",
                Uid = firebaseResponse.localId,
                Email = firebaseResponse.email,
                IdToken = firebaseResponse.idToken
            };
        }

        private string GetFirebaseError(string responseBody)
        {
            try
            {
                FirebaseErrorResponse? error =
                    JsonSerializer.Deserialize<FirebaseErrorResponse>(
                        responseBody);

                string code =
                    error?.error?.message ?? "";

                return code switch
                {
                    "EMAIL_EXISTS" =>
                        "An account with this email already exists.",

                    "INVALID_EMAIL" =>
                        "Please enter a valid email address.",

                    "WEAK_PASSWORD" =>
                        "The password is too weak.",

                    "EMAIL_NOT_FOUND" =>
                        "Incorrect email or password.",

                    "INVALID_PASSWORD" =>
                        "Incorrect email or password.",

                    "INVALID_LOGIN_CREDENTIALS" =>
                        "Incorrect email or password.",

                    "USER_DISABLED" =>
                        "This account has been disabled.",

                    _ =>
                        "Incorrect email or password."
                };
            }
            catch
            {
                return "Incorrect email or password.";
            }
        }
    }

    public class FirebaseResponse
    {
        public string idToken { get; set; } = "";

        public string email { get; set; } = "";

        public string refreshToken { get; set; } = "";

        public string expiresIn { get; set; } = "";

        public string localId { get; set; } = "";
    }

    public class FirebaseErrorResponse
    {
        public FirebaseError? error { get; set; }
    }

    public class FirebaseError
    {
        public string message { get; set; } = "";
    }
}