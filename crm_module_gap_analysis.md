# NetMetric CRM Module Gap Analysis

Static source analysis date: 2026-05-23. Scope: `apps/crm-web`, `services/crm/src/NetMetric.CRM.API/Controllers`, `services/crm/src/modules`, and `docs/crm/crm-module-endpoint-map.md`. No runtime/browser/API smoke was executed; runtime-only conclusions are marked as needs verification.

## 1. Executive Summary

- NetMetric CRM has a very broad backend surface: Customer/Company/Contact, Lead, Deal, Opportunity, Pipeline, Quote, Product Catalog, Ticket/SLA/Workflow, Support Inbox, Sales Forecasting, Marketing, Omnichannel, Calendar Sync, Contracts, Documents, Finance, Integrations, Knowledge Base, Workflow Automation, Analytics, AI, Tags, Tenants, and Work Management all have controller and/or module evidence.
- crm-web has two maturity tiers. Core modules have dedicated pages/forms/actions. Advanced modules mostly use `renderCrmModuleShell` + `OperationalModuleWorkspace`, which is useful endpoint discovery, but not a final CRM workflow UI.
- The most critical gaps are not backend absence; they are API-to-web parity, inconsistent action/list UX, and unfinished domain-specific screens.
- Strongest areas: Customers, Leads, Deals, Opportunities, Quotes, Product Catalog, Tickets, Pipeline, Ticket SLA/Workflow. Each is still partial when judged against modern CRM expectations.
- Highest risk gaps: Activities has a frontend route but no dedicated controller; Tasks is create/read-only without edit/complete/delete/detail; Product Catalog backend has bulk/export/template/image operations not exposed in UI; many active registry modules are generic previews.
- Permissions/tenant integration exists on both sides, but the frontend action layer is uneven. Backend controllers use `[Authorize(Policy = ...)]`; API has `RequireTenantContextMiddleware`, `TenantRouteGuardFilter`, tenant query filters and interceptors. Frontend has capability aliases and path gating, but many server actions only call `requireCrmSession(path)` and rely on path/backend checks rather than explicit per-action capability checks.
- Import/export/bulk is not standardized. Customers have import batches and frontend row selection bulk delete; Leads/Deals/Opportunities have raw bulk operation pages; Product Catalog has backend bulk/export/template endpoints but no UI; Companies/Contacts have import/bulk command evidence in backend application but no visible controller/UI.
- Modern CRM missing spine: one cross-module activity timeline with tasks, notes, emails, calls, meetings, tickets, conversations, documents, audit, and automations.

## 2. Module Inventory

| Module                  | Backend Exists            | Frontend Exists           | CRUD Status                               | Detail Page              | Import/Export                                   | Bulk Actions                                   | Search/Filter    | Status                        |
| ----------------------- | ------------------------- | ------------------------- | ----------------------------------------- | ------------------------ | ----------------------------------------------- | ---------------------------------------------- | ---------------- | ----------------------------- |
| Dashboard               | Yes                       | Yes                       | Readonly                                  | No                       | No                                              | No                                             | No               | Partial                       |
| Customers               | Yes                       | Yes                       | CRUD + actions                            | Yes                      | Import UI; export missing                       | UI bulk delete loops deletes                   | Yes              | Partial                       |
| Companies               | Yes                       | Yes                       | CRUD + logo + activate/deactivate         | Yes                      | Backend import command; no UI/API route visible | Backend command; no UI/API route visible       | Yes              | Partial                       |
| Contacts                | Yes                       | Yes                       | CRUD + set primary                        | Yes                      | Backend import command; no UI/API route visible | Backend command; no UI/API route visible       | Yes              | Partial                       |
| Addresses               | Yes                       | Embedded                  | CRUD-ish embedded                         | Embedded                 | No                                              | No                                             | No               | Partial                       |
| Customer Intelligence   | Yes                       | Yes                       | Read + mutation panels                    | Embedded + generic       | No                                              | Merge/detect actions, no queue UX              | Limited          | Risky/Inconsistent            |
| Leads                   | Yes                       | Yes                       | CRUD + lifecycle                          | Yes                      | No                                              | Bulk assign/delete                             | Yes              | Partial                       |
| Deals                   | Yes                       | Yes                       | CRUD + win/loss/reopen/review             | Yes                      | No                                              | Bulk owner assign                              | Yes              | Partial                       |
| Opportunities           | Yes                       | Yes                       | CRUD + stage/win/loss/relations/quotes    | Yes                      | No                                              | Bulk owner/stage                               | Yes              | Partial                       |
| Pipeline                | Yes                       | Yes                       | Pipeline/lost reason operations           | Board page               | No                                              | Stage move                                     | Limited          | Partial                       |
| Quotes                  | Yes                       | Yes                       | CRUD + lifecycle + CPQ/proposals          | Yes                      | Proposal templates; no document export          | No                                             | Yes              | Partial                       |
| Product Catalog         | Yes                       | Yes                       | Product/category CRUD                     | Yes                      | Backend export/template; UI missing             | Backend bulk; UI missing                       | Yes              | Risky/Inconsistent            |
| Tickets                 | Yes                       | Yes                       | CRUD                                      | Yes                      | No                                              | Backend module commands; controller/UI missing | Yes              | Partial                       |
| Ticket SLA              | Yes                       | Yes                       | Policy/rule/actions partial               | Embedded + SLA page      | No                                              | Run due escalations                            | ID filters       | Partial                       |
| Ticket Workflow         | Yes                       | Yes                       | Queue CRUD + ticket actions               | Embedded + workflow page | No                                              | Assign/status actions                          | ID filters       | Partial                       |
| Support Inbox           | Yes                       | Yes                       | Read + operations                         | No message detail        | No                                              | Sync/rule ops                                  | Basic            | Partial                       |
| Tasks / Work Management | Yes                       | Yes                       | Create/read only                          | No                       | No                                              | No                                             | Workspace tables | Partial                       |
| Activities              | Entity/workspace evidence | Generic route             | Readonly preview                          | No                       | No                                              | No                                             | Generic          | Frontend-only/Risky           |
| Sales Forecasting       | Yes                       | Generic shell             | Backend read/write, no real UI            | Generic op pages         | Snapshots backend                               | No                                             | Generic          | Backend-only/Partial Frontend |
| Marketing               | Yes                       | Generic shell             | Backend rich, no real UI                  | Generic op pages         | No                                              | No                                             | Generic          | Backend-only/Partial Frontend |
| Omnichannel             | Yes                       | Generic shell             | Backend rich, no inbox UI                 | Generic op pages         | No                                              | Backend-only conversation actions              | Generic          | Backend-only/Partial Frontend |
| Calendar Sync           | Yes                       | Generic shell             | Backend basic, no connection UI           | Generic                  | No                                              | Sync backend-only                              | Generic          | Backend-only/Partial Frontend |
| Contracts               | Yes                       | Generic shell             | List/detail/create backend                | Generic                  | No                                              | No                                             | Generic          | Backend-only/Partial Frontend |
| Documents               | Yes                       | Generic shell             | List/detail/create/version/review backend | Generic                  | No                                              | No                                             | Generic          | Backend-only/Partial Frontend |
| Finance                 | Yes                       | Generic shell             | Orders list/detail/create backend         | Generic                  | No                                              | No                                             | Generic          | Backend-only/Partial Frontend |
| Integration Hub         | Yes                       | Generic shell             | Extensive backend, no admin UI            | Generic                  | No                                              | Job ops backend-only                           | Generic          | Backend-only/Partial Frontend |
| Knowledge Base          | Yes                       | Generic shell             | Article/category backend, no editor UI    | Generic                  | No                                              | Publish/archive backend-only                   | Generic          | Backend-only/Partial Frontend |
| Workflow Automation     | Yes                       | Generic shell             | Rules/executions backend, no builder UI   | Generic                  | No                                              | Retry/evaluate backend-only                    | Generic          | Backend-only/Partial Frontend |
| Analytics               | Yes                       | Dashboard + generic shell | Read APIs                                 | Generic                  | No                                              | No                                             | Generic          | Partial                       |
| AI                      | Yes                       | Generic shell             | Workspace/provider backend                | Generic                  | No                                              | No                                             | Generic          | Backend-only/Partial Frontend |
| Tags                    | Yes                       | Generic shell             | List/create only                          | Generic                  | No                                              | No                                             | Generic          | Partial                       |
| Tenants/Settings        | Yes / settings unclear    | Generic + coming soon     | Tenant backend, settings missing          | Generic                  | No                                              | No                                             | Generic          | Risky/Inconsistent            |

