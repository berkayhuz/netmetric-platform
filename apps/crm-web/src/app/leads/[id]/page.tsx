import { notFound } from "next/navigation";
import Link from "next/link";
import { Button } from "@netmetric/ui";

import { CrmDeleteConfirmForm } from "@/components/delete/crm-delete-confirm-form";
import { CrmDeleteZone } from "@/components/delete/crm-delete-zone";
import { CrmEntityDetailPanel } from "@/components/shell/crm-entity-detail-panel";
import { CrmPageShell } from "@/components/shell/crm-page-shell";
import { deleteLeadAction } from "@/features/leads/actions/lead-mutation-actions";
import { LeadDetailWorkspace } from "@/features/leads/components/lead-detail-workspace";
import {
  getLeadDetailData,
  getLeadActivitiesTimelineData,
  getLeadTimelineData,
  getLeadWorkspaceData,
} from "@/features/leads/data/leads-data";
import { isGuid } from "@/features/shared/data/guid";
import {
  CrmApiError,
  type ActivityTimelineFeed,
  type LeadDetailDto,
  type LeadTimelineEventDto,
  type LeadWorkspaceDto,
} from "@/lib/crm-api";
import { crmCapabilityAllows } from "@/lib/crm-auth/crm-capabilities";
import { handleCrmApiPageError } from "@/lib/crm-auth/handle-crm-api-page-error";
import { requireCrmSession } from "@/lib/crm-auth/require-crm-session";
import { tCrm } from "@/lib/i18n/crm-i18n";
import { getRequestDateSettings } from "@/lib/i18n/request-date-settings";

export default async function LeadDetailPage({ params }: { params: Promise<{ id: string }> }) {
  const resolved = await params;
  const session = await requireCrmSession(`/leads/${resolved.id}`);
  const dateSettings = await getRequestDateSettings();
  const locale = dateSettings.locale;
  const canEdit = crmCapabilityAllows(session.capabilities, "leads.edit");
  const canDelete = crmCapabilityAllows(session.capabilities, "leads.delete");
  const canReadActivities = crmCapabilityAllows(session.capabilities, "activities.read");
  const canCreateActivities = crmCapabilityAllows(session.capabilities, "activities.create");

  if (!isGuid(resolved.id)) {
    notFound();
  }

  let lead: LeadDetailDto;
  let workspace: LeadWorkspaceDto;
  let timeline: LeadTimelineEventDto[];
  let unifiedTimelineFeed: ActivityTimelineFeed = {
    items: [],
    totalCount: 0,
    page: 1,
    pageSize: 10,
  };
  let isUnifiedTimelineUnavailable = false;

  try {
    [lead, workspace, timeline] = await Promise.all([
      getLeadDetailData(resolved.id, `/leads/${resolved.id}`),
      getLeadWorkspaceData(resolved.id, `/leads/${resolved.id}`),
      getLeadTimelineData(resolved.id, `/leads/${resolved.id}`),
    ]);

    if (canReadActivities) {
      try {
        unifiedTimelineFeed = await getLeadActivitiesTimelineData(
          resolved.id,
          `/leads/${resolved.id}`,
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

    handleCrmApiPageError(error, `/leads/${resolved.id}`);
  }

  return (
    <CrmPageShell
      title={lead.fullName}
      description={tCrm("crm.leads.pages.detail.description", locale)}
      actions={
        canEdit ? (
          <Button asChild>
            <Link href={`/leads/${resolved.id}/edit`}>
              {tCrm("crm.leads.actions.edit", locale)}
            </Link>
          </Button>
        ) : undefined
      }
    >
      <CrmEntityDetailPanel
        title={tCrm("crm.leads.pages.detail.profileTitle", locale)}
        fields={[
          { label: tCrm("crm.leads.fields.leadCode", locale), value: lead.leadCode },
          { label: tCrm("crm.leads.fields.fullName", locale), value: lead.fullName },
          { label: tCrm("crm.leads.fields.company", locale), value: lead.companyName },
          { label: tCrm("crm.leads.fields.email", locale), value: lead.email },
          { label: tCrm("crm.leads.fields.phone", locale), value: lead.phone },
          {
            label: tCrm("crm.leads.fields.status", locale),
            value: tCrm(`crm.leads.status.${lead.status}`, locale),
          },
          {
            label: tCrm("crm.leads.fields.source", locale),
            value: tCrm(`crm.leads.source.${lead.source}`, locale),
          },
          {
            label: tCrm("crm.leads.fields.priority", locale),
            value: tCrm(`crm.common.priority.${lead.priority}`, locale),
          },
          { label: tCrm("crm.leads.fields.score", locale), value: lead.totalScore },
          { label: tCrm("crm.leads.fields.grade", locale), value: String(lead.grade) },
          {
            label: tCrm("crm.leads.fields.slaBreached", locale),
            value: lead.slaBreached
              ? tCrm("crm.common.yes", locale)
              : tCrm("crm.common.no", locale),
          },
          {
            label: tCrm("crm.leads.fields.state", locale),
            value: lead.isActive
              ? tCrm("crm.common.active", locale)
              : tCrm("crm.common.inactive", locale),
          },
        ]}
      />
      <LeadDetailWorkspace
        canEdit={canEdit}
        dateSettings={dateSettings}
        lead={lead}
        locale={locale}
        timeline={timeline}
        unifiedTimeline={unifiedTimelineFeed}
        canReadActivities={canReadActivities}
        canCreateActivities={canReadActivities && canCreateActivities}
        isUnifiedTimelineUnavailable={isUnifiedTimelineUnavailable}
        workspace={workspace}
      />
      {canDelete ? (
        <CrmDeleteZone
          title={tCrm("crm.leads.actions.delete", locale)}
          description={tCrm("crm.leads.pages.detail.deleteDescription", locale)}
        >
          <CrmDeleteConfirmForm
            entityLabel={tCrm("crm.leads.entityName", locale)}
            entityName={lead.fullName}
            confirmValue="delete-lead"
            action={deleteLeadAction.bind(null, resolved.id)}
          />
        </CrmDeleteZone>
      ) : null}
    </CrmPageShell>
  );
}
