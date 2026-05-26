import "server-only";

import { createServerPerformanceLogger } from "@netmetric/observability/server";

import { getCrmApiConfig, joinCrmApiPath } from "./crm-api-config";
import { applyCorrelationId, getCorrelationIdFromHeaders } from "./correlation";
import { CrmApiError, statusToCrmApiErrorKind } from "./crm-api-errors";
import { crmApiEndpoints } from "./crm-api-endpoints";
import { listQueryToSearchParams } from "./query-model";
import { normalizeProblemDetails } from "./problem-details";
import type {
  AddressDto,
  AddressUpsertRequest,
  CompanyDetailDto,
  CompanyListItemDto,
  CompanyUpsertRequest,
  ContactDetailDto,
  ContactListItemDto,
  GlobalTrashListItemDto,
  ContactUpsertRequest,
  CrmPagedResult,
  CrmApiAuthContext,
  CrmApiRequestOptions,
  CrmListQuery,
  CustomerDetailDto,
  Customer360Dto,
  CustomerAccountHierarchyDto,
  CustomerAuditEventDto,
  CustomerConsentDto,
  CustomerDuplicateWarningDto,
  CustomerImportBatchDto,
  CustomerListItemDto,
  CustomerMergePreviewDto,
  CustomerSearchResultDto,
  CustomerUpsertRequest,
  AddAccountHierarchyNodeRequest,
  CancelCustomerImportBatchRequest,
  ChangeCustomerLifecycleStageRequest,
  CommitCustomerImportBatchRequest,
  CreateCustomerImportBatchRequest,
  MergeCustomersRequest,
  MoveAccountHierarchyNodeRequest,
  RevokeCustomerConsentRequest,
  ShareCustomerRecordRequest,
  UpsertCustomerConsentRequest,
  HttpMethod,
  LeadDetailDto,
  LeadListItemDto,
  LeadScoreDto,
  AssignLeadOwnerRequest,
  BulkAssignLeadOwnerRequest,
  BulkLeadIdsRequest,
  ChangeLeadStatusRequest,
  ConvertLeadToCustomerRequest,
  LeadCaptureRequest,
  LeadCaptureResultDto,
  LeadConversionResultDto,
  LeadTimelineEventDto,
  LeadWorkspaceDto,
  ScheduleLeadNextContactRequest,
  UpsertLeadQualificationRequest,
  UpsertLeadScoreRequest,
  DealDetailDto,
  DealListItemDto,
  DealUpsertRequest,
  DealWorkspaceDto,
  TicketDetailDto,
  TicketListItemDto,
  TicketUpdateRequest,
  TicketUpsertRequest,
  QuoteDetailDto,
  QuoteDateNoteRequest,
  QuoteDeclineRequest,
  QuoteTimelineEventDto,
  QuoteListItemDto,
  QuoteNoteRequest,
  QuoteReasonRequest,
  QuoteUpdateRequest,
  QuoteUpsertRequest,
  QuoteWorkspaceDto,
  CreateQuoteRevisionRequest,
  CpqValidationResultDto,
  CpqWorkspaceDto,
  ClassificationSchemeSummaryDto,
  ContractCreateRequest,
  ContractLifecycleSummaryDto,
  CreateClassificationSchemeRequest,
  CreateSmartLabelRuleRequest,
  CreateTagGroupRequest,
  CreateTagRequest,
  FinanceOperationsSummaryDto,
  GuidedSellingPlaybookDto,
  GuidedSellingRecommendationDto,
  ProposalTemplateDto,
  ProposalTemplateRequest,
  ProductBundleDto,
  ProductRuleDto,
  RunGuidedSellingRequest,
  UpsertGuidedSellingPlaybookRequest,
  UpsertProductBundleRequest,
  UpsertProductRuleRequest,
  AssignDealOwnerRequest,
  BulkAssignDealOwnerRequest,
  DealBulkOperationResultDto,
  DealOutcomeHistoryDto,
  DealOutcomeRequest,
  DealLostReasonDto,
  DealReviewUpsertRequest,
  DealWinLossReviewDto,
  DealWinLossSummaryDto,
  DealWinLossSummaryQuery,
  OpportunityDetailDto,
  OpportunityListItemDto,
  OpportunityWorkspaceDto,
  OpportunityTimelineEventDto,
  OpportunityStageHistoryDto,
  OpportunityLostReasonDto,
  OpportunityContactDto,
  OpportunityProductDto,
  AssignOpportunityOwnerRequest,
  ChangeOpportunityStageRequest,
  MarkOpportunityWonRequest,
  MarkOpportunityWonResultDto,
  MarkOpportunityLostRequest,
  AddOpportunityContactRequest,
  AddOpportunityProductRequest,
  OpportunityQuoteDetailDto,
  CreateOpportunityQuoteRequest,
  BulkAssignOpportunityOwnerRequest,
  BulkChangeOpportunityStageRequest,
  OpportunityBulkOperationResultDto,
  OpportunityUpdateRequest,
  OpportunityUpsertRequest,
  PipelineBoardDto,
  PipelineAnalyticsDto,
  PipelineStageMoveRequest,
  PipelineStageMoveResultDto,
  PipelineDto,
  PipelineSummaryDto,
  CreatePipelineRequest,
  UpdatePipelineRequest,
  PipelineLostReasonUpsertRequest,
  PipelineLeadConversionPreviewDto,
  PipelineLeadConversionRequest,
  PipelineLeadConversionResultDto,
  WorkManagementWorkspaceDto,
  ActivityTimelineFeed,
  ActivityTimelineItem,
  CreateActivityRequest,
  CreateActivityResponse,
  SupportInboxConnectionDto,
  SupportInboxConnectionCreateRequest,
  SupportInboxConnectionUpdateRequest,
  SupportInboxMessageDto,
  SupportInboxRuleCreateRequest,
  SupportInboxRuleUpdateRequest,
  SupportInboxSyncRequest,
  TicketEscalationRunDto,
  TicketSlaEscalationRuleDto,
  TicketSlaPolicyDto,
  TicketSlaPolicyUpsertRequest,
  TicketSlaEscalationRuleUpsertRequest,
  TicketSlaWorkspaceDto,
  AttachTicketSlaRequest,
  MarkTicketFirstResponseRequest,
  MarkTicketResolvedRequest,
  RunDueTicketEscalationsRequest,
  RunDueTicketEscalationsResultDto,
  TicketWorkflowQueueDto,
  TicketWorkflowQueueUpsertRequest,
  TicketWorkflowQueueUpdateRequest,
  AssignTicketWorkflowQueueRequest,
  AssignTicketWorkflowOwnerRequest,
  RecordTicketWorkflowStatusChangeRequest,
  TicketAssignmentHistoryDto,
  TicketStatusHistoryDto,
  SalesOrderCreateRequest,
  SmartLabelRuleSummaryDto,
  TagGroupSummaryDto,
  TagSummaryDto,
  WorkTaskDto,
  MeetingScheduleDto,
  CreateWorkTaskRequest,
  ScheduleMeetingRequest,
  UpdateWorkTaskRequest,
  CompleteWorkTaskRequest,
  AssignWorkTaskOwnerRequest,
  UpdateWorkTaskDueDateRequest,
  UpdateWorkTaskReminderRequest,
  LeadUpdateRequest,
  LeadUpsertRequest,
  ProductCatalogActiveStateRequest,
  ProductCatalogCategoryActiveStateRequest,
  ProductCatalogMetaDto,
  ProductCatalogStatsDto,
  CatalogBulkOperationResultDto,
  BulkCatalogItemIdsRequest,
  BulkSetActiveStateRequest,
  CrmApiDownloadPayload,
  ProductCatalogCategoryDto,
  ProductCatalogCategoryUpsertRequest,
  ProductCatalogItemDto,
  ProductCatalogLookupsDto,
  ProductImageDto,
  ProductCatalogUpsertRequest,
} from "./crm-api-types";

const crmApiPerformance = createServerPerformanceLogger({
  app: "crm-web",
  component: "crm-api-client",
  enabled: process.env.NETMETRIC_PERF_LOG === "1",
});

type RequestOptions = CrmApiRequestOptions & {
  method: HttpMethod;
  path: string;
  body?: unknown;
  query?: Record<string, string | number | boolean | undefined>;
  contentType?: string;
};

function toTelemetryPath(pathWithQuery: string): string {
  return pathWithQuery.split("?")[0] ?? pathWithQuery;
}

function withQuery(
  path: string,
  query: Record<string, string | number | boolean | undefined>,
): string {
  const params = new URLSearchParams();

  for (const [key, value] of Object.entries(query)) {
    if (value === undefined) {
      continue;
    }

    params.set(key, String(value));
  }

  const suffix = params.toString();
  return suffix ? `${path}?${suffix}` : path;
}

function readBodyAsJson(body: unknown, contentType?: string): BodyInit | undefined {
  if (body === undefined) {
    return undefined;
  }

  if (body instanceof FormData) {
    return body;
  }

  if (contentType && contentType !== "application/json") {
    return body as BodyInit;
  }

  return JSON.stringify(body);
}