## 3. Module-by-Module Findings

### Module: Customers

#### Current Backend Coverage

- `services/crm/src/NetMetric.CRM.API/Controllers/CustomerManagement/CustomersController.cs` exposes list/detail, contacts, 360, consents, hierarchy, duplicate detection/merge, audit timeline, shares, stakeholders, search, import-batches, create/update/image/VIP/delete.
- CustomerManagement module has import wizard, bulk command handlers, data quality, duplicate, audit, search document, and tenant-aware services.

#### Current Frontend Coverage

- Routes: `apps/crm-web/src/app/customers/page.tsx`, `new/page.tsx`, `[id]/page.tsx`, `[id]/edit/page.tsx`, `imports/page.tsx`, `imports/new/page.tsx`.
- Components/actions: `features/customers/components/customers-list-table.tsx`, `customer-detail-workspace.tsx`, `customer-import-panel.tsx`, `customer-imports-list-table.tsx`, `actions/customer-mutation-actions.ts`.

#### Working Features

- CRUD, list search/sort/page-size/pagination, delete, image upload/remove, VIP, address management, customer 360, consents, hierarchy, duplicate warnings/merge, audit timeline, import-batch lifecycle.

#### Missing / Incomplete Features

- Export is implied by `canExportCustomer` capability but no visible controller/UI endpoint was found.
- Bulk delete UI loops single `deleteCustomer` calls; backend bulk command handlers exist, but no visible `CustomersController` bulk route was found.
- Import appears row/form based; CSV/XLSX upload, template download, mapping wizard, and validation UX need verification.
- Saved views, dedupe queue, advanced segmentation, engagement timeline, email/call logging, ownership routing, and bulk field update are absent or partial.

#### Readonly / Placeholder Areas

- Advanced detail panels tolerate partial API failures with `Promise.allSettled`; section-level failure visibility needs verification.

#### API Exists but Web Missing

- Customer search/share revoke/stakeholder remove and some intelligence features are API/client-visible but not obviously productized as full workflows.

#### Web Exists but API Missing

- Export capability has no visible backend pair. Bulk UI lacks a visible backend bulk route.

#### Modern CRM Expectations Missing

- Unified timeline, saved list views, dedupe queue, merge conflict resolver, account/contact relationship graph, enrichment, GDPR/privacy center.

#### Risk Level

- Medium

#### Recommended Next Steps

- Add customer export or remove capability. Add controller bulk endpoints or reframe UI as per-record deletes. Productize dedupe/import/saved views.

### Module: Companies

#### Current Backend Coverage

- `CompaniesController.cs`: list/detail/create/update/logo upload/logo delete/activate/deactivate/delete.
- CustomerManagement application includes company import and bulk command handlers.

#### Current Frontend Coverage

- Routes: `companies/page.tsx`, `new/page.tsx`, `[id]/page.tsx`, `[id]/edit/page.tsx`.
- Files: `features/companies/data/companies-data.ts`, `actions/company-mutation-actions.ts`, `forms/company-form.tsx`, `components/company-detail-actions.tsx`.

