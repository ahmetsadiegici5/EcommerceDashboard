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
            _firestoreDb = FirestoreDb.Create(configuration["Firebase:ProjectId"]);
        }

        public static void Initialize(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Console.WriteLine("FirestoreService.Initialize başlatılıyor...");
            var projectId = configuration["Firebase:ProjectId"];
            
            if (string.IsNullOrEmpty(projectId))
            {
                throw new InvalidOperationException("Firebase:ProjectId yapılandırması bulunamadı.");
            }
            
            // Firebase Admin SDK'yı başlat
            if (FirebaseApp.DefaultInstance == null)
            {
                Console.WriteLine($"FirebaseApp.DefaultInstance null, {environment.EnvironmentName} ortamında yeni oluşturuluyor...");
                
                // 1. Ortam değişkenini kontrol et
                var credentialsEnvVar = Environment.GetEnvironmentVariable("FIREBASE_CREDENTIALS");
                var credentialsConfig = configuration["Firebase:Credentials"];
                
                string credentialPath = null;
                
                // Önce ortam değişkenini dene, sonra config
                if (!string.IsNullOrEmpty(credentialsEnvVar))
                {
                    credentialPath = credentialsEnvVar;
                }
                else if (!string.IsNullOrEmpty(credentialsConfig))
                {
                    credentialPath = credentialsConfig;
                }
                else if (!environment.IsProduction())
                {
                    // Geliştirme ortamında default dosya yolunu dene
                    credentialPath = "firebase-credentials.json";
                }
                
                if (string.IsNullOrEmpty(credentialPath))
                {
                    throw new InvalidOperationException(
                        "Firebase credentials yapılandırması bulunamadı. Aşağıdakilerden birini ayarlayın:\n" +
                        "1. FIREBASE_CREDENTIALS ortam değişkeni\n" +
                        "2. Firebase:Credentials yapılandırması (appsettings.json veya Production)\n" +
                        "3. Geliştirme ortamında: firebase-credentials.json dosyası");
                }
                
                try
                {
                    // JSON string veya dosya yolu olabilir
                    string credentialJson = null;
                    
                    if (credentialPath.StartsWith("{"))
                    {
                        // JSON string olarak gelen credentials
                        credentialJson = credentialPath;
                        Console.WriteLine("JSON string credentials kullanılıyor");
                    }
                    else
                    {
                        // Dosya yolu olarak gelen credentials
                        var currentDirectory = Directory.GetCurrentDirectory();
                        var fullPath = Path.Combine(currentDirectory, credentialPath);
                        
                        if (!File.Exists(fullPath))
                        {
                            fullPath = Path.Combine(AppContext.BaseDirectory, credentialPath);
                        }
                        
                        if (!File.Exists(fullPath))
                        {
                            throw new FileNotFoundException($"Firebase credentials dosyası bulunamadı: {credentialPath}");
                        }
                        
                        credentialJson = File.ReadAllText(fullPath);
                        Console.WriteLine($"Firebase credentials dosyasından yüklendi: {fullPath}");
                    }
                    
                    // Credentials'ı Google.Apis.Auth.OAuth2 ile parse et
                    using (var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(credentialJson)))
                    {
                        var credential = GoogleCredential.FromStream(stream)
                            .CreateScoped("https://www.googleapis.com/auth/cloud-platform");
                        
                        FirebaseApp.Create(new AppOptions()
                        {
                            Credential = credential,
                            ProjectId = projectId
                        });
                        
                        Console.WriteLine($"Firebase başarıyla başlatıldı. Project ID: {projectId}");
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(
                        $"Firebase credentials yüklenirken hata oluştu. Credentials yolu/değeri: {credentialPath}\n" +
                        $"Hata: {ex.Message}", ex);
                }
            }
        }

        public FirestoreDb GetFirestoreDb() => _firestoreDb;

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
