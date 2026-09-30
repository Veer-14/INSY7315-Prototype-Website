namespace PKValves.API.Models
{
    public class AuthResponse
    {
        public bool Success { get; set; }

        public string Message { get; set; } = "";

        public string Uid { get; set; } = "";

        public string Email { get; set; } = "";

        public string IdToken { get; set; } = "";
    }
}