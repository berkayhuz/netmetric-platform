# Local Search Verification

This runbook is for local development only. It verifies the NetMetric Global Search path end to end without adding admin endpoints, crawler behavior, direct SQL writes, or manual Meilisearch document writes.

## Prerequisites

Start the local stack:

```powershell
pnpm dev:up:nobuild
```

The verification path expects these local components:

- Gateway: `http://localhost:5030`
- Auth API
- CRM API
- Search API: `http://localhost:5310`
- Search Worker
- RabbitMQ
- Meilisearch: `http://localhost:7700`
- CRM `CustomerManagement` outbox enabled

Dynamic customer verification is asynchronous:

```text
CRM API -> CustomerManagement outbox -> RabbitMQ -> Search Worker -> Meilisearch -> Search API
```

## One-Command Verification

Use the wrapper when you want the full local search verification sequence.

```powershell
$env:NETMETRIC_DEV_SEED_PASSWORD = "<local-dev-password>"
scripts/dev/search-full-verify.ps1 -DeleteAfter
```

By default, full verification writes a deterministic JSON artifact to:

- `.local/dev/search-full-verify-summary.json`

This summary file contains verification booleans, timestamp, and diagnostics only. It does not include bearer tokens or passwords, so it is safe to share internally for troubleshooting.

The dynamic Opportunity fields are:

- `dynamicOpportunityVerified`
- `dynamicOpportunityDeleted`
- `opportunityOutboxDiagnostics`

The wrapper runs:

1. `scripts/dev/auth-token.ps1`
2. `scripts/dev/search-reseed.ps1`
3. `scripts/dev/search-verify.ps1`
4. `scripts/dev/search-customer-fixture.ps1`
5. `scripts/dev/search-company-fixture.ps1`
6. `scripts/dev/search-contact-fixture.ps1`
7. `scripts/dev/search-deal-fixture.ps1`
8. `scripts/dev/search-opportunity-fixture.ps1`
9. `scripts/dev/search-ticket-fixture.ps1`
10. `scripts/dev/search-lead-fixture.ps1`
11. `scripts/dev/search-pipeline-fixture.ps1`

Useful options:

- `-Password <value>`: use this instead of `NETMETRIC_DEV_SEED_PASSWORD`.
- `-DeleteAfter`: soft-delete the dynamic customer fixture through CRM API and verify search removal.
- `-SkipReseed`: skip resetting/reseeding `searchdocuments`.
- `-SkipDynamicCustomer`: skip CRM customer fixture creation and neutral-locale runtime verification.
- `-SkipCustomerFixture`: skip customer fixture runtime verification.
- `-SkipCompanyFixture`: skip company fixture runtime verification.
- `-SkipContactFixture`: skip contact fixture runtime verification.
- `-SkipTicketFixture`: skip ticket fixture runtime verification.
- `-SkipLeadFixture`: skip lead fixture runtime verification.
- `-SkipPipelineFixture`: skip pipeline fixture runtime verification.
- `-SkipOpportunityFixture`: skip opportunity fixture runtime verification.
- `-SkipDynamicFixtures`: skip all customer/company/contact/deal/opportunity/ticket/lead/pipeline runtime fixture checks.
- `-SkipOutboxDiagnostics`: skip Ticket outbox SQL diagnostics snapshot in ticket fixture/full verify.
- `-SummaryOutputPath <path>`: override the default summary artifact path (`.local/dev/search-full-verify-summary.json`).
- `-PrintToken`: pass through token printing from `auth-token.ps1`; avoid this unless you need it for manual debugging.

Example custom summary path:

```powershell
scripts/dev/search-full-verify.ps1 -DeleteAfter -SummaryOutputPath ".local/dev/search-full-verify-summary-ci.json"
```

## Step-by-Step Verification

Generate a local bearer token:

```powershell
$env:NETMETRIC_DEV_SEED_PASSWORD = "<local-dev-password>"
scripts/dev/auth-token.ps1
```

Reset/reseed localized static documents:

```powershell
scripts/dev/search-reseed.ps1
```

Verify anonymous, Account, and CRM localized static search through the gateway:

```powershell
scripts/dev/search-verify.ps1
```

Create a CRM customer through the real CRM API and verify the dynamic neutral document appears for both `tr-TR` and `en-US`:

```powershell
scripts/dev/search-customer-fixture.ps1
```

Create, verify, soft-delete, and verify removal:

