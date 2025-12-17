# E-Ticaret Satıcı Yönetim Paneli

Bu proje, e-ticaret satıcılarının ürün, sipariş ve kargo bilgilerini **Excel dosyaları** üzerinden yönetebilecekleri bir dashboard uygulamasıdır.

## 🚀 Özellikler

### Backend (ASP.NET Core Web API)
- ✅ Firebase Firestore veritabanı entegrasyonu
- ✅ Excel ile toplu ürün içe/dışa aktarma
- ✅ Excel ile kargo takip bilgisi içe/dışa aktarma
- ✅ Ürün yönetimi (CRUD)
- ✅ Sipariş yönetimi
- ✅ Kargo takip sistemi
- ✅ RESTful API
- ✅ Swagger/OpenAPI dokümantasyonu
- ✅ Firebase Auth + HttpOnly cookie tabanlı oturum yönetimi

### Excel Özellikleri
- 📊 Excel şablon indirme
- 📤 Excel dosyasından toplu veri yükleme
- 📥 Veritabanındaki verileri Excel'e aktarma
- ✏️ Excel üzerinden ürün fiyatı, stok, kargo bilgisi güncelleme

## 📋 Gereksinimler

- .NET 9.0 SDK
- Firebase hesabı ve Firestore veritabanı
- Node.js (Frontend için)

## 🔧 Kurulum

### 1. Firebase Yapılandırması