async function parseResponsePayload(response: Response): Promise<unknown> {
  const text = await response.text();

  if (!text) {
    return null;
  }

  try {
    return JSON.parse(text) as unknown;
  } catch {
    return text;
  }
}

function buildHeaders(
  authContext: CrmApiAuthContext | undefined,
  correlationId: string | undefined,
): Headers {
  const headers = new Headers();
  headers.set("accept", "application/json");

  if (authContext?.bearerToken) {
    headers.set("authorization", `Bearer ${authContext.bearerToken}`);
  }

  applyCorrelationId(headers, correlationId);
  return headers;
}

function createTimeoutSignal(timeoutMs: number, parentSignal?: AbortSignal): AbortSignal {
  const timeoutSignal = AbortSignal.timeout(timeoutMs);

  if (!parentSignal) {
    return timeoutSignal;
  }

  if (parentSignal.aborted) {
    return parentSignal;
  }

  const controller = new AbortController();
  const abort = () => controller.abort();

  parentSignal.addEventListener("abort", abort, { once: true });
  timeoutSignal.addEventListener("abort", abort, { once: true });

  return controller.signal;
}

async function request<TResponse>(options: RequestOptions): Promise<TResponse> {
  const pathWithQuery = options.query ? withQuery(options.path, options.query) : options.path;
  const requestUrl = joinCrmApiPath(pathWithQuery);
  const correlationId = options.correlationId;
  const headers = buildHeaders(options.authContext, correlationId);
  const body = readBodyAsJson(options.body, options.contentType);

  if (body && !options.contentType && !(body instanceof FormData)) {
    headers.set("content-type", "application/json");
  } else if (options.contentType) {
    headers.set("content-type", options.contentType);
  }

  const signal = createTimeoutSignal(
    options.timeoutMs ?? getCrmApiConfig().defaultTimeoutMs,
    options.signal,
  );

  const requestInit: RequestInit = {
    method: options.method,
    headers,
    cache: "no-store",
    signal,
    redirect: "manual",
  };

  if (body !== undefined) {
    requestInit.body = body;
  }

  let response: Response;
  const telemetryPath = toTelemetryPath(pathWithQuery);
  const startedAt = performance.now();
  try {
    response = await fetch(requestUrl, requestInit);
  } catch {
    crmApiPerformance.record("fetch.error", performance.now() - startedAt, {
      method: options.method,
      path: telemetryPath,
      status: "network_error",
    });
    const errorInput: ConstructorParameters<typeof CrmApiError>[0] = {
      message: "CRM API is unavailable.",
      status: 503,
      kind: "upstream_unavailable",
    };
    if (correlationId) {
      errorInput.correlationId = correlationId;
    }
    throw new CrmApiError(errorInput);
  }
  crmApiPerformance.record("fetch", performance.now() - startedAt, {
    method: options.method,
    path: telemetryPath,
    status: String(response.status),
    serverTiming: response.headers.get("server-timing") ?? "none",
  });

  const payload = await parseResponsePayload(response);
  const responseCorrelationId = getCorrelationIdFromHeaders(response.headers) ?? correlationId;

  if (!response.ok) {
    const problem = normalizeProblemDetails(payload);
    const fallbackMessage =
      typeof payload === "string" && payload.trim().length > 0
        ? payload
        : "CRM API request failed.";

    const errorInput: ConstructorParameters<typeof CrmApiError>[0] = {
      message: problem?.detail ?? problem?.title ?? fallbackMessage,
      status: response.status,
      kind: statusToCrmApiErrorKind(response.status),
    };

    if (problem) {
      errorInput.problem = problem;
    }

    if (responseCorrelationId) {
      errorInput.correlationId = responseCorrelationId;
    }

    throw new CrmApiError(errorInput);
  }

  if (response.status === 204 || payload === null) {
    return undefined as TResponse;
  }

  return payload as TResponse;
}

function parseFileNameFromDisposition(contentDisposition: string | null): string {
  if (!contentDisposition) {
    return "download.bin";
  }

  const utf8Match = /filename\*=UTF-8''([^;]+)/i.exec(contentDisposition);
  if (utf8Match?.[1]) {
    return decodeURIComponent(utf8Match[1]);
  }

  const simpleMatch = /filename="?([^"]+)"?/i.exec(contentDisposition);
  if (simpleMatch?.[1]) {
    return simpleMatch[1];
  }

  return "download.bin";
}

async function requestDownload(options: RequestOptions): Promise<CrmApiDownloadPayload> {
  const pathWithQuery = options.query ? withQuery(options.path, options.query) : options.path;
  const requestUrl = joinCrmApiPath(pathWithQuery);
  const correlationId = options.correlationId;
  const headers = buildHeaders(options.authContext, correlationId);
  const signal = createTimeoutSignal(
    options.timeoutMs ?? getCrmApiConfig().defaultTimeoutMs,
    options.signal,
  );

  const response = await fetch(requestUrl, {
    method: options.method,
    headers,
    cache: "no-store",
    signal,
    redirect: "manual",
  });
  const responseCorrelationId = getCorrelationIdFromHeaders(response.headers) ?? correlationId;

  if (!response.ok) {
    const payload = await parseResponsePayload(response);
    const problem = normalizeProblemDetails(payload);
    const errorInput: ConstructorParameters<typeof CrmApiError>[0] = {
      message: problem?.detail ?? problem?.title ?? "CRM API request failed.",
      status: response.status,
      kind: statusToCrmApiErrorKind(response.status),
    };
    if (problem) errorInput.problem = problem;
    if (responseCorrelationId) errorInput.correlationId = responseCorrelationId;
    throw new CrmApiError(errorInput);
  }

  const bytes = new Uint8Array(await response.arrayBuffer());
  return {
    bytes,
    contentType: response.headers.get("content-type") ?? "application/octet-stream",
    fileName: parseFileNameFromDisposition(response.headers.get("content-disposition")),
  };
}

function normalizePagedResult<TItem>(payload: unknown): CrmPagedResult<TItem> {
  const fallback: CrmPagedResult<TItem> = {
    items: [],
    totalCount: 0,
    pageNumber: 1,
    pageSize: 20,
    totalPages: 0,
  };

  if (!payload || typeof payload !== "object") {
    return fallback;
  }

  const candidate = payload as Record<string, unknown>;
  const items = Array.isArray(candidate.items) ? (candidate.items as TItem[]) : fallback.items;
  const totalCount =
    typeof candidate.totalCount === "number" ? candidate.totalCount : fallback.totalCount;
  const pageNumber =
    typeof candidate.pageNumber === "number" ? candidate.pageNumber : fallback.pageNumber;
  const pageSize = typeof candidate.pageSize === "number" ? candidate.pageSize : fallback.pageSize;
  const totalPages =
    typeof candidate.totalPages === "number"
      ? candidate.totalPages
      : pageSize > 0
        ? Math.ceil(totalCount / pageSize)
        : 0;

  return {
    items,
    totalCount,
    pageNumber,
    pageSize,
    totalPages,
  };
}

function listQueryToRecord(
  query?: CrmListQuery,
): Record<string, string | number | boolean | undefined> {
  if (!query) {
    return {};
  }

  const params = listQueryToSearchParams(query);
  return Object.fromEntries(params.entries());
}

