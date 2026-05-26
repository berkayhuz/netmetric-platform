import { CrmEmptyState } from "@/components/shell/crm-empty-state";
import { CrmPageShell } from "@/components/shell/crm-page-shell";
import { PipelineBoard } from "@/features/pipeline/components/pipeline-board";
import { PipelineOperationsPanel } from "@/features/pipeline/components/pipeline-operations-panel";
import {
  getPipelineAnalyticsData,
  getPipelineBoardData,
  getPipelineDetailData,
  getPipelineLostReasonsData,
  getPipelinesData,
} from "@/features/pipeline/data/pipeline-data";
import { isGuid } from "@/features/shared/data/guid";
import { crmCapabilityAllows } from "@/lib/crm-auth/crm-capabilities";
import { requireCrmSession } from "@/lib/crm-auth/require-crm-session";
import { tCrm } from "@/lib/i18n/crm-i18n";
import { getRequestLocale } from "@/lib/i18n/request-locale";

export default async function PipelinePage({
  searchParams,
}: {
  searchParams: Promise<Record<string, string | string[] | undefined>>;
}) {
  const session = await requireCrmSession("/pipeline");
  const locale = await getRequestLocale();
  const canManagePipelines = crmCapabilityAllows(session.capabilities, "pipeline.manage");
  const canReadLostReasons = crmCapabilityAllows(session.capabilities, "pipelineLostReasons.read");
  const canManageLostReasons = crmCapabilityAllows(
    session.capabilities,
    "pipelineLostReasons.manage",
  );

  const params = await searchParams;
  const selectedPipelineIdRaw =
    typeof params.pipelineId === "string"
      ? params.pipelineId
      : Array.isArray(params.pipelineId)
        ? params.pipelineId[0]
        : undefined;
  const ownerUserIdRaw =
    typeof params.ownerUserId === "string"
      ? params.ownerUserId
      : Array.isArray(params.ownerUserId)
        ? params.ownerUserId[0]
        : undefined;

  const pipelines = await getPipelinesData("/pipeline");

  if (pipelines.length === 0) {
    return (
      <CrmPageShell routePath="/pipeline" locale={locale}>
        <CrmEmptyState
          title={tCrm("crm.pipeline.emptyConfigurationTitle", locale)}
          description={tCrm("crm.pipeline.emptyConfigurationDescription", locale)}
        />
      </CrmPageShell>
    );
  }

  const selectedPipeline =
    selectedPipelineIdRaw && isGuid(selectedPipelineIdRaw)
      ? (pipelines.find((pipeline) => pipeline.id === selectedPipelineIdRaw) ?? pipelines[0])
      : pipelines[0];

  if (!selectedPipeline) {
    return (
      <CrmPageShell routePath="/pipeline" locale={locale}>
        <CrmEmptyState
          title={tCrm("crm.pipeline.noSelectableTitle", locale)}
          description={tCrm("crm.pipeline.noSelectableDescription", locale)}
        />
      </CrmPageShell>
    );
  }

  const ownerUserId = ownerUserIdRaw && isGuid(ownerUserIdRaw) ? ownerUserIdRaw : undefined;
  const board = await getPipelineBoardData(selectedPipeline.id, "/pipeline", ownerUserId);
  const pipelineDetail = await getPipelineDetailData(selectedPipeline.id, "/pipeline");
  const analytics = await getPipelineAnalyticsData(selectedPipeline.id, "/pipeline");
  const lostReasons = canReadLostReasons ? await getPipelineLostReasonsData("/pipeline") : [];

  return (
    <CrmPageShell
      routePath="/pipeline"
      locale={locale}
      description={tCrm("crm.pipeline.boardPageDescription", locale)}
    >
      <PipelineBoard board={board} locale={locale} />
      <PipelineOperationsPanel
        pipeline={pipelineDetail}
        analytics={analytics}
        lostReasons={lostReasons}
        canManagePipelines={canManagePipelines}
        canManageLostReasons={canManageLostReasons}
        locale={locale}
      />
    </CrmPageShell>
  );
}
