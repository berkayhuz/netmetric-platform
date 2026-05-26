# ADR Index

## ADR nedir?

ADR (Architectural Decision Record), önemli mimari kararların bağlamını, kararı ve etkilerini kaydeden kısa dokümandır.

## Yeni ADR nasıl eklenir?

1. `docs/adr/` altında sıradaki numara ile yeni dosya aç:
   - örn: `0003-short-title.md`
2. Başlıkta:
   - Status
   - Date
   - Context
   - Decision
   - Consequences
3. Bu index dosyasına link ekle.

## Naming convention

- `NNNN-kebab-case-title.md`
- NNNN dört haneli sıra numarasıdır (`0001`, `0002`, ...)

## Mevcut ADR listesi

- [0001-ui-package-boundaries](/docs/adr/0001-ui-package-boundaries.md)
- [0002-source-only-ui-package-decision](/docs/adr/0002-source-only-ui-package-decision.md)