```powershell
scripts/dev/search-customer-fixture.ps1 -DeleteAfter
```

Create a CRM company through the real CRM API and verify the dynamic neutral document appears for both `tr-TR` and `en-US`:

```powershell
scripts/dev/search-company-fixture.ps1
```

Create, verify, soft-delete, and verify removal:

```powershell
scripts/dev/search-company-fixture.ps1 -DeleteAfter
```

Create a CRM contact through the real CRM API and verify the dynamic neutral document appears for both `tr-TR` and `en-US`:

```powershell
scripts/dev/search-contact-fixture.ps1
```

Contact fixtures must be linked to an existing Customer or Company. The script supports:

- `-CompanyId <guid>`: use an existing Company and do not delete it.
- `-CustomerId <guid>`: use an existing Customer and do not delete it.
- `-CreateLinkedCompany`: explicitly request temporary linked company creation.

If neither `-CompanyId` nor `-CustomerId` is provided, the script creates a temporary linked Company through the CRM API by default.

Create, verify, soft-delete, and verify removal:

```powershell
scripts/dev/search-contact-fixture.ps1 -DeleteAfter
```

Create a CRM deal through the real CRM API and verify the dynamic neutral document appears for both `tr-TR` and `en-US`:

```powershell
scripts/dev/search-deal-fixture.ps1
```

Deal fixture dependencies:

- No linked opportunity/company is required.
- The fixture uses the real `POST /api/deals` payload (`dealCode`, `name`, `totalAmount`, `closedDate`).
- The token needs `deals.manage` for create/delete and `deals.read` for authenticated search verification.

Create, verify, soft-delete, and verify removal:

```powershell
scripts/dev/search-deal-fixture.ps1 -DeleteAfter
```

Optional diagnostic control:

```powershell
scripts/dev/search-deal-fixture.ps1 -DeleteAfter -SkipOutboxDiagnostics
```

Create a CRM opportunity through the real CRM API and verify the dynamic neutral document appears for both `tr-TR` and `en-US`:

```powershell
scripts/dev/search-opportunity-fixture.ps1
```

Opportunity fixture payload (safe minimal runtime payload):

```json
{
  "opportunityCode": "OPP-<timestamp>",
  "name": "Search Neutral Opportunity <timestamp>",
  "description": null,
  "estimatedAmount": 0,
  "expectedRevenue": null,
  "probability": 0,
  "estimatedCloseDate": null,
  "stage": 0,
  "status": 0,
  "priority": 1,
  "leadId": null,
  "customerId": null,
  "ownerUserId": null,
  "notes": null
}
```

Opportunity search mapping is deliberately narrow:

- `title`: `Opportunity.Name`
- `summary`: `Opportunity.OpportunityCode`
- `content`: `Opportunity.Name` plus `Opportunity.OpportunityCode` only
- `locale`: `neutral`
- `visibility`: permission gated with `opportunities.read`

Descriptions, notes, lost notes, stage notes, reasons, comments, financial values, forecast values, and freeform customer/commercial/private text are not indexed.

Create, verify, soft-delete, and verify removal:

```powershell
scripts/dev/search-opportunity-fixture.ps1 -DeleteAfter
```

Create a CRM ticket through the real CRM API and verify the dynamic neutral document appears for both `tr-TR` and `en-US`:

```powershell
scripts/dev/search-ticket-fixture.ps1
```

Ticket fixture dependencies:

- No linked customer/company/contact is required.
- The fixture uses the real `POST /api/tickets` payload (`subject`; defaults for ticket type/channel/priority).
- The token needs `tickets.manage` for create/delete and `tickets.read` for authenticated search verification.

Create, verify, soft-delete, and verify removal:

```powershell
scripts/dev/search-ticket-fixture.ps1 -DeleteAfter
```

Optional diagnostic control:

```powershell
scripts/dev/search-ticket-fixture.ps1 -DeleteAfter -SkipOutboxDiagnostics
```

## What Is Verified

Static localized documents:

- Turkish search terms such as `müşteriler`, `kişiler`, `güvenlik`, `oturumlar`, and `destek talepleri`.
- English search terms such as `customers`, `contacts`, `security`, `sessions`, and `tickets`.
- Locale-suffixed static IDs such as `crm-module-customers-tr-TR` and `account-page-profile-en-US`.
- No legacy unsuffixed static IDs after reseed.
- No legacy URL prefixes such as `/crm/`, `/account/`, or `/tools/`.