#### Working Features

- CRUD, logo, activate/deactivate, address management, list search/filter/sort/page.

#### Missing / Incomplete Features

- No import/export/bulk UI or visible controller routes for existing backend command handlers.
- No company detail related records: contacts, customers, opportunities, deals, tickets, documents, activities.
- No hierarchy/account tree UI except separate customer intelligence hierarchy functions.

#### Readonly / Placeholder Areas

- Detail is mostly readonly fields plus edit/action panels.

#### API Exists but Web Missing

- Backend import/bulk command handlers appear unexposed to crm-web.

#### Web Exists but API Missing

- No clear frontend-only feature found.

#### Modern CRM Expectations Missing

- Account hierarchy, associated records, activity timeline, account health, duplicate merge, territories.

#### Risk Level

- Medium

#### Recommended Next Steps

- Add related records/timeline, company import/export, and bulk owner/status actions.

### Module: Contacts

#### Current Backend Coverage

- `ContactsController.cs`: list/detail/create/update/set-primary/delete under `api/contacts` and `api/v1/contacts`.
- CustomerManagement application includes contact import/bulk command handlers.

#### Current Frontend Coverage

- Routes: `contacts/page.tsx`, `new/page.tsx`, `[id]/page.tsx`, `[id]/edit/page.tsx`.
- Files: `features/contacts/data/contacts-data.ts`, `actions/contact-mutation-actions.ts`, `forms/contact-form.tsx`, `components/contact-detail-actions.tsx`.

#### Working Features

- CRUD, set primary, list/detail/search/filter/sort/page.

#### Missing / Incomplete Features

- No contact import/export/bulk UI.
- No contact activity timeline, email/call log, sequences, consent/subscriptions, duplicate merge.

#### Readonly / Placeholder Areas

- Detail profile is readonly except set-primary/delete/edit.

#### API Exists but Web Missing

- Backend import/bulk handlers are not visible through controller/UI.

#### Web Exists but API Missing

- No major frontend-only contact feature found.

#### Modern CRM Expectations Missing

- Timeline, email/calendar sync, consent center, enrichment, relationship roles.

#### Risk Level

- Medium

#### Recommended Next Steps

- Add timeline/related records/import-export/bulk and consent view.

### Module: Addresses

#### Current Backend Coverage

- `AddressesController.cs`: add to company/customer, update, set-default, delete.

#### Current Frontend Coverage

- Embedded shared UI: `components/address/address-section.tsx`, `address-list.tsx`, `address-form.tsx`, `features/addresses/actions/address-mutation-actions.ts`.

#### Working Features

- Address create/update/delete/set-default on customer/company detail.

#### Missing / Incomplete Features

- No validation/geocoding, billing/shipping semantics, territory routing, standalone address management.

#### Risk Level

- Low

#### Recommended Next Steps

- Keep embedded model but add validation and billing/shipping/default semantics.

### Module: Customer Intelligence

#### Current Backend Coverage

- `CustomerIntelligenceController.cs`: workspace, portal summary, duplicate detect, merges, saved views, append activity, relationships, CDP events, identity resolution.
- Customer 360/consents/hierarchy/duplicates/audit also exposed through `CustomersController.cs`.

#### Current Frontend Coverage

- `customer-intelligence/page.tsx`, `[operationId]/page.tsx`, `features/customer-intelligence/components/customer-intelligence-dashboard.tsx`, `customer-intelligence-mutation-panels.tsx`.
- Customer detail embeds `CustomerDetailWorkspace` with 360/intelligence sections.

#### Working Features

- Customer detail consumes 360, consents, hierarchy, audit, contacts, duplicates.
- Dedicated intelligence page has operational data plus mutation panels.

#### Missing / Incomplete Features

- No dedupe queue, health dashboard, identity graph, relationship graph, saved-view manager, CDP event explorer, merge conflict resolver.
- Mutation panels appear raw/manual-ID style; usability needs verification.

#### Readonly / Placeholder Areas

- Generic operation shell and partial preview pages.

#### API Exists but Web Missing

- Saved views, identity resolution, CDP tracking, relationship graph are not productized.

#### Web Exists but API Missing

- Some write actions check broad `customerIntelligence.read` capability in frontend action code; backend policies are more specific.

#### Modern CRM Expectations Missing

- AI insights, scoring explainability, health trends, duplicate queue, merge wizard, account hierarchy graph.

#### Risk Level

- High

#### Recommended Next Steps

- Build real intelligence workbench: duplicates, health risks, identity resolution, relationship graph. Split read/manage capabilities.

### Module: Leads

#### Current Backend Coverage

- `LeadsController.cs`: list/detail/workspace/timeline/create/update/owner/status/next-contact/score/qualification/capture/convert/bulk owner/bulk delete/delete.

#### Current Frontend Coverage

- Routes: `leads/page.tsx`, `new`, `[id]`, `[id]/edit`, `bulk-operations`.
- Files: `features/leads/data/leads-data.ts`, `actions/lead-mutation-actions.ts`, `components/lead-detail-workspace.tsx`, `lead-bulk-actions-panel.tsx`, `forms/lead-form.tsx`.

#### Working Features

- CRUD, status/owner/next-contact, score/qualification, convert, timeline/workspace, bulk assign/delete.

#### Missing / Incomplete Features

- No import/export, capture form builder, dedupe, routing UI, sequences, nurture automation.
- Bulk page uses raw IDs rather than list selection.

#### API Exists but Web Missing

- `POST /api/leads/capture` has no crm-web builder/admin UI.

#### Modern CRM Expectations Missing

- Lead inbox, source attribution, lead scoring explanations, qualification playbooks, conversion wizard.

