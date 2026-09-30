namespace PKValves.API.Models
{
    public class UserProfile
    {
        public string Uid { get; set; } = "";

        public string FullName { get; set; } = "";

        public string Email { get; set; } = "";

        public string Phone { get; set; } = "";

        public DateTime CreatedAt { get; set; }
    }
}