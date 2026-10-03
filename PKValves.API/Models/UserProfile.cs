using Google.Cloud.Firestore;

namespace PKValves.API.Models
{
    [FirestoreData]
    public class UserProfile
    {
        [FirestoreProperty]
        public string Uid { get; set; } = "";

        [FirestoreProperty]
        public string FullName { get; set; } = "";

        [FirestoreProperty]
        public string Email { get; set; } = "";

        [FirestoreProperty]
        public string Phone { get; set; } = "";

        [FirestoreProperty]
        public Timestamp CreatedAt { get; set; }
    }
}