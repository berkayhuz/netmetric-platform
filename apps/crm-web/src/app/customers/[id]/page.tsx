import Link from "next/link";
import { notFound } from "next/navigation";
import { Badge, Button } from "@netmetric/ui";
import { AlertTriangle, Activity } from "lucide-react";

import { AddressSection } from "@/components/address/address-section";
import { CrmDeleteConfirmForm } from "@/components/delete/crm-delete-confirm-form";
import { CrmDeleteZone } from "@/components/delete/crm-delete-zone";
import { CrmImageUploader } from "@/components/media/crm-image-uploader";
import { CrmPageShell } from "@/components/shell/crm-page-shell";
import {
  deleteCustomerAction,
  removeCustomerImageAction,
  uploadCustomerImageAction,
} from "@/features/customers/actions/customer-mutation-actions";
import { CustomerDetailWorkspace } from "@/features/customers/components/customer-detail-workspace";
import {
  getCustomerActivitiesTimelineData,
  getCustomerDetailData,
} from "@/features/customers/data/customers-data";
import { isGuid } from "@/features/shared/data/guid";
import {
  CrmApiError,
  crmApiClient,
  type ContactListItemDto,
  type CrmPagedResult,
  type ActivityTimelineFeed,
  type Customer360Dto,
  type CustomerAccountHierarchyDto,
  type CustomerAuditEventDto,
  type CustomerConsentDto,
  type CustomerDetailDto,
  type CustomerDuplicateWarningDto,
} from "@/lib/crm-api";
import { crmCapabilityAllows } from "@/lib/crm-auth/crm-capabilities";
import { getCrmApiRequestOptions } from "@/lib/crm-auth/crm-api-request-options";
import { handleCrmApiPageError } from "@/lib/crm-auth/handle-crm-api-page-error";
import { requireCrmSession } from "@/lib/crm-auth/require-crm-session";
import { tCrm } from "@/lib/i18n/crm-i18n";
import { getRequestDateSettings } from "@/lib/i18n/request-date-settings";

