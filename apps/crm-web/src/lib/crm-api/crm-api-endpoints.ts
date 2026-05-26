import "server-only";

export type HttpMethod = "GET" | "POST" | "PUT" | "PATCH" | "DELETE";

type RouteDefinition = {
  method: HttpMethod;
  path: string;
};

export const crmApiEndpoints = {
  customersList: { method: "GET", path: "/api/customers" } satisfies RouteDefinition,
  customersDetail: (customerId: string) =>
    ({ method: "GET", path: `/api/customers/${customerId}` }) satisfies RouteDefinition,
  customersCreate: { method: "POST", path: "/api/customers" } satisfies RouteDefinition,
  customersUpdate: (customerId: string) =>
    ({ method: "PUT", path: `/api/customers/${customerId}` }) satisfies RouteDefinition,
  customersDelete: (customerId: string) =>
    ({ method: "DELETE", path: `/api/customers/${customerId}` }) satisfies RouteDefinition,
  customersImage: (customerId: string) =>
    ({ method: "POST", path: `/api/customers/${customerId}/image` }) satisfies RouteDefinition,
  customersImageDelete: (customerId: string) =>
    ({ method: "DELETE", path: `/api/customers/${customerId}/image` }) satisfies RouteDefinition,
  customerDuplicates: (customerId: string) =>
    ({ method: "GET", path: `/api/customers/${customerId}/duplicates` }) satisfies RouteDefinition,
  customerContacts: (customerId: string) =>
    ({ method: "GET", path: `/api/customers/${customerId}/contacts` }) satisfies RouteDefinition,
  customer360: (customerId: string) =>
    ({ method: "GET", path: `/api/customers/${customerId}/360` }) satisfies RouteDefinition,
  customerConsents: (customerId: string) =>
    ({ method: "GET", path: `/api/customers/${customerId}/consents` }) satisfies RouteDefinition,
  customerHierarchy: (customerId: string) =>
    ({ method: "GET", path: `/api/customers/${customerId}/hierarchy` }) satisfies RouteDefinition,
  customerHierarchyAdd: {
    method: "POST",
    path: "/api/customers/hierarchy",
  } satisfies RouteDefinition,
  customerHierarchyMove: (nodeId: string) =>
    ({ method: "POST", path: `/api/customers/hierarchy/${nodeId}/move` }) satisfies RouteDefinition,
  customerHierarchyRemove: (nodeId: string) =>
    ({ method: "DELETE", path: `/api/customers/hierarchy/${nodeId}` }) satisfies RouteDefinition,
  customerConsentUpsert: (customerId: string) =>
    ({ method: "POST", path: `/api/customers/${customerId}/consents` }) satisfies RouteDefinition,
  customerConsentRevoke: (customerId: string, consentId: string) =>
    ({
      method: "POST",
      path: `/api/customers/${customerId}/consents/${consentId}/revoke`,
    }) satisfies RouteDefinition,
  customerLifecycleStage: (customerId: string) =>
    ({
      method: "POST",
      path: `/api/customers/${customerId}/lifecycle-stage`,
    }) satisfies RouteDefinition,
  customerDataQualityRecalculate: (customerId: string) =>
    ({
      method: "POST",
      path: `/api/customers/${customerId}/data-quality/recalculate`,
    }) satisfies RouteDefinition,
  customerRelationshipHealthRecalculate: (customerId: string) =>
    ({
      method: "POST",
      path: `/api/customers/${customerId}/relationship-health/recalculate`,
    }) satisfies RouteDefinition,
  customerMergePreview: {
    method: "GET",
    path: "/api/customers/duplicates/merge-preview",
  } satisfies RouteDefinition,
  customerMerge: { method: "POST", path: "/api/customers/merge" } satisfies RouteDefinition,
  customerAuditTimeline: (customerId: string) =>
    ({
      method: "GET",
      path: `/api/customers/${customerId}/audit-timeline`,
    }) satisfies RouteDefinition,
  customerShare: (customerId: string) =>
    ({ method: "POST", path: `/api/customers/${customerId}/shares` }) satisfies RouteDefinition,
  customerShareRevoke: (customerId: string, shareId: string) =>
    ({
      method: "DELETE",
      path: `/api/customers/${customerId}/shares/${shareId}`,
    }) satisfies RouteDefinition,
  customerStakeholderRemove: (customerId: string, stakeholderId: string) =>
    ({
      method: "DELETE",
      path: `/api/customers/${customerId}/stakeholders/${stakeholderId}`,
    }) satisfies RouteDefinition,
  customersSearch: { method: "GET", path: "/api/customers/search" } satisfies RouteDefinition,
  customerImportBatchesCreate: {
    method: "POST",
    path: "/api/customers/import-batches",
  } satisfies RouteDefinition,
  customerImportBatchesList: {
    method: "GET",
    path: "/api/customers/import-batches",
  } satisfies RouteDefinition,
  customerImportBatchDetail: (batchId: string) =>
    ({ method: "GET", path: `/api/customers/import-batches/${batchId}` }) satisfies RouteDefinition,
  customerImportBatchPreview: (batchId: string) =>
    ({
      method: "POST",
      path: `/api/customers/import-batches/${batchId}/preview`,
    }) satisfies RouteDefinition,
  customerImportBatchValidate: (batchId: string) =>
    ({
      method: "POST",
      path: `/api/customers/import-batches/${batchId}/validate`,
    }) satisfies RouteDefinition,
  customerImportBatchCommit: (batchId: string) =>
    ({
      method: "POST",
      path: `/api/customers/import-batches/${batchId}/commit`,
    }) satisfies RouteDefinition,
  customerImportBatchCancel: (batchId: string) =>
    ({
      method: "POST",
      path: `/api/customers/import-batches/${batchId}/cancel`,
    }) satisfies RouteDefinition,
  customerMarkVip: (customerId: string) =>
    ({ method: "POST", path: `/api/customers/${customerId}/vip` }) satisfies RouteDefinition,

  contractsList: { method: "GET", path: "/api/contracts" } satisfies RouteDefinition,
  contractsDetail: (contractId: string) =>
    ({ method: "GET", path: `/api/contracts/${contractId}` }) satisfies RouteDefinition,
  contractsCreate: { method: "POST", path: "/api/contracts" } satisfies RouteDefinition,

  companiesList: { method: "GET", path: "/api/companies" } satisfies RouteDefinition,
  companiesDetail: (companyId: string) =>
    ({ method: "GET", path: `/api/companies/${companyId}` }) satisfies RouteDefinition,
  companiesCreate: { method: "POST", path: "/api/companies" } satisfies RouteDefinition,
  companiesUpdate: (companyId: string) =>
    ({ method: "PUT", path: `/api/companies/${companyId}` }) satisfies RouteDefinition,
  companiesActivate: (companyId: string) =>
    ({ method: "POST", path: `/api/companies/${companyId}/activate` }) satisfies RouteDefinition,
  companiesDeactivate: (companyId: string) =>
    ({ method: "POST", path: `/api/companies/${companyId}/deactivate` }) satisfies RouteDefinition,
  companiesDelete: (companyId: string) =>
    ({ method: "DELETE", path: `/api/companies/${companyId}` }) satisfies RouteDefinition,
  companiesLogo: (companyId: string) =>
    ({ method: "POST", path: `/api/companies/${companyId}/logo` }) satisfies RouteDefinition,
  companiesLogoDelete: (companyId: string) =>
    ({ method: "DELETE", path: `/api/companies/${companyId}/logo` }) satisfies RouteDefinition,

  contactsList: { method: "GET", path: "/api/contacts" } satisfies RouteDefinition,
  contactsDetail: (contactId: string) =>
    ({ method: "GET", path: `/api/contacts/${contactId}` }) satisfies RouteDefinition,
  contactsCreate: { method: "POST", path: "/api/contacts" } satisfies RouteDefinition,
  contactsUpdate: (contactId: string) =>
    ({ method: "PUT", path: `/api/contacts/${contactId}` }) satisfies RouteDefinition,
  contactsSetPrimary: (contactId: string) =>
    ({ method: "POST", path: `/api/contacts/${contactId}/set-primary` }) satisfies RouteDefinition,
  contactsDelete: (contactId: string) =>
    ({ method: "DELETE", path: `/api/contacts/${contactId}` }) satisfies RouteDefinition,
  trashList: { method: "GET", path: "/api/v1/trash" } satisfies RouteDefinition,
  trashRestore: (trashItemId: string) =>
    ({ method: "POST", path: `/api/v1/trash/${trashItemId}/restore` }) satisfies RouteDefinition,

  leadsList: { method: "GET", path: "/api/leads" } satisfies RouteDefinition,
  leadsDetail: (leadId: string) =>
    ({ method: "GET", path: `/api/leads/${leadId}` }) satisfies RouteDefinition,
  leadsCreate: { method: "POST", path: "/api/leads" } satisfies RouteDefinition,
  leadsUpdate: (leadId: string) =>
    ({ method: "PUT", path: `/api/leads/${leadId}` }) satisfies RouteDefinition,
  leadsDelete: (leadId: string) =>
    ({ method: "DELETE", path: `/api/leads/${leadId}` }) satisfies RouteDefinition,
  leadsWorkspace: (leadId: string) =>
    ({ method: "GET", path: `/api/leads/${leadId}/workspace` }) satisfies RouteDefinition,
  leadsTimeline: (leadId: string) =>
    ({ method: "GET", path: `/api/leads/${leadId}/timeline` }) satisfies RouteDefinition,
  leadsAssignOwner: (leadId: string) =>
    ({ method: "PATCH", path: `/api/leads/${leadId}/owner` }) satisfies RouteDefinition,
  leadsChangeStatus: (leadId: string) =>
    ({ method: "PATCH", path: `/api/leads/${leadId}/status` }) satisfies RouteDefinition,
  leadsScheduleNextContact: (leadId: string) =>
    ({ method: "PATCH", path: `/api/leads/${leadId}/next-contact` }) satisfies RouteDefinition,
  leadsUpsertScore: (leadId: string) =>
    ({ method: "PUT", path: `/api/leads/${leadId}/score` }) satisfies RouteDefinition,
  leadsUpsertQualification: (leadId: string) =>
    ({ method: "PUT", path: `/api/leads/${leadId}/qualification` }) satisfies RouteDefinition,
  leadsCapture: { method: "POST", path: "/api/leads/capture" } satisfies RouteDefinition,
  leadsConvert: (leadId: string) =>
    ({ method: "POST", path: `/api/leads/${leadId}/convert` }) satisfies RouteDefinition,
  leadsBulkAssignOwner: { method: "PATCH", path: "/api/leads/owner" } satisfies RouteDefinition,
  leadsBulkDelete: { method: "DELETE", path: "/api/leads/bulk" } satisfies RouteDefinition,

  dealsList: { method: "GET", path: "/api/deals" } satisfies RouteDefinition,
  dealsDetail: (dealId: string) =>
    ({ method: "GET", path: `/api/deals/${dealId}` }) satisfies RouteDefinition,
  dealsWorkspace: (dealId: string) =>
    ({ method: "GET", path: `/api/deals/${dealId}/workspace` }) satisfies RouteDefinition,
  dealsTimeline: (dealId: string) =>
    ({ method: "GET", path: `/api/deals/${dealId}/timeline` }) satisfies RouteDefinition,
  dealsCreate: { method: "POST", path: "/api/deals" } satisfies RouteDefinition,
  dealsUpdate: (dealId: string) =>
    ({ method: "PUT", path: `/api/deals/${dealId}` }) satisfies RouteDefinition,
  dealsDelete: (dealId: string) =>
    ({ method: "DELETE", path: `/api/deals/${dealId}` }) satisfies RouteDefinition,
  dealsAssignOwner: (dealId: string) =>
    ({ method: "PATCH", path: `/api/deals/${dealId}/owner` }) satisfies RouteDefinition,
  dealsBulkAssignOwner: { method: "PATCH", path: "/api/deals/owner" } satisfies RouteDefinition,
  dealsMarkWon: (dealId: string) =>
    ({ method: "POST", path: `/api/deals/${dealId}/won` }) satisfies RouteDefinition,
  dealsMarkLost: (dealId: string) =>
    ({ method: "POST", path: `/api/deals/${dealId}/lost` }) satisfies RouteDefinition,
  dealsReopen: (dealId: string) =>
    ({ method: "POST", path: `/api/deals/${dealId}/reopen` }) satisfies RouteDefinition,
  dealsWinLossSummary: {
    method: "GET",
    path: "/api/deals/win-loss/summary",
  } satisfies RouteDefinition,
  dealsLostReasons: {
    method: "GET",
    path: "/api/deals/win-loss/lost-reasons",
  } satisfies RouteDefinition,
  dealsUpsertWinLossReview: (dealId: string) =>
    ({ method: "PUT", path: `/api/deals/win-loss/${dealId}/review` }) satisfies RouteDefinition,

  quotesList: { method: "GET", path: "/api/quotes" } satisfies RouteDefinition,
  quotesDetail: (quoteId: string) =>
    ({ method: "GET", path: `/api/quotes/${quoteId}` }) satisfies RouteDefinition,
  quotesWorkspace: (quoteId: string) =>
    ({ method: "GET", path: `/api/quotes/${quoteId}/workspace` }) satisfies RouteDefinition,
  quotesTimeline: (quoteId: string) =>
    ({ method: "GET", path: `/api/quotes/${quoteId}/timeline` }) satisfies RouteDefinition,
  quotesValidation: (quoteId: string) =>
    ({ method: "GET", path: `/api/quotes/${quoteId}/validation` }) satisfies RouteDefinition,
  quotesCpqWorkspace: {
    method: "GET",
    path: "/api/quotes/cpq/workspace",
  } satisfies RouteDefinition,
  quotesCreate: { method: "POST", path: "/api/quotes" } satisfies RouteDefinition,
  quotesUpdate: (quoteId: string) =>
    ({ method: "PUT", path: `/api/quotes/${quoteId}` }) satisfies RouteDefinition,
  quotesDelete: (quoteId: string) =>
    ({ method: "DELETE", path: `/api/quotes/${quoteId}` }) satisfies RouteDefinition,
  quotesSubmit: (quoteId: string) =>
    ({ method: "POST", path: `/api/quotes/${quoteId}/submit` }) satisfies RouteDefinition,
  quotesApprove: (quoteId: string) =>
    ({ method: "POST", path: `/api/quotes/${quoteId}/approve` }) satisfies RouteDefinition,
  quotesReject: (quoteId: string) =>
    ({ method: "POST", path: `/api/quotes/${quoteId}/reject` }) satisfies RouteDefinition,
  quotesMarkSent: (quoteId: string) =>
    ({ method: "POST", path: `/api/quotes/${quoteId}/sent` }) satisfies RouteDefinition,
  quotesAccept: (quoteId: string) =>
    ({ method: "POST", path: `/api/quotes/${quoteId}/accepted` }) satisfies RouteDefinition,
  quotesDecline: (quoteId: string) =>
    ({ method: "POST", path: `/api/quotes/${quoteId}/declined` }) satisfies RouteDefinition,
  quotesExpire: (quoteId: string) =>
    ({ method: "POST", path: `/api/quotes/${quoteId}/expired` }) satisfies RouteDefinition,
  quotesCreateRevision: (quoteId: string) =>
    ({ method: "POST", path: `/api/quotes/${quoteId}/revisions` }) satisfies RouteDefinition,
  quotesProposalTemplates: {
    method: "GET",
    path: "/api/quotes/proposal-templates",
  } satisfies RouteDefinition,
  quotesCreateProposalTemplate: {
    method: "POST",
    path: "/api/quotes/proposal-templates",
  } satisfies RouteDefinition,
  quotesUpdateProposalTemplate: (templateId: string) =>
    ({
      method: "PUT",
      path: `/api/quotes/proposal-templates/${templateId}`,
    }) satisfies RouteDefinition,
  quotesDeleteProposalTemplate: (templateId: string) =>
    ({
      method: "DELETE",
      path: `/api/quotes/proposal-templates/${templateId}`,
    }) satisfies RouteDefinition,
  quotesRunGuidedSelling: {
    method: "POST",
    path: "/api/quotes/cpq/guided-selling",
  } satisfies RouteDefinition,
  quotesUpsertGuidedSellingPlaybook: (playbookId?: string | null) =>
    ({
      method: "PUT",
      path: playbookId
        ? `/api/quotes/cpq/guided-selling-playbooks/${playbookId}`
        : "/api/quotes/cpq/guided-selling-playbooks",
    }) satisfies RouteDefinition,
  quotesUpsertProductBundle: (bundleId?: string | null) =>
    ({
      method: "PUT",
      path: bundleId
        ? `/api/quotes/cpq/product-bundles/${bundleId}`
        : "/api/quotes/cpq/product-bundles",
    }) satisfies RouteDefinition,
  quotesUpsertProductRule: (ruleId?: string | null) =>
    ({
      method: "PUT",
      path: ruleId ? `/api/quotes/cpq/product-rules/${ruleId}` : "/api/quotes/cpq/product-rules",
    }) satisfies RouteDefinition,

  ticketsList: { method: "GET", path: "/api/tickets" } satisfies RouteDefinition,
  ticketsDetail: (ticketId: string) =>
    ({ method: "GET", path: `/api/tickets/${ticketId}` }) satisfies RouteDefinition,
  ticketsCreate: { method: "POST", path: "/api/tickets" } satisfies RouteDefinition,
  ticketsUpdate: (ticketId: string) =>
    ({ method: "PUT", path: `/api/tickets/${ticketId}` }) satisfies RouteDefinition,
  ticketsDelete: (ticketId: string) =>
    ({ method: "DELETE", path: `/api/tickets/${ticketId}` }) satisfies RouteDefinition,

  productCatalogList: { method: "GET", path: "/api/catalog/products" } satisfies RouteDefinition,
  productCatalogDetail: (productId: string) =>
    ({ method: "GET", path: `/api/catalog/products/${productId}` }) satisfies RouteDefinition,
  productCatalogImages: (productId: string) =>
    ({
      method: "GET",
      path: `/api/catalog/products/${productId}/images`,
    }) satisfies RouteDefinition,
  productCatalogCreate: { method: "POST", path: "/api/catalog/products" } satisfies RouteDefinition,
  productCatalogUpdate: (productId: string) =>
    ({ method: "PUT", path: `/api/catalog/products/${productId}` }) satisfies RouteDefinition,
  productCatalogDelete: (productId: string) =>
    ({ method: "DELETE", path: `/api/catalog/products/${productId}` }) satisfies RouteDefinition,
  productCatalogBulkDelete: {
    method: "DELETE",
    path: "/api/catalog/products/bulk",
  } satisfies RouteDefinition,
  productCatalogBulkSetActiveState: {
    method: "PATCH",
    path: "/api/catalog/products/bulk/active-state",
  } satisfies RouteDefinition,
  productCatalogSetActiveState: (productId: string) =>
    ({
      method: "PATCH",
      path: `/api/catalog/products/${productId}/active-state`,
    }) satisfies RouteDefinition,
  productCatalogExport: {
    method: "GET",
    path: "/api/catalog/products/export",
  } satisfies RouteDefinition,
  productCatalogTemplate: {
    method: "GET",
    path: "/api/catalog/products/template",
  } satisfies RouteDefinition,
  productCatalogMeta: {
    method: "GET",
    path: "/api/catalog/products/meta",
  } satisfies RouteDefinition,
  productCatalogStats: {
    method: "GET",
    path: "/api/catalog/products/stats",
  } satisfies RouteDefinition,
  productCatalogLookups: {
    method: "GET",
    path: "/api/catalog/products/lookups",
  } satisfies RouteDefinition,
  productCatalogSetPrimaryImage: (productId: string, productImageId: string) =>
    ({
      method: "PATCH",
      path: `/api/catalog/products/${productId}/images/${productImageId}/primary`,
    }) satisfies RouteDefinition,
  productCatalogDeleteImage: (productId: string, productImageId: string) =>
    ({
      method: "DELETE",
      path: `/api/catalog/products/${productId}/images/${productImageId}`,
    }) satisfies RouteDefinition,
  productCatalogCategoriesList: {
    method: "GET",
    path: "/api/catalog/categories",
  } satisfies RouteDefinition,
  productCatalogCategoriesDetail: (categoryId: string) =>
    ({ method: "GET", path: `/api/catalog/categories/${categoryId}` }) satisfies RouteDefinition,
  productCatalogCategoriesCreate: {
    method: "POST",
    path: "/api/catalog/categories",
  } satisfies RouteDefinition,
  productCatalogCategoriesUpdate: (categoryId: string) =>
    ({ method: "PUT", path: `/api/catalog/categories/${categoryId}` }) satisfies RouteDefinition,
  productCatalogCategoriesDelete: (categoryId: string) =>
    ({ method: "DELETE", path: `/api/catalog/categories/${categoryId}` }) satisfies RouteDefinition,
  productCatalogCategoriesSetActiveState: (categoryId: string) =>
    ({
      method: "PATCH",
      path: `/api/catalog/categories/${categoryId}/active-state`,
    }) satisfies RouteDefinition,

  opportunitiesList: { method: "GET", path: "/api/opportunities" } satisfies RouteDefinition,
  opportunitiesDetail: (opportunityId: string) =>
    ({ method: "GET", path: `/api/opportunities/${opportunityId}` }) satisfies RouteDefinition,
  opportunitiesWorkspace: (opportunityId: string) =>
    ({
      method: "GET",
      path: `/api/opportunities/${opportunityId}/workspace`,
    }) satisfies RouteDefinition,
  opportunitiesTimeline: (opportunityId: string) =>
    ({
      method: "GET",
      path: `/api/opportunities/${opportunityId}/timeline`,
    }) satisfies RouteDefinition,
  opportunitiesPipelineBoard: {
    method: "GET",
    path: "/api/opportunities/pipeline-board",
  } satisfies RouteDefinition,
  opportunitiesLostReasons: {
    method: "GET",
    path: "/api/opportunities/lost-reasons",
  } satisfies RouteDefinition,
  opportunitiesCreate: { method: "POST", path: "/api/opportunities" } satisfies RouteDefinition,
  opportunitiesUpdate: (opportunityId: string) =>
    ({ method: "PUT", path: `/api/opportunities/${opportunityId}` }) satisfies RouteDefinition,
  opportunitiesDelete: (opportunityId: string) =>
    ({ method: "DELETE", path: `/api/opportunities/${opportunityId}` }) satisfies RouteDefinition,
  opportunitiesAssignOwner: (opportunityId: string) =>
    ({
      method: "PATCH",
      path: `/api/opportunities/${opportunityId}/owner`,
    }) satisfies RouteDefinition,
  opportunitiesChangeStage: (opportunityId: string) =>
    ({
      method: "PATCH",
      path: `/api/opportunities/${opportunityId}/stage`,
    }) satisfies RouteDefinition,
  opportunitiesMarkWon: (opportunityId: string) =>
    ({ method: "POST", path: `/api/opportunities/${opportunityId}/won` }) satisfies RouteDefinition,
  opportunitiesMarkLost: (opportunityId: string) =>
    ({
      method: "POST",
      path: `/api/opportunities/${opportunityId}/lost`,
    }) satisfies RouteDefinition,
  opportunitiesAddContact: (opportunityId: string) =>
    ({
      method: "POST",
      path: `/api/opportunities/${opportunityId}/contacts`,
    }) satisfies RouteDefinition,
  opportunitiesAddProduct: (opportunityId: string) =>
    ({
      method: "POST",
      path: `/api/opportunities/${opportunityId}/products`,
    }) satisfies RouteDefinition,
  opportunitiesQuotes: (opportunityId: string) =>
    ({
      method: "GET",
      path: `/api/opportunities/${opportunityId}/quotes`,
    }) satisfies RouteDefinition,
  opportunitiesCreateQuote: (opportunityId: string) =>
    ({
      method: "POST",
      path: `/api/opportunities/${opportunityId}/quotes`,
    }) satisfies RouteDefinition,
  opportunitiesBulkAssignOwner: {
    method: "PATCH",
    path: "/api/opportunities/owner",
  } satisfies RouteDefinition,
  opportunitiesBulkChangeStage: {
    method: "PATCH",
    path: "/api/opportunities/stage",
  } satisfies RouteDefinition,

  pipelinesList: { method: "GET", path: "/api/opportunities/pipelines" } satisfies RouteDefinition,
  pipelinesDetail: (pipelineId: string) =>
    ({
      method: "GET",
      path: `/api/opportunities/pipelines/${pipelineId}`,
    }) satisfies RouteDefinition,
  pipelinesBoard: (pipelineId: string) =>
    ({
      method: "GET",
      path: `/api/opportunities/pipelines/${pipelineId}/board`,
    }) satisfies RouteDefinition,
  pipelinesAnalytics: (pipelineId: string) =>
    ({
      method: "GET",
      path: `/api/opportunities/pipelines/${pipelineId}/analytics`,
    }) satisfies RouteDefinition,
  pipelinesCreate: {
    method: "POST",
    path: "/api/opportunities/pipelines",
  } satisfies RouteDefinition,
  pipelinesUpdate: (pipelineId: string) =>
    ({
      method: "PUT",
      path: `/api/opportunities/pipelines/${pipelineId}`,
    }) satisfies RouteDefinition,
  pipelinesDelete: (pipelineId: string) =>
    ({
      method: "DELETE",
      path: `/api/opportunities/pipelines/${pipelineId}`,
    }) satisfies RouteDefinition,
  pipelinesOpportunityStageHistory: (opportunityId: string) =>
    ({
      method: "GET",
      path: `/api/opportunities/pipelines/items/${opportunityId}/stage-history`,
    }) satisfies RouteDefinition,
  pipelinesMoveOpportunityStage: (opportunityId: string) =>
    ({
      method: "POST",
      path: `/api/opportunities/pipelines/items/${opportunityId}/stage`,
    }) satisfies RouteDefinition,
  pipelineLostReasonsList: {
    method: "GET",
    path: "/api/pipeline/lost-reasons",
  } satisfies RouteDefinition,
  pipelineLostReasonsCreate: {
    method: "POST",
    path: "/api/pipeline/lost-reasons",
  } satisfies RouteDefinition,
  pipelineLostReasonsUpdate: (lostReasonId: string) =>
    ({
      method: "PUT",
      path: `/api/pipeline/lost-reasons/${lostReasonId}`,
    }) satisfies RouteDefinition,
  pipelineLeadConversionPreview: (leadId: string) =>
    ({
      method: "GET",
      path: `/api/pipeline/lead-conversions/${leadId}/preview`,
    }) satisfies RouteDefinition,
  pipelineLeadConversionConvert: (leadId: string) =>
    ({
      method: "POST",
      path: `/api/pipeline/lead-conversions/${leadId}`,
    }) satisfies RouteDefinition,

  ordersList: { method: "GET", path: "/api/orders" } satisfies RouteDefinition,
  ordersDetail: (orderId: string) =>
    ({ method: "GET", path: `/api/orders/${orderId}` }) satisfies RouteDefinition,
  ordersCreate: { method: "POST", path: "/api/orders" } satisfies RouteDefinition,

  tagsList: { method: "GET", path: "/api/tags" } satisfies RouteDefinition,
  tagGroupsList: { method: "GET", path: "/api/tags/groups" } satisfies RouteDefinition,
  smartLabelRulesList: {
    method: "GET",
    path: "/api/tags/smart-label-rules",
  } satisfies RouteDefinition,
  classificationSchemesList: {
    method: "GET",
    path: "/api/tags/classification-schemes",
  } satisfies RouteDefinition,
  tagsCreate: { method: "POST", path: "/api/tags" } satisfies RouteDefinition,
  tagGroupsCreate: { method: "POST", path: "/api/tags/groups" } satisfies RouteDefinition,
  smartLabelRulesCreate: {
    method: "POST",
    path: "/api/tags/smart-label-rules",
  } satisfies RouteDefinition,
  classificationSchemesCreate: {
    method: "POST",
    path: "/api/tags/classification-schemes",
  } satisfies RouteDefinition,

  workManagementWorkspace: {
    method: "GET",
    path: "/api/work-management/workspace",
  } satisfies RouteDefinition,
  workManagementCreateTask: {
    method: "POST",
    path: "/api/work-management/tasks",
  } satisfies RouteDefinition,
  workManagementTasksList: {
    method: "GET",
    path: "/api/work-management/tasks",
  } satisfies RouteDefinition,
  workManagementTaskDetail: (taskId: string) =>
    ({ method: "GET", path: `/api/work-management/tasks/${taskId}` }) satisfies RouteDefinition,
  workManagementTaskUpdate: (taskId: string) =>
    ({ method: "PUT", path: `/api/work-management/tasks/${taskId}` }) satisfies RouteDefinition,
  workManagementTaskComplete: (taskId: string) =>
    ({
      method: "PATCH",
      path: `/api/work-management/tasks/${taskId}/complete`,
    }) satisfies RouteDefinition,
  workManagementTaskReopen: (taskId: string) =>
    ({
      method: "PATCH",
      path: `/api/work-management/tasks/${taskId}/reopen`,
    }) satisfies RouteDefinition,
  workManagementTaskOwner: (taskId: string) =>
    ({
      method: "PATCH",
      path: `/api/work-management/tasks/${taskId}/owner`,
    }) satisfies RouteDefinition,
  workManagementTaskDueDate: (taskId: string) =>
    ({
      method: "PATCH",
      path: `/api/work-management/tasks/${taskId}/due-date`,
    }) satisfies RouteDefinition,
  workManagementTaskReminder: (taskId: string) =>
    ({
      method: "PATCH",
      path: `/api/work-management/tasks/${taskId}/reminder`,
    }) satisfies RouteDefinition,
  workManagementTaskDelete: (taskId: string) =>
    ({ method: "DELETE", path: `/api/work-management/tasks/${taskId}` }) satisfies RouteDefinition,
  workManagementScheduleMeeting: {
    method: "POST",
    path: "/api/work-management/meetings",
  } satisfies RouteDefinition,
  activitiesList: {
    method: "GET",
    path: "/api/activities",
  } satisfies RouteDefinition,
  activitiesCreate: {
    method: "POST",
    path: "/api/activities",
  } satisfies RouteDefinition,
  activityDetail: (activityId: string) =>
    ({ method: "GET", path: `/api/activities/${activityId}` }) satisfies RouteDefinition,
  relatedActivities: (entityType: string, entityId: string) =>
    ({
      method: "GET",
      path: `/api/activities/related/${entityType}/${entityId}`,
    }) satisfies RouteDefinition,

  supportInboxConnectionsList: {
    method: "GET",
    path: "/api/support-inbox/connections",
  } satisfies RouteDefinition,
  supportInboxConnectionsCreate: {
    method: "POST",
    path: "/api/support-inbox/connections",
  } satisfies RouteDefinition,
  supportInboxConnectionsUpdate: (connectionId: string) =>
    ({
      method: "PUT",
      path: `/api/support-inbox/connections/${connectionId}`,
    }) satisfies RouteDefinition,
  supportInboxConnectionSync: (connectionId: string) =>
    ({
      method: "POST",
      path: `/api/support-inbox/connections/${connectionId}/sync`,
    }) satisfies RouteDefinition,
  supportInboxMessagesList: {
    method: "GET",
    path: "/api/support-inbox/messages",
  } satisfies RouteDefinition,
  supportInboxRulesCreate: {
    method: "POST",
    path: "/api/support-inbox/rules",
  } satisfies RouteDefinition,
  supportInboxRulesUpdate: (ruleId: string) =>
    ({ method: "PUT", path: `/api/support-inbox/rules/${ruleId}` }) satisfies RouteDefinition,

  ticketSlaPoliciesList: {
    method: "GET",
    path: "/api/ticket-sla/policies",
  } satisfies RouteDefinition,
  ticketSlaPolicyCreate: {
    method: "POST",
    path: "/api/ticket-sla/policies",
  } satisfies RouteDefinition,
  ticketSlaPolicyUpdate: (policyId: string) =>
    ({
      method: "PUT",
      path: `/api/ticket-sla/policies/${policyId}`,
    }) satisfies RouteDefinition,
  ticketSlaPolicyDelete: (policyId: string) =>
    ({
      method: "DELETE",
      path: `/api/ticket-sla/policies/${policyId}`,
    }) satisfies RouteDefinition,
  ticketSlaPolicyEscalationRules: (policyId: string) =>
    ({
      method: "GET",
      path: `/api/ticket-sla/policies/${policyId}/escalation-rules`,
    }) satisfies RouteDefinition,
  ticketSlaEscalationRuleCreate: {
    method: "POST",
    path: "/api/ticket-sla/escalation-rules",
  } satisfies RouteDefinition,
  ticketSlaEscalationRuleUpdate: (ruleId: string) =>
    ({
      method: "PUT",
      path: `/api/ticket-sla/escalation-rules/${ruleId}`,
    }) satisfies RouteDefinition,
  ticketSlaWorkspace: (ticketId: string) =>
    ({
      method: "GET",
      path: `/api/ticket-sla/tickets/${ticketId}/workspace`,
    }) satisfies RouteDefinition,
  ticketSlaEscalationRuns: (ticketId: string) =>
    ({
      method: "GET",
      path: `/api/ticket-sla/tickets/${ticketId}/escalation-runs`,
    }) satisfies RouteDefinition,
  ticketSlaAttachToTicket: {
    method: "POST",
    path: "/api/ticket-sla/tickets/attach",
  } satisfies RouteDefinition,
  ticketSlaMarkFirstResponse: {
    method: "POST",
    path: "/api/ticket-sla/tickets/first-response",
  } satisfies RouteDefinition,
  ticketSlaMarkResolved: {
    method: "POST",
    path: "/api/ticket-sla/tickets/resolved",
  } satisfies RouteDefinition,
  ticketSlaRunDueEscalations: {
    method: "POST",
    path: "/api/ticket-sla/escalations/run-due",
  } satisfies RouteDefinition,
  ticketWorkflowQueues: {
    method: "GET",
    path: "/api/ticket-workflow/queues",
  } satisfies RouteDefinition,
  ticketWorkflowQueueCreate: {
    method: "POST",
    path: "/api/ticket-workflow/queues",
  } satisfies RouteDefinition,
  ticketWorkflowQueueUpdate: (queueId: string) =>
    ({
      method: "PUT",
      path: `/api/ticket-workflow/queues/${queueId}`,
    }) satisfies RouteDefinition,
  ticketWorkflowQueueDelete: (queueId: string) =>
    ({
      method: "DELETE",
      path: `/api/ticket-workflow/queues/${queueId}`,
    }) satisfies RouteDefinition,
  ticketWorkflowAssignmentHistory: (ticketId: string) =>
    ({
      method: "GET",
      path: `/api/ticket-workflow/tickets/${ticketId}/assignments`,
    }) satisfies RouteDefinition,
  ticketWorkflowStatusHistory: (ticketId: string) =>
    ({
      method: "GET",
      path: `/api/ticket-workflow/tickets/${ticketId}/status-history`,
    }) satisfies RouteDefinition,
  ticketWorkflowAssignQueue: (ticketId: string) =>
    ({
      method: "PATCH",
      path: `/api/ticket-workflow/tickets/${ticketId}/queue`,
    }) satisfies RouteDefinition,
  ticketWorkflowAssignOwner: (ticketId: string) =>
    ({
      method: "PATCH",
      path: `/api/ticket-workflow/tickets/${ticketId}/owner`,
    }) satisfies RouteDefinition,
  ticketWorkflowRecordStatusChange: (ticketId: string) =>
    ({
      method: "POST",
      path: `/api/ticket-workflow/tickets/${ticketId}/status-history`,
    }) satisfies RouteDefinition,

  addressesAddToCompany: (companyId: string) =>
    ({ method: "POST", path: `/api/addresses/companies/${companyId}` }) satisfies RouteDefinition,
  addressesAddToCustomer: (customerId: string) =>
    ({ method: "POST", path: `/api/addresses/customers/${customerId}` }) satisfies RouteDefinition,
  addressesUpdate: (addressId: string) =>
    ({ method: "PUT", path: `/api/addresses/${addressId}` }) satisfies RouteDefinition,
  addressesSetDefault: (addressId: string) =>
    ({ method: "POST", path: `/api/addresses/${addressId}/set-default` }) satisfies RouteDefinition,
  addressesDelete: (addressId: string) =>
    ({ method: "DELETE", path: `/api/addresses/${addressId}` }) satisfies RouteDefinition,
} as const;
