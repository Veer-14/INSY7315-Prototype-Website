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
                configuration["Firebase:ProjectId"]
                ?? throw new InvalidOperationException(
                    "Firebase ProjectId is missing.");

            string serviceAccountPath =
                configuration["Firebase:ServiceAccountPath"]
                ?? throw new InvalidOperationException(
                    "Firebase service account path is missing.");

            if (!File.Exists(serviceAccountPath))
            {
                throw new FileNotFoundException(
                    "Firebase service account file was not found.",
                    serviceAccountPath);
            }

            GoogleCredential credential =
                GoogleCredential.FromFile(serviceAccountPath);

            _firestore = new FirestoreDbBuilder
            {
                ProjectId = projectId,
                Credential = credential
            }.Build();
        }

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
                createdAt = Timestamp.FromDateTime(
                    user.CreatedAt.ToUniversalTime())
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
    }
}