import Link from "next/link";
import { notFound } from "next/navigation";
import { Button } from "@netmetric/ui";

import { AddressSection } from "@/components/address/address-section";
import { CrmDeleteConfirmForm } from "@/components/delete/crm-delete-confirm-form";
import { CrmDeleteZone } from "@/components/delete/crm-delete-zone";
import { CrmImageUploader } from "@/components/media/crm-image-uploader";
import { CrmEntityDetailPanel } from "@/components/shell/crm-entity-detail-panel";
import { CrmPageShell } from "@/components/shell/crm-page-shell";
import { ActivityComposer } from "@/features/activities/components/activity-composer";
import { ActivityTimelinePanel } from "@/features/activities/components/activity-timeline-panel";
import {
  activateCompanyAction,
  deactivateCompanyAction,
  deleteCompanyAction,
  removeCompanyLogoAction,
  uploadCompanyLogoAction,
} from "@/features/companies/actions/company-mutation-actions";
import { CompanyDetailActions } from "@/features/companies/components/company-detail-actions";
import {
  getCompanyActivitiesTimelineData,
  getCompanyDetailData,
} from "@/features/companies/data/companies-data";
import { isGuid } from "@/features/shared/data/guid";
import { CrmApiError, type ActivityTimelineFeed, type CompanyDetailDto } from "@/lib/crm-api";
import { crmCapabilityAllows } from "@/lib/crm-auth/crm-capabilities";
import { handleCrmApiPageError } from "@/lib/crm-auth/handle-crm-api-page-error";
import { requireCrmSession } from "@/lib/crm-auth/require-crm-session";
import { tCrm } from "@/lib/i18n/crm-i18n";
import { getRequestDateSettings } from "@/lib/i18n/request-date-settings";

export default async function CompanyDetailPage({ params }: { params: Promise<{ id: string }> }) {
  const resolved = await params;
  const session = await requireCrmSession(`/companies/${resolved.id}`);
  const dateSettings = await getRequestDateSettings();
  const locale = dateSettings.locale;
  const canEdit = crmCapabilityAllows(session.capabilities, "companies.edit");
  const canDelete = crmCapabilityAllows(session.capabilities, "companies.delete");
  const canReadActivities = crmCapabilityAllows(session.capabilities, "activities.read");
  const canCreateActivities = crmCapabilityAllows(session.capabilities, "activities.create");

  if (!isGuid(resolved.id)) {
    notFound();
  }

  let company: CompanyDetailDto;
  let unifiedTimelineFeed: ActivityTimelineFeed = {
    items: [],
    totalCount: 0,
    page: 1,
    pageSize: 10,
  };
  let isUnifiedTimelineUnavailable = false;

  try {
    company = await getCompanyDetailData(resolved.id, `/companies/${resolved.id}`);
    if (canReadActivities) {
      try {
        unifiedTimelineFeed = await getCompanyActivitiesTimelineData(
          resolved.id,
          `/companies/${resolved.id}`,
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

    handleCrmApiPageError(error, `/companies/${resolved.id}`);
  }

  return (
    <CrmPageShell
      title={company.name}
      description={tCrm("crm.companies.pages.detail.description", locale)}
      actions={
        canEdit ? (
          <Button asChild>
            <Link href={`/companies/${resolved.id}/edit`}>
              {tCrm("crm.companies.actions.edit", locale)}
            </Link>
          </Button>
        ) : undefined
      }
    >
      {canEdit ? (
        <CrmImageUploader
          altText={company.name}
          description={tCrm("crm.companies.pages.detail.logoDescription", locale)}
          imageUrl={company.logoUrl}
          removeAction={removeCompanyLogoAction.bind(null, resolved.id)}
          removeLabel={tCrm("crm.media.actions.remove", locale)}
          title={tCrm("crm.companies.pages.detail.logoTitle", locale)}
          uploadAction={uploadCompanyLogoAction.bind(null, resolved.id)}
          uploadLabel={tCrm("crm.media.actions.upload", locale)}
        />
      ) : null}
      <CrmEntityDetailPanel
        title={tCrm("crm.companies.pages.detail.profileTitle", locale)}
        fields={[
          { label: tCrm("crm.companies.fields.name", locale), value: company.name },
          { label: tCrm("crm.companies.fields.email", locale), value: company.email },
          { label: tCrm("crm.companies.fields.phone", locale), value: company.phone },
          { label: tCrm("crm.companies.fields.sector", locale), value: company.sector },
          { label: tCrm("crm.companies.fields.companyType", locale), value: company.companyType },
          { label: tCrm("crm.companies.fields.website", locale), value: company.website },
          { label: tCrm("crm.companies.fields.taxNumber", locale), value: company.taxNumber },
          {
            label: tCrm("crm.companies.fields.status", locale),
            value: company.isActive
              ? tCrm("crm.common.active", locale)
              : tCrm("crm.common.inactive", locale),
          },
        ]}
      />
      <AddressSection
        entityType="company"
        entityId={resolved.id}
        addresses={company.addresses}
        canManage={canEdit || canDelete}
      />
      {canReadActivities ? (
        <div className="space-y-4">
          {canCreateActivities ? (
            <ActivityComposer
              primaryRecord={{ entityType: "company", entityId: resolved.id }}
              locale={locale}
            />
          ) : null}
          <ActivityTimelinePanel
            activities={unifiedTimelineFeed.items}
            dateSettings={dateSettings}
            locale={locale}
            title={tCrm("crm.activities.sections.unifiedTimelinePreviewTitle", locale)}
            description={tCrm("crm.activities.sections.unifiedTimelinePreviewDescription", locale)}
            unavailable={isUnifiedTimelineUnavailable}
          />
        </div>
      ) : null}
      {canEdit ? (
        <CompanyDetailActions
          isActive={company.isActive}
          activateAction={activateCompanyAction.bind(null, resolved.id)}
          deactivateAction={deactivateCompanyAction.bind(null, resolved.id)}
        />
      ) : null}
      {canDelete ? (
        <CrmDeleteZone
          title={tCrm("crm.companies.actions.delete", locale)}
          description={tCrm("crm.companies.pages.detail.deleteDescription", locale)}
          locale={locale}
        >
          <CrmDeleteConfirmForm
            entityLabel={tCrm("crm.companies.entityName", locale)}
            entityName={company.name}
            confirmValue="delete-company"
            action={deleteCompanyAction.bind(null, resolved.id)}
          />
        </CrmDeleteZone>
      ) : null}
    </CrmPageShell>
  );
}
