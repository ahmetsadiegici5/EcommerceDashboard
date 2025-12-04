# Proje Başlangıç Rehberi

## 🎯 Proje Özeti

E-ticaret satıcılarının ürün, sipariş ve kargo bilgilerini **Excel ile yönetebilecekleri** bir dashboard uygulaması.

**Önemli Özellik**: Satıcılar Excel dosyaları ile toplu veri girişi, güncelleme ve raporlama yapabilir!

## 📁 Proje Yapısı

```
EcommerceDashboard/
├── EcommerceAPI/                 # ASP.NET Core Backend
│   ├── Controllers/              # API Endpoints
│   │   ├── ProductsController.cs    # Ürün işlemleri + Excel
│   │   ├── OrdersController.cs      # Sipariş yönetimi
│   │   └── ShippingController.cs    # Kargo takip + Excel
│   ├── Models/                   # Veri Modelleri
│   │   ├── Product.cs
│   │   ├── Order.cs
│   │   ├── Shipping.cs
│   │   └── Seller.cs
│   ├── Services/                 # İş Mantığı
│   │   ├── FirestoreService.cs     # Firebase işlemleri
│   │   └── ExcelService.cs         # Excel import/export
│   ├── appsettings.json          # Yapılandırma (Firebase ayarları)
│   └── Program.cs                # Uygulama başlangıcı
│
├── seller-dashboard/             # React Frontend (opsiyonel)
│   └── (React uygulaması)
│
└── README.md                     # Proje dokümantasyonu
```

## 🚀 Hızlı Başlangıç

### 1. Firebase Kurulumu (ÖNEMLİ!)

1. Firebase Console'a gidin: https://console.firebase.google.com/
2. Yeni proje oluşturun veya mevcut projeyi seçin
3. Firestore Database'i etkinleştirin (test modunda başlatabilirsiniz)
4. **Servis Hesabı Anahtarı Oluşturun**:
   - Proje Ayarları > Servis Hesapları
   - "Yeni özel anahtar oluştur" butonuna tıklayın
   - İndirilen JSON dosyasını `EcommerceAPI/firebase-credentials.json` olarak kaydedin

5. `appsettings.json` dosyasını düzenleyin:
```json
{
  "Firebase": {
    "ProjectId": "YOUR_PROJECT_ID_HERE",  // Firebase Project ID'nizi yazın
    "CredentialPath": "firebase-credentials.json"
  }
}
```

### 2. Backend'i Çalıştırma

```bash
cd EcommerceAPI
dotnet run
```

API şu adreste çalışacak: `https://localhost:5001`

**Swagger UI**: `https://localhost:5001/swagger` adresinden API'yi test edebilirsiniz!

## 📊 Excel ile Nasıl Çalışılır?

### Ürün Yönetimi

1. **Şablon İndirme**:
   - GET `/api/products/template` endpoint'ini çağırın
   - Örnek Excel dosyasını indirin

2. **Verileri Excel'de Doldurun**:
   ```
   Ürün Adı | Açıklama | Fiyat | Stok | Kategori | SKU | Resim URL | Aktif
   Laptop   | Gaming   | 15000 | 10   | Elektronik | LPT-001 | url | true
   ```

3. **Excel'i Yükleyin**:
   - POST `/api/products/import?sellerId=seller123`
   - Excel dosyasını form-data olarak gönderin

4. **Verileri Excel'e Aktarın**:
   - GET `/api/products/export`
   - Tüm ürünleriniz Excel formatında inecek

### Kargo Takibi

1. **Şablon İndirme**: GET `/api/shipping/template`
2. **Excel Doldurma**:
   ```
   Sipariş ID | Takip No | Kargo Firması | Durum | Konum
   ORD-123   | TK789    | Aras Kargo    | Shipped | İstanbul
   ```
3. **Yükleme**: POST `/api/shipping/import`
4. **Dışa Aktarma**: GET `/api/shipping/export`

## 🎯 Kullanım Senaryoları

### Senaryo 1: Toplu Ürün Ekleme
1. Excel şablonunu indirin
2. 100 ürünü Excel'de hazırlayın
3. Tek seferde sisteme yükleyin
4. Firebase'de otomatik olarak kayıt oluşturulur

### Senaryo 2: Fiyat Güncelleme
1. Mevcut ürünleri Excel'e aktarın
2. Fiyatları Excel'de güncelleyin
3. Güncellenmiş dosyayı yükleyin
4. Sistem otomatik olarak günceller

### Senaryo 3: Kargo Takip
1. Siparişlerinizi listeleyin
2. Kargo takip numaralarını Excel'de ekleyin
3. Toplu olarak sisteme yükleyin
4. Müşteriler kargo durumunu görebilir

## 🧪 API Test Etme (Postman/Swagger)

### Ürün Ekleme (JSON)
```http
POST https://localhost:5001/api/products
Content-Type: application/json

{
  "name": "Test Ürünü",
  "description": "Açıklama",
  "price": 99.99,
  "stock": 100,
  "category": "Elektronik",
  "sku": "TEST-001",
  "sellerId": "seller123",
  "isActive": true
}
```

### Excel Yükleme
```http
POST https://localhost:5001/api/products/import?sellerId=seller123
Content-Type: multipart/form-data
Body: [Excel dosyası seçin]
```

## 🔍 Önemli Notlar

### Firebase Güvenliği
- ⚠️ `firebase-credentials.json` dosyasını asla Git'e yüklemeyin!
- ✅ `.gitignore` dosyası zaten bu dosyayı hariç tutuyor
- ✅ Üretim ortamında environment variables kullanın

### Excel Formatı
- ✅ Sadece `.xlsx` ve `.xls` dosyaları desteklenir
- ✅ İlk satır başlık satırı olmalıdır
- ✅ Zorunlu alanlar `*` ile işaretlidir

### Veritabanı
- Firebase Firestore koleksiyonları otomatik oluşturulur
- `products`, `orders`, `shipping`, `sellers` koleksiyonları

## 🐛 Sorun Giderme

### "Firebase credentials bulunamadı" hatası
- `firebase-credentials.json` dosyasının `EcommerceAPI` klasöründe olduğundan emin olun
- Dosya adının tam olarak aynı olduğunu kontrol edin

### "Excel dosyası yüklenemedi" hatası
- Dosyanın `.xlsx` uzantılı olduğundan emin olun
- Excel formatının şablona uygun olduğunu kontrol edin
- İlk satırın başlık içerdiğinden emin olun

### Port çakışması
- `appsettings.json` veya `launchSettings.json` içinden portu değiştirebilirsiniz

## 📞 Yardım

API dokümantasyonu için Swagger UI kullanın: `https://localhost:5001/swagger`

## ✅ Sonraki Adımlar

1. Firebase'i kurun ve credentials dosyasını ekleyin
2. Backend'i çalıştırın
3. Swagger'dan API'yi test edin
4. Excel şablonunu indirin
5. İlk ürünlerinizi Excel ile ekleyin!

**Başarılar! 🎉**