#### Risk Level

- Medium

#### Recommended Next Steps

- Add selection bulk, import/export, capture-form management, guided conversion.

### Module: Deals

#### Current Backend Coverage

- `DealsController.cs`: list/detail/workspace/timeline/create/update/owner/bulk owner/won/lost/reopen/delete.
- `DealWinLossController.cs`: summary, lost reasons, review.

#### Current Frontend Coverage

- Routes: `deals/page.tsx`, `new`, `[id]`, `[id]/edit`, `bulk-operations`.
- Files: `features/deals/data/deals-data.ts`, `actions/deal-mutation-actions.ts`, `deal-lifecycle-actions.ts`, `components/deal-detail-workspace.tsx`, `deal-bulk-actions-panel.tsx`.

#### Working Features

- CRUD, lifecycle, workspace/timeline, owner assign, win/loss review, bulk owner.

#### Missing / Incomplete Features

- No import/export, deal kanban, product/quote association UX, list selection bulk, competitor/playbook UI.

#### API Exists but Web Missing

- Win-loss summary is API/client-visible but not a dedicated analytics UI.

#### Modern CRM Expectations Missing

- Drag/drop pipeline, stage automation, buying committee, forecast category, deal room, competitor/loss insights.

#### Risk Level

- Medium

#### Recommended Next Steps

- Unify Deal vs Opportunity pipeline story. Add kanban/list bulk/win-loss dashboard/import-export.

### Module: Opportunities

#### Current Backend Coverage

- `OpportunitiesController.cs`: list/detail/workspace/timeline/pipeline-board/lost-reasons/create/update/owner/stage/won/lost/add contacts/add products/quotes/create quote/bulk owner/bulk stage/delete.

#### Current Frontend Coverage

- Routes: `opportunities/page.tsx`, `new`, `[id]`, `[id]/edit`, `bulk-operations`.
- Files: `features/opportunities/data/opportunities-data.ts`, `actions/opportunity-mutation-actions.ts`, `opportunity-lifecycle-actions.ts`, `components/opportunity-detail-workspace.tsx`, `opportunity-bulk-actions-panel.tsx`.

#### Working Features

- CRUD, stage/owner/won/lost, product/contact/quote relations, stage history, bulk owner/stage.

#### Missing / Incomplete Features

- No import/export, selection-driven bulk, integrated kanban on list, guided lookup selectors for related records.

#### Modern CRM Expectations Missing

- Buying committee, product line editor, mutual action plan, stage gates, forecast integration.

#### Risk Level

- Medium

#### Recommended Next Steps

- Make `/pipeline` first-class opportunity board. Replace GUID inputs with selectors. Add import/export.

### Module: Pipeline

#### Current Backend Coverage

- `PipelinesController.cs`, `PipelineOpportunitiesController.cs`, `LostReasonsController.cs`, `LeadConversionsController.cs` cover pipelines, board/analytics, stage history/move, lost reasons, lead conversion preview/convert.

#### Current Frontend Coverage

- `pipeline/page.tsx`, `features/pipeline/data/pipeline-data.ts`, `pipeline-board.tsx`, `pipeline-operations-panel.tsx`, `pipeline-stage-move-form.tsx`, pipeline actions.

#### Working Features

- Pipeline board/read, pipeline create/update/delete, lost reason create/update, opportunity stage move.

#### Missing / Incomplete Features

- No pipeline detail/config routes, no drag/drop, no stage probability/required fields/automation builder, no lost reason delete.

#### Modern CRM Expectations Missing

- Visual pipeline designer, drag/drop kanban, forecast category config, conversion funnel analytics.

#### Risk Level

- Medium

#### Recommended Next Steps

- Add pipeline config pages and drag/drop stage movement.

### Module: Quotes

#### Current Backend Coverage

- `QuotesController.cs`: list/detail/workspace/timeline/validation/CPQ workspace/create/update/submit/approve/reject/sent/accepted/declined/expired/revisions/delete/proposal templates/guided selling/playbooks/bundles/rules.

#### Current Frontend Coverage

- Routes: `quotes/page.tsx`, `new`, `[id]`, `[id]/edit`.
- Files: `features/quotes/data/quotes-data.ts`, `actions/quote-mutation-actions.ts`, `quote-lifecycle-actions.ts`, `quote-cpq-actions.ts`, `components/quote-detail-workspace.tsx`, `quote-guided-selling-panel.tsx`, `forms/quote-form.tsx`.

#### Working Features

- CRUD, lifecycle, revisions, workspace/timeline/validation, CPQ/guided selling, proposal template actions.

#### Missing / Incomplete Features

- No PDF/proposal generation, e-signature, approval inbox, dedicated proposal template admin, full CPQ admin UI, bulk actions.

#### Modern CRM Expectations Missing

- Document rendering, version diff, pricing/discount guardrails, product configurator, approval queue, e-sign.

#### Risk Level

- Medium

#### Recommended Next Steps

- Add quote document generation and CPQ admin pages.

### Module: Product Catalog

#### Current Backend Coverage

- `CatalogProductsController.cs`: product CRUD, patch, active state, bulk create/update/delete/active-state, export, template, meta, stats, lookups, images, primary image, image delete.
- `CatalogCategoriesController.cs`: category CRUD, active state, image upload/delete.

#### Current Frontend Coverage

- Routes: `product-catalog/page.tsx`, `new`, `[id]`, `[id]/edit`, `categories/page.tsx`, `categories/new`, `categories/[id]`, `categories/[id]/edit`.
- Files: `features/product-catalog/data/product-catalog-data.ts`, data tables, forms, actions.

#### Working Features

- Product/category list/detail/create/edit/delete, active state, lookups, images upload evidence.

#### Missing / Incomplete Features

