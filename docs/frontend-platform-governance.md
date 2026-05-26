# Frontend Platform Governance

## 1. Platform Bazli Frontend Monorepo Standardi

- `apps/*` sadece deploy edilen urun uygulamalari icindir.
- `packages/frontend/*` tekrar kullanilan platform yetenekleri icindir.
- Yeni bir kod birden fazla uygulama tarafindan tuketilecekse `packages/frontend/*` altina alinmalidir.
- `@netmetric/ui` ortak tasarim sistemi ve component API katmanidir.
- `@netmetric/config` typed environment ve uygulama konfigurasyon sozlesmelerini tasir.
- `packages -> apps` importu yasaktir, bagimlilik yonu `apps -> packages` olmalidir.

## 2. shadcn Kullanim Standardi

- shadcn tabanli componentler sadece `packages/frontend/ui/src/components` altinda gelistirilir.
- `primitives/*`: temel UI yapilari ve Radix kompozisyonu.
- `layout/*`, `forms/*`, `overlay/*`, `navigation/*`: domain-seviyesi UI bloklari.
- shadcn kodu birebir alinmaz; token, accessibility ve naming standartlarina gore adapte edilir.
- Radix bagimliliklari sadece `@netmetric/ui` package seviyesinde yonetilir.
- Public export sadece `src/index.ts` (server-safe) ve `src/client.ts` (client-only) uzerinden yapilir.

## 3. Component Naming Standard

- Dosya adlari kebab-case olmalidir (`button-group.tsx`).
- React component adlari PascalCase olmalidir (`ButtonGroup`).
- Variant isimleri ortak bir sozlukten secilir: `default`, `destructive`, `outline`, `secondary`, `ghost`, `link`.
- Compound componentlerde parent prefix korunur (`DialogHeader`, `DialogFooter`).
- Barrel export sadece package giris dosyalarinda kullanilir; feature altinda keyfi ekstra barrel olusturulmaz.

## 4. Accessibility Rules

- Butun interaktif ogeler klavye ile ulasilabilir olmalidir.
- `aria-*` kullanimi ESLint ve a11y testleri ile kontrol edilir.
- `:focus-visible` davranisi globals seviyesinde standartlastirilir.
- Dialog/Sheet/Popover gibi overlay patternleri Radix primitive contractlarina uyar.
- Renk kontrasti token tabanli semantic renklerle korunur.
- Reduced motion destegi zorunludur (`prefers-reduced-motion`).

## 5. Variant Policy

- Variant tanimlari `cva` ile yapilir.
- `defaultVariants` zorunludur.
- `size` ve `variant` isimleri platform genelinde tutarli olmalidir.
- App-specific (uygulamaya ozel) variant adlari shared UI package icine alinmaz.
- `destructive`, `secondary`, `outline`, `ghost` semantigi standarttir.

## 6. Token Usage Rules

- Raw color/hardcoded deger kullanimi yasaktir.
- Spacing/radius/shadow/motion degerleri token kaynakli olmalidir.
- Semantic tokenlar (`--color-*`) componentlerde dogrudan kullanilir.
- Brand token ve semantic token ayrimi korunur.
- Ucuncu taraf class selector zorunlulugu varsa (or. chart internals) allowlist ile sinirlandirilir.

## 7. Dark Mode Contract

- Dark mode class-based (`.dark`) calisir.
- Tema gecisleri token tabanli yapilir.
- Component seviyesinde gereksiz `dark:` sinif spam'i yapilmaz.
- Theme switching `ThemeProvider` uzerinden tek noktadan yapilir.
- SSR hydration riskini azaltmak icin erken theme init script kullanilir.

## 8. Responsive Breakpoints

- Mobile-first yaklasim zorunludur.
- Breakpoint kararlarinda Tailwind standardi (`sm`, `md`, `lg`, `xl`) korunur.
- Layout standardi once akiskan, sonra breakpoint-genisleme prensibine gore kurulur.
- Container policy uygulama tipine gore belirlenir:
  - dashboard: daha genis ve veri yogun layout
  - public/auth: okunabilirlik odakli daha kontrollu genislik

## 9. Animation Standards

- Motion sureleri tokenlardan gelmelidir (`--nm-motion-fast`, `--nm-motion-base`, `--nm-motion-slow`).
- Easing fonksiyonlari standart token ile tanimlanir (`--nm-ease-standard`).
- Reduced motion fallback zorunludur.
- Micro interactionlar sade, kisa ve amaca yonelik olmalidir.
- Gereksiz dekoratif ve uzun sureli animasyonlar platform standardina aykiridir.
