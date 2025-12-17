using Google.Cloud.Firestore;

namespace EcommerceAPI.Services
{
    public interface IFirestoreService
    {
        FirestoreDb GetFirestoreDb();
        Task<T?> GetDocumentAsync<T>(string collectionName, string documentId) where T : class;
        Task<List<T>> GetAllDocumentsAsync<T>(string collectionName) where T : class;
        Task<List<T>> GetDocumentsPagedAsync<T>(string collectionName, int pageNumber, int pageSize, string? orderByField = null, bool descending = false) where T : class;
        Task<List<T>> GetDocumentsByIdsAsync<T>(string collectionName, IEnumerable<string> ids) where T : class;
        Task<string> AddDocumentAsync<T>(string collectionName, T document) where T : class;
        Task UpdateDocumentAsync(string collectionName, string documentId, Dictionary<string, object> updates);
        Task UpdateDocumentAsync<T>(string collectionName, string documentId, T document) where T : class;
        Task DeleteDocumentAsync(string collectionName, string documentId);
        Task<List<T>> QueryDocumentsAsync<T>(
            string collectionName,
            string field,
            object value,
            string? orderByField = null,
            bool descending = false,
            int? limit = null,
            int? offset = null) where T : class;
    }
}
