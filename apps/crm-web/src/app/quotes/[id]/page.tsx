import { notFound } from "next/navigation";
import Link from "next/link";
import { Button } from "@netmetric/ui";

import { CrmDeleteConfirmForm } from "@/components/delete/crm-delete-confirm-form";
import { CrmDeleteZone } from "@/components/delete/crm-delete-zone";
import { CrmPageShell } from "@/components/shell/crm-page-shell";
import { QuoteDetailWorkspace } from "@/features/quotes/components/quote-detail-workspace";
import { deleteQuoteAction } from "@/features/quotes/actions/quote-mutation-actions";
import { getQuoteTimelineData, getQuoteWorkspaceData } from "@/features/quotes/data/quotes-data";
import { isGuid } from "@/features/shared/data/guid";
import {
  CrmApiError,
  crmApiClient,
  type ActivityTimelineFeed,
  type CpqValidationResultDto,
  type CpqWorkspaceDto,
  type ProposalTemplateDto,
  type QuoteDetailDto,
  type QuoteTimelineEventDto,
  type QuoteWorkspaceDto,
} from "@/lib/crm-api";
import { crmCapabilityAllows } from "@/lib/crm-auth/crm-capabilities";
import { getCrmApiRequestOptions } from "@/lib/crm-auth/crm-api-request-options";
import { handleCrmApiPageError } from "@/lib/crm-auth/handle-crm-api-page-error";
import { requireCrmSession } from "@/lib/crm-auth/require-crm-session";
import { getRequestDateSettings } from "@/lib/i18n/request-date-settings";
import { tCrm } from "@/lib/i18n/crm-i18n";

export default async function QuoteDetailPage({ params }: { params: Promise<{ id: string }> }) {
  const resolved = await params;
  const session = await requireCrmSession(`/quotes/${resolved.id}`);
  const dateSettings = await getRequestDateSettings();
  const locale = dateSettings.locale;
  const canEdit = crmCapabilityAllows(session.capabilities, "quotes.edit");
  const canDelete = crmCapabilityAllows(session.capabilities, "quotes.delete");
  const canReadProposals = crmCapabilityAllows(session.capabilities, "proposals.read");
  const canManageProposals = crmCapabilityAllows(session.capabilities, "proposals.manage");
  const canReadActivities = crmCapabilityAllows(session.capabilities, "activities.read");
  const canCreateActivities = crmCapabilityAllows(session.capabilities, "activities.create");

  if (!isGuid(resolved.id)) {
    notFound();
  }

  let quote: QuoteDetailDto;
  let workspace: QuoteWorkspaceDto;
  let timeline: QuoteTimelineEventDto[] = [];
  let validation: CpqValidationResultDto = { isValid: true, violations: [] };
  let cpqWorkspace: CpqWorkspaceDto | null = null;
  let proposalTemplates: ProposalTemplateDto[] | null = null;
  let unifiedTimelineFeed: ActivityTimelineFeed = {
    items: [],
    totalCount: 0,
    page: 1,
    pageSize: 10,
  };
  let isUnifiedTimelineUnavailable = false;
  let isValidationUnavailable = false;
  let isCpqWorkspaceUnavailable = false;
  let isProposalTemplatesUnavailable = false;

  try {
    workspace = await getQuoteWorkspaceData(resolved.id, `/quotes/${resolved.id}`);
    quote = workspace.quote;
    timeline = await getQuoteTimelineData(resolved.id, `/quotes/${resolved.id}`);
    try {
      const options = await getCrmApiRequestOptions();
      validation = await crmApiClient.validateQuoteConfiguration(resolved.id, options);
    } catch {
      isValidationUnavailable = true;
    }
    try {
      const options = await getCrmApiRequestOptions();
      cpqWorkspace = await crmApiClient.getQuoteCpqWorkspace(options);
    } catch {
      isCpqWorkspaceUnavailable = true;
    }
    if (canReadProposals) {
      try {
        const options = await getCrmApiRequestOptions();
        proposalTemplates = await crmApiClient.listProposalTemplates(undefined, options);
      } catch {
        isProposalTemplatesUnavailable = true;
      }
    }
    if (canReadActivities) {
      try {
        const options = await getCrmApiRequestOptions();
        unifiedTimelineFeed = await crmApiClient.listRelatedActivities(
          "quote",
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

    handleCrmApiPageError(error, `/quotes/${resolved.id}`);
  }

  if (isValidationUnavailable || isCpqWorkspaceUnavailable || isProposalTemplatesUnavailable) {
    isUnifiedTimelineUnavailable = true;
  }

  return (
    <CrmPageShell
      title={quote.quoteNumber}
      description={tCrm("crm.quotes.detail.description", locale)}
      actions={
        canEdit ? (
          <Button asChild>
            <Link href={`/quotes/${resolved.id}/edit`}>
              {tCrm("crm.quotes.actions.edit", locale)}
            </Link>
          </Button>
        ) : undefined
      }
    >
      <QuoteDetailWorkspace
        quoteId={resolved.id}
        workspace={workspace}
        timeline={timeline}
        validation={validation}
        cpqWorkspace={cpqWorkspace}
        proposalTemplates={proposalTemplates}
        canManageQuote={canEdit}
        canManageProposals={canManageProposals}
        canReadActivities={canReadActivities}
        canCreateActivities={canCreateActivities}
        unifiedTimeline={unifiedTimelineFeed}
        isUnifiedTimelineUnavailable={isUnifiedTimelineUnavailable}
        dateSettings={dateSettings}
        locale={locale}
      />
      {canDelete ? (
        <CrmDeleteZone
          title={tCrm("crm.quotes.delete.title", locale)}
          description={tCrm("crm.quotes.delete.description", locale)}
        >
          <CrmDeleteConfirmForm
            entityLabel={tCrm("crm.quotes.entityLabel", locale)}
            entityName={quote.quoteNumber}
            confirmValue="delete-quote"
            action={deleteQuoteAction.bind(null, resolved.id)}
          />
        </CrmDeleteZone>
      ) : null}
    </CrmPageShell>
  );
}