Security:

- Anonymous users do not see CRM or Account documents.
- Public pricing remains searchable anonymously.
- Search responses do not expose `content`.

Authenticated localized search:

- Account Profile, Sessions, MFA in Turkish and English.
- CRM Customers, Contacts, and Tickets for a permissioned local token.

Dynamic neutral compatibility:

- A real CRM customer is created through `POST /api/v1/customers`.
- Search Worker indexes the customer through CRM outbox and RabbitMQ.
- The dynamic customer has `locale = neutral`.
- The customer is returned for both `locale=tr-TR` and `locale=en-US`.
- Anonymous search cannot see the dynamic customer.
- With `-DeleteAfter`, CRM soft-delete removes the search document.
- The same neutral-locale behavior is verified for Customer, Company, Contact, Deal, Opportunity, Ticket, Lead, and Pipeline fixtures.

Create a CRM lead through the real CRM API and verify the dynamic neutral document appears for both `tr-TR` and `en-US`:

```powershell
scripts/dev/search-lead-fixture.ps1
```

Lead fixture payload (safe minimal runtime payload):

```json
{
  "fullName": "Search Neutral Lead <timestamp>",
  "source": 0,
  "status": 0,
  "priority": 1
}
```

`source/status/priority` must be numeric enums in local runtime.

Create, verify, soft-delete, and verify removal:

```powershell
scripts/dev/search-lead-fixture.ps1 -DeleteAfter
```

Create a CRM pipeline through the real CRM API and verify the dynamic neutral document appears for both `tr-TR` and `en-US`:

```powershell
scripts/dev/search-pipeline-fixture.ps1
```

Pipeline fixture payload (safe minimal runtime payload):

```json
{
  "name": "Search Neutral Pipeline <timestamp>",
  "description": null,
  "isDefault": false,
  "displayOrder": 9000,
  "stages": [
    {
      "name": "Search Stage A",
      "description": null,
      "displayOrder": 1,
      "probability": 10,
      "isWinStage": false,
      "isLostStage": false
    },
    {
      "name": "Search Stage Won",
      "description": null,
      "displayOrder": 2,
      "probability": 100,
      "isWinStage": true,
      "isLostStage": false
    }
  ]
}
```

Create, verify, delete, and verify removal:

```powershell
scripts/dev/search-pipeline-fixture.ps1 -DeleteAfter
```

## Troubleshooting

Search Worker not running:

- Dynamic customer verification will time out or warn that the worker process was not detected.
- Restart the local stack with Search API selected:

```powershell
pnpm dev:up:nobuild
```

Stale CRM API binary:

- Symptom: a newly-created dynamic customer appears in Meilisearch with `locale = en` or URL `/crm/customers/...`.
- Fix: restart the local CRM API so it runs current code and emits `locale = neutral` plus app-local `/customers/{id}` URLs.

CustomerManagement outbox disabled:

- Symptom: customer creation succeeds, but Search Worker never receives a `search.index.crm` event.
- Fix: start the local stack with Search API selected; `scripts/dev-up.ps1` enables `CustomerManagement__Outbox__Enabled=true` for this search verification profile.

Ticket outbox not running:

- Symptom: ticket create succeeds, but ticket never appears in dynamic search verification.
- Check CRM API logs for `TicketManagement outbox processor started.`.
- If logs show `TicketManagement outbox processor is disabled`, set `TicketManagement__Outbox__Enabled=true` for local CRM API startup.

Ticket outbox table missing:

- Symptom: ticket create or outbox publish errors mention `TicketManagementOutboxMessages`.
- Fix: restart local CRM API in Development so schema bootstrap runs for `TicketManagementDbContext` and creates missing tables.
- If a stale binary was running, restart via `pnpm dev:up:nobuild` to pick up current module code.

Ticket outbox diagnostics interpretation:

- The ticket fixture now prints a read-only local snapshot with:
  - `tableExists`
  - `pendingCount`
  - `retryCount`
  - `deadLetterCount`
  - `processedCount`
  - `oldestPendingAgeSeconds`
  - `recentFailureCount`
- `tableExists=false`: run `pnpm dev:up:nobuild` to apply local dev DB guards.
- High `pendingCount`/`oldestPendingAgeSeconds`: outbox processor or Search Worker is likely not running.
- Non-zero `deadLetterCount` or `recentFailureCount`: inspect CRM API and Search Worker logs around `search.index.crm` and `search.delete.crm`.

