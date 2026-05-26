# CRM Module Endpoint Map

This document summarizes source-visible endpoint availability in `services/crm/src/NetMetric.CRM.API/Controllers` for crm-web planning.

| Module                          | Folder                   | Controller Visible               | List Visible | Detail Visible | Mutation Visible | Frontend Status  | Recommended Next Phase                                                     |
| ------------------------------- | ------------------------ | -------------------------------- | ------------ | -------------- | ---------------- | ---------------- | -------------------------------------------------------------------------- |
| Dashboard                       | AnalyticsReporting       | Yes (`AnalyticsController`)      | Partial      | Partial        | No               | read_only        | Keep read-only; add role dashboards later                                  |
| Customer Management - Customers | CustomerManagement       | Yes                              | Yes          | Yes            | Yes              | active           | Implemented                                                                |
| Customer Management - Companies | CustomerManagement       | Yes                              | Yes          | Yes            | Yes              | active           | Implemented                                                                |
| Customer Management - Contacts  | CustomerManagement       | Yes                              | Yes          | Yes            | Yes              | active           | Implemented                                                                |
| Address Management              | CustomerManagement       | Yes (`AddressesController`)      | No           | No             | Yes              | active           | Implemented in detail pages                                                |
| Customer Intelligence           | CustomerIntelligence     | Yes                              | No           | Partial        | Yes              | read_only        | Implemented (workspace + portal summary reads; mutations pending UI flows) |
| Lead Management                 | LeadManagement           | Yes                              | Yes          | Yes            | Yes              | active           | Implemented                                                                |
| Deal Management                 | DealManagement           | Yes (`DealsController`)          | Yes          | Yes            | Yes              | active           | Implemented                                                                |
| Opportunity Management          | OpportunityManagement    | Yes                              | Yes          | Yes            | Yes              | active           | Implemented                                                                |
| Pipeline Management             | PipelineManagement       | Yes                              | Yes          | Yes            | Yes              | active           | Implemented                                                                |
| Quote Management                | QuoteManagement          | Yes                              | Yes          | Yes            | Yes              | active           | Implemented (basic CRUD)                                                   |
| Sales Forecasting               | SalesForecasting         | Yes                              | Partial      | Partial        | Yes              | contract_pending | Forecast workspace phase                                                   |
| Product Catalog                 | ProductCatalog           | Yes                              | Yes          | Yes            | Yes              | contract_pending | Catalog list/detail phase                                                  |
| Support Inbox Integration       | SupportInboxIntegration  | Yes                              | Partial      | Partial        | Yes              | read_only        | Implemented (read-only connections/messages)                               |
| Ticket Management               | TicketManagement         | Yes (`TicketsController`)        | Yes          | Yes            | Yes              | active           | Implemented (basic CRUD)                                                   |
| Ticket SLA Management           | TicketSlaManagement      | Yes                              | Yes          | Partial        | Yes              | active           | Implemented (policy/rule mutations + reads)                                |
| Ticket Workflow Management      | TicketWorkflowManagement | Yes (`TicketWorkflowController`) | Partial      | Partial        | Yes              | active           | Implemented (queue + ticket workflow actions)                              |
| Marketing Automation            | MarketingAutomation      | Yes                              | Partial      | Partial        | Yes              | contract_pending | Marketing workspace phase                                                  |
| Omnichannel                     | Omnichannel              | Yes                              | Yes          | Yes            | Yes              | contract_pending | Omnichannel conversation phase                                             |
| Calendar Sync                   | CalendarSync             | Yes                              | Partial      | Partial        | Yes              | contract_pending | Calendar sync workspace phase                                              |
| Contract Lifecycle              | ContractLifecycle        | Yes                              | No           | Yes            | Yes              | contract_pending | Contracts detail/create phase                                              |
| Document Management             | DocumentManagement       | Yes                              | Yes          | Yes            | Yes              | contract_pending | Documents list/detail phase                                                |
| Finance Operations              | FinanceOperations        | Yes                              | No           | Yes            | Yes              | contract_pending | Finance order phase                                                        |
| Integration Hub                 | IntegrationHub           | Yes                              | Partial      | Partial        | Yes              | contract_pending | Integrations workspace phase                                               |
| Knowledge Base Management       | KnowledgeBaseManagement  | Yes                              | Yes          | Yes            | Yes              | contract_pending | Knowledge base phase                                                       |
| Work Management                 | WorkManagement           | Yes                              | Partial      | No             | Yes              | active           | Add task detail + activities read routes                                   |
| Workflow Automation             | WorkflowAutomation       | Yes                              | Yes          | Yes            | Yes              | contract_pending | Workflow rules phase                                                       |
| Artificial Intelligence         | ArtificialIntelligence   | Yes                              | Partial      | Partial        | Yes              | contract_pending | AI provider workspace phase                                                |
| Tag Management                  | TagManagement            | Yes (`TagsController`)           | No           | No             | Yes              | contract_pending | Add read endpoints before crm-web wiring                                   |
| Tenant Management               | TenantManagement         | Yes                              | No           | Partial        | Yes              | contract_pending | Tenant settings phase                                                      |

Notes:

