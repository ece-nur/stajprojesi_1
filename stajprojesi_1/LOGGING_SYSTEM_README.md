# 🔍 Kapsamlı Loglama Sistemi

Bu proje için geliştirilmiş kapsamlı loglama sistemi, kullanıcı etkileşimlerini, sistem olaylarını ve güvenlik olaylarını detaylı bir şekilde takip eder.

## 🏗️ Sistem Mimarisi

### 1. Backend Logging Service (`ILoggingService`)
- **Kullanıcı İşlemleri**: Giriş, çıkış, sayfa erişimleri
- **Veri İşlemleri**: CRUD operasyonları (Ekleme, Güncelleme, Silme, Okuma)
- **Güvenlik Olayları**: Yetkisiz erişimler, başarısız girişler
- **Sistem Olayları**: Hatalar, uyarılar, bilgi mesajları

### 2. Frontend Logging Service (`FrontendLogger`)
- **Kullanıcı Etkileşimleri**: Tıklamalar, form gönderimleri, input değişiklikleri
- **Sayfa Olayları**: Sayfa yükleme, kapanma, navigasyon
- **AJAX İstekleri**: API çağrıları, hatalar, performans metrikleri
- **Session Takibi**: Kullanıcı oturum süreleri, etkileşim sayıları

### 3. API Logging Controller (`LoggingApiController`)
- Frontend log'larını backend'e entegre eder
- RESTful API endpoint'leri sağlar
- Health check endpoint'i

## 📊 Log Kategorileri

### 🔐 Giriş/Çıkış Logları
```
🔐 BAŞARILI GİRİŞ: 👤 Kullanıcı: admin | 🌍 IP: 192.168.1.1 | 🔍 User Agent: Mozilla/5.0... | ⏰ Zaman: 2024-01-15 10:30:00
🚪 ÇIKIŞ: 👤 Kullanıcı: admin | 🌍 IP: 192.168.1.1 | ⏰ Zaman: 2024-01-15 11:45:00
❌ BAŞARISIZ GİRİŞ: 👤 Kullanıcı: test | 🌍 IP: 192.168.1.2 | 🔍 User Agent: Mozilla/5.0... | 📝 Sebep: Yanlış şifre | ⏰ Zaman: 2024-01-15 10:25:00
```

### 💾 Veri İşlem Logları
```
💾 VERİ İŞLEMİ: Kaydetme | 🏷️ Tür: Referans | 🆔 ID: Yeni | 👤 Kullanıcı: admin | 🌍 IP: 192.168.1.1 | 📝 Detay: Tür: BasvuranBirim, Ad: Test Birimi
💾 VERİ İŞLEMİ: Güncelleme | 🏷️ Tür: Referans | 🆔 ID: 123 | 👤 Kullanıcı: admin | 🌍 IP: 192.168.1.1 | 📝 Detay: Yeni ad: Güncellenmiş Birim
💾 VERİ İŞLEMİ: Silme | 🏷️ Tür: Referans | 🆔 ID: 123 | 👤 Kullanıcı: admin | 🌍 IP: 192.168.1.1 | 📝 Detay: Soft delete yapıldı
```

### 👤 Kullanıcı İşlem Logları
```
👤 KULLANICI İŞLEMİ: Referans Ekleme Sayfası Erişimi | 👤 Kullanıcı: admin | 📍 Yol: GET /Referans/Ekle | 🌍 IP: 192.168.1.1 | 🔍 User Agent: Mozilla/5.0... | 📝 Detay: GET /Referans/Ekle
👤 KULLANICI İŞLEMİ: Referans Listesi Sayfası Erişimi | 👤 Kullanıcı: admin | 📍 Yol: GET /Referans/Liste | 🌍 IP: 192.168.1.1 | 🔍 User Agent: Mozilla/5.0... | 📝 Detay: GET /Referans/Liste
```

### 🔒 Güvenlik Olay Logları
```
🔒 GÜVENLİK OLAYI: Yetkisiz Erişim | 👤 Kullanıcı: Anonim | 🌍 IP: 192.168.1.3 | 🔍 User Agent: Mozilla/5.0... | 📝 Detay: Referans listesi sayfasına yetkisiz erişim | ⏰ Zaman: 2024-01-15 10:20:00
🔒 GÜVENLİK OLAYI: Başarısız Giriş Denemesi | 👤 Kullanıcı: Anonim | 🌍 IP: 192.168.1.4 | 🔍 User Agent: Mozilla/5.0... | 📝 Detay: Email: test@test.com, IP: 192.168.1.4 | ⏰ Zaman: 2024-01-15 10:15:00
```

### 🌐 Frontend Etkileşim Logları
```
🔍 [USER_INTERACTION] Button Click: {
  sessionId: "session_1705314000000_abc123",
  action: "Button Click",
  category: "user_interaction",
  details: { buttonName: "Düzenle Butonu" },
  userAgent: "Mozilla/5.0...",
  url: "http://localhost:5287/Referans/Liste",
  timestamp: "2024-01-15T10:30:00.000Z",
  interactionCount: 15
}

🔍 [NAVIGATION] Menu Click: {
  sessionId: "session_1705314000000_abc123",
  action: "Menu Click",
  category: "navigation",
  details: { menuName: "Referans", menuUrl: "/Referans/Liste" },
  userAgent: "Mozilla/5.0...",
  url: "http://localhost:5287/Referans/Liste",
  timestamp: "2024-01-15T10:30:00.000Z",
  interactionCount: 16
}
```