- Backend bulk/export/template/meta/stats/primary-image/delete-image endpoints are not fully surfaced in UI.
- No import UI; backend has export/template but no explicit import endpoint.
- No price books, variants, bundles UI, multi-currency price list management.

#### API Exists but Web Missing

- `/api/catalog/products/bulk`, `/export`, `/template`, `/meta`, `/stats`, image primary/delete.

#### Risk Level

- High

#### Recommended Next Steps

- Wire bulk/export/template/meta/stats/image-management. Add import or bulk upload flow.

### Module: Tickets

#### Current Backend Coverage

- `TicketsController.cs`: list/detail/create/update/delete.
- TicketManagement module contains comments, categories, timeline, bulk assign/delete handlers, outbox and tenant filters.

#### Current Frontend Coverage

- Routes: `tickets/page.tsx`, `new`, `[id]`, `[id]/edit`.
- Files: `features/tickets/data/tickets-data.ts`, `actions/ticket-mutation-actions.ts`, `components/ticket-detail-workspace.tsx`, `ticket-detail-action-panels.tsx`, `forms/ticket-form.tsx`.

#### Working Features

- Ticket CRUD plus embedded SLA/workflow data/actions on detail.

#### Missing / Incomplete Features

- Comment/reply/thread, categories, timeline, bulk actions are not controller/UI-visible despite backend module evidence.
- No import/export, macros, attachments, CSAT, conversation merge/split.

#### Risk Level

- Medium

#### Recommended Next Steps

- Surface ticket comments/timeline/category/bulk endpoints and build support-agent workspace.

### Module: Ticket SLA

#### Current Backend Coverage

- `TicketSlaController.cs`: policy list/create/update/delete, escalation rules list/create/update, ticket SLA workspace, escalation runs, attach, first response, resolved, run due escalations.

#### Current Frontend Coverage

- Routes: `ticket-sla/page.tsx`, `operations/page.tsx`, `[operationId]`.
- Files: `features/ticket-sla/data/ticket-sla-data.ts`, `forms/ticket-sla-mutation-panels.tsx`, `actions/ticket-sla-mutation-actions.ts`.

#### Working Features

- Policies/rules reads and mutation panels, ticket SLA embed on detail, attach/first-response/resolved/run-due.

#### Missing / Incomplete Features

- No escalation-rule delete endpoint/UI, no SLA timer/breach dashboard, raw ID filters/selectors.

#### Risk Level

- Medium

#### Recommended Next Steps

- Add selectors, SLA dashboard, and decide rule delete lifecycle.

### Module: Ticket Workflow

#### Current Backend Coverage

- `TicketWorkflowController.cs`: queues list/create/update/delete, assignment/status history, assign queue/owner, record status change.

#### Current Frontend Coverage

- Routes: `ticket-workflows/page.tsx`, `operations/page.tsx`, `[operationId]`.
- Files: `features/ticket-workflows/data/*.ts`, `forms/ticket-workflow-mutation-panels.tsx`, `actions/ticket-workflow-mutation-actions.ts`.

#### Working Features

- Queue CRUD, assignment/status actions, ticket detail embeds histories/actions.

#### Missing / Incomplete Features

- No queue board, ticket list by queue, routing rules, assignment balancing, transition designer.

#### Risk Level

- Medium

#### Recommended Next Steps

- Build queue board and workflow rules UI.

### Module: Support Inbox

#### Current Backend Coverage

- `SupportInboxController.cs`: connections list/create/update/sync, messages list, rules create/update.

#### Current Frontend Coverage

- Routes: `support-inbox/page.tsx`, `operations/page.tsx`, `[operationId]`.
- Files: `features/support-inbox/data/support-inbox-data.ts`, filter form, mutation panels/actions.

#### Working Features

- Read connections/messages, basic filters/pagination, operation forms.

#### Missing / Incomplete Features

- No message detail/thread, reply, assignment, convert/link workflow, rule list/read/delete, full connection admin UX.

#### Risk Level

- Medium

#### Recommended Next Steps

- Build unified inbox conversation UI and align with Omnichannel.

### Module: Tasks / Work Management / Activities

#### Current Backend Coverage

- `WorkManagementController.cs`: workspace read, create task, schedule meeting.
- WorkManagement module includes WorkTask, ActivityLog, MeetingSchedule.

#### Current Frontend Coverage

- `tasks/page.tsx`, `tasks/new/page.tsx`, `tasks/meetings/new/page.tsx`, `work-management/page.tsx`, `activities/page.tsx`.
- Files: `features/tasks/data/tasks-data.ts`, `components/tasks-workspace.tsx`, `actions/work-management-create-actions.ts`.

#### Working Features

- Workspace tables, create task, schedule meeting.

#### Missing / Incomplete Features

- No task detail/edit/delete/complete, no activity controller/list/detail/create, no cross-record timeline.

#### Web Exists but API Missing

- `/activities` route exists but no dedicated `ActivitiesController` was found.

#### Modern CRM Expectations Missing

- Calls/emails/notes/meetings/tasks unified timeline, reminders, recurring tasks, completion states.

#### Risk Level

- High

#### Recommended Next Steps

- Make Activity/Task a first-class API and UI spine.

### Module: Generic Operational Modules

Covers Sales Forecasting, Marketing, Omnichannel, Calendar Sync, Contracts, Documents, Finance, Integration Hub, Knowledge Base, Workflow Automation, Analytics, AI, Tags, Tenants.

#### Current Backend Coverage

- Controllers exist for these modules except settings-specific backend is unclear. Evidence includes `SalesForecastsController.cs`, `CampaignsController.cs`, `OmnichannelController.cs`, `CalendarSyncController.cs`, `ContractsController.cs`, `DocumentsController.cs`, `OrdersController.cs`, `IntegrationHubController.cs`, `KnowledgeBaseArticlesController.cs`, `WorkflowAutomationController.cs`, `AnalyticsController.cs`, `ArtificialIntelligenceController.cs`, `TagsController.cs`, `TenantsController.cs`.

