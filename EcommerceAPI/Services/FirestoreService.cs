using Google.Cloud.Firestore;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;

namespace EcommerceAPI.Services
{
    public class FirestoreService : IFirestoreService
    {
        private readonly FirestoreDb _firestoreDb;

        public FirestoreService(IConfiguration configuration)
        {
            Initialize(configuration);
            _firestoreDb = FirestoreDb.Create(configuration["Firebase:ProjectId"]);
        }

        public static void Initialize(IConfiguration configuration)
        {
            Console.WriteLine("FirestoreService.Initialize başlatılıyor...");
            var projectId = configuration["Firebase:ProjectId"];
            
            // Firebase Admin SDK'yı başlat
            if (FirebaseApp.DefaultInstance == null)
            {
                Console.WriteLine("FirebaseApp.DefaultInstance null, yeni oluşturuluyor...");
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
        }        public FirestoreDb GetFirestoreDb() => _firestoreDb;

        public async Task<T?> GetDocumentAsync<T>(string collectionName, string documentId) where T : class
        {
            try
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
            catch (Grpc.Core.RpcException ex)
            {
                Console.WriteLine($"Firebase bağlantı hatası (GetDocument): {ex.Status.Detail}");
                throw new InvalidOperationException($"Firebase veritabanına bağlanılamadı. İnternet bağlantınızı ve Firebase ayarlarınızı kontrol edin. Detay: {ex.Status.StatusCode}", ex);
            }
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

        public async Task<List<T>> GetDocumentsByIdsAsync<T>(string collectionName, IEnumerable<string> ids) where T : class
        {
            var docIds = ids.ToList();
            if (!docIds.Any()) return new List<T>();

            // Firestore 'IN' query supports up to 30 items.
            // For production, we should chunk this. For now, we'll implement simple chunking.
            var documents = new List<T>();
            var chunks = docIds.Chunk(30);

            foreach (var chunk in chunks)
            {
                var query = _firestoreDb.Collection(collectionName).WhereIn(FieldPath.DocumentId, chunk);
                var snapshot = await query.GetSnapshotAsync();

                foreach (var document in snapshot.Documents)
                {
                    var item = document.ConvertTo<T>();
                    
                    var idProperty = typeof(T).GetProperty("Id");
                    if (idProperty != null && idProperty.CanWrite)
                    {
                        idProperty.SetValue(item, document.Id);
                    }
                    
                    documents.Add(item);
                }
            }

            return documents;
        }

        public async Task<List<T>> GetDocumentsPagedAsync<T>(string collectionName, int pageNumber, int pageSize, string? orderByField = null, bool descending = false) where T : class
        {
            var offset = (pageNumber - 1) * pageSize;
            Query query = _firestoreDb.Collection(collectionName);

            if (!string.IsNullOrEmpty(orderByField))
            {
                query = descending ? query.OrderByDescending(orderByField) : query.OrderBy(orderByField);
            }

            query = query.Offset(offset).Limit(pageSize);
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

        public async Task<string> AddDocumentAsync<T>(string collectionName, T document) where T : class
        {
            var docRef = await _firestoreDb.Collection(collectionName).AddAsync(document);
            return docRef.Id;
        }

        public async Task UpdateDocumentAsync(string collectionName, string documentId, Dictionary<string, object> updates)
        {
            var docRef = _firestoreDb.Collection(collectionName).Document(documentId);
            await docRef.UpdateAsync(updates);
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

        public async Task<List<T>> QueryDocumentsAsync<T>(
            string collectionName,
            string field,
            object value,
            string? orderByField = null,
            bool descending = false,
            int? limit = null,
            int? offset = null) where T : class
        {
            Query query = _firestoreDb.Collection(collectionName).WhereEqualTo(field, value);

            if (!string.IsNullOrWhiteSpace(orderByField))
            {
                query = descending ? query.OrderByDescending(orderByField) : query.OrderBy(orderByField);
            }

            if (offset.HasValue)
            {
                query = query.Offset(offset.Value);
            }

            if (limit.HasValue)
            {
                query = query.Limit(limit.Value);
            }

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
