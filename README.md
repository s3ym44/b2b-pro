# B2B Procurement Platform

B2B tedarik yönetim sistemi - RFQ (Teklif Talebi) ve tedarikçi yönetimi platformu.

## Özellikler

- 🏢 Şirket ve kullanıcı yönetimi
- 📦 Malzeme kataloğu
- 🤝 Tedarikçi yönetimi
- 📋 RFQ (Teklif Talebi) oluşturma ve yönetimi
- 💰 Teklif verme ve karşılaştırma
- 📊 Raporlama ve analitik
- 🔔 Bildirim sistemi

## Teknolojiler

- **Backend:** ASP.NET Core 8.0
- **Database:** SQLite (Development) / PostgreSQL (Production optional)
- **ORM:** Entity Framework Core
- **Frontend:** Razor Views, Bootstrap 5, Font Awesome
- **Real-time:** SignalR

## Railway'e Deploy

### Adımlar

1. **GitHub'a Push:**
   ```bash
   git init
   git add .
   git commit -m "Initial commit"
   git remote add origin https://github.com/username/B2BProcurement.git
   git push -u origin main
   ```

2. **Railway'de Yeni Proje:**
   - [Railway.app](https://railway.app) adresine gidin
   - "New Project" → "Deploy from GitHub repo"
   - Repository'yi seçin

3. **Otomatik Deploy:**
   - Railway, Dockerfile'ı otomatik algılayacak
   - Build ve deploy işlemi başlayacak

### Environment Variables (Opsiyonel)

Railway dashboard'dan şu değişkenleri ayarlayabilirsiniz:

| Variable | Description | Default |
|----------|-------------|---------|
| `ASPNETCORE_ENVIRONMENT` | Runtime environment | `Production` |
| `ConnectionStrings__DefaultConnection` | Database connection | SQLite (built-in) |

### Health Check

Uygulama `/health` endpoint'inde health check sunar.

## Yerel Geliştirme

### Gereksinimler

- .NET 8.0 SDK
- SQLite

### Çalıştırma

```bash
cd B2BProcurement
dotnet restore
dotnet run
```

Uygulama `http://localhost:5117` adresinde çalışacaktır.

### Demo Hesaplar

| E-posta | Şifre | Şirket |
|---------|-------|--------|
| admin@abc.com | Demo123! | ABC Otomotiv |
| admin@xyz.com | Demo123! | XYZ Tekstil |
| admin@demo.com | Demo123! | Demo Metal |

## Proje Yapısı

```
B2BProcurement/
├── B2BProcurement/           # Web uygulaması (MVC)
│   ├── Controllers/          # MVC Controller'ları
│   ├── Views/                # Razor Views
│   ├── wwwroot/              # Static dosyalar
│   └── Program.cs            # Uygulama başlangıcı
├── B2BProcurement.Core/      # Entity ve Enum tanımları
├── B2BProcurement.Data/      # DbContext ve Repository'ler
├── B2BProcurement.Business/  # Servisler ve DTO'lar
├── Dockerfile                # Docker konfigürasyonu
└── railway.toml              # Railway konfigürasyonu
```

## Lisans

MIT License