#### Current Frontend Coverage

- Routes mostly call `renderCrmModuleShell(path)` and `[operationId]` pages call `renderCrmOperationShell(path, operationId)`.
- `OperationalModuleWorkspace` renders endpoint previews, skipped/failed/empty states, and quick-create panels.

#### Working Features

- Authenticated endpoint preview, generic operation pages, limited quick-create/mutation forms for selected modules.

#### Missing / Incomplete Features

- Domain-specific list/detail/forms/tables are generally missing.
- Many write endpoints are backend-only or generic-form-only.
- Import/export/bulk/action UX is absent.
- Tenant route placeholder resolution needs runtime verification for tenant-scoped endpoints.

#### Module-Specific High-Value Gaps

- Sales Forecasting: no quota grid, commit forecast, adjustments/snapshots UI, rollups.
- Marketing: no campaign builder, segment builder, template editor, journey canvas.
- Omnichannel: no conversation inbox/thread/reply/link UI.
- Calendar Sync: no provider connection/OAuth/sync status UI.
- Contracts: no lifecycle, renewal, approval, e-sign UI.
- Documents: no library/upload/version/review/attachment UI.
- Finance: no order/invoice/payment/subscription UI.
- Integration Hub: no provider credential, jobs/dead-letter, API keys/webhooks admin.
- Knowledge Base: no article editor, category management, publish/archive UI.
- Workflow Automation: no rule builder/execution monitor/approval inbox.
- Analytics: dashboard not wired to analytics endpoints.
- AI: no provider settings or insight review UI.
- Tags: no edit/delete/assignment endpoints/UI; no record tag picker.
- Tenants/Settings: generic tenant preview only; settings route is coming soon.

#### Risk Level

- Medium to High

#### Recommended Next Steps

- Keep generic shell as discovery/admin preview, but productize modules in priority order: Sales Forecasting, Omnichannel/Support Inbox, Integration Hub, Workflow Automation, Analytics, Marketing, Knowledge Base, Documents/Contracts.

## 4. Cross-Module Problems

- Repeated table/list implementations: `CrmEntityListPage`, `CrmRecordsTable`, `CrmDataTableAdapter`, `CustomersListTable`, Product Catalog tables, and operational preview tables overlap but do not share one behavior contract.
- Search/filter/sort/pagination inconsistency: some modules server-page, some client-filter current page rows, some generic-preview payloads only. Advanced filters and saved views are missing.
- Bulk action standard missing: raw `/bulk-operations` pages for Leads/Deals/Opportunities, row selection only in Customers, backend-only bulk in Product Catalog, backend module-only bulk in Tickets/CustomerManagement.
- Import/export standard missing: Customers import batches exist; Product Catalog export/template backend is unbound; Companies/Contacts import handlers are not controller/UI-visible; most modules lack both.
- Permission standard incomplete: backend policies are strong; frontend path capabilities are useful, but mutation actions should explicitly check create/edit/delete/manage capabilities instead of relying only on path session and backend 403.
- Tenant model is mixed: some endpoints infer tenant from auth context; others use `tenants/{tenantId}` path params. Operational shell placeholder substitution must be verified with real sessions.
- Error/loading/empty states are not uniform: entity pages, product fallback, operational skipped/failed, and partial `Promise.allSettled` detail panels behave differently.
- Relationship UX is weak: many actions appear to require raw GUIDs instead of search-select controls for owners, products, contacts, tickets, customers, policies, queues.
- Audit/search/outbox is backend-rich but frontend-thin: Customers expose audit timeline; most modules do not.
- Generic shell status can mislead: module registry marks many modules `active`, while product UX is only endpoint preview.
- Modern CRM spine missing: no universal activity/timeline model across customers, companies, contacts, leads, deals, opportunities, quotes, tickets, documents, conversations, tasks, and notes.

## 5. Backend vs Frontend Gap Matrix

| Feature / Module          | Backend                         | Frontend            | Gap                                                 | Priority |
| ------------------------- | ------------------------------- | ------------------- | --------------------------------------------------- | -------- |
| Customers CRUD/360/import | Strong                          | Strong partial      | Export, bulk route, saved views, dedupe queue       | P1       |
| Companies                 | Strong CRUD                     | Basic CRUD          | Import/export/bulk, related records                 | P1       |
| Contacts                  | Strong CRUD                     | Basic CRUD          | Import/export/bulk, timeline/consent                | P1       |
| Leads                     | Strong lifecycle                | Good partial        | Import/export, capture UI, guided conversion        | P1       |
| Deals                     | Strong lifecycle                | Good partial        | Kanban, import/export, selection bulk               | P1       |
| Opportunities             | Strong lifecycle                | Good partial        | Kanban integration, lookup selectors, import/export | P1       |
| Pipeline                  | Strong                          | Partial             | Drag/drop, config/detail pages                      | P1       |
| Quotes/CPQ                | Strong                          | Partial             | PDF/e-sign, CPQ admin, approval queue               | P1       |
| Product Catalog           | Strong incl. bulk/export        | CRUD partial        | Bulk/export/template/image parity missing           | P0/P1    |
| Tickets                   | Basic controller, richer module | CRUD partial        | Comments/timeline/bulk/category API/UI              | P0/P1    |
| SLA/Workflow              | Strong                          | Partial             | Better agent UX, selectors, dashboards              | P2       |
| Support Inbox             | Partial strong                  | Partial             | Conversation detail/reply/convert/rules list        | P1       |
| Tasks/Activities          | Basic/missing                   | Partial/generic     | Detail/edit/complete/activity API missing           | P0       |
| Sales Forecasting         | Strong                          | Generic             | Forecast dashboard/forms missing                    | P1       |
| Marketing                 | Strong                          | Generic             | Campaign/segment/template/journey UI missing        | P2       |
| Omnichannel               | Strong                          | Generic             | Inbox/conversation UI missing                       | P1       |
| Integration Hub           | Very strong                     | Generic             | Admin UI missing                                    | P1       |
| Workflow Automation       | Strong                          | Generic             | Builder/execution monitor missing                   | P1       |
| Analytics                 | Strong read APIs                | Dashboard not wired | Real analytics dashboard missing                    | P1       |
| Tags                      | Partial backend                 | Generic             | Assignment/edit/delete missing                      | P1       |
| Tenants/Settings          | Tenant backend                  | Generic/coming soon | Settings UI missing                                 | P0/P1    |