1. [Firebase Console](https://console.firebase.google.com/) üzerinden bir proje oluşturun
2. Firestore Database'i etkinleştirin
3. Proje ayarları > Servis Hesapları > Yeni özel anahtar oluştur
4. İndirilen JSON dosyasını `EcommerceAPI` klasörüne `firebase-credentials.json` adıyla kaydedin

### 2. Backend Kurulumu

```bash
cd EcommerceAPI

# appsettings.json dosyasını düzenleyin
# Firebase ProjectId'nizi girin

# Projeyi çalıştırın
dotnet run
```

API şu adreste çalışacaktır: `http://localhost:5039`

Swagger UI: `http://localhost:5039/swagger`

## 📚 API Endpoints

### Kimlik Doğrulama (Auth)

```
POST   /api/auth/register        - Yeni kullanıcı oluştur
POST   /api/auth/session         - Firebase ID token gönderip HttpOnly cookie oluştur
DELETE /api/auth/session         - Oturumu sonlandır
```

### Ürünler (Products)

```
GET    /api/products              - Aktif kullanıcının ürünlerini listele
GET    /api/products/{id}         - Tek ürün getir
POST   /api/products              - Yeni ürün ekle
PUT    /api/products/{id}         - Ürün güncelle
DELETE /api/products/{id}         - Ürün sil

# Excel İşlemleri
GET    /api/products/template            - Excel şablonu indir
GET    /api/products/export              - Ürünleri Excel'e aktar
POST   /api/products/import              - Excel'den ürün yükle (kimlik doğrulanan satıcıya göre)
```

### Siparişler (Orders)

```
GET    /api/orders                - Aktif kullanıcının siparişlerini listele
GET    /api/orders/{id}           - Tek sipariş getir
POST   /api/orders                - Yeni sipariş oluştur
PUT    /api/orders/{id}           - Sipariş güncelle
PUT    /api/orders/{id}/status    - Sipariş durumu güncelle
```

### Kargo Takibi (Shipping)

```
GET    /api/shipping                     - Aktif kullanıcının kargo kayıtlarını listele
GET    /api/shipping/{id}                - Kargo kaydı getir
GET    /api/shipping/tracking/{number}   - Takip numarasıyla sorgula
GET    /api/shipping/order/{orderId}     - Sipariş için kargo bilgisi
POST   /api/shipping                     - Yeni kargo kaydı ekle
PUT    /api/shipping/{id}                - Kargo kaydı güncelle
POST   /api/shipping/{id}/events         - Kargo durumu ekle

# Excel İşlemleri
GET    /api/shipping/template     - Excel şablonu indir
GET    /api/shipping/export       - Kargo bilgilerini Excel'e aktar
POST   /api/shipping/import       - Excel'den kargo bilgisi yükle
```

## 📊 Excel Kullanımı

### Ürün Excel Formatı

| Ürün Adı * | Açıklama | Fiyat * | Stok * | Kategori * | SKU * | Resim URL | Aktif |
|-----------|----------|---------|--------|-----------|-------|-----------|-------|
| Örnek Ürün | Açıklama | 99.99 | 100 | Elektronik | SKU-123 | url | true |

### Kargo Takip Excel Formatı

| Sipariş ID * | Takip Numarası * | Kargo Firması * | Durum | Mevcut Konum |
|-------------|-----------------|----------------|-------|--------------|
| ORD-12345 | TK123456789 | Aras Kargo | Shipped | İstanbul |

### Excel İşlemleri Nasıl Yapılır?

1. **Şablon İndirme**: API'den ilgili `/template` endpoint'ini çağırın
2. **Veri Girişi**: İndirilen Excel dosyasını doldurun
3. **Yükleme**: Doldurduğunuz dosyayı `/import` endpoint'ine POST edin
4. **Dışa Aktarma**: `/export` endpoint'inden mevcut verileri indirin

## 🛠️ Teknolojiler

### Backend
- ASP.NET Core 9.0
- Firebase Admin SDK v3.4
- Google Cloud Firestore v3.11
- EPPlus v7.4 (Excel işlemleri)
- FluentValidation (Doğrulama)
- Swagger/OpenAPI (Swashbuckle)
- JWT Bearer Authentication

### Frontend
- React 19 + TypeScript
- Vite 7
- Material UI (MUI) v7
- Firebase Client SDK v12
- Axios, React Router, Recharts

### Veritabanı
- Firebase Firestore (NoSQL)

## 📝 Veri Modelleri

### Product (Ürün)
- Name, Description, Price, Stock
- Category, SKU, ImageUrl
- SellerId, IsActive
- CreatedAt, UpdatedAt

### Order (Sipariş)
- OrderNumber, SellerId, CustomerId
- Items (List<OrderItem>)
- TotalAmount, Status
- ShippingAddress, TrackingNumber
- OrderDate, ShippedDate, DeliveredDate

### Shipping (Kargo)
- OrderId, TrackingNumber, Carrier
- Status, CurrentLocation
- Events (List<ShippingEvent>)
- EstimatedDeliveryDate, ActualDeliveryDate

## 🔐 Kimlik Doğrulama Akışı

1. Frontend Firebase client'ı ile `signInWithEmailAndPassword` veya `createUserWithEmailAndPassword` çağrılır.
2. Firebase'den alınan ID token `POST /api/auth/session` endpoint'ine gönderilir.
3. Backend token'ı doğrular ve HttpOnly + Secure cookie'ye yazar.
4. Axios istekleri `withCredentials: true` olduğu için tarayıcı çerezi otomatik gönderir.
5. Çıkışta `DELETE /api/auth/session` çağrılır ve cookie silinir.

## 🔐 Güvenlik Notları

- Firebase credentials dosyasını `.gitignore`'a ekleyin
- Üretim ortamında çevre değişkenleri kullanın
- Tüm kimlik doğrulama tarayıcıya görünmeyen HttpOnly cookie üzerinden yürütülür; `localStorage`'da token tutulmaz

## 📱 Frontend (seller-dashboard)

Bu projede **React 19 + TypeScript + Vite** ile geliştirilmiş bir satıcı paneli bulunmaktadır.

### Frontend Kurulumu

```bash
cd seller-dashboard
npm install
npm run dev
```

Frontend şu adreste çalışacaktır: `http://localhost:5173`

### Kullanılan Teknolojiler
- React 19 + TypeScript
- Vite (Build tool)
- Material UI (MUI) v7
- React Router v7
- Axios (HTTP client)
- Firebase Auth (Client SDK)
- Recharts (Grafikler)
- XLSX (Excel işlemleri)

### Mevcut Sayfalar
- 🏠 Dashboard (Ana sayfa/istatistikler)
- 📦 Ürünler (Ürün listesi ve yönetimi)
- 🛒 Siparişler (Sipariş yönetimi)
- 🚚 Kargo (Kargo takip ekranı)
- 📊 Excel (Excel import/export işlemleri)
- 🔐 Giriş (Login sayfası)

## 🤝 Katkıda Bulunma

1. Fork yapın
2. Feature branch oluşturun (`git checkout -b feature/amazing-feature`)
3. Commit yapın (`git commit -m 'Add amazing feature'`)
4. Push yapın (`git push origin feature/amazing-feature`)
5. Pull Request açın

## 📄 Lisans

Bu proje eğitim amaçlıdır. EPPlus kütüphanesi NonCommercial lisansı ile kullanılmaktadır.

## 💡 Sonraki Adımlar

- [x] Frontend uygulaması (React + TypeScript + Vite)
- [x] Kullanıcı authentication (Firebase Auth)
- [x] Satıcı paneli görselleri (Material UI)
- [x] Raporlama ve istatistikler (Dashboard sayfası)
- [ ] Email bildirimleri
- [ ] Toplu ürün güncelleme
- [ ] Gelişmiş filtreleme ve arama
- [ ] Birim testleri (Unit tests)