- "Partial" means non-standard workspace/summary endpoints are source-visible but not yet mapped into crm-web list/detail patterns.
- WorkManagement currently exposes `GET /api/work-management/workspace` for read operations and `POST /api/work-management/tasks`, `POST /api/work-management/meetings` for creation flows.
- Dedicated `GET /api/tasks/{id}` and `GET /api/activities` routes are not source-visible in `NetMetric.CRM.API`.
- TagManagement currently exposes create-focused endpoints only (`POST /api/tags`, `POST /api/tags/groups`, `POST /api/tags/smart-label-rules`, `POST /api/tags/classification-schemes`).
- Source-visible read endpoints (`GET /api/tags`, `GET /api/tags/{id}`) and assignment endpoints (`assign/unassign`, `entity tags`) are not present in the consolidated API controller surface.
- DealManagement read endpoints are wired in crm-web: `GET /api/deals`, `GET /api/deals/{dealId:guid}`.
- DealManagement basic mutations are wired in crm-web: `POST /api/deals`, `PUT /api/deals/{dealId:guid}`, `DELETE /api/deals/{dealId:guid}`.
- Deal lifecycle actions are wired in crm-web: `PATCH /api/deals/{dealId:guid}/owner`, `POST /api/deals/{dealId:guid}/won`, `POST /api/deals/{dealId:guid}/lost`, `POST /api/deals/{dealId:guid}/reopen`.
- `GET /api/deals/win-loss/lost-reasons` is used for lost reason options.
- Bulk owner change (`PATCH /api/deals/owner`) and win-loss review (`PUT /api/deals/win-loss/{dealId:guid}/review`) remain intentionally unimplemented in crm-web.
- QuoteManagement read endpoints are wired in crm-web: `GET /api/quotes`, `GET /api/quotes/{quoteId:guid}`.
- QuoteManagement basic CRUD is wired in crm-web: `POST /api/quotes`, `PUT /api/quotes/{quoteId:guid}`, `DELETE /api/quotes/{quoteId:guid}`.
- QuoteManagement lifecycle actions are wired in crm-web: `POST /api/quotes/{quoteId:guid}/submit`, `POST /api/quotes/{quoteId:guid}/approve`, `POST /api/quotes/{quoteId:guid}/reject`, `POST /api/quotes/{quoteId:guid}/sent`, `POST /api/quotes/{quoteId:guid}/accepted`, `POST /api/quotes/{quoteId:guid}/declined`, `POST /api/quotes/{quoteId:guid}/expired`, `POST /api/quotes/{quoteId:guid}/revisions`.
- QuoteManagement CPQ/workspace/timeline/validation dedicated route UIs remain intentionally unimplemented in crm-web for this phase.
- TicketManagement read endpoints are wired in crm-web: `GET /api/tickets`, `GET /api/tickets/{ticketId:guid}`.
- TicketManagement basic CRUD is wired in crm-web: `POST /api/tickets`, `PUT /api/tickets/{ticketId:guid}`, `DELETE /api/tickets/{ticketId:guid}`.
- SupportInbox read endpoints are source-visible: `GET /api/support-inbox/connections`, `GET /api/support-inbox/messages`.
- SupportInbox read endpoints are wired in crm-web as read-only views with message paging/filter query pass-through (`connectionId`, `linkedToTicket`, `page`, `pageSize`).
- SupportInbox mutations (`create/update connection`, `sync`, `rules`) remain intentionally unimplemented in crm-web for this phase.
- TicketSla read endpoints are source-visible: `GET /api/ticket-sla/policies`, `GET /api/ticket-sla/policies/{policyId:guid}/escalation-rules`, `GET /api/ticket-sla/tickets/{ticketId:guid}/workspace`, `GET /api/ticket-sla/tickets/{ticketId:guid}/escalation-runs`.
- TicketSla read endpoints are wired in crm-web as read-only sections for policies, policy escalation rules, ticket SLA workspace, and ticket escalation runs.
- TicketSla mutations implemented in crm-web: `POST /api/ticket-sla/policies`, `PUT /api/ticket-sla/policies/{policyId:guid}`, `DELETE /api/ticket-sla/policies/{policyId:guid}`, `POST /api/ticket-sla/escalation-rules`, `PUT /api/ticket-sla/escalation-rules/{ruleId:guid}`.
- TicketSla escalation-rule delete route is not source-visible in `TicketSlaController`; crm-web intentionally does not implement escalation-rule delete.
- TicketWorkflow read endpoints are source-visible: `GET /api/ticket-workflow/queues`, `GET /api/ticket-workflow/tickets/{ticketId:guid}/assignments`, `GET /api/ticket-workflow/tickets/{ticketId:guid}/status-history`.
- TicketWorkflow read endpoints are wired in crm-web for queue list, ticket assignment history, and ticket status history.
- TicketWorkflow mutations implemented in crm-web: `POST /api/ticket-workflow/queues`, `PUT /api/ticket-workflow/queues/{queueId:guid}`, `DELETE /api/ticket-workflow/queues/{queueId:guid}`, `PATCH /api/ticket-workflow/tickets/{ticketId:guid}/queue`, `PATCH /api/ticket-workflow/tickets/{ticketId:guid}/owner`, `POST /api/ticket-workflow/tickets/{ticketId:guid}/status-history`.
- "Unknown" indicates module folder visibility without a clearly mapped dedicated controller surface in `NetMetric.CRM.API` route naming.
- crm-web implements lead/opportunity CRUD and pipeline stage movement, and keeps remaining non-implemented modules as route shells.
