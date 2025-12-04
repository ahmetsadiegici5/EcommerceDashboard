# Firebase Yapılandırma Şablonu

## Bu dosya, firebase-credentials.json için bir şablondur

Firebase Console'dan aldığınız servis hesabı JSON dosyasını bu formatta kaydedin:

```json
{
  "type": "service_account",
  "project_id": "YOUR_PROJECT_ID",
  "private_key_id": "YOUR_PRIVATE_KEY_ID",
  "private_key": "-----BEGIN PRIVATE KEY-----\n...\n-----END PRIVATE KEY-----\n",
  "client_email": "firebase-adminsdk-xxxxx@your-project.iam.gserviceaccount.com",
  "client_id": "YOUR_CLIENT_ID",
  "auth_uri": "https://accounts.google.com/o/oauth2/auth",
  "token_uri": "https://oauth2.googleapis.com/token",
  "auth_provider_x509_cert_url": "https://www.googleapis.com/oauth2/v1/certs",
  "client_x509_cert_url": "YOUR_CERT_URL",
  "universe_domain": "googleapis.com"
}
```

## Firebase Kurulum Adımları:

1. https://console.firebase.google.com/ adresine gidin
2. Yeni bir proje oluşturun veya mevcut projenizi seçin
3. Proje Ayarları > Servis Hesapları > Yeni özel anahtar oluştur
4. İndirilen JSON dosyasını `firebase-credentials.json` adıyla EcommerceAPI klasörüne kaydedin
5. `appsettings.json` dosyasında ProjectId'nizi güncelleyin

## Firestore Database Kurulumu:

1. Firebase Console'da Firestore Database sekmesine gidin
2. "Veritabanı Oluştur" butonuna tıklayın
3. Test modunda veya üretim modunda başlatın
4. Konum seçin (örn: europe-west1)

## Koleksiyonlar:

Aşağıdaki koleksiyonlar otomatik olarak oluşturulacaktır:
- `products` - Ürün bilgileri
- `orders` - Sipariş bilgileri
- `shipping` - Kargo takip bilgileri
- `sellers` - Satıcı bilgileri
