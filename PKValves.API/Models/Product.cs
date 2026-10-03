using Google.Cloud.Firestore;

namespace PKValves.API.Models
{
    [FirestoreData]
    public class Product
    {
        [FirestoreProperty("id")]
        public int Id { get; set; }

        [FirestoreProperty("name")]
        public string Name { get; set; } = "";

        [FirestoreProperty("category")]
        public string Category { get; set; } = "";

        [FirestoreProperty("description")]
        public string Description { get; set; } = "";

        [FirestoreProperty("image")]
        public string Image { get; set; } = "";
    }
}