## 6. Priority Roadmap

### P0 - Critical

- Create a first-class Activities/Timeline API and UI or hide/rename `/activities` until backed by real functionality.
- Add Tasks detail/edit/complete/delete lifecycle; current Tasks are not enough for daily CRM operation.
- Resolve Product Catalog API-Web mismatch for bulk/export/template/meta/stats/image primary/delete.
- Standardize mutation permission checks in frontend actions.
- Clarify Tenant/Settings module: build settings UI or keep it out of active navigation.

### P1 - High

- Implement import/export/bulk framework for Customers, Companies, Contacts, Leads, Deals, Opportunities, Product Catalog, Tickets.
- Convert high-value generic modules to real UI: Sales Forecasting, Omnichannel/Support Inbox, Integration Hub, Workflow Automation, Analytics.
- Add related-record panels and unified timelines to core records.
- Replace raw GUID forms with search-select relationship controls.
- Add shared selection-driven bulk toolbar to common table primitives.

### P2 - Medium

- Add saved views, advanced filters, column personalization, consistent server-side sorting/filtering.
- Add SLA breach dashboard, ticket queue board, routing/assignment rules.
- Add Knowledge Base editor, Document library, Calendar Sync settings, Marketing campaign/template/segment flows.

### P3 - Nice to Have

- AI summaries, next-best-action, scoring explainability, enrichment.
- Quote e-sign/document rendering, contract obligation tracking.
- Connector marketplace, webhook/API key self-service polish.
- Forecast scenarios and manager commits.

## 7. Suggested Implementation Phases

### Phase A

- Purpose: Stabilize core CRM usability and remove misleading surfaces.
- Scope: Activities/Tasks lifecycle, Product Catalog parity, mutation permission standard, Settings/Tenant clarity, shared table/bulk foundation.
- Affected modules: Activities, Tasks, Product Catalog, Tenant/Settings, all mutation actions.
- Risk: Medium because it touches shared auth/action/table patterns.
- Expected output: core CRM routes are honest, actions are consistently gated, and daily task/activity/catalog workflows stop feeling incomplete.

### Phase B

- Purpose: Complete primary entity workflows.
- Scope: Import/export/bulk, saved views, related-record panels, unified timeline for Customers/Companies/Contacts/Leads/Deals/Opportunities/Quotes/Tickets.
- Affected modules: core sales/service modules.
- Risk: Medium-high due API contract additions and shared timeline model.
- Expected output: usable CRM for real sales/service teams without raw GUID workarounds.

### Phase C

- Purpose: Productize high-value backend modules.
- Scope: Sales Forecasting dashboard, Omnichannel/Support Inbox agent console, Integration Hub admin, Workflow Automation builder/execution monitor, Analytics dashboard.
- Affected modules: SalesForecasting, Omnichannel, SupportInbox, IntegrationHub, WorkflowAutomation, AnalyticsReporting.
- Risk: High because it requires domain UX and backend contract hardening.
- Expected output: advanced backend investment becomes visible user value.

### Phase D

- Purpose: Modern CRM differentiation.
- Scope: Marketing automation UI, Knowledge Base editor, Document/Contract lifecycle, Quote document/e-sign, AI insights/enrichment, advanced forecasting.
- Affected modules: Marketing, KnowledgeBase, DocumentManagement, ContractLifecycle, QuoteManagement, ArtificialIntelligence.
- Risk: Medium after Phase A-C foundations.
- Expected output: NetMetric approaches HubSpot/Salesforce/Pipedrive/Twenty-style completeness.

## 8. Evidence Appendix

Key frontend evidence:

- `apps/crm-web/src/lib/crm-api/crm-api-endpoints.ts` - frontend endpoint catalog.
- `apps/crm-web/src/lib/crm-api/crm-api-client.ts` - client methods for core and operational modules.
- `apps/crm-web/src/lib/crm-auth/crm-capabilities.ts` - frontend capability aliases and path gating.
- `apps/crm-web/src/features/modules/module-registry.ts` - registry marks most modules active, settings coming soon.
- `apps/crm-web/src/features/modules/data/operational-module-data.ts` - operational endpoint definitions and placeholder behavior.
- `apps/crm-web/src/features/modules/render-module-shell.tsx` - generic shell routing.
- `apps/crm-web/src/features/modules/components/operational-module-workspace.tsx` - generic preview UI.
- `apps/crm-web/src/components/shell/crm-entity-list-page.tsx`, `crm-records-table.tsx`, `crm-data-table-adapter.tsx`, `crm-list-filter-bar.tsx`, `crm-pagination.tsx` - list/table primitives.
- `apps/crm-web/src/features/customers/*` - customer list/detail/import/action coverage.
- `apps/crm-web/src/features/companies/*`, `contacts/*`, `leads/*`, `deals/*`, `opportunities/*`, `pipeline/*`, `quotes/*`, `product-catalog/*`, `tickets/*`, `ticket-sla/*`, `ticket-workflows/*`, `support-inbox/*`, `tasks/*` - dedicated module UI/action coverage.

