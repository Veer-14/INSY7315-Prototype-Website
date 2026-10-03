using Google.Cloud.Firestore;

namespace PKValves.API.Models
{
    [FirestoreData]
    public class WishlistItem
    {
        [FirestoreProperty("productId")]
        public int ProductId { get; set; }

        [FirestoreProperty("addedAt")]
        public Timestamp AddedAt { get; set; }
    }
}