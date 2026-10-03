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
                    user.CreatedAt.ToDateTime())
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

            Dictionary<string, object> data =
                snapshot.ToDictionary();

            return new UserProfile
            {
                Uid = data.ContainsKey("uid")
                    ? data["uid"]?.ToString() ?? uid
                    : uid,

                FullName = data.ContainsKey("fullName")
                    ? data["fullName"]?.ToString() ?? ""
                    : "",

                Email = data.ContainsKey("email")
                    ? data["email"]?.ToString() ?? ""
                    : "",

                Phone = data.ContainsKey("phone")
                    ? data["phone"]?.ToString() ?? ""
                    : "",

                CreatedAt = data.ContainsKey("createdAt") &&
                            data["createdAt"] is Timestamp timestamp
                    ? timestamp
                    : Timestamp.FromDateTime(
                        DateTime.UtcNow)
            };
        }
    }
}