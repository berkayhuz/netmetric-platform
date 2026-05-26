import { notFound } from "next/navigation";
import Link from "next/link";
import { Button } from "@netmetric/ui";

import { CrmDeleteConfirmForm } from "@/components/delete/crm-delete-confirm-form";
import { CrmDeleteZone } from "@/components/delete/crm-delete-zone";
import { CrmPageShell } from "@/components/shell/crm-page-shell";
import {
  changeDealOwnerAction,
  markDealLostAction,
  markDealWonAction,
  reopenDealAction,
} from "@/features/deals/actions/deal-lifecycle-actions";
import { DealDetailWorkspace } from "@/features/deals/components/deal-detail-workspace";
import { deleteDealAction } from "@/features/deals/actions/deal-mutation-actions";
import {
  getDealActivitiesTimelineData,
  getDealLostReasonsData,
  getDealTimelineData,
  getDealWinLossSummaryData,
  getDealWorkspaceData,
} from "@/features/deals/data/deals-data";
import {
  DealLifecycleActionPanel,
  DealOwnerActionPanel,
} from "@/features/deals/forms/deal-lifecycle-panels";
import { isGuid } from "@/features/shared/data/guid";
import {
  CrmApiError,
  type ActivityTimelineFeed,
  type DealDetailDto,
  type DealLostReasonDto,
  type DealOutcomeHistoryDto,
  type DealWinLossSummaryDto,
  type DealWorkspaceDto,
} from "@/lib/crm-api";
import { crmCapabilityAllows } from "@/lib/crm-auth/crm-capabilities";
import { handleCrmApiPageError } from "@/lib/crm-auth/handle-crm-api-page-error";
import { requireCrmSession } from "@/lib/crm-auth/require-crm-session";
import { getRequestDateSettings } from "@/lib/i18n/request-date-settings";
import { tCrm } from "@/lib/i18n/crm-i18n";

export default async function DealDetailPage({ params }: { params: Promise<{ id: string }> }) {
  const resolved = await params;
  const session = await requireCrmSession(`/deals/${resolved.id}`);
  const dateSettings = await getRequestDateSettings();
  const locale = dateSettings.locale;
  const canEdit = crmCapabilityAllows(session.capabilities, "deals.edit");
  const canDelete = crmCapabilityAllows(session.capabilities, "deals.delete");
  const canReadWinLoss = crmCapabilityAllows(session.capabilities, "winLoss.read");
  const canManageWinLoss = crmCapabilityAllows(session.capabilities, "winLoss.manage");
  const canReadActivities = crmCapabilityAllows(session.capabilities, "activities.read");
  const canCreateActivities = crmCapabilityAllows(session.capabilities, "activities.create");

  if (!isGuid(resolved.id)) {
    notFound();
  }

  let deal: DealDetailDto;
  let workspace: DealWorkspaceDto;
  let timeline: DealOutcomeHistoryDto[] = [];
  let lostReasons: DealLostReasonDto[] = [];
  let winLossSummary: DealWinLossSummaryDto | null = null;
  let unifiedTimelineFeed: ActivityTimelineFeed = {
    items: [],
    totalCount: 0,
    page: 1,
    pageSize: 10,
  };
  let isUnifiedTimelineUnavailable = false;

  try {
    workspace = await getDealWorkspaceData(resolved.id, `/deals/${resolved.id}`);
    deal = workspace.deal;
    timeline = await getDealTimelineData(resolved.id, `/deals/${resolved.id}`);
    lostReasons = canReadWinLoss
      ? await getDealLostReasonsData(`/deals/${resolved.id}`)
      : workspace.lostReasons;
    winLossSummary = canReadWinLoss
      ? await getDealWinLossSummaryData({}, `/deals/${resolved.id}`)
      : null;
    if (canReadActivities) {
      try {
        unifiedTimelineFeed = await getDealActivitiesTimelineData(
          resolved.id,
          `/deals/${resolved.id}`,
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

    handleCrmApiPageError(error, `/deals/${resolved.id}`);
  }

  return (
    <CrmPageShell
      title={deal.name}
      description={tCrm("crm.deals.pages.detail.description", locale)}
      actions={
        canEdit ? (
          <Button asChild>
            <Link href={`/deals/${resolved.id}/edit`}>
              {tCrm("crm.deals.actions.edit", locale)}
            </Link>
          </Button>
        ) : undefined
      }
    >
      <DealDetailWorkspace
        deal={deal}
        workspace={{ ...workspace, lostReasons }}
        timeline={timeline}
        winLossSummary={winLossSummary}
        canManageWinLoss={canManageWinLoss}
        canReadActivities={canReadActivities}
        canCreateActivities={canCreateActivities}
        unifiedTimeline={unifiedTimelineFeed}
        isUnifiedTimelineUnavailable={isUnifiedTimelineUnavailable}
        dateSettings={dateSettings}
        locale={locale}
      />
      {canEdit ? (
        <div className="grid gap-4 lg:grid-cols-2">
          <DealOwnerActionPanel
            dealId={resolved.id}
            ownerUserId={deal.ownerUserId ?? null}
            action={changeDealOwnerAction.bind(null, resolved.id)}
          />
          <DealLifecycleActionPanel
            title={tCrm("crm.deals.actions.markWon", locale)}
            description={tCrm("crm.deals.lifecycle.markWonDescription", locale)}
            confirmValue="mark-deal-won"
            action={markDealWonAction.bind(null, resolved.id)}
            rowVersion={deal.rowVersion}
          />
          <DealLifecycleActionPanel
            title={tCrm("crm.deals.actions.markLost", locale)}
            description={tCrm("crm.deals.lifecycle.markLostDescription", locale)}
            confirmValue="mark-deal-lost"
            action={markDealLostAction.bind(null, resolved.id)}
            showLostReason
            lostReasons={lostReasons}
            rowVersion={deal.rowVersion}
          />
          <DealLifecycleActionPanel
            title={tCrm("crm.deals.actions.reopen", locale)}
            description={tCrm("crm.deals.lifecycle.reopenDescription", locale)}
            confirmValue="reopen-deal"
            action={reopenDealAction.bind(null, resolved.id)}
            rowVersion={deal.rowVersion}
          />
        </div>
      ) : null}
      {canDelete ? (
        <CrmDeleteZone
          title={tCrm("crm.deals.actions.delete", locale)}
          description={tCrm("crm.deals.pages.detail.deleteDescription", locale)}
        >
          <CrmDeleteConfirmForm
            entityLabel={tCrm("crm.deals.entityName", locale)}
            entityName={deal.name}
            confirmValue="delete-deal"
            action={deleteDealAction.bind(null, resolved.id)}
          />
        </CrmDeleteZone>
      ) : null}
    </CrmPageShell>
  );
}