Key backend controller evidence:

- `services/crm/src/NetMetric.CRM.API/Controllers/CustomerManagement/CustomersController.cs`
- `services/crm/src/NetMetric.CRM.API/Controllers/CustomerManagement/CompaniesController.cs`
- `services/crm/src/NetMetric.CRM.API/Controllers/CustomerManagement/ContactsController.cs`
- `services/crm/src/NetMetric.CRM.API/Controllers/CustomerManagement/AddressesController.cs`
- `services/crm/src/NetMetric.CRM.API/Controllers/Customers/CustomerIntelligenceController.cs`
- `services/crm/src/NetMetric.CRM.API/Controllers/Leads/LeadsController.cs`
- `services/crm/src/NetMetric.CRM.API/Controllers/Deals/DealsController.cs`
- `services/crm/src/NetMetric.CRM.API/Controllers/Deals/DealWinLossController.cs`
- `services/crm/src/NetMetric.CRM.API/Controllers/Opportunities/OpportunitiesController.cs`
- `services/crm/src/NetMetric.CRM.API/Controllers/Pipelines/PipelinesController.cs`
- `services/crm/src/NetMetric.CRM.API/Controllers/Pipelines/PipelineOpportunitiesController.cs`
- `services/crm/src/NetMetric.CRM.API/Controllers/Pipelines/LostReasonsController.cs`
- `services/crm/src/NetMetric.CRM.API/Controllers/Pipelines/LeadConversionsController.cs`
- `services/crm/src/NetMetric.CRM.API/Controllers/Quotes/QuotesController.cs`
- `services/crm/src/NetMetric.CRM.API/Controllers/Catalogs/CatalogProductsController.cs`
- `services/crm/src/NetMetric.CRM.API/Controllers/Catalogs/CatalogCategoriesController.cs`
- `services/crm/src/NetMetric.CRM.API/Controllers/Tickets/TicketsController.cs`
- `services/crm/src/NetMetric.CRM.API/Controllers/Tickets/TicketSlaController.cs`
- `services/crm/src/NetMetric.CRM.API/Controllers/Tickets/TicketWorkflowController.cs`
- `services/crm/src/NetMetric.CRM.API/Controllers/Supports/SupportInboxController.cs`
- `services/crm/src/NetMetric.CRM.API/Controllers/WorkManagement/WorkManagementController.cs`
- `services/crm/src/NetMetric.CRM.API/Controllers/SalesForecasts/SalesForecastsController.cs`
- `services/crm/src/NetMetric.CRM.API/Controllers/Marketings/CampaignsController.cs`
- `services/crm/src/NetMetric.CRM.API/Controllers/Omnichannel/OmnichannelController.cs`
- `services/crm/src/NetMetric.CRM.API/Controllers/Integrations/IntegrationHubController.cs`
- `services/crm/src/NetMetric.CRM.API/Controllers/Integrations/IntegrationWebhookIngestionController.cs`
- `services/crm/src/NetMetric.CRM.API/Controllers/Knowledges/KnowledgeBaseArticlesController.cs`
- `services/crm/src/NetMetric.CRM.API/Controllers/Knowledges/KnowledgeBaseCategoriesController.cs`
- `services/crm/src/NetMetric.CRM.API/Controllers/Workflows/WorkflowAutomationController.cs`
- `services/crm/src/NetMetric.CRM.API/Controllers/Analytics/AnalyticsController.cs`
- `services/crm/src/NetMetric.CRM.API/Controllers/ArtificialIntelligence/ArtificialIntelligenceController.cs`
- `services/crm/src/NetMetric.CRM.API/Controllers/Tags/TagsController.cs`
- `services/crm/src/NetMetric.CRM.API/Controllers/Tenants/TenantsController.cs`

Key backend auth/tenant/infrastructure evidence:

- `services/crm/src/NetMetric.CRM.API/Compatibility/AuthorizationPolicies.cs`
- `services/crm/src/NetMetric.CRM.API/Security/TenantRouteGuardFilter.cs`
- `services/crm/src/NetMetric.CRM.API/Middleware/RequireTenantContextMiddleware.cs`
- `services/crm/src/NetMetric.CRM.API/DependencyInjection/CrmModuleServiceCollectionExtensions.cs`
- `services/crm/src/NetMetric.CRM.API/Program.cs`
- `services/crm/src/modules/*/*Infrastructure/Persistence/*DbContext.cs`
- `services/crm/src/modules/CustomerManagement/NetMetric.CRM.CustomerManagement.Application/Features/CustomerOperations/CustomerImportWizardCommands.cs`
- `services/crm/src/modules/CustomerManagement/NetMetric.CRM.CustomerManagement.Application/Features/Bulk`
- `services/crm/src/modules/LeadManagement/NetMetric.CRM.LeadManagement.Application/Features/Bulk`
- `services/crm/src/modules/ProductCatalog/NetMetric.CRM.ProductCatalog.Application/Features/CatalogItems`
- `services/crm/src/modules/QuoteManagement/NetMetric.CRM.QuoteManagement.Application/Handlers`
- `services/crm/src/modules/TicketManagement/NetMetric.CRM.TicketManagement.Application/Features`

Existing planning evidence:

- `docs/crm/crm-module-endpoint-map.md`

Needs verification:

- Runtime behavior of operational placeholder substitution for `{tenantId}`, `{customerId}`, `{ticketId}`, `{operationId}` style routes.
- Whether every action form is realistically usable with production data rather than raw GUID input.
- Whether global search or hidden components consume additional `crmApiClient` methods not visible in route-level inspection.
- Whether current uncommitted changes are intended final state or still work-in-progress.
