import { notFound } from "next/navigation";
import Link from "next/link";
import { Button } from "@netmetric/ui";

import { CrmDeleteConfirmForm } from "@/components/delete/crm-delete-confirm-form";
import { CrmDeleteZone } from "@/components/delete/crm-delete-zone";
import { CrmPageShell } from "@/components/shell/crm-page-shell";
import { deleteOpportunityAction } from "@/features/opportunities/actions/opportunity-mutation-actions";
import { OpportunityDetailWorkspace } from "@/features/opportunities/components/opportunity-detail-workspace";
import {
  getOpportunityDetailData,
  getOpportunityLostReasonsData,
  getOpportunityQuotesData,
  getOpportunityTimelineData,
  getOpportunityWorkspaceData,
} from "@/features/opportunities/data/opportunities-data";
import { isGuid } from "@/features/shared/data/guid";
import {
  CrmApiError,
  crmApiClient,
  type ActivityTimelineFeed,
  type OpportunityDetailDto,
  type OpportunityLostReasonDto,
  type OpportunityQuoteDetailDto,
  type OpportunityStageHistoryDto,
  type OpportunityTimelineEventDto,
  type OpportunityWorkspaceDto,
} from "@/lib/crm-api";
import { crmCapabilityAllows } from "@/lib/crm-auth/crm-capabilities";
import { getCrmApiRequestOptions } from "@/lib/crm-auth/crm-api-request-options";
import { handleCrmApiPageError } from "@/lib/crm-auth/handle-crm-api-page-error";
import { requireCrmSession } from "@/lib/crm-auth/require-crm-session";
import { getRequestDateSettings } from "@/lib/i18n/request-date-settings";
import { tCrm } from "@/lib/i18n/crm-i18n";

export default async function OpportunityDetailPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const resolved = await params;
  const session = await requireCrmSession(`/opportunities/${resolved.id}`);
  const dateSettings = await getRequestDateSettings();
  const locale = dateSettings.locale;
  const canEdit = crmCapabilityAllows(session.capabilities, "opportunities.edit");
  const canDelete = crmCapabilityAllows(session.capabilities, "opportunities.delete");
  const canReadQuotes = crmCapabilityAllows(session.capabilities, "opportunityQuotes.read");
  const canManageQuotes = crmCapabilityAllows(session.capabilities, "opportunityQuotes.manage");
  const canReadActivities = crmCapabilityAllows(session.capabilities, "activities.read");
  const canCreateActivities = crmCapabilityAllows(session.capabilities, "activities.create");
  const canReadStageHistory = crmCapabilityAllows(
    session.capabilities,
    "pipelineStageHistory.read",
  );

  if (!isGuid(resolved.id)) {
    notFound();
  }

  let opportunity: OpportunityDetailDto;
  let workspace: OpportunityWorkspaceDto;
  let timeline: OpportunityTimelineEventDto[] = [];
  let stageHistory: OpportunityStageHistoryDto[] = [];
  let lostReasons: OpportunityLostReasonDto[] = [];
  let quotes: OpportunityQuoteDetailDto[] = [];
  let unifiedTimelineFeed: ActivityTimelineFeed = {
    items: [],
    totalCount: 0,
    page: 1,
    pageSize: 10,
  };
  let isUnifiedTimelineUnavailable = false;

  try {
    try {
      workspace = await getOpportunityWorkspaceData(resolved.id, `/opportunities/${resolved.id}`);
      opportunity = workspace.opportunity;
    } catch (error) {
      if (error instanceof CrmApiError && error.kind === "not_found") {
        opportunity = await getOpportunityDetailData(resolved.id, `/opportunities/${resolved.id}`);
        workspace = {
          opportunity,
          totalQuoteAmount: null,
          quoteCount: 0,
          activityCount: 0,
          stageChangeCount: 0,
        };
      } else {
        throw error;
      }
    }
    timeline = await getOpportunityTimelineData(resolved.id, `/opportunities/${resolved.id}`);
    lostReasons = await getOpportunityLostReasonsData(`/opportunities/${resolved.id}`);
    if (canReadStageHistory) {
      try {
        const options = await getCrmApiRequestOptions();
        stageHistory = await crmApiClient.getPipelineOpportunityStageHistory(resolved.id, options);
      } catch (error) {
        if (error instanceof CrmApiError && error.kind === "not_found") {
          stageHistory = [];
        } else {
          throw error;
        }
      }
    } else {
      stageHistory = [];
    }
    quotes = canReadQuotes
      ? await getOpportunityQuotesData(resolved.id, `/opportunities/${resolved.id}`)
      : [];
    if (canReadActivities) {
      try {
        const options = await getCrmApiRequestOptions();
        unifiedTimelineFeed = await crmApiClient.listRelatedActivities(
          "opportunity",
          resolved.id,
          {
            page: 1,
            pageSize: 10,
          },
          options,
        );
      } catch {
        isUnifiedTimelineUnavailable = true;
      }
    }
  } catch (error) {
    if (error instanceof CrmApiError && error.kind === "not_found") {
      notFound();
    }

    handleCrmApiPageError(error, `/opportunities/${resolved.id}`);
  }

  return (
    <CrmPageShell
      title={opportunity.name}
      description={tCrm("crm.opportunities.pages.detail.description", locale)}
      actions={
        canEdit ? (
          <Button asChild>
            <Link href={`/opportunities/${resolved.id}/edit`}>
              {tCrm("crm.opportunities.actions.edit", locale)}
            </Link>
          </Button>
        ) : undefined
      }
    >
      <OpportunityDetailWorkspace
        opportunity={opportunity}
        workspace={workspace}
        timeline={timeline}
        stageHistory={stageHistory}
        lostReasons={lostReasons}
        quotes={quotes}
        canEdit={canEdit}
        canManageQuotes={canManageQuotes}
        canReadActivities={canReadActivities}
        canCreateActivities={canCreateActivities}
        unifiedTimeline={unifiedTimelineFeed}
        isUnifiedTimelineUnavailable={isUnifiedTimelineUnavailable}
        dateSettings={dateSettings}
        locale={locale}
      />
      {canDelete ? (
        <CrmDeleteZone
          title={tCrm("crm.opportunities.actions.delete", locale)}
          description={tCrm("crm.opportunities.pages.detail.deleteDescription", locale)}
        >
          <CrmDeleteConfirmForm
            entityLabel={tCrm("crm.opportunities.entityName", locale)}
            entityName={opportunity.name}
            confirmValue="delete-opportunity"
            action={deleteOpportunityAction.bind(null, resolved.id)}
          />
        </CrmDeleteZone>
      ) : null}
    </CrmPageShell>
  );
}
