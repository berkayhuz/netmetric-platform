# SonarQube Local Setup (NetMetric Monorepo)

Bu dokuman, NetMetric monorepo icin backend (.NET) ve frontend (Next.js/TypeScript) SonarQube analizlerini ayri projeler olarak calistirmak icin hazirlandi.

## 1) Local SonarQube'u ayaga kaldirma

Repo root dizininde:

```powershell
docker compose -f .\docker-compose.sonar.yml up -d
```

Sonra SonarQube arayuzunu ac:

- [http://localhost:9000](http://localhost:9000)

Kapatmak icin:

```powershell
docker compose -f .\docker-compose.sonar.yml down
```

Veriyi sifirlamak (tum local Sonar verisi silinir):

```powershell
docker compose -f .\docker-compose.sonar.yml down -v
```

## 2) Ilk giris

Ilk giriste varsayilan hesap:

- Kullanici: `admin`
- Sifre: `admin`

Ilk login sonrasinda sifre degistirme ekrani gelir; local gelistirme icin yeni bir sifre belirleyin.

## 3) Token kullanimi

SonarQube UI'dan user token olusturun, sonra terminalde sadece environment variable olarak set edin:

```powershell
$env:SONAR_TOKEN = "your-token"
```

Token dosyaya yazilmaz, commit edilmez.

## 4) Backend analizi calistirma (.NET)

```powershell
pnpm run sonar:backend
```

Bu script sirayla su adimlari calistirir:

1. `dotnet sonarscanner begin`
2. `dotnet restore`
3. `dotnet build`
4. `dotnet test` + OpenCover coverage
5. `dotnet sonarscanner end`

Varsayilan backend Sonar projesi:

- Project key: `netmetric_backend`
- Project name: `NetMetric Backend`

## 5) Frontend analizi calistirma (Next.js/TypeScript)

```powershell
pnpm run sonar:frontend
```

Frontend script:

1. `pnpm install --frozen-lockfile`
2. Uygun frontend check/test scriptlerini calistirir
3. Coverage script varsa calistirir
4. `sonar-scanner -Dproject.settings=sonar-project.frontend.properties`

Coverage davranisi iki moddur:

- `pnpm run sonar:frontend`: local mod. Coverage script yoksa veya coverage uretimi fail olursa warning verip analize devam eder.
- `pnpm run sonar:frontend:strict`: strict mod. Coverage script yoksa veya coverage uretimi fail olursa komut fail olur.

Not: `CI=true` veya `CI=1` ortami varsa frontend script strict coverage moduna otomatik gecer.

Varsayilan frontend Sonar projesi:

- Project key: `netmetric_frontend`
- Project name: `NetMetric Frontend`

## 6) Tum analizleri tek komutla calistirma

```powershell
pnpm run sonar:all
```

Bu komut backend basariliysa frontend'i calistirir. Herhangi bir adim fail olursa komut fail olur.

Strict coverage ile tum analiz:

```powershell
pnpm run sonar:all:strict
```

## 7) Neden backend ve frontend ayri SonarQube projesi?

- Farkli teknoloji stack'leri (C# ve TypeScript) ayri quality profile ile yonetilir.
- Coverage ve test metrikleri stack bazli daha dogru okunur.
- Quality Gate kararlarini her katman icin bagimsiz uygulamak kolaylasir.
- CI/CD pipeline'inda sorun izolasyonu hizlanir.

## 8) Hangi dosyalar Git'e girer / girmez?

Git'e girmesi gerekenler:

- `docker-compose.sonar.yml`
- `sonar-project.frontend.properties`
- `scripts/sonar-backend.ps1`
- `scripts/sonar-frontend.ps1`
- `scripts/sonar-all.ps1`
- `docs/sonarqube.md`

Git'e girmemesi gerekenler:

- `.sonarqube/`
- `.scannerwork/`
- `.sonar/`
- `TestResults/`
- `coverage/`
- `**/coverage.opencover.xml`
- `**/coverage.cobertura.xml`
- `**/lcov.info`

## 9) Onerilen Quality Gate

Baslangic icin onerilen minimum hedefler:

- Vulnerabilities: `0`
- Bugs: `0`
- Security Hotspots: review edilmis olmasi zorunlu
- Duplication: `%3` - `%5` araligini asmamasi
- Coverage hedefi:

1. Backend yeni kod coverage >= `%80`
2. Frontend yeni kod coverage >= `%70`

Takimin olgunluguna gore coverage hedefleri kademeli artirilabilir.

## 10) Troubleshooting

### `dotnet sonarscanner` bulunamadi

Global tool kurun:

```powershell
dotnet tool install --global dotnet-sonarscanner
```

Gerekirse yeni terminal acin.

### `sonar-scanner` bulunamadi

SonarScanner CLI kurulu ve `PATH` icinde olmali. Kurulumdan sonra terminali yeniden acin ve `sonar-scanner --version` ile dogrulayin.

### Coverage Sonar'da 0 gorunuyor

- Frontend icin `lcov.info` dosyalarinin olustugunu kontrol edin.
- Backend icin `coverage.opencover.xml` dosyalarinin test ciktilarinda uretildigini kontrol edin.
- Coverage script'inin fail edip etmedigini terminal logunda kontrol edin.

### `http://localhost:9000` acilmiyor

- Container durumunu kontrol edin:

```powershell
docker ps
```

- Loglari kontrol edin:

```powershell
docker logs netmetric-sonarqube
docker logs netmetric-sonar-db
```

- Port cakismasi varsa 9000 portunu kullanan sureci kapatin veya compose portunu degistirin.