export const crmApiClient = {
  listCustomers(query: CrmListQuery = {}, options: CrmApiRequestOptions = {}) {
    return request<unknown>({
      method: crmApiEndpoints.customersList.method,
      path: crmApiEndpoints.customersList.path,
      query: listQueryToRecord(query),
      ...options,
    }).then(normalizePagedResult<CustomerListItemDto>);
  },

  getCustomerById(customerId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.customersDetail(customerId);
    return request<CustomerDetailDto>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  createCustomer(input: CustomerUpsertRequest, options: CrmApiRequestOptions = {}) {
    return request<CustomerDetailDto>({
      method: crmApiEndpoints.customersCreate.method,
      path: crmApiEndpoints.customersCreate.path,
      body: input,
      ...options,
    });
  },

  updateCustomer(
    customerId: string,
    input: CustomerUpsertRequest,
    options: CrmApiRequestOptions = {},
  ) {
    const endpoint = crmApiEndpoints.customersUpdate(customerId);
    return request<CustomerDetailDto>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  deleteCustomer(customerId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.customersDelete(customerId);
    return request<void>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  uploadCustomerImage(customerId: string, formData: FormData, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.customersImage(customerId);
    return request<CustomerDetailDto>({
      method: endpoint.method,
      path: endpoint.path,
      body: formData,
      ...options,
    });
  },

  removeCustomerImage(customerId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.customersImageDelete(customerId);
    return request<CustomerDetailDto>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  findCustomerDuplicates(customerId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.customerDuplicates(customerId);
    return request<CustomerDuplicateWarningDto[]>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  listCustomerContacts(
    customerId: string,
    query: CrmListQuery = {},
    options: CrmApiRequestOptions = {},
  ) {
    const endpoint = crmApiEndpoints.customerContacts(customerId);
    return request<unknown>({
      method: endpoint.method,
      path: endpoint.path,
      query: listQueryToRecord(query),
      ...options,
    }).then(normalizePagedResult<ContactListItemDto>);
  },

  getCustomer360(customerId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.customer360(customerId);
    return request<Customer360Dto>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  getCustomerConsents(customerId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.customerConsents(customerId);
    return request<CustomerConsentDto[]>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  getCustomerHierarchy(customerId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.customerHierarchy(customerId);
    return request<CustomerAccountHierarchyDto>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  addCustomerHierarchyNode(
    input: AddAccountHierarchyNodeRequest,
    options: CrmApiRequestOptions = {},
  ) {
    return request<string>({
      method: crmApiEndpoints.customerHierarchyAdd.method,
      path: crmApiEndpoints.customerHierarchyAdd.path,
      body: input,
      ...options,
    });
  },

  moveCustomerHierarchyNode(
    nodeId: string,
    input: MoveAccountHierarchyNodeRequest,
    options: CrmApiRequestOptions = {},
  ) {
    const endpoint = crmApiEndpoints.customerHierarchyMove(nodeId);
    return request<string>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  removeCustomerHierarchyNode(nodeId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.customerHierarchyRemove(nodeId);
    return request<void>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  upsertCustomerConsent(
    customerId: string,
    input: UpsertCustomerConsentRequest,
    options: CrmApiRequestOptions = {},
  ) {
    const endpoint = crmApiEndpoints.customerConsentUpsert(customerId);
    return request<string>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  revokeCustomerConsent(
    customerId: string,
    consentId: string,
    input: RevokeCustomerConsentRequest,
    options: CrmApiRequestOptions = {},
  ) {
    const endpoint = crmApiEndpoints.customerConsentRevoke(customerId, consentId);
    return request<void>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  changeCustomerLifecycleStage(
    customerId: string,
    input: ChangeCustomerLifecycleStageRequest,
    options: CrmApiRequestOptions = {},
  ) {
    const endpoint = crmApiEndpoints.customerLifecycleStage(customerId);
    return request<void>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  recalculateCustomerDataQuality(customerId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.customerDataQualityRecalculate(customerId);
    return request<number>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  recalculateCustomerRelationshipHealth(customerId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.customerRelationshipHealthRecalculate(customerId);
    return request<number>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  getCustomerMergePreview(
    masterCustomerId: string,
    duplicateCustomerId: string,
    options: CrmApiRequestOptions = {},
  ) {
    return request<CustomerMergePreviewDto>({
      method: crmApiEndpoints.customerMergePreview.method,
      path: crmApiEndpoints.customerMergePreview.path,
      query: { masterCustomerId, duplicateCustomerId },
      ...options,
    });
  },

  mergeCustomers(input: MergeCustomersRequest, options: CrmApiRequestOptions = {}) {
    return request<string>({
      method: crmApiEndpoints.customerMerge.method,
      path: crmApiEndpoints.customerMerge.path,
      body: input,
      ...options,
    });
  },

  getCustomerAuditTimeline(
    customerId: string,
    query: { page?: number; pageSize?: number } = {},
    options: CrmApiRequestOptions = {},
  ) {
    const endpoint = crmApiEndpoints.customerAuditTimeline(customerId);
    return request<CustomerAuditEventDto[]>({
      method: endpoint.method,
      path: endpoint.path,
      query,
      ...options,
    });
  },

  shareCustomerRecord(
    customerId: string,
    input: ShareCustomerRecordRequest,
    options: CrmApiRequestOptions = {},
  ) {
    const endpoint = crmApiEndpoints.customerShare(customerId);
    return request<string>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  revokeCustomerShare(customerId: string, shareId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.customerShareRevoke(customerId, shareId);
    return request<void>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  removeCustomerStakeholder(
    customerId: string,
    stakeholderId: string,
    options: CrmApiRequestOptions = {},
  ) {
    const endpoint = crmApiEndpoints.customerStakeholderRemove(customerId, stakeholderId);
    return request<void>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  searchCustomers(term: string, take = 20, options: CrmApiRequestOptions = {}) {
    return request<CustomerSearchResultDto[]>({
      method: crmApiEndpoints.customersSearch.method,
      path: crmApiEndpoints.customersSearch.path,
      query: { term, take },
      ...options,
    });
  },

  createCustomerImportBatch(
    input: CreateCustomerImportBatchRequest,
    options: CrmApiRequestOptions = {},
  ) {
    return request<string>({
      method: crmApiEndpoints.customerImportBatchesCreate.method,
      path: crmApiEndpoints.customerImportBatchesCreate.path,
      body: input,
      ...options,
    });
  },

  listCustomerImportBatches(take = 50, options: CrmApiRequestOptions = {}) {
    return request<CustomerImportBatchDto[]>({
      method: crmApiEndpoints.customerImportBatchesList.method,
      path: crmApiEndpoints.customerImportBatchesList.path,
      query: { take },
      ...options,
    });
  },

  getCustomerImportBatch(batchId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.customerImportBatchDetail(batchId);
    return request<CustomerImportBatchDto>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  previewCustomerImportBatch(batchId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.customerImportBatchPreview(batchId);
    return request<CustomerImportBatchDto>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  validateCustomerImportBatch(batchId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.customerImportBatchValidate(batchId);
    return request<CustomerImportBatchDto>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  commitCustomerImportBatch(
    batchId: string,
    input: CommitCustomerImportBatchRequest,
    options: CrmApiRequestOptions = {},
  ) {
    const endpoint = crmApiEndpoints.customerImportBatchCommit(batchId);
    return request<CustomerImportBatchDto>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  cancelCustomerImportBatch(
    batchId: string,
    input: CancelCustomerImportBatchRequest,
    options: CrmApiRequestOptions = {},
  ) {
    const endpoint = crmApiEndpoints.customerImportBatchCancel(batchId);
    return request<void>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  markCustomerVip(customerId: string, isVip: boolean, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.customerMarkVip(customerId);
    return request<void>({
      method: endpoint.method,
      path: endpoint.path,
      query: { isVip },
      ...options,
    });
  },

  listCompanies(query: CrmListQuery = {}, options: CrmApiRequestOptions = {}) {
    return request<unknown>({
      method: crmApiEndpoints.companiesList.method,
      path: crmApiEndpoints.companiesList.path,
      query: listQueryToRecord(query),
      ...options,
    }).then(normalizePagedResult<CompanyListItemDto>);
  },

  getCompanyById(companyId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.companiesDetail(companyId);
    return request<CompanyDetailDto>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  createCompany(input: CompanyUpsertRequest, options: CrmApiRequestOptions = {}) {
    return request<CompanyDetailDto>({
      method: crmApiEndpoints.companiesCreate.method,
      path: crmApiEndpoints.companiesCreate.path,
      body: input,
      ...options,
    });
  },

  updateCompany(
    companyId: string,
    input: CompanyUpsertRequest,
    options: CrmApiRequestOptions = {},
  ) {
    const endpoint = crmApiEndpoints.companiesUpdate(companyId);
    return request<CompanyDetailDto>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  activateCompany(companyId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.companiesActivate(companyId);
    return request<void>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  deactivateCompany(companyId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.companiesDeactivate(companyId);
    return request<void>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  deleteCompany(companyId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.companiesDelete(companyId);
    return request<void>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  uploadCompanyLogo(companyId: string, formData: FormData, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.companiesLogo(companyId);
    return request<CompanyDetailDto>({
      method: endpoint.method,
      path: endpoint.path,
      body: formData,
      ...options,
    });
  },

  removeCompanyLogo(companyId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.companiesLogoDelete(companyId);
    return request<CompanyDetailDto>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  listContacts(query: CrmListQuery = {}, options: CrmApiRequestOptions = {}) {
    return request<unknown>({
      method: crmApiEndpoints.contactsList.method,
      path: crmApiEndpoints.contactsList.path,
      query: listQueryToRecord(query),
      ...options,
    }).then(normalizePagedResult<ContactListItemDto>);
  },

  listTrashItems(query: CrmListQuery = {}, options: CrmApiRequestOptions = {}) {
    return request<unknown>({
      method: crmApiEndpoints.trashList.method,
      path: crmApiEndpoints.trashList.path,
      query: listQueryToRecord(query),
      ...options,
    }).then(normalizePagedResult<GlobalTrashListItemDto>);
  },

  restoreTrashItem(trashItemId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.trashRestore(trashItemId);
    return request<void>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  getContactById(contactId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.contactsDetail(contactId);
    return request<ContactDetailDto>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  listLeads(query: CrmListQuery = {}, options: CrmApiRequestOptions = {}) {
    return request<unknown>({
      method: crmApiEndpoints.leadsList.method,
      path: crmApiEndpoints.leadsList.path,
      query: listQueryToRecord(query),
      ...options,
    }).then(normalizePagedResult<LeadListItemDto>);
  },

  getLeadById(leadId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.leadsDetail(leadId);
    return request<LeadDetailDto>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  createLead(input: LeadUpsertRequest, options: CrmApiRequestOptions = {}) {
    return request<LeadDetailDto>({
      method: crmApiEndpoints.leadsCreate.method,
      path: crmApiEndpoints.leadsCreate.path,
      body: input,
      ...options,
    });
  },

  updateLead(leadId: string, input: LeadUpdateRequest, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.leadsUpdate(leadId);
    return request<LeadDetailDto>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  deleteLead(leadId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.leadsDelete(leadId);
    return request<void>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  getLeadWorkspace(leadId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.leadsWorkspace(leadId);
    return request<LeadWorkspaceDto>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  getLeadTimeline(leadId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.leadsTimeline(leadId);
    return request<LeadTimelineEventDto[]>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  assignLeadOwner(
    leadId: string,
    input: AssignLeadOwnerRequest,
    options: CrmApiRequestOptions = {},
  ) {
    const endpoint = crmApiEndpoints.leadsAssignOwner(leadId);
    return request<void>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  changeLeadStatus(
    leadId: string,
    input: ChangeLeadStatusRequest,
    options: CrmApiRequestOptions = {},
  ) {
    const endpoint = crmApiEndpoints.leadsChangeStatus(leadId);
    return request<void>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  scheduleLeadNextContact(
    leadId: string,
    input: ScheduleLeadNextContactRequest,
    options: CrmApiRequestOptions = {},
  ) {
    const endpoint = crmApiEndpoints.leadsScheduleNextContact(leadId);
    return request<void>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  upsertLeadScore(
    leadId: string,
    input: UpsertLeadScoreRequest,
    options: CrmApiRequestOptions = {},
  ) {
    const endpoint = crmApiEndpoints.leadsUpsertScore(leadId);
    return request<LeadScoreDto>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  upsertLeadQualification(
    leadId: string,
    input: UpsertLeadQualificationRequest,
    options: CrmApiRequestOptions = {},
  ) {
    const endpoint = crmApiEndpoints.leadsUpsertQualification(leadId);
    return request<void>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  convertLeadToCustomer(
    leadId: string,
    input: ConvertLeadToCustomerRequest,
    options: CrmApiRequestOptions = {},
  ) {
    const endpoint = crmApiEndpoints.leadsConvert(leadId);
    return request<LeadConversionResultDto>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  captureLead(input: LeadCaptureRequest, options: CrmApiRequestOptions = {}) {
    return request<LeadCaptureResultDto>({
      method: crmApiEndpoints.leadsCapture.method,
      path: crmApiEndpoints.leadsCapture.path,
      body: input,
      ...options,
    });
  },

  bulkAssignLeadsOwner(input: BulkAssignLeadOwnerRequest, options: CrmApiRequestOptions = {}) {
    return request<{ affected: number }>({
      method: crmApiEndpoints.leadsBulkAssignOwner.method,
      path: crmApiEndpoints.leadsBulkAssignOwner.path,
      body: input,
      ...options,
    });
  },

  bulkDeleteLeads(input: BulkLeadIdsRequest, options: CrmApiRequestOptions = {}) {
    return request<{ affected: number }>({
      method: crmApiEndpoints.leadsBulkDelete.method,
      path: crmApiEndpoints.leadsBulkDelete.path,
      body: input,
      ...options,
    });
  },

  listOpportunities(query: CrmListQuery = {}, options: CrmApiRequestOptions = {}) {
    return request<unknown>({
      method: crmApiEndpoints.opportunitiesList.method,
      path: crmApiEndpoints.opportunitiesList.path,
      query: listQueryToRecord(query),
      ...options,
    }).then(normalizePagedResult<OpportunityListItemDto>);
  },

  listDeals(query: CrmListQuery = {}, options: CrmApiRequestOptions = {}) {
    return request<unknown>({
      method: crmApiEndpoints.dealsList.method,
      path: crmApiEndpoints.dealsList.path,
      query: listQueryToRecord(query),
      ...options,
    }).then(normalizePagedResult<DealListItemDto>);
  },

  listQuotes(query: CrmListQuery = {}, options: CrmApiRequestOptions = {}) {
    return request<unknown>({
      method: crmApiEndpoints.quotesList.method,
      path: crmApiEndpoints.quotesList.path,
      query: listQueryToRecord(query),
      ...options,
    }).then(normalizePagedResult<QuoteListItemDto>);
  },

  listTickets(query: CrmListQuery = {}, options: CrmApiRequestOptions = {}) {
    return request<unknown>({
      method: crmApiEndpoints.ticketsList.method,
      path: crmApiEndpoints.ticketsList.path,
      query: listQueryToRecord(query),
      ...options,
    }).then(normalizePagedResult<TicketListItemDto>);
  },

  getTicketById(ticketId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.ticketsDetail(ticketId);
    return request<TicketDetailDto>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  createTicket(input: TicketUpsertRequest, options: CrmApiRequestOptions = {}) {
    return request<TicketDetailDto>({
      method: crmApiEndpoints.ticketsCreate.method,
      path: crmApiEndpoints.ticketsCreate.path,
      body: input,
      ...options,
    });
  },

  updateTicket(ticketId: string, input: TicketUpdateRequest, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.ticketsUpdate(ticketId);
    return request<TicketDetailDto>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  deleteTicket(ticketId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.ticketsDelete(ticketId);
    return request<void>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  listProductCatalogItems(query: CrmListQuery = {}, options: CrmApiRequestOptions = {}) {
    return request<unknown>({
      method: crmApiEndpoints.productCatalogList.method,
      path: crmApiEndpoints.productCatalogList.path,
      query: listQueryToRecord(query),
      ...options,
    }).then(normalizePagedResult<ProductCatalogItemDto>);
  },

  getProductCatalogItemById(productId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.productCatalogDetail(productId);
    return request<ProductCatalogItemDto>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  listProductCatalogItemImages(productId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.productCatalogImages(productId);
    return request<ProductImageDto[]>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  createProductCatalogItem(input: ProductCatalogUpsertRequest, options: CrmApiRequestOptions = {}) {
    return request<ProductCatalogItemDto>({
      method: crmApiEndpoints.productCatalogCreate.method,
      path: crmApiEndpoints.productCatalogCreate.path,
      body: input,
      ...options,
    });
  },

  updateProductCatalogItem(
    productId: string,
    input: ProductCatalogUpsertRequest,
    options: CrmApiRequestOptions = {},
  ) {
    const endpoint = crmApiEndpoints.productCatalogUpdate(productId);
    return request<ProductCatalogItemDto>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  deleteProductCatalogItem(productId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.productCatalogDelete(productId);
    return request<void>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  bulkDeleteProductCatalogItems(
    input: BulkCatalogItemIdsRequest,
    options: CrmApiRequestOptions = {},
  ) {
    return request<CatalogBulkOperationResultDto>({
      method: crmApiEndpoints.productCatalogBulkDelete.method,
      path: crmApiEndpoints.productCatalogBulkDelete.path,
      body: input,
      ...options,
    });
  },

  bulkSetProductCatalogItemsActiveState(
    input: BulkSetActiveStateRequest,
    options: CrmApiRequestOptions = {},
  ) {
    return request<CatalogBulkOperationResultDto>({
      method: crmApiEndpoints.productCatalogBulkSetActiveState.method,
      path: crmApiEndpoints.productCatalogBulkSetActiveState.path,
      body: input,
      ...options,
    });
  },

  setProductCatalogItemActiveState(
    productId: string,
    input: ProductCatalogActiveStateRequest,
    options: CrmApiRequestOptions = {},
  ) {
    const endpoint = crmApiEndpoints.productCatalogSetActiveState(productId);
    return request<ProductCatalogItemDto>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  getProductCatalogLookups(options: CrmApiRequestOptions = {}) {
    return request<ProductCatalogLookupsDto>({
      method: crmApiEndpoints.productCatalogLookups.method,
      path: crmApiEndpoints.productCatalogLookups.path,
      ...options,
    });
  },

  getProductCatalogMeta(options: CrmApiRequestOptions = {}) {
    return request<ProductCatalogMetaDto>({
      method: crmApiEndpoints.productCatalogMeta.method,
      path: crmApiEndpoints.productCatalogMeta.path,
      ...options,
    });
  },

  getProductCatalogStats(options: CrmApiRequestOptions = {}) {
    return request<ProductCatalogStatsDto>({
      method: crmApiEndpoints.productCatalogStats.method,
      path: crmApiEndpoints.productCatalogStats.path,
      ...options,
    });
  },

  downloadProductCatalogExport(query: CrmListQuery = {}, options: CrmApiRequestOptions = {}) {
    return requestDownload({
      method: crmApiEndpoints.productCatalogExport.method,
      path: crmApiEndpoints.productCatalogExport.path,
      query: listQueryToRecord(query),
      ...options,
    });
  },

  downloadProductCatalogTemplate(options: CrmApiRequestOptions = {}) {
    return requestDownload({
      method: crmApiEndpoints.productCatalogTemplate.method,
      path: crmApiEndpoints.productCatalogTemplate.path,
      ...options,
    });
  },

  setProductCatalogImagePrimary(
    productId: string,
    productImageId: string,
    options: CrmApiRequestOptions = {},
  ) {
    const endpoint = crmApiEndpoints.productCatalogSetPrimaryImage(productId, productImageId);
    return request<void>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  deleteProductCatalogImage(
    productId: string,
    productImageId: string,
    options: CrmApiRequestOptions = {},
  ) {
    const endpoint = crmApiEndpoints.productCatalogDeleteImage(productId, productImageId);
    return request<void>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  listProductCatalogCategories(query: CrmListQuery = {}, options: CrmApiRequestOptions = {}) {
    return request<unknown>({
      method: crmApiEndpoints.productCatalogCategoriesList.method,
      path: crmApiEndpoints.productCatalogCategoriesList.path,
      query: listQueryToRecord(query),
      ...options,
    }).then(normalizePagedResult<ProductCatalogCategoryDto>);
  },

  getProductCatalogCategoryById(categoryId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.productCatalogCategoriesDetail(categoryId);
    return request<ProductCatalogCategoryDto>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  createProductCatalogCategory(
    input: ProductCatalogCategoryUpsertRequest,
    options: CrmApiRequestOptions = {},
  ) {
    return request<ProductCatalogCategoryDto>({
      method: crmApiEndpoints.productCatalogCategoriesCreate.method,
      path: crmApiEndpoints.productCatalogCategoriesCreate.path,
      body: input,
      ...options,
    });
  },

  updateProductCatalogCategory(
    categoryId: string,
    input: ProductCatalogCategoryUpsertRequest,
    options: CrmApiRequestOptions = {},
  ) {
    const endpoint = crmApiEndpoints.productCatalogCategoriesUpdate(categoryId);
    return request<ProductCatalogCategoryDto>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  deleteProductCatalogCategory(categoryId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.productCatalogCategoriesDelete(categoryId);
    return request<void>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  setProductCatalogCategoryActiveState(
    categoryId: string,
    input: ProductCatalogCategoryActiveStateRequest,
    options: CrmApiRequestOptions = {},
  ) {
    const endpoint = crmApiEndpoints.productCatalogCategoriesSetActiveState(categoryId);
    return request<ProductCatalogCategoryDto>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  getQuoteById(quoteId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.quotesDetail(quoteId);
    return request<QuoteDetailDto>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  getQuoteWorkspace(quoteId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.quotesWorkspace(quoteId);
    return request<QuoteWorkspaceDto>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  getQuoteTimeline(quoteId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.quotesTimeline(quoteId);
    return request<QuoteTimelineEventDto[]>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  validateQuoteConfiguration(quoteId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.quotesValidation(quoteId);
    return request<CpqValidationResultDto>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  getQuoteCpqWorkspace(options: CrmApiRequestOptions = {}) {
    return request<CpqWorkspaceDto>({
      method: crmApiEndpoints.quotesCpqWorkspace.method,
      path: crmApiEndpoints.quotesCpqWorkspace.path,
      ...options,
    });
  },

  createQuote(input: QuoteUpsertRequest, options: CrmApiRequestOptions = {}) {
    return request<QuoteDetailDto>({
      method: crmApiEndpoints.quotesCreate.method,
      path: crmApiEndpoints.quotesCreate.path,
      body: input,
      ...options,
    });
  },

  updateQuote(quoteId: string, input: QuoteUpdateRequest, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.quotesUpdate(quoteId);
    return request<QuoteDetailDto>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  deleteQuote(quoteId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.quotesDelete(quoteId);
    return request<void>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  submitQuote(quoteId: string, input: QuoteNoteRequest, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.quotesSubmit(quoteId);
    return request<void>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  approveQuote(quoteId: string, input: QuoteNoteRequest, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.quotesApprove(quoteId);
    return request<void>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  rejectQuote(quoteId: string, input: QuoteReasonRequest, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.quotesReject(quoteId);
    return request<void>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  markQuoteSent(quoteId: string, input: QuoteDateNoteRequest, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.quotesMarkSent(quoteId);
    return request<void>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  acceptQuote(quoteId: string, input: QuoteDateNoteRequest, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.quotesAccept(quoteId);
    return request<void>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  declineQuote(quoteId: string, input: QuoteDeclineRequest, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.quotesDecline(quoteId);
    return request<void>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  expireQuote(quoteId: string, input: QuoteDateNoteRequest, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.quotesExpire(quoteId);
    return request<void>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  createQuoteRevision(
    quoteId: string,
    input: CreateQuoteRevisionRequest,
    options: CrmApiRequestOptions = {},
  ) {
    const endpoint = crmApiEndpoints.quotesCreateRevision(quoteId);
    return request<QuoteDetailDto>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  listProposalTemplates(isActive?: boolean | null, options: CrmApiRequestOptions = {}) {
    return request<ProposalTemplateDto[]>({
      method: crmApiEndpoints.quotesProposalTemplates.method,
      path: crmApiEndpoints.quotesProposalTemplates.path,
      query: { isActive: isActive ?? undefined },
      ...options,
    });
  },

  createProposalTemplate(input: ProposalTemplateRequest, options: CrmApiRequestOptions = {}) {
    return request<ProposalTemplateDto>({
      method: crmApiEndpoints.quotesCreateProposalTemplate.method,
      path: crmApiEndpoints.quotesCreateProposalTemplate.path,
      body: input,
      ...options,
    });
  },

  updateProposalTemplate(
    templateId: string,
    input: ProposalTemplateRequest,
    options: CrmApiRequestOptions = {},
  ) {
    const endpoint = crmApiEndpoints.quotesUpdateProposalTemplate(templateId);
    return request<ProposalTemplateDto>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  deleteProposalTemplate(templateId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.quotesDeleteProposalTemplate(templateId);
    return request<void>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  runGuidedSelling(input: RunGuidedSellingRequest, options: CrmApiRequestOptions = {}) {
    return request<GuidedSellingRecommendationDto[]>({
      method: crmApiEndpoints.quotesRunGuidedSelling.method,
      path: crmApiEndpoints.quotesRunGuidedSelling.path,
      body: input,
      ...options,
    });
  },

  upsertGuidedSellingPlaybook(
    playbookId: string | null,
    input: UpsertGuidedSellingPlaybookRequest,
    options: CrmApiRequestOptions = {},
  ) {
    const endpoint = crmApiEndpoints.quotesUpsertGuidedSellingPlaybook(playbookId);
    return request<GuidedSellingPlaybookDto>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  upsertProductBundle(
    bundleId: string | null,
    input: UpsertProductBundleRequest,
    options: CrmApiRequestOptions = {},
  ) {
    const endpoint = crmApiEndpoints.quotesUpsertProductBundle(bundleId);
    return request<ProductBundleDto>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  upsertProductRule(
    ruleId: string | null,
    input: UpsertProductRuleRequest,
    options: CrmApiRequestOptions = {},
  ) {
    const endpoint = crmApiEndpoints.quotesUpsertProductRule(ruleId);
    return request<ProductRuleDto>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  getDealById(dealId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.dealsDetail(dealId);
    return request<DealDetailDto>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  getDealWorkspace(dealId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.dealsWorkspace(dealId);
    return request<DealWorkspaceDto>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  getDealTimeline(dealId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.dealsTimeline(dealId);
    return request<DealOutcomeHistoryDto[]>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  createDeal(input: DealUpsertRequest, options: CrmApiRequestOptions = {}) {
    return request<DealDetailDto>({
      method: crmApiEndpoints.dealsCreate.method,
      path: crmApiEndpoints.dealsCreate.path,
      body: input,
      ...options,
    });
  },

  updateDeal(dealId: string, input: DealUpsertRequest, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.dealsUpdate(dealId);
    return request<DealDetailDto>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  deleteDeal(dealId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.dealsDelete(dealId);
    return request<void>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  assignDealOwner(
    dealId: string,
    input: AssignDealOwnerRequest,
    options: CrmApiRequestOptions = {},
  ) {
    const endpoint = crmApiEndpoints.dealsAssignOwner(dealId);
    return request<void>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  bulkAssignDealsOwner(input: BulkAssignDealOwnerRequest, options: CrmApiRequestOptions = {}) {
    return request<DealBulkOperationResultDto>({
      method: crmApiEndpoints.dealsBulkAssignOwner.method,
      path: crmApiEndpoints.dealsBulkAssignOwner.path,
      body: input,
      ...options,
    });
  },

  markDealWon(dealId: string, input: DealOutcomeRequest, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.dealsMarkWon(dealId);
    return request<void>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  markDealLost(dealId: string, input: DealOutcomeRequest, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.dealsMarkLost(dealId);
    return request<void>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  reopenDeal(dealId: string, input: DealOutcomeRequest, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.dealsReopen(dealId);
    return request<void>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  getDealWinLossSummary(query: DealWinLossSummaryQuery = {}, options: CrmApiRequestOptions = {}) {
    return request<DealWinLossSummaryDto>({
      method: crmApiEndpoints.dealsWinLossSummary.method,
      path: crmApiEndpoints.dealsWinLossSummary.path,
      query: {
        from: query.from ?? undefined,
        to: query.to ?? undefined,
        ownerUserId: query.ownerUserId ?? undefined,
      },
      ...options,
    });
  },

  listDealLostReasons(options: CrmApiRequestOptions = {}) {
    return request<DealLostReasonDto[]>({
      method: crmApiEndpoints.dealsLostReasons.method,
      path: crmApiEndpoints.dealsLostReasons.path,
      ...options,
    });
  },

  upsertDealWinLossReview(
    dealId: string,
    input: DealReviewUpsertRequest,
    options: CrmApiRequestOptions = {},
  ) {
    const endpoint = crmApiEndpoints.dealsUpsertWinLossReview(dealId);
    return request<DealWinLossReviewDto>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  getOpportunityById(opportunityId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.opportunitiesDetail(opportunityId);
    return request<OpportunityDetailDto>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  getOpportunityWorkspace(opportunityId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.opportunitiesWorkspace(opportunityId);
    return request<OpportunityWorkspaceDto>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  getOpportunityTimeline(opportunityId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.opportunitiesTimeline(opportunityId);
    return request<OpportunityTimelineEventDto[]>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  getOpportunityPipelineBoard(
    query: { ownerUserId?: string; search?: string; maxItemsPerStage?: number } = {},
    options: CrmApiRequestOptions = {},
  ) {
    return request<PipelineBoardDto>({
      method: crmApiEndpoints.opportunitiesPipelineBoard.method,
      path: crmApiEndpoints.opportunitiesPipelineBoard.path,
      query,
      ...options,
    });
  },

  listOpportunityLostReasons(options: CrmApiRequestOptions = {}) {
    return request<OpportunityLostReasonDto[]>({
      method: crmApiEndpoints.opportunitiesLostReasons.method,
      path: crmApiEndpoints.opportunitiesLostReasons.path,
      ...options,
    });
  },

  createOpportunity(input: OpportunityUpsertRequest, options: CrmApiRequestOptions = {}) {
    return request<OpportunityDetailDto>({
      method: crmApiEndpoints.opportunitiesCreate.method,
      path: crmApiEndpoints.opportunitiesCreate.path,
      body: input,
      ...options,
    });
  },

  updateOpportunity(
    opportunityId: string,
    input: OpportunityUpdateRequest,
    options: CrmApiRequestOptions = {},
  ) {
    const endpoint = crmApiEndpoints.opportunitiesUpdate(opportunityId);
    return request<OpportunityDetailDto>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  assignOpportunityOwner(
    opportunityId: string,
    input: AssignOpportunityOwnerRequest,
    options: CrmApiRequestOptions = {},
  ) {
    const endpoint = crmApiEndpoints.opportunitiesAssignOwner(opportunityId);
    return request<void>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  changeOpportunityStage(
    opportunityId: string,
    input: ChangeOpportunityStageRequest,
    options: CrmApiRequestOptions = {},
  ) {
    const endpoint = crmApiEndpoints.opportunitiesChangeStage(opportunityId);
    return request<void>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  markOpportunityWon(
    opportunityId: string,
    input: MarkOpportunityWonRequest,
    options: CrmApiRequestOptions = {},
  ) {
    const endpoint = crmApiEndpoints.opportunitiesMarkWon(opportunityId);
    return request<MarkOpportunityWonResultDto>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  markOpportunityLost(
    opportunityId: string,
    input: MarkOpportunityLostRequest,
    options: CrmApiRequestOptions = {},
  ) {
    const endpoint = crmApiEndpoints.opportunitiesMarkLost(opportunityId);
    return request<void>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  addOpportunityContact(
    opportunityId: string,
    input: AddOpportunityContactRequest,
    options: CrmApiRequestOptions = {},
  ) {
    const endpoint = crmApiEndpoints.opportunitiesAddContact(opportunityId);
    return request<OpportunityContactDto>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  addOpportunityProduct(
    opportunityId: string,
    input: AddOpportunityProductRequest,
    options: CrmApiRequestOptions = {},
  ) {
    const endpoint = crmApiEndpoints.opportunitiesAddProduct(opportunityId);
    return request<OpportunityProductDto>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  listOpportunityQuotes(opportunityId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.opportunitiesQuotes(opportunityId);
    return request<OpportunityQuoteDetailDto[]>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  createOpportunityQuote(
    opportunityId: string,
    input: CreateOpportunityQuoteRequest,
    options: CrmApiRequestOptions = {},
  ) {
    const endpoint = crmApiEndpoints.opportunitiesCreateQuote(opportunityId);
    return request<OpportunityQuoteDetailDto>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  bulkAssignOpportunitiesOwner(
    input: BulkAssignOpportunityOwnerRequest,
    options: CrmApiRequestOptions = {},
  ) {
    return request<OpportunityBulkOperationResultDto>({
      method: crmApiEndpoints.opportunitiesBulkAssignOwner.method,
      path: crmApiEndpoints.opportunitiesBulkAssignOwner.path,
      body: input,
      ...options,
    });
  },

  bulkChangeOpportunitiesStage(
    input: BulkChangeOpportunityStageRequest,
    options: CrmApiRequestOptions = {},
  ) {
    return request<OpportunityBulkOperationResultDto>({
      method: crmApiEndpoints.opportunitiesBulkChangeStage.method,
      path: crmApiEndpoints.opportunitiesBulkChangeStage.path,
      body: input,
      ...options,
    });
  },

  deleteOpportunity(opportunityId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.opportunitiesDelete(opportunityId);
    return request<void>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  listPipelines(options: CrmApiRequestOptions = {}) {
    return request<PipelineSummaryDto[]>({
      method: crmApiEndpoints.pipelinesList.method,
      path: crmApiEndpoints.pipelinesList.path,
      ...options,
    });
  },

  createPipeline(input: CreatePipelineRequest, options: CrmApiRequestOptions = {}) {
    return request<PipelineDto>({
      method: crmApiEndpoints.pipelinesCreate.method,
      path: crmApiEndpoints.pipelinesCreate.path,
      body: input,
      ...options,
    });
  },

  updatePipeline(
    pipelineId: string,
    input: UpdatePipelineRequest,
    options: CrmApiRequestOptions = {},
  ) {
    const endpoint = crmApiEndpoints.pipelinesUpdate(pipelineId);
    return request<PipelineDto>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  deletePipeline(pipelineId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.pipelinesDelete(pipelineId);
    return request<void>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  getPipelineById(pipelineId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.pipelinesDetail(pipelineId);
    return request<PipelineDto>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  getPipelineAnalytics(pipelineId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.pipelinesAnalytics(pipelineId);
    return request<PipelineAnalyticsDto>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  getPipelineBoard(
    pipelineId: string,
    query: { ownerUserId?: string } = {},
    options: CrmApiRequestOptions = {},
  ) {
    const endpoint = crmApiEndpoints.pipelinesBoard(pipelineId);
    return request<PipelineBoardDto>({
      method: endpoint.method,
      path: endpoint.path,
      ...(query.ownerUserId ? { query: { ownerUserId: query.ownerUserId } } : {}),
      ...options,
    });
  },

  moveOpportunityStage(
    opportunityId: string,
    input: PipelineStageMoveRequest,
    options: CrmApiRequestOptions = {},
  ) {
    const endpoint = crmApiEndpoints.pipelinesMoveOpportunityStage(opportunityId);
    return request<PipelineStageMoveResultDto>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  getPipelineOpportunityStageHistory(opportunityId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.pipelinesOpportunityStageHistory(opportunityId);
    return request<OpportunityStageHistoryDto[]>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  listPipelineLostReasons(options: CrmApiRequestOptions = {}) {
    return request<OpportunityLostReasonDto[]>({
      method: crmApiEndpoints.pipelineLostReasonsList.method,
      path: crmApiEndpoints.pipelineLostReasonsList.path,
      ...options,
    });
  },

  createPipelineLostReason(
    input: PipelineLostReasonUpsertRequest,
    options: CrmApiRequestOptions = {},
  ) {
    return request<OpportunityLostReasonDto>({
      method: crmApiEndpoints.pipelineLostReasonsCreate.method,
      path: crmApiEndpoints.pipelineLostReasonsCreate.path,
      body: input,
      ...options,
    });
  },

  updatePipelineLostReason(
    lostReasonId: string,
    input: PipelineLostReasonUpsertRequest,
    options: CrmApiRequestOptions = {},
  ) {
    const endpoint = crmApiEndpoints.pipelineLostReasonsUpdate(lostReasonId);
    return request<OpportunityLostReasonDto>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  getPipelineLeadConversionPreview(leadId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.pipelineLeadConversionPreview(leadId);
    return request<PipelineLeadConversionPreviewDto>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  convertPipelineLead(
    leadId: string,
    input: PipelineLeadConversionRequest,
    options: CrmApiRequestOptions = {},
  ) {
    const endpoint = crmApiEndpoints.pipelineLeadConversionConvert(leadId);
    return request<PipelineLeadConversionResultDto>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  getWorkManagementWorkspace(options: CrmApiRequestOptions = {}) {
    return request<WorkManagementWorkspaceDto>({
      method: crmApiEndpoints.workManagementWorkspace.method,
      path: crmApiEndpoints.workManagementWorkspace.path,
      ...options,
    });
  },

  listSupportInboxConnections(options: CrmApiRequestOptions = {}) {
    return request<SupportInboxConnectionDto[]>({
      method: crmApiEndpoints.supportInboxConnectionsList.method,
      path: crmApiEndpoints.supportInboxConnectionsList.path,
      ...options,
    });
  },

  createSupportInboxConnection(
    input: SupportInboxConnectionCreateRequest,
    options: CrmApiRequestOptions = {},
  ) {
    return request<SupportInboxConnectionDto>({
      method: crmApiEndpoints.supportInboxConnectionsCreate.method,
      path: crmApiEndpoints.supportInboxConnectionsCreate.path,
      body: input,
      ...options,
    });
  },

  updateSupportInboxConnection(
    connectionId: string,
    input: SupportInboxConnectionUpdateRequest,
    options: CrmApiRequestOptions = {},
  ) {
    const endpoint = crmApiEndpoints.supportInboxConnectionsUpdate(connectionId);
    return request<SupportInboxConnectionDto>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  triggerSupportInboxSync(
    connectionId: string,
    input: SupportInboxSyncRequest,
    options: CrmApiRequestOptions = {},
  ) {
    const endpoint = crmApiEndpoints.supportInboxConnectionSync(connectionId);
    return request<void>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  listSupportInboxMessages(
    query: {
      connectionId?: string;
      linkedToTicket?: boolean;
      page?: number;
      pageSize?: number;
    } = {},
    options: CrmApiRequestOptions = {},
  ) {
    return request<unknown>({
      method: crmApiEndpoints.supportInboxMessagesList.method,
      path: crmApiEndpoints.supportInboxMessagesList.path,
      query: {
        connectionId: query.connectionId,
        linkedToTicket: query.linkedToTicket,
        page: query.page,
        pageSize: query.pageSize,
      },
      ...options,
    }).then(normalizePagedResult<SupportInboxMessageDto>);
  },

  createSupportInboxRule(input: SupportInboxRuleCreateRequest, options: CrmApiRequestOptions = {}) {
    return request<void>({
      method: crmApiEndpoints.supportInboxRulesCreate.method,
      path: crmApiEndpoints.supportInboxRulesCreate.path,
      body: input,
      ...options,
    });
  },

  updateSupportInboxRule(
    ruleId: string,
    input: SupportInboxRuleUpdateRequest,
    options: CrmApiRequestOptions = {},
  ) {
    const endpoint = crmApiEndpoints.supportInboxRulesUpdate(ruleId);
    return request<void>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  listTicketSlaPolicies(options: CrmApiRequestOptions = {}) {
    return request<TicketSlaPolicyDto[]>({
      method: crmApiEndpoints.ticketSlaPoliciesList.method,
      path: crmApiEndpoints.ticketSlaPoliciesList.path,
      ...options,
    });
  },

  createTicketSlaPolicy(input: TicketSlaPolicyUpsertRequest, options: CrmApiRequestOptions = {}) {
    return request<{ id: string }>({
      method: crmApiEndpoints.ticketSlaPolicyCreate.method,
      path: crmApiEndpoints.ticketSlaPolicyCreate.path,
      body: input,
      ...options,
    });
  },

  updateTicketSlaPolicy(
    policyId: string,
    input: TicketSlaPolicyUpsertRequest,
    options: CrmApiRequestOptions = {},
  ) {
    const endpoint = crmApiEndpoints.ticketSlaPolicyUpdate(policyId);
    return request<void>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  deleteTicketSlaPolicy(policyId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.ticketSlaPolicyDelete(policyId);
    return request<void>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  listTicketSlaEscalationRules(policyId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.ticketSlaPolicyEscalationRules(policyId);
    return request<TicketSlaEscalationRuleDto[]>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  createTicketSlaEscalationRule(
    input: TicketSlaEscalationRuleUpsertRequest,
    options: CrmApiRequestOptions = {},
  ) {
    return request<{ id: string }>({
      method: crmApiEndpoints.ticketSlaEscalationRuleCreate.method,
      path: crmApiEndpoints.ticketSlaEscalationRuleCreate.path,
      body: input,
      ...options,
    });
  },

  updateTicketSlaEscalationRule(
    ruleId: string,
    input: TicketSlaEscalationRuleUpsertRequest,
    options: CrmApiRequestOptions = {},
  ) {
    const endpoint = crmApiEndpoints.ticketSlaEscalationRuleUpdate(ruleId);
    return request<void>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  getTicketSlaWorkspace(ticketId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.ticketSlaWorkspace(ticketId);
    return request<TicketSlaWorkspaceDto>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  listTicketEscalationRuns(ticketId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.ticketSlaEscalationRuns(ticketId);
    return request<TicketEscalationRunDto[]>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  attachTicketSla(input: AttachTicketSlaRequest, options: CrmApiRequestOptions = {}) {
    return request<void>({
      method: crmApiEndpoints.ticketSlaAttachToTicket.method,
      path: crmApiEndpoints.ticketSlaAttachToTicket.path,
      body: input,
      ...options,
    });
  },

  markTicketFirstResponse(
    input: MarkTicketFirstResponseRequest,
    options: CrmApiRequestOptions = {},
  ) {
    return request<void>({
      method: crmApiEndpoints.ticketSlaMarkFirstResponse.method,
      path: crmApiEndpoints.ticketSlaMarkFirstResponse.path,
      body: input,
      ...options,
    });
  },

  markTicketResolved(input: MarkTicketResolvedRequest, options: CrmApiRequestOptions = {}) {
    return request<void>({
      method: crmApiEndpoints.ticketSlaMarkResolved.method,
      path: crmApiEndpoints.ticketSlaMarkResolved.path,
      body: input,
      ...options,
    });
  },

  runDueTicketEscalations(
    input: RunDueTicketEscalationsRequest,
    options: CrmApiRequestOptions = {},
  ) {
    return request<RunDueTicketEscalationsResultDto>({
      method: crmApiEndpoints.ticketSlaRunDueEscalations.method,
      path: crmApiEndpoints.ticketSlaRunDueEscalations.path,
      body: input,
      ...options,
    });
  },

  listTicketWorkflowQueues(options: CrmApiRequestOptions = {}) {
    return request<TicketWorkflowQueueDto[]>({
      method: crmApiEndpoints.ticketWorkflowQueues.method,
      path: crmApiEndpoints.ticketWorkflowQueues.path,
      ...options,
    });
  },

  createTicketWorkflowQueue(
    input: TicketWorkflowQueueUpsertRequest,
    options: CrmApiRequestOptions = {},
  ) {
    return request<{ id: string }>({
      method: crmApiEndpoints.ticketWorkflowQueueCreate.method,
      path: crmApiEndpoints.ticketWorkflowQueueCreate.path,
      body: input,
      ...options,
    });
  },

  updateTicketWorkflowQueue(
    queueId: string,
    input: TicketWorkflowQueueUpdateRequest,
    options: CrmApiRequestOptions = {},
  ) {
    const endpoint = crmApiEndpoints.ticketWorkflowQueueUpdate(queueId);
    return request<void>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  deleteTicketWorkflowQueue(queueId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.ticketWorkflowQueueDelete(queueId);
    return request<void>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  listTicketAssignmentHistory(ticketId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.ticketWorkflowAssignmentHistory(ticketId);
    return request<TicketAssignmentHistoryDto[]>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  listTicketStatusHistory(ticketId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.ticketWorkflowStatusHistory(ticketId);
    return request<TicketStatusHistoryDto[]>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  assignTicketWorkflowQueue(
    ticketId: string,
    input: AssignTicketWorkflowQueueRequest,
    options: CrmApiRequestOptions = {},
  ) {
    const endpoint = crmApiEndpoints.ticketWorkflowAssignQueue(ticketId);
    return request<void>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  assignTicketWorkflowOwner(
    ticketId: string,
    input: AssignTicketWorkflowOwnerRequest,
    options: CrmApiRequestOptions = {},
  ) {
    const endpoint = crmApiEndpoints.ticketWorkflowAssignOwner(ticketId);
    return request<void>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  recordTicketWorkflowStatusChange(
    ticketId: string,
    input: RecordTicketWorkflowStatusChangeRequest,
    options: CrmApiRequestOptions = {},
  ) {
    const endpoint = crmApiEndpoints.ticketWorkflowRecordStatusChange(ticketId);
    return request<void>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  listContracts(options: CrmApiRequestOptions = {}) {
    return request<ContractLifecycleSummaryDto[]>({
      method: crmApiEndpoints.contractsList.method,
      path: crmApiEndpoints.contractsList.path,
      ...options,
    });
  },

  getContractById(contractId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.contractsDetail(contractId);
    return request<ContractLifecycleSummaryDto>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  createContract(input: ContractCreateRequest, options: CrmApiRequestOptions = {}) {
    return request<ContractLifecycleSummaryDto>({
      method: crmApiEndpoints.contractsCreate.method,
      path: crmApiEndpoints.contractsCreate.path,
      body: input,
      ...options,
    });
  },

  listOrders(options: CrmApiRequestOptions = {}) {
    return request<FinanceOperationsSummaryDto[]>({
      method: crmApiEndpoints.ordersList.method,
      path: crmApiEndpoints.ordersList.path,
      ...options,
    });
  },

  getOrderById(orderId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.ordersDetail(orderId);
    return request<FinanceOperationsSummaryDto>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  createOrder(input: SalesOrderCreateRequest, options: CrmApiRequestOptions = {}) {
    return request<FinanceOperationsSummaryDto>({
      method: crmApiEndpoints.ordersCreate.method,
      path: crmApiEndpoints.ordersCreate.path,
      body: input,
      ...options,
    });
  },

  listTags(options: CrmApiRequestOptions = {}) {
    return request<TagSummaryDto[]>({
      method: crmApiEndpoints.tagsList.method,
      path: crmApiEndpoints.tagsList.path,
      ...options,
    });
  },

  listTagGroups(options: CrmApiRequestOptions = {}) {
    return request<TagGroupSummaryDto[]>({
      method: crmApiEndpoints.tagGroupsList.method,
      path: crmApiEndpoints.tagGroupsList.path,
      ...options,
    });
  },

  listSmartLabelRules(options: CrmApiRequestOptions = {}) {
    return request<SmartLabelRuleSummaryDto[]>({
      method: crmApiEndpoints.smartLabelRulesList.method,
      path: crmApiEndpoints.smartLabelRulesList.path,
      ...options,
    });
  },

  listClassificationSchemes(options: CrmApiRequestOptions = {}) {
    return request<ClassificationSchemeSummaryDto[]>({
      method: crmApiEndpoints.classificationSchemesList.method,
      path: crmApiEndpoints.classificationSchemesList.path,
      ...options,
    });
  },

  createTag(input: CreateTagRequest, options: CrmApiRequestOptions = {}) {
    return request<{ id: string }>({
      method: crmApiEndpoints.tagsCreate.method,
      path: crmApiEndpoints.tagsCreate.path,
      body: input,
      ...options,
    });
  },

  createTagGroup(input: CreateTagGroupRequest, options: CrmApiRequestOptions = {}) {
    return request<{ id: string }>({
      method: crmApiEndpoints.tagGroupsCreate.method,
      path: crmApiEndpoints.tagGroupsCreate.path,
      body: input,
      ...options,
    });
  },

  createSmartLabelRule(input: CreateSmartLabelRuleRequest, options: CrmApiRequestOptions = {}) {
    return request<{ id: string }>({
      method: crmApiEndpoints.smartLabelRulesCreate.method,
      path: crmApiEndpoints.smartLabelRulesCreate.path,
      body: input,
      ...options,
    });
  },

  createClassificationScheme(
    input: CreateClassificationSchemeRequest,
    options: CrmApiRequestOptions = {},
  ) {
    return request<{ id: string }>({
      method: crmApiEndpoints.classificationSchemesCreate.method,
      path: crmApiEndpoints.classificationSchemesCreate.path,
      body: input,
      ...options,
    });
  },

  createWorkTask(input: CreateWorkTaskRequest, options: CrmApiRequestOptions = {}) {
    return request<WorkTaskDto>({
      method: crmApiEndpoints.workManagementCreateTask.method,
      path: crmApiEndpoints.workManagementCreateTask.path,
      body: input,
      ...options,
    });
  },

  listWorkTasks(query: CrmListQuery = {}, options: CrmApiRequestOptions = {}) {
    return request<unknown>({
      method: crmApiEndpoints.workManagementTasksList.method,
      path: crmApiEndpoints.workManagementTasksList.path,
      query: listQueryToRecord(query),
      ...options,
    }).then(normalizePagedResult<WorkTaskDto>);
  },

  getWorkTask(taskId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.workManagementTaskDetail(taskId);
    return request<WorkTaskDto>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  updateWorkTask(taskId: string, input: UpdateWorkTaskRequest, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.workManagementTaskUpdate(taskId);
    return request<WorkTaskDto>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  completeWorkTask(
    taskId: string,
    input: CompleteWorkTaskRequest,
    options: CrmApiRequestOptions = {},
  ) {
    const endpoint = crmApiEndpoints.workManagementTaskComplete(taskId);
    return request<WorkTaskDto>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  reopenWorkTask(taskId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.workManagementTaskReopen(taskId);
    return request<WorkTaskDto>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  assignWorkTaskOwner(
    taskId: string,
    input: AssignWorkTaskOwnerRequest,
    options: CrmApiRequestOptions = {},
  ) {
    const endpoint = crmApiEndpoints.workManagementTaskOwner(taskId);
    return request<WorkTaskDto>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  updateWorkTaskDueDate(
    taskId: string,
    input: UpdateWorkTaskDueDateRequest,
    options: CrmApiRequestOptions = {},
  ) {
    const endpoint = crmApiEndpoints.workManagementTaskDueDate(taskId);
    return request<WorkTaskDto>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  updateWorkTaskReminder(
    taskId: string,
    input: UpdateWorkTaskReminderRequest,
    options: CrmApiRequestOptions = {},
  ) {
    const endpoint = crmApiEndpoints.workManagementTaskReminder(taskId);
    return request<WorkTaskDto>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  deleteWorkTask(taskId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.workManagementTaskDelete(taskId);
    return request<void>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  scheduleWorkMeeting(input: ScheduleMeetingRequest, options: CrmApiRequestOptions = {}) {
    return request<MeetingScheduleDto>({
      method: crmApiEndpoints.workManagementScheduleMeeting.method,
      path: crmApiEndpoints.workManagementScheduleMeeting.path,
      body: input,
      ...options,
    });
  },

  listActivities(
    query: {
      type?: string;
      sourceModule?: string;
      ownerUserId?: string;
      from?: string;
      to?: string;
      page?: number;
      pageSize?: number;
    } = {},
    options: CrmApiRequestOptions = {},
  ) {
    return request<ActivityTimelineFeed>({
      method: crmApiEndpoints.activitiesList.method,
      path: crmApiEndpoints.activitiesList.path,
      query: {
        type: query.type,
        sourceModule: query.sourceModule,
        ownerUserId: query.ownerUserId,
        from: query.from,
        to: query.to,
        page: query.page,
        pageSize: query.pageSize,
      },
      ...options,
    });
  },

  getActivity(activityId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.activityDetail(activityId);
    return request<ActivityTimelineItem>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  listRelatedActivities(
    entityType: string,
    entityId: string,
    query: {
      from?: string;
      to?: string;
      page?: number;
      pageSize?: number;
    } = {},
    options: CrmApiRequestOptions = {},
  ) {
    const endpoint = crmApiEndpoints.relatedActivities(entityType, entityId);
    return request<ActivityTimelineFeed>({
      method: endpoint.method,
      path: endpoint.path,
      query: {
        from: query.from,
        to: query.to,
        page: query.page,
        pageSize: query.pageSize,
      },
      ...options,
    });
  },

  createActivity(input: CreateActivityRequest, options: CrmApiRequestOptions = {}) {
    return request<CreateActivityResponse>({
      method: crmApiEndpoints.activitiesCreate.method,
      path: crmApiEndpoints.activitiesCreate.path,
      body: input,
      ...options,
    });
  },

  createContact(input: ContactUpsertRequest, options: CrmApiRequestOptions = {}) {
    return request<ContactDetailDto>({
      method: crmApiEndpoints.contactsCreate.method,
      path: crmApiEndpoints.contactsCreate.path,
      body: input,
      ...options,
    });
  },

  updateContact(
    contactId: string,
    input: ContactUpsertRequest,
    options: CrmApiRequestOptions = {},
  ) {
    const endpoint = crmApiEndpoints.contactsUpdate(contactId);
    return request<ContactDetailDto>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  setPrimaryContact(contactId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.contactsSetPrimary(contactId);
    return request<void>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  deleteContact(contactId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.contactsDelete(contactId);
    return request<void>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  addAddressToCompany(
    companyId: string,
    input: AddressUpsertRequest,
    options: CrmApiRequestOptions = {},
  ) {
    const endpoint = crmApiEndpoints.addressesAddToCompany(companyId);
    return request<AddressDto>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  addAddressToCustomer(
    customerId: string,
    input: AddressUpsertRequest,
    options: CrmApiRequestOptions = {},
  ) {
    const endpoint = crmApiEndpoints.addressesAddToCustomer(customerId);
    return request<AddressDto>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  updateAddress(
    addressId: string,
    input: AddressUpsertRequest,
    options: CrmApiRequestOptions = {},
  ) {
    const endpoint = crmApiEndpoints.addressesUpdate(addressId);
    return request<AddressDto>({
      method: endpoint.method,
      path: endpoint.path,
      body: input,
      ...options,
    });
  },

  setDefaultAddress(addressId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.addressesSetDefault(addressId);
    return request<void>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  deleteAddress(addressId: string, options: CrmApiRequestOptions = {}) {
    const endpoint = crmApiEndpoints.addressesDelete(addressId);
    return request<void>({
      method: endpoint.method,
      path: endpoint.path,
      ...options,
    });
  },

  fetchOperationalEndpoint(
    path: string,
    query: Record<string, string | number | boolean | undefined> = {},
    options: CrmApiRequestOptions = {},
  ) {
    return request<unknown>({
      method: "GET",
      path,
      query,
      ...options,
    });
  },

  mutateOperationalEndpoint(
    method: Exclude<HttpMethod, "GET">,
    path: string,
    body: unknown,
    options: CrmApiRequestOptions = {},
  ) {
    return request<unknown>({
      method,
      path,
      body,
      ...options,
    });
  },
};
