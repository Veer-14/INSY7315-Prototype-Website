using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using PKValves.API.Models;

namespace PKValves.API.Services
{
    public class FirestoreService
    {
        private readonly FirestoreDb _firestore;

        public FirestoreService(IConfiguration configuration)
        {
            string projectId =
                Environment.GetEnvironmentVariable("Firebase__ProjectId")
                ?? configuration["Firebase:ProjectId"]
                ?? throw new InvalidOperationException(
                    "Firebase ProjectId is missing.");

            

            string? serviceAccountBase64 =
                Environment.GetEnvironmentVariable(
                    "FIREBASE_SERVICE_ACCOUNT_BASE64");

            string? serviceAccountJson =
                configuration["Firebase:ServiceAccountJson"];

            // Azure
            if (!string.IsNullOrWhiteSpace(serviceAccountBase64))
            {
                try
                {
                    byte[] jsonBytes =
                        Convert.FromBase64String(serviceAccountBase64);

                    serviceAccountJson =
                        System.Text.Encoding.UTF8.GetString(jsonBytes);
                }
                catch (FormatException)
                {
                    throw new InvalidOperationException(
                        "Firebase service account environment variable contains invalid Base64.");
                }
            }

            if (string.IsNullOrWhiteSpace(serviceAccountJson))
            {
                throw new InvalidOperationException(
                    "Firebase service account credentials are missing.");
            }

            GoogleCredential credential =
                GoogleCredential.FromJson(serviceAccountJson);

            _firestore = new FirestoreDbBuilder
            {
                ProjectId = projectId,
                Credential = credential
            }.Build();
        }


        // =====================================================
        // USER METHODS
        // =====================================================

        public async Task CreateUserAsync(UserProfile user)
        {
            DocumentReference document =
                _firestore
                    .Collection("users")
                    .Document(user.Uid);

            await document.SetAsync(new
            {
                uid = user.Uid,
                fullName = user.FullName,
                email = user.Email,
                phone = user.Phone,
                createdAt = user.CreatedAt
            });
        }


        public async Task<UserProfile?> GetUserAsync(string uid)
        {
            DocumentReference document =
                _firestore
                    .Collection("users")
                    .Document(uid);

            DocumentSnapshot snapshot =
                await document.GetSnapshotAsync();

            if (!snapshot.Exists)
            {
                return null;
            }

            return snapshot.ConvertTo<UserProfile>();
        }


        // =====================================================
        // PRODUCT METHODS
        // =====================================================

        public async Task<List<Product>> GetProductsAsync()
        {
            CollectionReference productsCollection =
                _firestore.Collection("products");

            QuerySnapshot snapshot =
                await productsCollection.GetSnapshotAsync();

            List<Product> products = new();

            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (document.Exists)
                {
                    Product product =
                        document.ConvertTo<Product>();

                    products.Add(product);
                }
            }

            return products
                .OrderBy(p => p.Id)
                .ToList();
        }


        public async Task<Product?> GetProductAsync(int id)
        {
            DocumentReference document =
                _firestore
                    .Collection("products")
                    .Document(id.ToString());

            DocumentSnapshot snapshot =
                await document.GetSnapshotAsync();

            if (!snapshot.Exists)
            {
                return null;
            }

            return snapshot.ConvertTo<Product>();
        }


        // =====================================================
        // WISHLIST METHODS
        // =====================================================

        public async Task AddToWishlistAsync(
            string uid,
            int productId)
        {
            DocumentReference wishlistDocument =
                _firestore
                    .Collection("users")
                    .Document(uid)
                    .Collection("wishlist")
                    .Document(productId.ToString());

            await wishlistDocument.SetAsync(new
            {
                productId = productId,
                addedAt = Timestamp.GetCurrentTimestamp()
            });
        }


        public async Task RemoveFromWishlistAsync(
            string uid,
            int productId)
        {
            DocumentReference wishlistDocument =
                _firestore
                    .Collection("users")
                    .Document(uid)
                    .Collection("wishlist")
                    .Document(productId.ToString());

            await wishlistDocument.DeleteAsync();
        }


        public async Task<List<int>> GetWishlistProductIdsAsync(
            string uid)
        {
            CollectionReference wishlistCollection =
                _firestore
                    .Collection("users")
                    .Document(uid)
                    .Collection("wishlist");

            QuerySnapshot snapshot =
                await wishlistCollection.GetSnapshotAsync();

            List<int> productIds = new();

            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (!document.Exists)
                {
                    continue;
                }

                WishlistItem item =
                    document.ConvertTo<WishlistItem>();

                productIds.Add(item.ProductId);
            }

            return productIds;
        }
    }
}