export default async function CustomerDetailPage({ params }: { params: Promise<{ id: string }> }) {
  const resolved = await params;
  const session = await requireCrmSession(`/customers/${resolved.id}`);
  const dateSettings = await getRequestDateSettings();
  const locale = dateSettings.locale;
  const canEdit = crmCapabilityAllows(session.capabilities, "customers.edit");
  const canDelete = crmCapabilityAllows(session.capabilities, "customers.delete");
  const canReadContacts = crmCapabilityAllows(session.capabilities, "contacts.read");
  const canReadHealth = crmCapabilityAllows(session.capabilities, "customerIntelligence.read");
  const canReadActivities = crmCapabilityAllows(session.capabilities, "activities.read");
  const canCreateActivities = crmCapabilityAllows(session.capabilities, "activities.create");
  const canReviewDuplicates = crmCapabilityAllows(
    session.capabilities,
    "customers.duplicates.review",
  );

  if (!isGuid(resolved.id)) {
    notFound();
  }

  let customer: CustomerDetailDto;
  let duplicateWarnings: CustomerDuplicateWarningDto[] = [];
  let customer360: Customer360Dto | null = null;
  let contacts: CrmPagedResult<ContactListItemDto> | null = null;
  let consents: CustomerConsentDto[] = [];
  let hierarchy: CustomerAccountHierarchyDto | null = null;
  let auditTimeline: CustomerAuditEventDto[] = [];
  let unifiedTimelineFeed: ActivityTimelineFeed = {
    items: [],
    totalCount: 0,
    page: 1,
    pageSize: 10,
  };
  let isUnifiedTimelineUnavailable = false;

  try {
    customer = await getCustomerDetailData(resolved.id, `/customers/${resolved.id}`);
    const options = await getCrmApiRequestOptions();
    const [
      customer360Result,
      consentsResult,
      hierarchyResult,
      contactsResult,
      auditResult,
      duplicatesResult,
    ] = await Promise.allSettled([
      crmApiClient.getCustomer360(resolved.id, options),
      crmApiClient.getCustomerConsents(resolved.id, options),
      crmApiClient.getCustomerHierarchy(resolved.id, options),
      canReadContacts
        ? crmApiClient.listCustomerContacts(resolved.id, { page: 1, pageSize: 10 }, options)
        : Promise.resolve(null),
      canReadHealth
        ? crmApiClient.getCustomerAuditTimeline(resolved.id, { page: 1, pageSize: 50 }, options)
        : Promise.resolve([]),
      canReviewDuplicates
        ? crmApiClient.findCustomerDuplicates(resolved.id, options)
        : Promise.resolve([]),
    ]);

    customer360 = customer360Result.status === "fulfilled" ? customer360Result.value : null;
    consents = consentsResult.status === "fulfilled" ? (consentsResult.value ?? []) : [];
    hierarchy = hierarchyResult.status === "fulfilled" ? hierarchyResult.value : null;
    contacts = contactsResult.status === "fulfilled" ? contactsResult.value : null;
    auditTimeline = auditResult.status === "fulfilled" ? (auditResult.value ?? []) : [];
    duplicateWarnings =
      duplicatesResult.status === "fulfilled" ? (duplicatesResult.value ?? []) : [];

    if (canReadActivities) {
      try {
        unifiedTimelineFeed = await getCustomerActivitiesTimelineData(
          resolved.id,
          `/customers/${resolved.id}`,
          {
            page: 1,
            pageSize: 10,
          },
        );
      } catch {
        isUnifiedTimelineUnavailable = true;
      }
    }
  } catch (error) {
    if (error instanceof CrmApiError && error.kind === "not_found") {
      notFound();
    }

    handleCrmApiPageError(error, `/customers/${resolved.id}`);
  }

  const dqScore = customer360?.dataQuality?.score ?? 0;
  const rhScore = customer360?.relationshipHealth?.score ?? 0;
  const nextActions = customer360?.suggestedNextActions ?? [];

  return (
    <CrmPageShell
      title={customer.fullName}
      description={tCrm("crm.customers.pages.detail.description", locale)}
      actions={
        <div className="flex items-center gap-2">
          {customer.isVip ? (
            <Badge variant="outline" className="bg-amber-500/10 text-amber-500 border-amber-500/20">
              {tCrm("crm.customers.fields.vip", locale)}
            </Badge>
          ) : null}
          {canEdit ? (
            <Button asChild size="sm">
              <Link href={`/customers/${resolved.id}/edit`}>
                {tCrm("crm.customers.actions.edit", locale)}
              </Link>
            </Button>
          ) : null}
        </div>
      }
    >
      <div className="mx-auto w-full pt-6 pb-16 px-4 md:px-6">
        {/* Outermost container section: Single workspace canvas wrapping everything */}
        <section>
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-10 items-start">
            {/* Main Area (Left 2/3) */}
            <div className="lg:col-span-2 space-y-8">
              {canEdit ? (
                <div className="pb-6 border-b border-border/30">
                  <CrmImageUploader
                    altText={customer.fullName}
                    description={tCrm("crm.customers.pages.detail.imageDescription", locale)}
                    imageUrl={customer.imageUrl}
                    removeAction={removeCustomerImageAction.bind(null, resolved.id)}
                    removeLabel={tCrm("crm.media.actions.remove", locale)}
                    title={tCrm("crm.customers.pages.detail.imageTitle", locale)}
                    uploadAction={uploadCustomerImageAction.bind(null, resolved.id)}
                    uploadLabel={tCrm("crm.media.actions.upload", locale)}
                  />
                </div>
              ) : null}

              <div>
                <CustomerDetailWorkspace
                  auditTimeline={auditTimeline}
                  canManage={canEdit}
                  canReadHealth={canReadHealth}
                  canReviewDuplicates={canReviewDuplicates}
                  consents={consents}
                  contacts={contacts}
                  customer={customer}
                  customer360={customer360}
                  dateSettings={dateSettings}
                  duplicateWarnings={duplicateWarnings}
                  hierarchy={hierarchy}
                  canReadActivities={canReadActivities}
                  canCreateActivities={canCreateActivities}
                  unifiedTimeline={unifiedTimelineFeed}
                  isUnifiedTimelineUnavailable={isUnifiedTimelineUnavailable}
                  locale={locale}
                />
              </div>

              <div className="pt-6 border-t border-border/30">
                <AddressSection
                  entityType="customer"
                  entityId={resolved.id}
                  addresses={customer.addresses}
                  canManage={canEdit || canDelete}
                />
              </div>

              {canDelete ? (
                <div className="pt-6 border-t border-border/30">
                  <CrmDeleteZone
                    title={tCrm("crm.customers.actions.delete", locale)}
                    description={tCrm("crm.customers.pages.detail.deleteDescription", locale)}
                    locale={locale}
                    dangerTitle={tCrm("crm.delete.dangerTitle", locale)}
                  >
                    <CrmDeleteConfirmForm
                      entityLabel={tCrm("crm.customers.entityName", locale)}
                      entityName={customer.fullName}
                      confirmValue="delete-customer"
                      action={deleteCustomerAction.bind(null, resolved.id)}
                    />
                  </CrmDeleteZone>
                </div>
              ) : null}
            </div>

            {/* Sidebar Area (Right 1/3) - Separated by left border line on desktop */}
            <div className="space-y-8 lg:border-l lg:border-border/30 lg:pl-8 lg:sticky lg:top-6 lg:self-start">
              {/* Health & Intelligence Score Section */}
              {canReadHealth ? (
                <div className="space-y-4">
                  <h3 className="text-sm font-semibold text-foreground tracking-tight flex items-center gap-2">
                    <Activity className="size-4 text-primary" />
                    <span>{tCrm("crm.customers.pages.detail.customer360Title", locale)}</span>
                  </h3>
                  <div className="space-y-4">
                    {/* Data Quality Score Bar */}
                    <div className="space-y-1">
                      <div className="flex justify-between text-xs font-medium">
                        <span className="text-muted-foreground">
                          {tCrm("crm.customers.fields.dataQuality", locale)}
                        </span>
                        <span className="text-foreground">{dqScore ?? "-"}%</span>
                      </div>
                      <div className="h-1.5 w-full bg-muted-foreground/10 rounded-full overflow-hidden">
                        <div
                          className="h-full bg-primary transition-all duration-500"
                          style={{ width: `${Math.min(Math.max(Number(dqScore) || 0, 0), 100)}%` }}
                        />
                      </div>
                    </div>

                    {/* Relationship Health Score Bar */}
                    <div className="space-y-1">
                      <div className="flex justify-between text-xs font-medium">
                        <span className="text-muted-foreground">
                          {tCrm("crm.customers.fields.relationshipHealth", locale)}
                        </span>
                        <span className="text-foreground">{rhScore ?? "-"}%</span>
                      </div>
                      <div className="h-1.5 w-full bg-muted-foreground/10 rounded-full overflow-hidden">
                        <div
                          className="h-full bg-emerald-500 transition-all duration-500"
                          style={{ width: `${Math.min(Math.max(Number(rhScore) || 0, 0), 100)}%` }}
                        />
                      </div>
                    </div>

                    {/* Quick Metrics Grid */}
                    <div className="grid grid-cols-2 gap-3 pt-2">
                      <div className="p-3 rounded-lg border border-border/20 bg-background/30">
                        <div className="text-[10px] uppercase font-semibold text-muted-foreground tracking-wider">
                          Open Tickets
                        </div>
                        <div className="text-lg font-semibold mt-0.5 text-foreground">
                          {customer360?.relationshipHealth?.openTicketCount ?? 0}
                        </div>
                      </div>
                      <div className="p-3 rounded-lg border border-border/20 bg-background/30">
                        <div className="text-[10px] uppercase font-semibold text-muted-foreground tracking-wider">
                          Overdue
                        </div>
                        <div className="text-lg font-semibold mt-0.5 text-rose-500">
                          {customer360?.relationshipHealth?.overdueTicketCount ?? 0}
                        </div>
                      </div>
                      <div className="p-3 rounded-lg border border-border/20 bg-background/30">
                        <div className="text-[10px] uppercase font-semibold text-muted-foreground tracking-wider">
                          Opportunities
                        </div>
                        <div className="text-lg font-semibold mt-0.5 text-foreground">
                          {customer360?.relationshipHealth?.openOpportunityCount ?? 0}
                        </div>
                      </div>
                      <div className="p-3 rounded-lg border border-border/20 bg-background/30">
                        <div className="text-[10px] uppercase font-semibold text-muted-foreground tracking-wider">
                          Unpaid
                        </div>
                        <div className="text-lg font-semibold mt-0.5 text-amber-500">
                          {customer360?.relationshipHealth?.unpaidInvoiceCount ?? 0}
                        </div>
                      </div>
                    </div>

                    {/* Next Actions */}
                    {nextActions.length > 0 ? (
                      <div className="space-y-2 border-t border-border/20 pt-4">
                        <div className="text-[10px] font-semibold text-muted-foreground uppercase tracking-wider">
                          Suggested Actions
                        </div>
                        <div className="flex flex-wrap gap-1.5">
                          {nextActions.slice(0, 6).map((action) => (
                            <Badge
                              key={action.code}
                              variant="outline"
                              className="text-[11px] bg-background/30"
                            >
                              {action.title}
                            </Badge>
                          ))}
                        </div>
                      </div>
                    ) : null}
                  </div>
                </div>
              ) : null}

              {/* Profile Field Details Grid (Flat metadata list) */}
              <div className="space-y-4 pt-6 border-t border-border/20">
                <h3 className="text-sm font-semibold text-foreground tracking-tight">
                  {tCrm("crm.customers.pages.detail.profileTitle", locale)}
                </h3>
                <dl className="space-y-3 text-xs">
                  <SidebarField
                    label={tCrm("crm.customers.fields.email", locale)}
                    value={customer.email}
                  />
                  <SidebarField
                    label={tCrm("crm.customers.fields.mobilePhoneShort", locale)}
                    value={customer.mobilePhone}
                  />
                  <SidebarField
                    label={tCrm("crm.customers.fields.company", locale)}
                    value={customer.companyName}
                  />
                  <SidebarField
                    label={tCrm("crm.customers.fields.customerType", locale)}
                    value={customer.customerType}
                  />
                  <SidebarField
                    label={tCrm("crm.customers.fields.department", locale)}
                    value={customer.department}
                  />
                  <SidebarField
                    label={tCrm("crm.customers.fields.jobTitle", locale)}
                    value={customer.jobTitle}
                  />
                  <SidebarField
                    label={tCrm("crm.customers.fields.identityNumber", locale)}
                    value={customer.identityNumber}
                  />
                  <SidebarField
                    label={tCrm("crm.customers.fields.ownerUserId", locale)}
                    value={customer.ownerUserId}
                  />
                  <div className="flex justify-between py-1.5 border-b border-border/20 last:border-0 items-center">
                    <dt className="text-muted-foreground font-medium">
                      {tCrm("crm.customers.fields.status", locale)}
                    </dt>
                    <dd className="font-semibold text-foreground">
                      <Badge
                        variant="outline"
                        className={
                          customer.isActive
                            ? "bg-emerald-500/10 text-emerald-500 border-emerald-500/20"
                            : "bg-muted text-muted-foreground"
                        }
                      >
                        {customer.isActive
                          ? tCrm("crm.common.active", locale)
                          : tCrm("crm.common.inactive", locale)}
                      </Badge>
                    </dd>
                  </div>
                  <div className="flex justify-between py-1.5 border-b border-border/20 last:border-0 items-center">
                    <dt className="text-muted-foreground font-medium">
                      {tCrm("crm.customers.fields.vip", locale)}
                    </dt>
                    <dd className="font-semibold text-foreground">
                      <Badge
                        variant="outline"
                        className={
                          customer.isVip
                            ? "bg-amber-500/10 text-amber-500 border-amber-500/20"
                            : "bg-muted text-muted-foreground"
                        }
                      >
                        {customer.isVip
                          ? tCrm("crm.common.yes", locale)
                          : tCrm("crm.common.no", locale)}
                      </Badge>
                    </dd>
                  </div>
                </dl>
              </div>

              {/* Duplicate Warnings (Flat warnings list) */}
              {canReviewDuplicates && duplicateWarnings.length > 0 ? (
                <div className="space-y-4 pt-6 border-t border-border/20">
                  <div className="flex items-center gap-2 text-amber-500">
                    <AlertTriangle className="size-4" />
                    <h3 className="text-sm font-semibold tracking-tight">
                      {tCrm("crm.customers.duplicates.title", locale)}
                    </h3>
                  </div>
                  <div className="space-y-3 text-xs">
                    <p className="text-muted-foreground">
                      {tCrm("crm.customers.duplicates.description", locale)}
                    </p>
                    <ul className="space-y-2">
                      {duplicateWarnings.map((warning) => (
                        <li
                          key={warning.candidateId}
                          className="rounded-lg border border-amber-500/20 bg-background/30 p-3 space-y-1"
                        >
                          <div className="font-medium text-foreground">
                            {tCrm("crm.customers.duplicates.candidate", locale)}:{" "}
                            <span className="font-mono text-[11px]">{warning.candidateId}</span>
                          </div>
                          <div className="text-muted-foreground">
                            {tCrm("crm.customers.duplicates.confidence", locale)}:{" "}
                            <span className="font-semibold text-foreground">{warning.score}</span>
                          </div>
                          <div className="text-muted-foreground">
                            {tCrm("crm.customers.duplicates.reason", locale)}:{" "}
                            <span className="text-foreground">{warning.reasons.join(", ")}</span>
                          </div>
                        </li>
                      ))}
                    </ul>
                    <p className="text-[11px] text-muted-foreground">
                      {tCrm("crm.customers.duplicates.manualResolution", locale)}
                    </p>
                  </div>
                </div>
              ) : null}
            </div>
          </div>
        </section>
      </div>
    </CrmPageShell>
  );
}

function SidebarField({
  label,
  value,
}: Readonly<{ label: string; value: string | number | null | undefined }>) {
  return (
    <div className="flex justify-between py-1.5 border-b border-border/20 last:border-0 items-start gap-4">
      <dt className="text-muted-foreground font-medium shrink-0">{label}</dt>
      <dd className="font-semibold text-foreground text-right break-all">{value ?? "-"}</dd>
    </div>
  );
}
