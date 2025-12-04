using Google.Cloud.Firestore;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;

namespace EcommerceAPI.Services
{
    public class FirestoreService
    {
        private readonly FirestoreDb _firestoreDb;

        public FirestoreService(IConfiguration configuration)
        {
            var projectId = configuration["Firebase:ProjectId"];
            
            // Firebase Admin SDK'yı başlat
            if (FirebaseApp.DefaultInstance == null)
            {
                var credentialPath = configuration["Firebase:CredentialPath"];
                
                if (string.IsNullOrEmpty(credentialPath))
                {
                    throw new InvalidOperationException("Firebase:CredentialPath yapılandırması bulunamadı.");
                }
                
                // Tam dosya yolunu oluştur - önce mevcut dizini dene
                var currentDirectory = Directory.GetCurrentDirectory();
                var fullPath = Path.Combine(currentDirectory, credentialPath);
                
                if (!File.Exists(fullPath))
                {
                    // AppContext.BaseDirectory'yi dene
                    fullPath = Path.Combine(AppContext.BaseDirectory, credentialPath);
                }
                
                if (!File.Exists(fullPath))
                {
                    throw new FileNotFoundException($"Firebase credentials dosyası bulunamadı. Aranan yollar:\n" +
                        $"1. {Path.Combine(currentDirectory, credentialPath)}\n" +
                        $"2. {Path.Combine(AppContext.BaseDirectory, credentialPath)}");
                }
                
                // GOOGLE_APPLICATION_CREDENTIALS environment variable'ı ayarla
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", fullPath);
                
                FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile(fullPath),
                    ProjectId = projectId
                });
            }

            _firestoreDb = FirestoreDb.Create(projectId);
        }

        public FirestoreDb GetFirestoreDb() => _firestoreDb;

        public async Task<T?> GetDocumentAsync<T>(string collectionName, string documentId) where T : class
        {
            var docRef = _firestoreDb.Collection(collectionName).Document(documentId);
            var snapshot = await docRef.GetSnapshotAsync();
            
            if (snapshot.Exists)
            {
                var item = snapshot.ConvertTo<T>();
                
                // Id property'sini set et
                var idProperty = typeof(T).GetProperty("Id");
                if (idProperty != null && idProperty.CanWrite)
                {
                    idProperty.SetValue(item, snapshot.Id);
                }
                
                return item;
            }
            
            return null;
        }

        public async Task<List<T>> GetAllDocumentsAsync<T>(string collectionName) where T : class
        {
            var snapshot = await _firestoreDb.Collection(collectionName).GetSnapshotAsync();
            var documents = new List<T>();

            foreach (var document in snapshot.Documents)
            {
                var item = document.ConvertTo<T>();
                
                // Id property'sini set et
                var idProperty = typeof(T).GetProperty("Id");
                if (idProperty != null && idProperty.CanWrite)
                {
                    idProperty.SetValue(item, document.Id);
                }
                
                documents.Add(item);
            }

            return documents;
        }

        public async Task<string> AddDocumentAsync<T>(string collectionName, T document) where T : class
        {
            var docRef = await _firestoreDb.Collection(collectionName).AddAsync(document);
            return docRef.Id;
        }

        public async Task UpdateDocumentAsync<T>(string collectionName, string documentId, T document) where T : class
        {
            var docRef = _firestoreDb.Collection(collectionName).Document(documentId);
            await docRef.SetAsync(document, SetOptions.MergeAll);
        }

        public async Task DeleteDocumentAsync(string collectionName, string documentId)
        {
            var docRef = _firestoreDb.Collection(collectionName).Document(documentId);
            await docRef.DeleteAsync();
        }

        public async Task<List<T>> QueryDocumentsAsync<T>(string collectionName, string field, object value) where T : class
        {
            var query = _firestoreDb.Collection(collectionName).WhereEqualTo(field, value);
            var snapshot = await query.GetSnapshotAsync();
            var documents = new List<T>();

            foreach (var document in snapshot.Documents)
            {
                var item = document.ConvertTo<T>();
                
                // Id property'sini set et
                var idProperty = typeof(T).GetProperty("Id");
                if (idProperty != null && idProperty.CanWrite)
                {
                    idProperty.SetValue(item, document.Id);
                }
                
                documents.Add(item);
            }

            return documents;
        }
    }
}
