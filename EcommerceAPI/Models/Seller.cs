using Google.Cloud.Firestore;

namespace EcommerceAPI.Models
{
    [FirestoreData]
    public class Seller
    {
        [FirestoreProperty]
        public string? Id { get; set; }

        [FirestoreProperty]
        public string CompanyName { get; set; } = string.Empty;

        [FirestoreProperty]
        public string Email { get; set; } = string.Empty;

        [FirestoreProperty]
        public string Phone { get; set; } = string.Empty;

        [FirestoreProperty]
        public string Address { get; set; } = string.Empty;

        [FirestoreProperty]
        public string TaxNumber { get; set; } = string.Empty;

        [FirestoreProperty]
        public bool IsActive { get; set; } = true;

        [FirestoreProperty]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
