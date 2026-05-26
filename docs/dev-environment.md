# Local Dev Environment (Enterprise-style baseline)

Bu akış, localde production-grade bağımlılıkları Docker ile ayağa kaldırır ve API süreçlerini kontrollü başlatır.

## Kapsam

- SQL Server (Docker, `14333`)
- Redis (`6379`)
- RabbitMQ + Management UI (`5672`, `15672`)
- SonarQube + PostgreSQL (`9000`)
- API Gateway (`http://localhost:5030`)
- Servis API'leri otomatik discovery ile başlatılır (`services/**/src/*.API.csproj`)

## Komutlar

```powershell
pnpm dev:up
pnpm dev:seed:auth
pnpm dev:clean
```

Doğrudan script:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\dev-up.ps1
powershell -ExecutionPolicy Bypass -File .\scripts\dev-seed-auth.ps1
powershell -ExecutionPolicy Bypass -File .\scripts\dev-clean.ps1 -PurgeVolumes
```

## Davranış

- `dev-up.ps1`
  - Docker stack'i `docker-compose.dev.yml` ile kaldırır.
  - Auth-web için yoksa `apps/auth-web/.env.local` oluşturur.
  - Gateway + API projelerini başlatır ve PID takibi yapar.
  - Loglar: `.local/dev/logs`
- `dev-seed-auth.ps1`
  - Gateway üzerinden `/api/auth/register` çağırarak seed kullanıcı oluşturur.
- `dev-clean.ps1`
  - `dev-up` ile başlatılan API süreçlerini kapatır.
  - Docker stack'i durdurur.
  - `-PurgeVolumes` verilirse volume'leri de siler.

## Notlar

- Gateway/Auth trusted-gateway imza secret'ları `dev-up.ps1` içinde process-level env override ile set edilir.
- Auth API connection string local SQL container'a yönlendirilir.
- Redis ve RabbitMQ ayarları Auth API için env override ile zorlanır.
