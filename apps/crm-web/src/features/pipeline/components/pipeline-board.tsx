import { Badge, Text, cn } from "@netmetric/ui";

import { moveOpportunityStageAction } from "@/features/pipeline/actions/pipeline-stage-actions";
import { PipelineStageMoveForm } from "@/features/pipeline/components/pipeline-stage-move-form";
import { opportunityStageOptions } from "@/features/shared/forms/options";
import type { PipelineBoardDto } from "@/lib/crm-api";
import { CrmEmptyState } from "@/components/shell/crm-empty-state";
import { CrmSectionCard } from "@/components/shell/crm-content-primitives";
import { isGuid } from "@/features/shared/data/guid";
import { tCrm } from "@/lib/i18n/crm-i18n";

function normalizeLabel(value: string): string {
  return value.trim().toLowerCase().replace(/\s+/g, "");
}

function mapColumnNameToStageValue(columnName: string): number | undefined {
  const normalized = normalizeLabel(columnName);
  const matched = opportunityStageOptions.find(
    (option) => normalizeLabel(option.label) === normalized,
  );
  return matched?.value;
}

type PipelineBoardProps = Readonly<{
  board: PipelineBoardDto;
  locale?: string | null;
}>;

export function PipelineBoard({ board, locale }: PipelineBoardProps) {
  const totalOpportunities = board.columns.reduce(
    (total, column) => total + column.opportunityCount,
    0,
  );

  return (
    <CrmSectionCard
      title={board.pipelineName}
      description={tCrm("crm.pipeline.board.description", locale)}
      actions={
        <Badge variant="outline">
          {tCrm("crm.pipeline.board.totalOpportunities", locale, {
            count: totalOpportunities,
          })}
        </Badge>
      }
      className="overflow-hidden"
    >
      <div
        className="-mx-6 -mb-6 overflow-x-auto px-6 pb-6"
        aria-label={tCrm("crm.pipeline.board.ariaLabel", locale)}
        role="list"
      >
        <div className="flex min-w-full gap-4">
          {board.columns.map((column) => (
            <PipelineColumn key={column.stageId} column={column} locale={locale} />
          ))}
        </div>
      </div>
    </CrmSectionCard>
  );
}

function PipelineColumn({
  column,
  locale,
}: Readonly<{
  column: PipelineBoardDto["columns"][number];
  locale?: string | null | undefined;
}>) {
  const defaultStageValue = mapColumnNameToStageValue(column.name);

  return (
    <section
      className="flex min-h-[24rem] w-[19rem] shrink-0 flex-col rounded-md border bg-muted/20"
      aria-labelledby={`pipeline-stage-${column.stageId}`}
      role="listitem"
    >
      <header className="border-b bg-background/70 p-3">
        <div className="flex items-start justify-between gap-3">
          <div className="min-w-0">
            <h3 id={`pipeline-stage-${column.stageId}`} className="truncate text-sm font-semibold">
              {column.name}
            </h3>
            <Text className="mt-1 text-xs text-muted-foreground">
              {tCrm("crm.pipeline.board.columnSummary", locale, {
                count: column.opportunityCount,
                total: column.totalValue,
              })}
            </Text>
          </div>
          <Badge variant="outline">
            {tCrm("crm.pipeline.board.probability", locale, {
              probability: column.probability,
            })}
          </Badge>
        </div>
      </header>

      <div className="flex-1 space-y-3 p-3">
        {column.opportunities.length === 0 ? (
          <CrmEmptyState
            compact
            title={tCrm("crm.pipeline.board.emptyStage", locale)}
            description={tCrm("crm.pipeline.board.emptyStageDescription", locale)}
          />
        ) : (
          column.opportunities.map((opportunity) => (
            <article
              key={opportunity.id}
              className={cn(
                "space-y-3 rounded-md border bg-background p-3 shadow-sm transition",
                "hover:border-primary/40 hover:shadow-md focus-within:ring-2 focus-within:ring-ring/40",
              )}
            >
              <div className="space-y-1">
                <div className="flex items-start justify-between gap-3">
                  <div className="min-w-0">
                    <h4 className="truncate text-sm font-medium">{opportunity.name}</h4>
                    <Text className="text-xs text-muted-foreground">
                      {opportunity.opportunityCode}
                    </Text>
                  </div>
                  {opportunity.isStale ? (
                    <Badge variant="secondary">{tCrm("crm.pipeline.board.stale", locale)}</Badge>
                  ) : null}
                </div>
                <Text className="text-xs text-muted-foreground">
                  {opportunity.customerName ?? tCrm("crm.pipeline.board.noCustomer", locale)}
                </Text>
              </div>

              <div className="grid grid-cols-2 gap-2 text-xs">
                <PipelineFact
                  label={tCrm("crm.pipeline.board.amountLabel", locale)}
                  value={String(opportunity.amount)}
                />
                <PipelineFact
                  label={tCrm("crm.pipeline.board.closeDate", locale)}
                  value={opportunity.estimatedCloseDate ?? "-"}
                />
              </div>

              {opportunity.warningCount > 0 ? (
                <Badge variant="outline">
                  {tCrm("crm.pipeline.board.warnings", locale, {
                    count: opportunity.warningCount,
                  })}
                </Badge>
              ) : null}

              {defaultStageValue !== undefined && isGuid(opportunity.id) ? (
                <PipelineStageMoveForm
                  opportunityId={opportunity.id}
                  currentStage={defaultStageValue}
                  action={moveOpportunityStageAction.bind(null, opportunity.id)}
                />
              ) : (
                <Text className="text-xs text-muted-foreground">
                  {tCrm("crm.pipeline.board.stageMovementUnavailable", locale)}
                </Text>
              )}
            </article>
          ))
        )}
      </div>
    </section>
  );
}

function PipelineFact({ label, value }: Readonly<{ label: string; value: string }>) {
  return (
    <div className="rounded-md border bg-muted/30 px-2 py-1.5">
      <div className="text-[11px] font-medium uppercase text-muted-foreground">{label}</div>
      <div className="truncate text-sm font-semibold">{value}</div>
    </div>
  );
}