Fixture retained after script run:

- Customer, company, and contact fixtures are retained by default for manual follow-up checks.
- Use `-DeleteAfter` to delete fixtures through CRM API and verify removal from search.

Delete verification failed:

- If create/search checks pass but delete removal fails, confirm Search Worker is running and consuming `search.delete.crm`.
- Retry the fixture script with a fresh token after verifying gateway/CRM/Search API health endpoints.

Ticket validation payload errors:

- Symptom: ticket fixture create returns `400`.
- Verify the payload still matches `TicketUpsertRequest` (at minimum `subject` required, max length 200).
- Re-run with a fresh unique subject and ensure the token has `tickets.manage`.

Deal outbox not running:

- Symptom: deal create succeeds, but deal never appears in search.
- Check CRM API logs for `DealManagement outbox processor started.`.
- If disabled, set `DealManagement__Outbox__Enabled=true` for local CRM API startup.

DealManagementOutboxMessages table missing:

- Symptom: deal outbox publish errors mention `DealManagementOutboxMessages`.
- Fix: restart local CRM API in Development and run `pnpm dev:up:nobuild` so schema bootstrap/dev guard can create the table.

Lead create returns `400` due to enum parsing:

- Symptom: `POST /api/leads` rejects payload when enum values are sent as strings.
- Fix: send numeric enums (`source`, `status`, `priority`) in fixture payload.

Lead create validation failure for missing full name:

- Symptom: lead create fails because `fullName` is missing or whitespace.
- Fix: send non-empty `fullName` in fixture payload.

Lead outbox not running:

- Symptom: lead create succeeds, but lead never appears in search.
- Check CRM API logs for `LeadManagement outbox processor started.`.
- If disabled, set `LeadManagement__Outbox__Enabled=true` for local CRM API startup.

LeadManagementOutboxMessages table missing:

- Symptom: lead outbox publish errors mention `LeadManagementOutboxMessages`.
- Fix: restart local CRM API in Development and run `pnpm dev:up:nobuild` so schema bootstrap/dev guard can create the table.

Pipeline outbox not running:

- Symptom: pipeline create succeeds, but pipeline never appears in search.
- Check CRM API logs for `PipelineManagement outbox processor started.`.
- If disabled, set `PipelineManagement__Outbox__Enabled=true` for local CRM API startup.

PipelineManagementOutboxMessages table missing:

- Symptom: pipeline outbox publish errors mention `PipelineManagementOutboxMessages`.
- Fix: restart local CRM API in Development and run `pnpm dev:up:nobuild` so schema bootstrap/dev guard can create the table.

Opportunity outbox not running:

- Symptom: opportunity create succeeds, but opportunity never appears in search.
- Check CRM API logs for `OpportunityManagement outbox processor started.`.
- If disabled, set `OpportunityManagement__Outbox__Enabled=true` for local CRM API startup.

OpportunityManagementOutboxMessages table missing:

- Symptom: opportunity create or outbox publish errors mention `OpportunityManagementOutboxMessages`.
- Fix: restart local CRM API in Development and run `pnpm dev:up:nobuild` so schema bootstrap/dev guard can create the table.

Legacy Opportunity rows:

- Historical PipelineManagement or LeadManagement local Opportunity rows can remain from before the ownership refactor.
- OpportunityManagement is authoritative for new cross-module Opportunity writes from Phase 14A onward.
- No legacy migration or backfill is included in the local search verification flow.

Missing auth token:

- Symptom: authenticated checks fail before querying search.
- Fix:

```powershell
$env:NETMETRIC_DEV_SEED_PASSWORD = "<local-dev-password>"
scripts/dev/auth-token.ps1
```

Expired auth token:

- Symptom: authenticated checks return no Account/CRM documents even though static docs exist.
- Fix: rerun `scripts/dev/auth-token.ps1`.

Gateway or `/api/search` returns `502`:

- Confirm gateway and Search API readiness:

```powershell
Invoke-WebRequest http://localhost:5030/health/ready
Invoke-WebRequest http://localhost:5310/health/ready
```

Meilisearch index empty or old IDs remain:

- Rerun the reseed script:

```powershell
scripts/dev/search-reseed.ps1
```

Retained dynamic fixtures:

- `scripts/dev/search-customer-fixture.ps1` keeps the fixture by default so it can be used for follow-up checks.
- Use `-DeleteAfter` when you want the fixture removed through the CRM API and verified absent from search.