## 🚀 Kullanım Örnekleri

### Backend'de Log Kullanımı
```csharp
// Controller'da
public class ReferansController : BaseController
{
    private readonly ILoggingService _loggingService;
    
    public IActionResult Ekle()
    {
        var kullaniciAdi = HttpContext.Session.GetString("KullaniciAdi");
        var ipAddress = HttpContext.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";
        
        _loggingService.LogUserAction("Referans Ekleme Sayfası Erişimi", "GET /Referans/Ekle", kullaniciAdi, ipAddress);
        
        // ... sayfa işlemleri
        return View();
    }
    
    [HttpPost]
    public IActionResult Kaydet(string Tipi, string ReferansAdi)
    {
        var kullaniciAdi = HttpContext.Session.GetString("KullaniciAdi");
        var ipAddress = HttpContext.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";
        
        _loggingService.LogUserAction("Referans Kaydetme İşlemi", $"Tür: {Tipi}, Ad: {ReferansAdi}", kullaniciAdi, ipAddress);
        
        try
        {
            // ... kaydetme işlemi
            _loggingService.LogDataOperation("Kaydetme", "Referans", "Yeni", kullaniciAdi, true, $"Tür: {Tipi}, Ad: {ReferansAdi}");
            return RedirectToAction("Ekle");
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Referans Kaydetme Hatası", ex.Message, ex, kullaniciAdi);
            throw;
        }
    }
}
```

### Frontend'de Log Kullanımı
```javascript
// Manuel log gönderimi
window.frontendLogger.logEvent('Custom Action', 'Custom details', 'custom');

// Hata logu
window.frontendLogger.logError(new Error('Test error'), 'Test context');

// Özel olay
window.frontendLogger.sendManualLog('Custom Action', 'Custom details', 'custom');

// Buton tıklama logu (otomatik)
<button onclick="logButtonClick('Test Butonu')">Test</button>

// Select değişiklik logu (otomatik)
<select onchange="logSelectChange('Test Select', this.value)">
    <option value="">Seçiniz</option>
</select>
```

## 📁 Dosya Yapısı

```
stajprojesi_1/
├── Services/
│   └── LoggingService.cs          # Ana loglama servisi
├── Controllers/
│   ├── LoggingApiController.cs    # Frontend log API'si
│   ├── AccountController.cs       # Giriş/çıkış logları
│   └── ReferansController.cs      # CRUD operasyon logları
├── Views/
│   ├── Shared/
│   │   └── _Layout.cshtml        # Global frontend logging
│   └── Referans/
│       └── Liste.cshtml          # Sayfa özel logları
└── wwwroot/
    └── js/
        └── logging.js             # Frontend logging service
```

## ⚙️ Konfigürasyon

### Program.cs'de Servis Kaydı
```csharp
// Logging service'i ekle
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ILoggingService, LoggingService>();
```

### Serilog Konfigürasyonu
```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.File(@"C:\Logs\stajapp-.log", rollingInterval: RollingInterval.Day)
    .WriteTo.Console()
    .CreateLogger();
```

## 🔍 Log Dosyası Konumu

- **Ana log dosyası**: `C:\Logs\stajapp-YYYYMMDD.log`
- **Günlük rotasyon**: Her gün yeni dosya
- **Log seviyesi**: Debug ve üzeri
- **Console çıktısı**: Geliştirme ortamında

## 📈 Performans Özellikleri

- **Asenkron loglama**: UI'ı bloklamaz
- **Seçici frontend log gönderimi**: Sadece önemli olaylar backend'e gönderilir
- **Batch processing**: Toplu log işleme
- **Memory efficient**: Gereksiz log verisi tutulmaz

## 🛡️ Güvenlik Özellikleri

- **IP adresi takibi**: Tüm loglarda IP adresi kaydedilir
- **User Agent takibi**: Tarayıcı bilgileri loglanır
- **Session takibi**: Kullanıcı oturum bilgileri
- **Yetki kontrolü**: Yetkisiz erişimler loglanır

## 🔧 Geliştirme ve Test

### Test Etme
1. Uygulamayı çalıştırın: `dotnet run`
2. Farklı sayfalara gidin
3. CRUD işlemleri yapın
4. Log dosyasını kontrol edin: `C:\Logs\stajapp-YYYYMMDD.log`
5. Browser console'da frontend logları görün

### Debug Modu
- Frontend logları browser console'da görünür
- Backend logları hem dosyaya hem console'a yazılır
- Detaylı hata mesajları ve stack trace'ler

## 📝 Gelecek Geliştirmeler

- [ ] Database logging (SQL Server)
- [ ] Log analizi dashboard'u
- [ ] Email alert sistemi
- [ ] Log retention policies
- [ ] Performance metrics
- [ ] User behavior analytics

## 🤝 Katkıda Bulunma

Bu loglama sistemi sürekli geliştirilmektedir. Yeni özellikler ve iyileştirmeler için:

1. Issue açın
2. Feature request gönderin
3. Pull request yapın
4. Dokümantasyonu güncelleyin

---

**Son Güncelleme**: 15 Ocak 2024  
**Versiyon**: 1.0.0  
**Geliştirici**: AI Assistant  
**Lisans**: MIT

