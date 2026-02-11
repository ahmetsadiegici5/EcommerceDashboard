# E-Ticaret Satıcı Paneli

Satıcıların ürün, sipariş ve kargo süreçlerini tek bir panelden yönetebildiği full-stack bir web uygulaması. Backend ASP.NET Core 9 Web API, frontend React + TypeScript ile geliştirilmiştir. Veriler Cloud Firestore'da tutulur, kimlik doğrulama Firebase Auth ile yapılır.

## Özellikler

- **Ürünler:** ekleme, düzenleme, silme, arama ve düşük stok filtresi
- **Excel:** şablon indirme, toplu ürün yükleme ve dışa aktarma
- **Siparişler:** listeleme, detay görüntüleme ve durum güncelleme (Beklemede, İşleniyor, Kargoda, Teslim Edildi, İptal)
- **Kargo:** takip numarasıyla sorgulama ve sipariş bazlı kargo kayıtları
- **Dashboard:** satış ve stok durumunu gösteren grafikler

## Mimari

- Backend katmanlı bir yapıdadır: Controller → Service → Firestore.
- Girişte Firebase'den alınan token, backend tarafından HttpOnly cookie olarak saklanır. Böylece token tarayıcıda JavaScript ile erişilebilir bir yerde tutulmaz.
- Her satıcı yalnızca kendi ürün ve siparişlerine erişebilir; bu kontrol controller seviyesinde yapılır.
- Girdi doğrulama FluentValidation ile yapılır. Hata yönetimi özel bir middleware ile sağlanır, istek sınırlaması ise .NET'in yerleşik rate limiter'ı ile yapılır.
- API dokümantasyonu Swagger ile sunulur.

## Kullanılan teknolojiler

- **Backend:** C#, .NET 9, ASP.NET Core Web API, FluentValidation, EPPlus, Firebase Admin SDK
- **Frontend:** React 19, TypeScript, Vite, Material UI, MUI DataGrid, Recharts
- **Veritabanı / Auth:** Cloud Firestore, Firebase Authentication
- **Test / CI:** xUnit, Moq, Vitest, GitHub Actions

## Kurulum

Gereksinimler: .NET 9 SDK, Node.js 20+ ve Authentication ile Firestore'u etkin bir Firebase projesi.

**Backend**

Firebase Console'dan indirilen servis hesabı dosyası `EcommerceAPI/firebase-credentials.json` olarak kaydedilir (ayrıntılar: [FIREBASE_SETUP.md](EcommerceAPI/FIREBASE_SETUP.md)).

```bash
cd EcommerceAPI
dotnet run          # http://localhost:5039, Swagger: /swagger
```

**Frontend**

```bash
cd seller-dashboard
cp .env.example .env
npm install
npm run dev         # http://localhost:5173
```

**Testler**

```bash
dotnet test
cd seller-dashboard && npm test
```
