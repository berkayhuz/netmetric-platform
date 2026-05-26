import "server-only";

export type SortDirection = "asc" | "desc";
export type HttpMethod = "GET" | "POST" | "PUT" | "PATCH" | "DELETE";

export type CustomerType = string | number;
export type CompanyType = string | number;
export type GenderType = string | number;

export type ProblemDetails = {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  traceId?: string;
  correlationId?: string;
  errorCode?: string;
  errors?: Record<string, string[]>;
};

export type CrmApiAuthContext = {
  bearerToken?: string;
};

export type CrmApiRequestOptions = {
  authContext?: CrmApiAuthContext;
  correlationId?: string;
  timeoutMs?: number;
  signal?: AbortSignal;
};

export type CrmListQuery = {
  page?: number;
  pageSize?: number;
  search?: string;
  sortBy?: string;
  sortDirection?: SortDirection;
  filters?: Record<string, string | number | boolean | undefined | null>;
};

export type CrmNormalizedListQuery = {
  page: number;
  pageSize: number;
  search?: string;
  sortBy?: string;
  sortDirection?: SortDirection;
  filters: Record<string, string | number | boolean>;
};

export type CrmPagedResult<TItem> = {
  items: TItem[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
};

export type AddressDto = {
  id: string;
  addressType: string | number;
  line1: string;
  line2?: string | null;
  district?: string | null;
  city?: string | null;
  state?: string | null;
  country?: string | null;
  zipCode?: string | null;
  isDefault: boolean;
  rowVersion: string;
};

export type CustomerContactSummaryDto = {
  id: string;
  fullName: string;
  email?: string | null;
  mobilePhone?: string | null;
  isPrimaryContact: boolean;
};

export type CustomerListItemDto = {
  id: string;
  fullName: string;
  email?: string | null;
  mobilePhone?: string | null;
  customerType: CustomerType;
  isVip: boolean;
  companyName?: string | null;
  isActive: boolean;
  createdAt: string;
  rowVersion: string;
  imageUrl?: string | null;
};

export type CustomerDetailDto = {
  id: string;
  firstName: string;
  lastName: string;
  fullName: string;
  title?: string | null;
  email?: string | null;
  mobilePhone?: string | null;
  workPhone?: string | null;
  personalPhone?: string | null;
  birthDate?: string | null;
  gender: GenderType;
  department?: string | null;
  jobTitle?: string | null;
  description?: string | null;
  notes?: string | null;
  ownerUserId?: string | null;
  customerType: CustomerType;
  identityNumber?: string | null;
  isVip: boolean;
  companyId?: string | null;
  companyName?: string | null;
  isActive: boolean;
  addresses: AddressDto[];
  contacts: CustomerContactSummaryDto[];
  rowVersion: string;
  imageUrl?: string | null;
};

export type CustomerDuplicateWarningDto = {
  candidateId: string;
  entityType: string | number;
  score: number;
  reasons: string[];
};

export type Customer360SummaryItemDto = {
  id: string;
  title: string;
  status?: string | null;
  occurredAtUtc?: string | null;
  amount?: number | null;
  url?: string | null;
};

export type Customer360Dto = {
  basicInfo?: {
    id: string;
    fullName: string;
    email?: string | null;
    mobilePhone?: string | null;
    isVip: boolean;
    isActive: boolean;
    createdAt: string;
    updatedAt?: string | null;
  } | null;
  lifecycleStage?: string | number | null;
  ownerUserId?: string | null;
  consentSummary?: {
    marketingAllowed: boolean;
    channels: Array<{
      channel: string | number;
      purpose: string | number;
      status: string | number;
      validUntilUtc?: string | null;
    }>;
  } | null;
  dataQuality?: {
    score: number;
    duplicateRiskScore: number;
    staleDataRiskScore: number;
    missingFields: string[];
    invalidFields: string[];
    recommendations: string[];
    calculatedAtUtc: string;
  } | null;
  relationshipHealth?: {
    score: number;
    riskLevel: string | number;
    lastActivityAtUtc?: string | null;
    openTicketCount: number;
    overdueTicketCount: number;
    openOpportunityCount: number;
    unpaidInvoiceCount: number;
    calculatedAtUtc: string;
  } | null;
  openLeads: Customer360SummaryItemDto[];
  openOpportunities: Customer360SummaryItemDto[];
  openDeals: Customer360SummaryItemDto[];
  quotes: Customer360SummaryItemDto[];
  tickets: Customer360SummaryItemDto[];
  contracts: Customer360SummaryItemDto[];
  documents: Customer360SummaryItemDto[];
  activities: Customer360SummaryItemDto[];
  communications: Customer360SummaryItemDto[];
  timeline: Array<{
    occurredAtUtc: string;
    source: string;
    action: string;
    title: string;
    relatedEntityId?: string | null;
    metadata?: string | null;
  }>;
  duplicateWarnings: CustomerDuplicateWarningDto[];
  suggestedNextActions: Array<{
    code: string;
    title: string;
    priority: string;
  }>;
};

export type CustomerConsentDto = {
  id: string;
  channel: string | number;
  purpose: string | number;
  status: string | number;
  source: string | number;
  validFromUtc: string;
  validUntilUtc?: string | null;
  evidenceText?: string | null;
};

export type CustomerAccountHierarchyNodeDto = {
  id: string;
  companyId: string;
  parentCompanyId?: string | null;
  name: string;
  relationshipType: string;
  displayOrder: number;
  isPrimary: boolean;
  children: CustomerAccountHierarchyNodeDto[];
};

export type CustomerAccountHierarchyDto = {
  roots: CustomerAccountHierarchyNodeDto[];
};

export type CustomerAuditEventDto = {
  id: string;
  action: string | number;
  fieldName?: string | null;
  oldValueMasked?: string | null;
  newValueMasked?: string | null;
  actorUserId: string;
  occurredAtUtc: string;
  correlationId: string;
};

export type CustomerSearchResultDto = {
  id: string;
  title: string;
  subtitle?: string | null;
  email?: string | null;
  phone?: string | null;
};

export type CustomerMergeConflictDto = {
  fieldName: string;
  masterValue?: string | null;
  duplicateValue?: string | null;
};

export type CustomerMergePreviewDto = {
  masterCustomerId: string;
  duplicateCustomerId: string;
  conflicts: CustomerMergeConflictDto[];
  relatedRecordsToReassign: string[];
};

export type CustomerImportRowDto = {
  id: string;
  rowNumber: number;
  status: string | number;
  importedEntityId?: string | null;
  validationErrorsJson?: string | null;
  duplicateWarningsJson?: string | null;
  mappedDataJson?: string | null;
};

export type CustomerImportBatchDto = {
  id: string;
  fileName: string;
  source: string;
  status: string | number;
  totalRows: number;
  validRows: number;
  invalidRows: number;
  duplicateRows: number;
  createdAt: string;
  completedAt?: string | null;
  rows: CustomerImportRowDto[];
};

export type UpsertCustomerConsentRequest = {
  channel: number;
  purpose: number;
  status: number;
  source: number;
  validUntilUtc?: string | null;
  evidenceText?: string | null;
  evidenceIpAddress?: string | null;
  evidenceUserAgent?: string | null;
  reason?: string | null;
};

export type RevokeCustomerConsentRequest = {
  reason: string;
};

export type ChangeCustomerLifecycleStageRequest = {
  newStage: number;
  reason?: string | null;
};

export type MergeCustomersRequest = {
  masterCustomerId: string;
  duplicateCustomerId: string;
  resolvedFields: Record<string, string | null>;
  reason: string;
};

export type ShareCustomerRecordRequest = {
  sharedWithUserId?: string | null;
  sharedWithTeamId?: string | null;
  accessLevel: number;
  validUntilUtc?: string | null;
  reason: string;
};

export type AddAccountHierarchyNodeRequest = {
  companyId: string;
  parentCompanyId?: string | null;
  relationshipType: number;
  displayOrder: number;
  isPrimary: boolean;
};

export type MoveAccountHierarchyNodeRequest = {
  newParentCompanyId?: string | null;
  reason: string;
};

export type CreateCustomerImportBatchRequest = {
  fileName: string;
  source: string;
  rows: Array<Record<string, string | null>>;
};

export type CommitCustomerImportBatchRequest = {
  duplicateStrategy: number;
};

export type CancelCustomerImportBatchRequest = {
  reason?: string | null;
};

export type CompanyListItemDto = {
  id: string;
  name: string;
  email?: string | null;
  phone?: string | null;
  companyType: CompanyType;
  sector?: string | null;
  isActive: boolean;
  contactCount: number;
  rowVersion: string;
  logoUrl?: string | null;
};

export type CompanyDetailDto = {
  id: string;
  name: string;
  taxNumber?: string | null;
  taxOffice?: string | null;
  website?: string | null;
  email?: string | null;
  phone?: string | null;
  sector?: string | null;
  employeeCountRange?: string | null;
  annualRevenue?: number | null;
  description?: string | null;
  notes?: string | null;
  companyType: CompanyType;
  ownerUserId?: string | null;
  parentCompanyId?: string | null;
  isActive: boolean;
  addresses: AddressDto[];
  rowVersion: string;
  logoUrl?: string | null;
};

export type ContactListItemDto = {
  id: string;
  fullName: string;
  email?: string | null;
  mobilePhone?: string | null;
  companyName?: string | null;
  customerName?: string | null;
  isPrimaryContact: boolean;
  isActive: boolean;
  rowVersion: string;
};

export type GlobalTrashListItemDto = {
  id: string;
  entityType: string;
  entityId: string;
  displayName: string;
  summary?: string | null;
  sourceModule: string;
  originalRoute?: string | null;
  deletedAtUtc: string;
  deletedByUserId?: string | null;
  deletedByDisplayName?: string | null;
  expiresAtUtc: string;
  status: string;
};

export type ContactDetailDto = {
  id: string;
  firstName: string;
  lastName: string;
  fullName: string;
  title?: string | null;
  email?: string | null;
  mobilePhone?: string | null;
  workPhone?: string | null;
  personalPhone?: string | null;
  birthDate?: string | null;
  gender: GenderType;
  department?: string | null;
  jobTitle?: string | null;
  description?: string | null;
  notes?: string | null;
  ownerUserId?: string | null;
  companyId?: string | null;
  companyName?: string | null;
  customerId?: string | null;
  customerName?: string | null;
  isPrimaryContact: boolean;
  isActive: boolean;
  rowVersion: string;
};

export type LeadListItemDto = {
  id: string;
  leadCode: string;
  fullName: string;
  companyName?: string | null;
  email?: string | null;
  phone?: string | null;
  status: string | number;
  source: string | number;
  priority: string | number;
  ownerUserId?: string | null;
  estimatedBudget?: number | null;
  nextContactDate?: string | null;
  totalScore: number;
  grade: string | number;
  qualificationFramework: string | number;
  slaTargetTime?: string | null;
  slaBreached: boolean;
  isActive: boolean;
  rowVersion: string;
};

export type LeadScoreDto = {
  id: string;
  score: number;
  scoreReason?: string | null;
  scoredByUserId?: string | null;
  scoredAt: string;
};

export type LeadOwnershipHistoryDto = {
  id: string;
  previousOwnerId?: string | null;
  newOwnerId?: string | null;
  assignmentReason?: string | null;
  assignmentRuleId?: string | null;
  assignedAt: string;
  assignedByUserId?: string | null;
};

export type LeadDetailDto = {
  id: string;
  leadCode: string;
  fullName: string;
  companyName?: string | null;
  email?: string | null;
  phone?: string | null;
  jobTitle?: string | null;
  description?: string | null;
  estimatedBudget?: number | null;
  nextContactDate?: string | null;
  source: string | number;
  status: string | number;
  priority: string | number;
  companyId?: string | null;
  ownerUserId?: string | null;
  convertedCustomerId?: string | null;
  notes?: string | null;
  totalScore: number;
  fitScore: number;
  grade: string | number;
  qualificationFramework: string | number;
  qualificationData?: string | null;
  slaTargetTime?: string | null;
  firstContactTime?: string | null;
  slaBreached: boolean;
  utmSource?: string | null;
  utmMedium?: string | null;
  utmCampaign?: string | null;
  referrerUrl?: string | null;
  isActive: boolean;
  scores: LeadScoreDto[];
  ownershipHistories: LeadOwnershipHistoryDto[];
  rowVersion: string;
};

export type LeadWorkspaceDto = {
  lead: LeadDetailDto;
  scoreHistoryCount: number;
  latestScore?: number | null;
  relatedOpenOpportunityCount: number;
};

export type LeadTimelineEventDto = {
  occurredAt: string;
  eventType: string;
  title: string;
  description?: string | null;
};

export type LeadConversionResultDto = {
  leadId: string;
  customerId: string;
  opportunityId?: string | null;
  leadStatus: string;
};

export type LeadCaptureRequest = {
  fullName: string;
  email?: string | null;
  phone?: string | null;
  companyName?: string | null;
  jobTitle?: string | null;
  description?: string | null;
  source: number;
  captureFormId?: string | null;
  referrerUrl?: string | null;
  utmSource?: string | null;
  utmMedium?: string | null;
  utmCampaign?: string | null;
  utmTerm?: string | null;
  utmContent?: string | null;
  dynamicData?: Record<string, unknown> | null;
  honeypot?: string | null;
  captchaToken?: string | null;
};

export type LeadCaptureResultDto = {
  leadId: string;
};

export type LeadUpsertRequest = {
  fullName: string;
  companyName?: string | null;
  email?: string | null;
  phone?: string | null;
  jobTitle?: string | null;
  description?: string | null;
  estimatedBudget?: number | null;
  nextContactDate?: string | null;
  source: number;
  status: number;
  priority: number;
  companyId?: string | null;
  ownerUserId?: string | null;
  notes?: string | null;
};

export type LeadUpdateRequest = LeadUpsertRequest & {
  rowVersion?: string | null;
};

export type AssignLeadOwnerRequest = {
  ownerUserId?: string | null;
};

export type ChangeLeadStatusRequest = {
  status: number;
};

export type ScheduleLeadNextContactRequest = {
  nextContactDate?: string | null;
};

export type UpsertLeadScoreRequest = {
  score: number;
  scoreReason?: string | null;
};

export type UpsertLeadQualificationRequest = {
  frameworkType: number;
  qualificationDataJson: string;
};

export type ConvertLeadToCustomerRequest = {
  customerType: number;
  markCustomerAsVip: boolean;
  createOpportunity: boolean;
  opportunityName?: string | null;
  estimatedAmount?: number | null;
  companyId?: string | null;
};

export type BulkAssignLeadOwnerRequest = {
  leadIds: string[];
  ownerUserId?: string | null;
};

export type BulkLeadIdsRequest = {
  leadIds: string[];
};

export type OpportunityListItemDto = {
  id: string;
  opportunityCode: string;
  name: string;
  estimatedAmount?: number | null;
  expectedRevenue?: number | null;
  probability: number;
  stage: string | number;
  status: string | number;
  priority: string | number;
  estimatedCloseDate?: string | null;
  leadId?: string | null;
  customerId?: string | null;
  ownerUserId?: string | null;
  isActive: boolean;
};

export type OpportunityDetailDto = {
  id: string;
  opportunityCode: string;
  name: string;
  description?: string | null;
  estimatedAmount?: number | null;
  expectedRevenue?: number | null;
  probability: number;
  estimatedCloseDate?: string | null;
  stage: string | number;
  pipelineId?: string | null;
  pipelineStageId?: string | null;
  status: string | number;
  priority: string | number;
  leadId?: string | null;
  customerId?: string | null;
  ownerUserId?: string | null;
  lostReasonId?: string | null;
  lostNote?: string | null;
  notes?: string | null;
  isActive: boolean;
  rowVersion: string;
};

export type OpportunityWorkspaceDto = {
  opportunity: OpportunityDetailDto;
  totalQuoteAmount?: number | null;
  quoteCount: number;
  activityCount: number;
  stageChangeCount: number;
};

export type OpportunityTimelineEventDto = {
  occurredAt: string;
  eventType: string;
  title: string;
  description?: string | null;
};

export type OpportunityStageHistoryDto = {
  id: string;
  opportunityId?: string | null;
  oldStage: string | number;
  newStage: string | number;
  changedAt: string;
  changedByUserId?: string | null;
  note?: string | null;
};

export type OpportunityLostReasonDto = {
  id: string;
  name: string;
  description?: string | null;
  isDefault: boolean;
  rowVersion?: string | null;
};

export type OpportunityContactDto = {
  id: string;
  contactId: string;
  isDecisionMaker: boolean;
  isPrimary: boolean;
};

export type OpportunityProductDto = {
  id: string;
  productId: string;
  quantity: number;
  unitPrice: number;
  discountRate: number;
  vatRate: number;
};

export type OpportunityUpsertRequest = {
  opportunityCode: string;
  name: string;
  description?: string | null;
  estimatedAmount: number;
  expectedRevenue?: number | null;
  probability: number;
  estimatedCloseDate?: string | null;
  stage: number;
  status: number;
  priority: number;
  leadId?: string | null;
  customerId?: string | null;
  ownerUserId?: string | null;
  notes?: string | null;
};

export type OpportunityUpdateRequest = OpportunityUpsertRequest & {
  rowVersion: string;
};

export type AssignOpportunityOwnerRequest = {
  ownerUserId?: string | null;
};

export type ChangeOpportunityStageRequest = {
  newStage: number;
  note?: string | null;
  rowVersion?: string | null;
};

export type MarkOpportunityWonRequest = {
  dealName?: string | null;
  closedDate: string;
  rowVersion?: string | null;
};

export type MarkOpportunityWonResultDto = {
  dealId: string;
};

export type MarkOpportunityLostRequest = {
  lostReasonId?: string | null;
  lostNote?: string | null;
  rowVersion?: string | null;
};

export type AddOpportunityContactRequest = {
  contactId: string;
  isDecisionMaker: boolean;
  isPrimary: boolean;
};

export type AddOpportunityProductRequest = {
  productId: string;
  quantity: number;
  unitPrice: number;
  discountRate: number;
  vatRate: number;
};

export type OpportunityQuoteItemDto = {
  id: string;
  productId: string;
  description?: string | null;
  quantity: number;
  unitPrice: number;
  discountRate: number;
  taxRate: number;
  lineTotal: number;
};

export type OpportunityQuoteDetailDto = {
  id: string;
  quoteNumber: string;
  opportunityId?: string | null;
  quoteDate: string;
  validUntil?: string | null;
  subTotal?: number | null;
  discountTotal?: number | null;
  taxTotal?: number | null;
  grandTotal?: number | null;
  termsAndConditions?: string | null;
  ownerUserId?: string | null;
  currencyCode: string;
  exchangeRate?: number | null;
  items: OpportunityQuoteItemDto[];
  rowVersion: string;
};

export type OpportunityQuoteLineRequest = {
  productId: string;
  description?: string | null;
  quantity: number;
  unitPrice: number;
  discountRate: number;
  taxRate: number;
};

export type CreateOpportunityQuoteRequest = {
  quoteNumber: string;
  quoteDate: string;
  validUntil?: string | null;
  termsAndConditions?: string | null;
  ownerUserId?: string | null;
  currencyCode: string;
  exchangeRate: number;
  items: OpportunityQuoteLineRequest[];
};

export type BulkAssignOpportunityOwnerRequest = {
  opportunityIds: string[];
  ownerUserId?: string | null;
};

export type BulkChangeOpportunityStageRequest = {
  opportunityIds: string[];
  newStage: number;
  note?: string | null;
};

export type OpportunityBulkOperationResultDto = {
  affected: number;
};

export type PipelineSummaryDto = {
  id: string;
  name: string;
  stageCount: number;
  isDefault: boolean;
};

export type PipelineStageDto = {
  id: string;
  pipelineId: string;
  name: string;
  description?: string | null;
  displayOrder: number;
  probability: number;
  isWinStage: boolean;
  isLostStage: boolean;
  forecastCategory: string | number;
  requiredFields?: PipelineStageRequiredFieldDto[];
  exitCriteria?: PipelineStageExitCriteriaDto[];
  rowVersion: string;
};

export type PipelineStageRequiredFieldDto = {
  id: string;
  fieldName: string;
  displayName?: string | null;
  validationRule?: string | null;
  errorMessage?: string | null;
};

export type PipelineStageExitCriteriaDto = {
  id: string;
  name: string;
  description?: string | null;
  isMandatory: boolean;
};

export type PipelineDto = {
  id: string;
  name: string;
  description?: string | null;
  isDefault: boolean;
  displayOrder: number;
  stages: PipelineStageDto[];
  rowVersion: string;
};

export type PipelineBoardOpportunityDto = {
  id: string;
  name: string;
  opportunityCode: string;
  amount: number;
  customerName?: string | null;
  estimatedCloseDate?: string | null;
  isStale: boolean;
  warningCount: number;
};

export type PipelineBoardColumnDto = {
  stageId: string;
  name: string;
  probability: number;
  opportunityCount: number;
  totalValue: number;
  opportunities: PipelineBoardOpportunityDto[];
};

export type PipelineBoardDto = {
  pipelineId: string;
  pipelineName: string;
  columns: PipelineBoardColumnDto[];
};

export type PipelineStageRequest = {
  id?: string | null;
  name: string;
  description?: string | null;
  displayOrder: number;
  probability: number;
  isWinStage: boolean;
  isLostStage: boolean;
};

export type CreatePipelineRequest = {
  name: string;
  description?: string | null;
  isDefault: boolean;
  displayOrder: number;
  stages: PipelineStageRequest[];
};

export type UpdatePipelineRequest = CreatePipelineRequest & {
  id: string;
  rowVersion: string;
};

export type PipelineStageAgingDto = {
  stageId: string;
  stageName: string;
  opportunityCount: number;
  averageDaysInStage: number;
  staleCount: number;
};

export type PipelineAnalyticsDto = {
  pipelineId: string;
  healthScore: number;
  velocityDays: number;
  coverageRatio: number;
  totalOpportunities: number;
  totalValue: number;
  stageAging: PipelineStageAgingDto[];
};

export type PipelineStageMoveRequest = {
  newStage: number;
  newPipelineStageId?: string | null;
  note?: string | null;
  lostReasonId?: string | null;
  lostNote?: string | null;
  rowVersion?: string | null;
};

export type PipelineLostReasonUpsertRequest = {
  name: string;
  description?: string | null;
  isDefault: boolean;
  rowVersion?: string | null;
};

export type PipelineLeadConversionPreviewDto = {
  leadId: string;
  leadCode: string;
  fullName: string;
  companyName?: string | null;
  email?: string | null;
  estimatedBudget?: number | null;
  alreadyConverted: boolean;
  convertedCustomerId?: string | null;
};

export type PipelineLeadConversionRequest = {
  createCustomer: boolean;
  createOpportunity: boolean;
  existingCustomerId?: string | null;
  opportunityName?: string | null;
  estimatedAmount?: number | null;
  initialStage: number;
  priority: number;
  ownerUserId?: string | null;
  notes?: string | null;
};

export type PipelineLeadConversionResultDto = {
  leadId: string;
  customerId?: string | null;
  opportunityId?: string | null;
  leadStatus: string | number;
  message: string;
};

export type PipelineStageMoveResultDto = {
  opportunityId: string;
  previousStage: string | number;
  currentStage: string | number;
  status: string | number;
  lostReasonId?: string | null;
  lostNote?: string | null;
  rowVersion?: string | null;
};

export type DealListItemDto = {
  id: string;
  dealCode: string;
  name: string;
  totalAmount?: number | null;
  closedDate: string;
  opportunityId?: string | null;
  companyId?: string | null;
  ownerUserId?: string | null;
  stage: string | number;
  outcome: string | number;
  isActive: boolean;
};

export type DealOutcomeHistoryDto = {
  id: string;
  outcome: string;
  stage: string;
  occurredAt: string;
  changedByUserId?: string | null;
  lostReasonId?: string | null;
  note?: string | null;
};

export type DealWinLossReviewDto = {
  id: string;
  dealId: string;
  outcome: string;
  summary?: string | null;
  strengths?: string | null;
  risks?: string | null;
  competitorName?: string | null;
  competitorPrice?: number | null;
  customerFeedback?: string | null;
  reviewedAt?: string | null;
  reviewedByUserId?: string | null;
  rowVersion?: string | null;
};

export type DealDetailDto = {
  id: string;
  dealCode: string;
  name: string;
  totalAmount?: number | null;
  closedDate: string;
  opportunityId?: string | null;
  companyId?: string | null;
  ownerUserId?: string | null;
  stage: string | number;
  outcome: string | number;
  lostReasonId?: string | null;
  lostNote?: string | null;
  isActive: boolean;
  review?: DealWinLossReviewDto | null;
  history: DealOutcomeHistoryDto[];
  rowVersion: string;
};

export type DealWorkspaceDto = {
  deal: DealDetailDto;
  lostReasons: DealLostReasonDto[];
  timeline: DealOutcomeHistoryDto[];
};

export type DealUpsertRequest = {
  dealCode: string;
  name: string;
  totalAmount: number;
  closedDate: string;
  opportunityId?: string | null;
  companyId?: string | null;
  ownerUserId?: string | null;
  notes?: string | null;
  rowVersion?: string | null;
};

export type AssignDealOwnerRequest = {
  ownerUserId?: string | null;
};

export type BulkAssignDealOwnerRequest = {
  dealIds: string[];
  ownerUserId?: string | null;
};

export type DealBulkOperationResultDto = {
  affectedCount: number;
};

export type DealOutcomeRequest = {
  occurredAt?: string | null;
  lostReasonId?: string | null;
  note?: string | null;
  rowVersion?: string | null;
};

export type DealReviewUpsertRequest = {
  outcome: string;
  summary?: string | null;
  strengths?: string | null;
  risks?: string | null;
  competitorName?: string | null;
  competitorPrice?: number | null;
  customerFeedback?: string | null;
  rowVersion?: string | null;
};

export type DealLostReasonDto = {
  id: string;
  name: string;
  description?: string | null;
  isDefault: boolean;
};

export type DealLostReasonBreakdownDto = {
  lostReasonId?: string | null;
  label: string;
  count: number;
  totalAmount: number;
};

export type DealWinLossSummaryDto = {
  totalDeals: number;
  wonDeals: number;
  lostDeals: number;
  wonAmount: number;
  lostAmount: number;
  lostReasonBreakdown: DealLostReasonBreakdownDto[];
};

export type DealWinLossSummaryQuery = {
  from?: string | null;
  to?: string | null;
  ownerUserId?: string | null;
};

export type QuoteListItemDto = {
  id: string;
  quoteNumber: string;
  proposalTitle?: string | null;
  status: string | number;
  quoteDate: string;
  validUntil?: string | null;
  grandTotal?: number | null;
  currencyCode: string;
  opportunityId?: string | null;
  customerId?: string | null;
  ownerUserId?: string | null;
  revisionNumber: number;
  isActive: boolean;
};

export type QuoteItemDto = {
  id: string;
  productId: string;
  description?: string | null;
  quantity: number;
  unitPrice: number;
  discountRate: number;
  taxRate: number;
  lineTotal: number;
};

export type QuoteLineUpsertRequest = {
  productId: string;
  description?: string | null;
  quantity: number;
  unitPrice: number;
  discountRate: number;
  taxRate: number;
};

export type QuoteStatusHistoryDto = {
  id: string;
  oldStatus?: string | number | null;
  newStatus: string | number;
  changedAt: string;
  changedByUserId?: string | null;
  note?: string | null;
};

export type QuoteDetailDto = {
  id: string;
  quoteNumber: string;
  proposalTitle?: string | null;
  proposalSummary?: string | null;
  proposalBody?: string | null;
  status: string | number;
  quoteDate: string;
  validUntil?: string | null;
  subTotal?: number | null;
  discountTotal?: number | null;
  taxTotal?: number | null;
  grandTotal?: number | null;
  termsAndConditions?: string | null;
  opportunityId?: string | null;
  customerId?: string | null;
  ownerUserId?: string | null;
  currencyCode: string;
  exchangeRate?: number | null;
  revisionNumber: number;
  parentQuoteId?: string | null;
  proposalTemplateId?: string | null;
  submittedAt?: string | null;
  approvedAt?: string | null;
  sentAt?: string | null;
  acceptedAt?: string | null;
  declinedAt?: string | null;
  expiredAt?: string | null;
  rejectionReason?: string | null;
  declineReason?: string | null;
  items: QuoteItemDto[];
  history: QuoteStatusHistoryDto[];
  rowVersion: string;
};

export type QuoteWorkspaceDto = {
  quote: QuoteDetailDto;
  canEdit: boolean;
  canSubmit: boolean;
  canApprove: boolean;
  canReject: boolean;
  canSend: boolean;
  canAccept: boolean;
  canDecline: boolean;
  canExpire: boolean;
};

export type QuoteTimelineEventDto = {
  occurredAt: string;
  eventType: string;
  title: string;
  description?: string | null;
};

export type QuoteUpsertRequest = {
  quoteNumber: string;
  proposalTitle?: string | null;
  proposalSummary?: string | null;
  proposalBody?: string | null;
  quoteDate: string;
  validUntil?: string | null;
  opportunityId?: string | null;
  customerId?: string | null;
  ownerUserId?: string | null;
  currencyCode: string;
  exchangeRate: number;
  termsAndConditions?: string | null;
  proposalTemplateId?: string | null;
  items: QuoteLineUpsertRequest[];
};

export type QuoteUpdateRequest = QuoteUpsertRequest & {
  rowVersion: string;
};

export type QuoteNoteRequest = {
  note?: string | null;
  rowVersion?: string | null;
};

export type QuoteReasonRequest = {
  reason: string;
  rowVersion?: string | null;
};

export type QuoteDateNoteRequest = {
  at?: string | null;
  note?: string | null;
  rowVersion?: string | null;
};

export type QuoteDeclineRequest = {
  at?: string | null;
  reason: string;
  rowVersion?: string | null;
};

export type CreateQuoteRevisionRequest = {
  newQuoteNumber: string;
};

export type CpqValidationResultDto = {
  isValid: boolean;
  violations: string[];
};

export type ProposalTemplateDto = {
  id: string;
  name: string;
  subjectTemplate?: string | null;
  bodyTemplate: string;
  isDefault: boolean;
  isActive: boolean;
  notes?: string | null;
};

export type ProposalTemplateRequest = {
  name: string;
  subjectTemplate?: string | null;
  bodyTemplate: string;
  isDefault: boolean;
  isActive: boolean;
  notes?: string | null;
};

export type ProductBundleItemDto = {
  productId: string;
  quantity: number;
  isOptional: boolean;
};

export type ProductBundleLineInput = {
  productId: string;
  quantity: number;
  isOptional: boolean;
};

export type ProductBundleDto = {
  id: string;
  code: string;
  name: string;
  description?: string | null;
  segment?: string | null;
  industry?: string | null;
  discountRate: number;
  minimumBudget?: number | null;
  items: ProductBundleItemDto[];
  isActive: boolean;
  rowVersion: string;
};

export type UpsertProductBundleRequest = {
  code: string;
  name: string;
  description?: string | null;
  segment?: string | null;
  industry?: string | null;
  discountRate: number;
  minimumBudget?: number | null;
  items: ProductBundleLineInput[];
  rowVersion?: string | null;
};

export type ProductRuleDto = {
  id: string;
  name: string;
  ruleType: string;
  triggerProductId?: string | null;
  targetProductId?: string | null;
  minimumQuantity?: number | null;
  maximumDiscountRate?: number | null;
  severity: string;
  message: string;
  criteriaJson?: string | null;
  isActive: boolean;
  rowVersion: string;
};

export type UpsertProductRuleRequest = {
  name: string;
  ruleType: string;
  triggerProductId?: string | null;
  targetProductId?: string | null;
  minimumQuantity?: number | null;
  maximumDiscountRate?: number | null;
  severity: string;
  message: string;
  criteriaJson?: string | null;
  rowVersion?: string | null;
};

export type GuidedSellingPlaybookDto = {
  id: string;
  name: string;
  segment?: string | null;
  industry?: string | null;
  minimumBudget?: number | null;
  maximumBudget?: number | null;
  requiredCapabilities?: string | null;
  recommendedBundleCodes: string[];
  qualificationJson?: string | null;
  isActive: boolean;
  rowVersion: string;
};

export type UpsertGuidedSellingPlaybookRequest = {
  name: string;
  segment?: string | null;
  industry?: string | null;
  minimumBudget?: number | null;
  maximumBudget?: number | null;
  requiredCapabilities?: string | null;
  recommendedBundleCodes: string[];
  qualificationJson?: string | null;
  rowVersion?: string | null;
};

export type RunGuidedSellingRequest = {
  segment?: string | null;
  industry?: string | null;
  budget?: number | null;
  requiredCapabilities: string[];
};

export type GuidedSellingRecommendationDto = {
  playbookName: string;
  bundleId: string;
  bundleCode: string;
  bundleName: string;
  estimatedDiscountRate: number;
  score: number;
  reasons: string[];
};

export type CpqWorkspaceDto = {
  productRules: ProductRuleDto[];
  productBundles: ProductBundleDto[];
  guidedSellingPlaybooks: GuidedSellingPlaybookDto[];
};

export type ContractLifecycleSummaryDto = {
  id: string;
  code: string;
  name: string;
  description?: string | null;
  isActive: boolean;
};

export type ContractCreateRequest = {
  code: string;
  name: string;
  description?: string | null;
};

export type FinanceOperationsSummaryDto = {
  id: string;
  code: string;
  name: string;
  description?: string | null;
  isActive: boolean;
};

export type SalesOrderCreateRequest = {
  code: string;
  name: string;
  description?: string | null;
};

export type TagSummaryDto = {
  tagId: string;
  name: string;
  color: string;
  groupName?: string | null;
};

export type TagGroupSummaryDto = {
  id: string;
  name: string;
  entityType?: string | null;
  color?: string | null;
};

export type SmartLabelRuleSummaryDto = {
  id: string;
  name: string;
  entityType?: string | null;
  conditionJson?: string | null;
};

export type ClassificationSchemeSummaryDto = {
  id: string;
  name: string;
  entityType?: string | null;
};

export type CreateTagRequest = {
  name: string;
  color: string;
};

export type CreateTagGroupRequest = {
  name: string;
  color?: string | null;
};

export type CreateSmartLabelRuleRequest = {
  name: string;
  entityType: string;
  conditionJson: string;
};

export type CreateClassificationSchemeRequest = {
  name: string;
  entityType: string;
};

export type WorkTaskDto = {
  id: string;
  title: string;
  description: string;
  ownerUserId?: string | null;
  dueAtUtc: string;
  reminderAtUtc?: string | null;
  priority: number;
  status: string;
  completedAtUtc?: string | null;
  completedByUserId?: string | null;
  completionNote?: string | null;
};

export type ActivityTimelineMetadataValue = string | number | boolean | null;

export type ActivityRelatedRecord = {
  entityType: string;
  entityId: string;
  displayName?: string | null;
  relationRole?: string | null;
};

export type ActivityTimelineItem = {
  id: string;
  occurredAtUtc: string;
  type: string;
  title: string;
  description?: string | null;
  status?: string | null;
  sourceModule: string;
  sourceEntityType: string;
  sourceEntityId?: string | null;
  actorUserId?: string | null;
  ownerUserId?: string | null;
  relatedRecords: ActivityRelatedRecord[];
  metadata: Record<string, ActivityTimelineMetadataValue>;
};

export type ActivityTimelineFeed = {
  items: ActivityTimelineItem[];
  totalCount: number;
  page: number;
  pageSize: number;
};

export type ActivityCreateType = "note" | "call" | "email";
export type ActivityCallDirection = "inbound" | "outbound";
export type ActivityCallOutcome = "connected" | "no_answer" | "voicemail" | "other";

export type CreateActivityRelatedRecord = {
  entityType: string;
  entityId: string;
  relationRole: string;
};

export type CreateActivityNotePayload = {
  body: string;
};

export type CreateActivityCallPayload = {
  direction: ActivityCallDirection;
  outcome: ActivityCallOutcome;
  durationSeconds?: number;
  summary?: string | null;
};

export type CreateActivityEmailPayload = {
  subject: string;
  bodySummary: string;
  direction: ActivityCallDirection;
  to?: string[];
  cc?: string[];
};

export type CreateActivityPayload =
  | CreateActivityNotePayload
  | CreateActivityCallPayload
  | CreateActivityEmailPayload;

export type CreateActivityRequest = {
  type: ActivityCreateType;
  occurredAtUtc?: string;
  title?: string | null;
  description?: string | null;
  relatedRecords: CreateActivityRelatedRecord[];
  payload: CreateActivityPayload;
};

export type CreateActivityResponse = {
  activityId: string;
  type: ActivityCreateType;
  createdAtUtc: string;
  sourceEntityType: string;
  sourceEntityId: string;
  timelineItem: ActivityTimelineItem;
};

export type MeetingScheduleDto = {
  id: string;
  title: string;
  startsAtUtc: string;
  endsAtUtc: string;
  organizerEmail?: string | null;
  requiresExternalSync: boolean;
};

export type WorkManagementWorkspaceDto = {
  tasks: WorkTaskDto[];
  meetings: MeetingScheduleDto[];
  openTaskCount: number;
  upcomingMeetingCount: number;
};

export type SupportInboxConnectionDto = {
  id: string;
  name: string;
  provider: string | number;
  emailAddress: string;
  host: string;
  port: number;
  username: string;
  useSsl: boolean;
  isActive: boolean;
};

export type SupportInboxMessageDto = {
  id: string;
  connectionId: string;
  ticketId?: string | null;
  externalMessageId: string;
  fromAddress: string;
  subject: string;
  receivedAtUtc: string;
  status: string;
  processingError?: string | null;
};

export type SupportInboxConnectionCreateRequest = {
  name: string;
  provider: number;
  emailAddress: string;
  host: string;
  port: number;
  username: string;
  secretReference: string;
  useSsl: boolean;
};

export type SupportInboxConnectionUpdateRequest = {
  name: string;
  host: string;
  port: number;
  username: string;
  secretReference: string;
  useSsl: boolean;
  isActive: boolean;
};

export type SupportInboxSyncRequest = {
  dryRun: boolean;
};

export type SupportInboxRuleCreateRequest = {
  connectionId: string;
  name: string;
  matchSender?: string | null;
  matchSubjectContains?: string | null;
  assignToQueueId?: string | null;
  ticketCategoryId?: string | null;
  slaPolicyId?: string | null;
  autoCreateTicket: boolean;
};

export type SupportInboxRuleUpdateRequest = {
  name: string;
  matchSender?: string | null;
  matchSubjectContains?: string | null;
  assignToQueueId?: string | null;
  ticketCategoryId?: string | null;
  slaPolicyId?: string | null;
  autoCreateTicket: boolean;
  isActive: boolean;
};

export type TicketSlaPolicyDto = {
  id: string;
  name: string;
  ticketCategoryId?: string | null;
  priority: number;
  firstResponseTargetMinutes: number;
  resolutionTargetMinutes: number;
  isDefault: boolean;
};

export type TicketSlaEscalationRuleDto = {
  id: string;
  slaPolicyId: string;
  metricType: string;
  triggerBeforeOrAfterMinutes: number;
  actionType: string;
  targetTeamId?: string | null;
  targetUserId?: string | null;
  isEnabled: boolean;
};

export type TicketSlaWorkspaceDto = {
  ticketId: string;
  slaPolicyId: string;
  firstResponseDueAtUtc: string;
  resolutionDueAtUtc: string;
  firstRespondedAtUtc?: string | null;
  resolvedAtUtc?: string | null;
  isFirstResponseBreached: boolean;
  isResolutionBreached: boolean;
};

export type TicketEscalationRunDto = {
  id: string;
  ticketId: string;
  escalationRuleId: string;
  metricType: string;
  executedAtUtc: string;
  note: string;
};

export type TicketSlaPolicyUpsertRequest = {
  name: string;
  ticketCategoryId?: string | null;
  priority: number;
  firstResponseTargetMinutes: number;
  resolutionTargetMinutes: number;
  isDefault: boolean;
};

export type TicketSlaEscalationRuleUpsertRequest = {
  slaPolicyId: string;
  metricType: string;
  triggerBeforeOrAfterMinutes: number;
  actionType: string;
  targetTeamId?: string | null;
  targetUserId?: string | null;
  isEnabled: boolean;
};

export type AttachTicketSlaRequest = {
  ticketId: string;
  slaPolicyId: string;
  createdAtUtc: string;
};

export type MarkTicketFirstResponseRequest = {
  ticketId: string;
  respondedAtUtc: string;
};

export type MarkTicketResolvedRequest = {
  ticketId: string;
  resolvedAtUtc: string;
};

export type RunDueTicketEscalationsRequest = {
  utcNow: string;
};

export type RunDueTicketEscalationsResultDto = {
  processed: number;
};

export type TicketWorkflowQueueDto = {
  id: string;
  code: string;
  name: string;
  description?: string | null;
  assignmentStrategy: string;
  isDefault: boolean;
};

export type TicketWorkflowQueueUpsertRequest = {
  code: string;
  name: string;
  description?: string | null;
  assignmentStrategy: number;
  isDefault: boolean;
};

export type TicketWorkflowQueueUpdateRequest = {
  name: string;
  description?: string | null;
  assignmentStrategy: number;
  isDefault: boolean;
};

export type AssignTicketWorkflowQueueRequest = {
  previousQueueId?: string | null;
  newQueueId: string;
  reason?: string | null;
};

export type AssignTicketWorkflowOwnerRequest = {
  previousOwnerUserId?: string | null;
  newOwnerUserId: string;
  queueId?: string | null;
  reason?: string | null;
};

export type RecordTicketWorkflowStatusChangeRequest = {
  previousStatus: string;
  newStatus: string;
  note?: string | null;
};

export type TicketAssignmentHistoryDto = {
  id: string;
  ticketId: string;
  previousOwnerUserId?: string | null;
  newOwnerUserId?: string | null;
  previousQueueId?: string | null;
  newQueueId?: string | null;
  changedByUserId?: string | null;
  reason?: string | null;
  changedAtUtc: string;
};

export type TicketStatusHistoryDto = {
  id: string;
  ticketId: string;
  previousStatus: string;
  newStatus: string;
  changedByUserId?: string | null;
  note?: string | null;
  changedAtUtc: string;
};

export type TicketListItemDto = {
  id: string;
  ticketNumber: string;
  subject: string;
  status: string | number;
  priority: string | number;
  ticketType: string | number;
  assignedUserId?: string | null;
  customerId?: string | null;
  contactId?: string | null;
  openedAt: string;
  closedAt?: string | null;
  isActive: boolean;
};

export type TicketCommentDto = {
  id: string;
  comment: string;
  isInternal: boolean;
  createdAt: string;
  createdBy?: string | null;
};

export type TicketDetailDto = {
  id: string;
  ticketNumber: string;
  subject: string;
  description?: string | null;
  status: string | number;
  priority: string | number;
  ticketType: string | number;
  channel: string | number;
  assignedUserId?: string | null;
  customerId?: string | null;
  contactId?: string | null;
  ticketCategoryId?: string | null;
  slaPolicyId?: string | null;
  firstResponseDueAt?: string | null;
  resolveDueAt?: string | null;
  openedAt: string;
  closedAt?: string | null;
  notes?: string | null;
  isActive: boolean;
  comments: TicketCommentDto[];
  rowVersion: string;
};

export type TicketUpsertRequest = {
  subject: string;
  description?: string | null;
  ticketType: number;
  channel: number;
  priority: number;
  assignedUserId?: string | null;
  customerId?: string | null;
  contactId?: string | null;
  ticketCategoryId?: string | null;
  slaPolicyId?: string | null;
  firstResponseDueAt?: string | null;
  resolveDueAt?: string | null;
  notes?: string | null;
  rowVersion?: string | null;
};

export type TicketUpdateRequest = TicketUpsertRequest;

export type ProductCatalogItemDto = {
  id: string;
  code: string;
  name: string;
  description?: string | null;
  isActive: boolean;
  categoryId?: string | null;
  categoryCode?: string | null;
  categoryName?: string | null;
  unitPrice?: number | null;
  currencyCode: string;
  defaultDiscountRate: number;
  defaultTaxRate: number;
  primaryImageUrl?: string | null;
};

export type ProductImageDto = {
  id: string;
  productId: string;
  mediaAssetId: string;
  publicUrl: string;
  sortOrder: number;
  isPrimary: boolean;
  altText?: string | null;
};

export type ProductCatalogUpsertRequest = {
  code: string;
  name: string;
  description?: string | null;
  categoryId?: string | null;
  unitPrice?: number | null;
  currencyCode: string;
  defaultDiscountRate: number;
  defaultTaxRate: number;
};

export type ProductCatalogActiveStateRequest = {
  isActive: boolean;
};

export type ProductCatalogCategoryDto = {
  id: string;
  code: string;
  name: string;
  description?: string | null;
  isActive: boolean;
  primaryImageUrl?: string | null;
};

export type ProductCatalogCategoryUpsertRequest = {
  code: string;
  name: string;
  description?: string | null;
};

export type ProductCatalogCategoryActiveStateRequest = {
  isActive: boolean;
};

export type ProductCatalogLookupItemDto = {
  id: string;
  code: string;
  name: string;
};

export type ProductCatalogLookupsDto = {
  products: ProductCatalogLookupItemDto[];
  categories: ProductCatalogLookupItemDto[];
  priceLists: ProductCatalogLookupItemDto[];
  discountMatrices: ProductCatalogLookupItemDto[];
  productBindings: ProductCatalogLookupItemDto[];
  currencies: string[];
};

export type ProductCatalogMetaDto = {
  module: string;
  version: string;
  resources: string[];
  features: string[];
};

export type ProductCatalogStatsDto = {
  productCount: number;
  activeProductCount: number;
  categoryCount: number;
  activeCategoryCount: number;
  priceListCount: number;
  activePriceListCount: number;
  discountMatrixCount: number;
  activeDiscountMatrixCount: number;
  productBindingCount: number;
  activeProductBindingCount: number;
};

export type CatalogBulkOperationResultDto = {
  requestedCount: number;
  processedCount: number;
};

export type BulkCatalogItemIdsRequest = {
  ids: string[];
};

export type BulkSetActiveStateRequest = {
  ids: string[];
  isActive: boolean;
};

export type CrmApiDownloadPayload = {
  bytes: Uint8Array;
  contentType: string;
  fileName: string;
};

export type CreateWorkTaskRequest = {
  title: string;
  description: string;
  ownerUserId?: string | null;
  dueAtUtc: string;
  priority: number;
};

export type UpdateWorkTaskRequest = {
  title: string;
  description: string;
  priority: number;
};

export type CompleteWorkTaskRequest = {
  completionNote?: string | null;
};

export type AssignWorkTaskOwnerRequest = {
  ownerUserId?: string | null;
};

export type UpdateWorkTaskDueDateRequest = {
  dueAtUtc: string;
};

export type UpdateWorkTaskReminderRequest = {
  reminderAtUtc?: string | null;
};

export type ScheduleMeetingRequest = {
  title: string;
  startsAtUtc: string;
  endsAtUtc: string;
  organizerEmail: string;
  attendeeSummary: string;
  requiresExternalSync: boolean;
};

export type CustomerUpsertRequest = {
  firstName: string;
  lastName: string;
  title?: string | null;
  email?: string | null;
  mobilePhone?: string | null;
  workPhone?: string | null;
  personalPhone?: string | null;
  birthDate?: string | null;
  gender: GenderType;
  department?: string | null;
  jobTitle?: string | null;
  description?: string | null;
  notes?: string | null;
  ownerUserId?: string | null;
  customerType: CustomerType;
  identityNumber?: string | null;
  isVip: boolean;
  isActive: boolean;
  companyId?: string | null;
  rowVersion?: string | null;
};

export type CompanyUpsertRequest = {
  name: string;
  taxNumber?: string | null;
  taxOffice?: string | null;
  website?: string | null;
  email?: string | null;
  phone?: string | null;
  sector?: string | null;
  employeeCountRange?: string | null;
  annualRevenue?: number | null;
  description?: string | null;
  notes?: string | null;
  companyType: CompanyType;
  ownerUserId?: string | null;
  parentCompanyId?: string | null;
  rowVersion?: string | null;
};

export type ContactUpsertRequest = {
  firstName: string;
  lastName: string;
  title?: string | null;
  email?: string | null;
  mobilePhone?: string | null;
  workPhone?: string | null;
  personalPhone?: string | null;
  birthDate?: string | null;
  gender: GenderType;
  department?: string | null;
  jobTitle?: string | null;
  description?: string | null;
  notes?: string | null;
  ownerUserId?: string | null;
  companyId?: string | null;
  customerId?: string | null;
  isPrimaryContact: boolean;
  rowVersion?: string | null;
};

export type AddressUpsertRequest = {
  addressType: string | number;
  line1: string;
  line2?: string | null;
  district?: string | null;
  city?: string | null;
  state?: string | null;
  country?: string | null;
  zipCode?: string | null;
  isDefault: boolean;
  rowVersion?: string | null;
};
