import {
  Badge,
  Button,
  Field,
  FieldContent,
  FieldLabel,
  Input,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
  Text,
  Textarea,
} from "@netmetric/ui";

import { CrmMetricGrid, CrmSectionCard } from "@/components/shell/crm-content-primitives";
import { CrmEmptyState } from "@/components/shell/crm-empty-state";
import {
  createPipelineFormAction,
  createPipelineLostReasonFormAction,
  deletePipelineFormAction,
  updatePipelineFormAction,
  updatePipelineLostReasonFormAction,
} from "@/features/pipeline/actions/pipeline-management-actions";
import type { OpportunityLostReasonDto, PipelineAnalyticsDto, PipelineDto } from "@/lib/crm-api";
import { tCrm } from "@/lib/i18n/crm-i18n";

type PipelineOperationsPanelProps = {
  pipeline: PipelineDto;
  analytics?: PipelineAnalyticsDto | null;
  lostReasons: OpportunityLostReasonDto[];
  canManagePipelines: boolean;
  canManageLostReasons: boolean;
  locale?: string | null | undefined;
};

export function PipelineOperationsPanel({
  pipeline,
  analytics,
  lostReasons,
  canManagePipelines,
  canManageLostReasons,
  locale,
}: Readonly<PipelineOperationsPanelProps>) {
  return (
    <section className="space-y-5">
      {analytics ? <PipelineAnalyticsCard analytics={analytics} locale={locale} /> : null}

      <div className="grid gap-4 xl:grid-cols-[minmax(0,1.15fr)_minmax(22rem,0.85fr)]">
        {canManagePipelines ? <PipelineManagementCard pipeline={pipeline} locale={locale} /> : null}
        <LostReasonsCard
          lostReasons={lostReasons}
          canManageLostReasons={canManageLostReasons}
          locale={locale}
        />
      </div>
    </section>
  );
}

function PipelineAnalyticsCard({
  analytics,
  locale,
}: Readonly<{ analytics: PipelineAnalyticsDto; locale?: string | null | undefined }>) {
  return (
    <section className="space-y-4">
      <CrmMetricGrid
        items={[
          {
            label: tCrm("crm.pipeline.analytics.healthScore", locale),
            value: analytics.healthScore,
            description: tCrm("crm.pipeline.analytics.healthScoreDescription", locale),
            tone: analytics.healthScore >= 80 ? "success" : "warning",
          },
          {
            label: tCrm("crm.pipeline.analytics.velocityDays", locale),
            value: analytics.velocityDays,
            description: tCrm("crm.pipeline.analytics.velocityDaysDescription", locale),
          },
          {
            label: tCrm("crm.pipeline.analytics.coverageRatio", locale),
            value: analytics.coverageRatio,
            description: tCrm("crm.pipeline.analytics.coverageRatioDescription", locale),
          },
          {
            label: tCrm("crm.pipeline.analytics.totalOpportunities", locale),
            value: analytics.totalOpportunities,
            description: tCrm("crm.pipeline.analytics.totalOpportunitiesDescription", locale),
          },
          {
            label: tCrm("crm.pipeline.analytics.totalValue", locale),
            value: analytics.totalValue,
            description: tCrm("crm.pipeline.analytics.totalValueDescription", locale),
          },
        ]}
      />

      <CrmSectionCard
        title={tCrm("crm.pipeline.analytics.title", locale)}
        description={tCrm("crm.pipeline.analytics.description", locale)}
      >
        {analytics.stageAging.length === 0 ? (
          <CrmEmptyState
            compact
            title={tCrm("crm.pipeline.analytics.emptyStageAgingTitle", locale)}
            description={tCrm("crm.pipeline.analytics.emptyStageAging", locale)}
          />
        ) : (
          <div className="overflow-hidden rounded-md border">
            <Table>
              <TableHeader>
                <TableRow className="bg-muted/40">
                  <TableHead>{tCrm("crm.pipeline.fields.stage", locale)}</TableHead>
                  <TableHead>{tCrm("crm.pipeline.analytics.averageDaysInStage", locale)}</TableHead>
                  <TableHead>{tCrm("crm.pipeline.analytics.staleCount", locale)}</TableHead>
                  <TableHead>{tCrm("crm.pipeline.analytics.totalOpportunities", locale)}</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {analytics.stageAging.map((stage) => (
                  <TableRow key={stage.stageId}>
                    <TableCell className="font-medium">{stage.stageName}</TableCell>
                    <TableCell>{stage.averageDaysInStage}</TableCell>
                    <TableCell>{stage.staleCount}</TableCell>
                    <TableCell>{stage.opportunityCount}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>
        )}
      </CrmSectionCard>
    </section>
  );
}

function PipelineManagementCard({
  pipeline,
  locale,
}: Readonly<{ pipeline: PipelineDto; locale?: string | null | undefined }>) {
  const stagesJson = JSON.stringify(
    pipeline.stages.map((stage) => ({
      id: stage.id,
      name: stage.name,
      description: stage.description,
      displayOrder: stage.displayOrder,
      probability: stage.probability,
      isWinStage: stage.isWinStage,
      isLostStage: stage.isLostStage,
    })),
  );

  return (
    <CrmSectionCard
      title={tCrm("crm.pipeline.management.title", locale)}
      description={tCrm("crm.pipeline.management.description", locale)}
      actions={
        pipeline.isDefault ? (
          <Badge variant="outline">{tCrm("crm.pipeline.fields.isDefault", locale)}</Badge>
        ) : null
      }
    >
      <div className="space-y-5">
        <form
          action={updatePipelineFormAction}
          className="grid gap-3 rounded-md border bg-muted/20 p-4 md:grid-cols-2"
        >
          <input type="hidden" name="pipelineId" value={pipeline.id} />
          <input type="hidden" name="rowVersion" value={pipeline.rowVersion} />
          <input type="hidden" name="stagesJson" value={stagesJson} />
          <Field>
            <FieldLabel htmlFor="pipeline-update-name">
              {tCrm("crm.pipeline.fields.name", locale)}
            </FieldLabel>
            <FieldContent>
              <Input defaultValue={pipeline.name} id="pipeline-update-name" name="name" />
            </FieldContent>
          </Field>
          <Field>
            <FieldLabel htmlFor="pipeline-update-display-order">
              {tCrm("crm.pipeline.fields.displayOrder", locale)}
            </FieldLabel>
            <FieldContent>
              <Input
                defaultValue={String(pipeline.displayOrder)}
                id="pipeline-update-display-order"
                name="displayOrder"
              />
            </FieldContent>
          </Field>
          <Field className="md:col-span-2">
            <FieldLabel htmlFor="pipeline-update-description">
              {tCrm("crm.pipeline.fields.description", locale)}
            </FieldLabel>
            <FieldContent>
              <Textarea
                defaultValue={pipeline.description ?? ""}
                id="pipeline-update-description"
                name="description"
                rows={3}
              />
            </FieldContent>
          </Field>
          <label className="flex items-center gap-2 text-sm">
            <input
              defaultChecked={pipeline.isDefault}
              name="isDefault"
              type="checkbox"
              value="true"
            />
            {tCrm("crm.pipeline.fields.isDefault", locale)}
          </label>
          <div className="flex flex-wrap gap-2 md:col-span-2">
            <Button type="submit">{tCrm("crm.pipeline.actions.updatePipeline", locale)}</Button>
            <Button form="pipeline-delete-form" type="submit" variant="destructive">
              {tCrm("crm.pipeline.actions.deletePipeline", locale)}
            </Button>
          </div>
        </form>

        <form id="pipeline-delete-form" action={deletePipelineFormAction}>
          <input type="hidden" name="pipelineId" value={pipeline.id} />
          <input type="hidden" name="confirm" value="delete-pipeline" />
        </form>

        <form
          action={createPipelineFormAction}
          className="grid gap-3 rounded-md border border-dashed bg-background p-4 md:grid-cols-2"
        >
          <div className="md:col-span-2">
            <Text className="text-sm font-medium">
              {tCrm("crm.pipeline.management.createTitle", locale)}
            </Text>
            <Text className="text-xs text-muted-foreground">
              {tCrm("crm.pipeline.management.createDescription", locale)}
            </Text>
          </div>
          <Field>
            <FieldLabel htmlFor="pipeline-create-name">
              {tCrm("crm.pipeline.fields.name", locale)}
            </FieldLabel>
            <FieldContent>
              <Input id="pipeline-create-name" name="name" />
            </FieldContent>
          </Field>
          <Field>
            <FieldLabel htmlFor="pipeline-create-display-order">
              {tCrm("crm.pipeline.fields.displayOrder", locale)}
            </FieldLabel>
            <FieldContent>
              <Input defaultValue="0" id="pipeline-create-display-order" name="displayOrder" />
            </FieldContent>
          </Field>
          <Field className="md:col-span-2">
            <FieldLabel htmlFor="pipeline-create-description">
              {tCrm("crm.pipeline.fields.description", locale)}
            </FieldLabel>
            <FieldContent>
              <Textarea id="pipeline-create-description" name="description" rows={2} />
            </FieldContent>
          </Field>
          <Field>
            <FieldLabel htmlFor="pipeline-create-stage-name">
              {tCrm("crm.pipeline.fields.stageName", locale)}
            </FieldLabel>
            <FieldContent>
              <Input defaultValue="Prospecting" id="pipeline-create-stage-name" name="stageName" />
            </FieldContent>
          </Field>
          <Field>
            <FieldLabel htmlFor="pipeline-create-stage-order">
              {tCrm("crm.pipeline.fields.stageDisplayOrder", locale)}
            </FieldLabel>
            <FieldContent>
              <Input defaultValue="0" id="pipeline-create-stage-order" name="stageDisplayOrder" />
            </FieldContent>
          </Field>
          <Field>
            <FieldLabel htmlFor="pipeline-create-stage-probability">
              {tCrm("crm.pipeline.fields.stageProbability", locale)}
            </FieldLabel>
            <FieldContent>
              <Input
                defaultValue="10"
                id="pipeline-create-stage-probability"
                inputMode="decimal"
                name="stageProbability"
              />
            </FieldContent>
          </Field>
          <label className="flex items-center gap-2 text-sm">
            <input name="isDefault" type="checkbox" value="true" />
            {tCrm("crm.pipeline.fields.isDefault", locale)}
          </label>
          <div className="md:col-span-2">
            <Button type="submit" variant="outline">
              {tCrm("crm.pipeline.actions.createPipeline", locale)}
            </Button>
          </div>
        </form>
      </div>
    </CrmSectionCard>
  );
}

function LostReasonsCard({
  lostReasons,
  canManageLostReasons,
  locale,
}: Readonly<{
  lostReasons: OpportunityLostReasonDto[];
  canManageLostReasons: boolean;
  locale?: string | null | undefined;
}>) {
  return (
    <CrmSectionCard
      title={tCrm("crm.pipeline.lostReasons.title", locale)}
      description={tCrm("crm.pipeline.lostReasons.description", locale)}
      actions={
        <Badge variant="outline">
          {tCrm("crm.pipeline.lostReasons.count", locale, { count: lostReasons.length })}
        </Badge>
      }
    >
      <div className="space-y-5">
        {lostReasons.length === 0 ? (
          <CrmEmptyState
            compact
            title={tCrm("crm.pipeline.lostReasons.emptyTitle", locale)}
            description={tCrm("crm.pipeline.lostReasons.empty", locale)}
          />
        ) : (
          <div className="overflow-hidden rounded-md border">
            <Table>
              <TableHeader>
                <TableRow className="bg-muted/40">
                  <TableHead>{tCrm("crm.pipeline.fields.name", locale)}</TableHead>
                  <TableHead>{tCrm("crm.pipeline.fields.description", locale)}</TableHead>
                  <TableHead>{tCrm("crm.pipeline.fields.isDefault", locale)}</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {lostReasons.map((reason) => (
                  <TableRow key={reason.id}>
                    <TableCell className="font-medium">{reason.name}</TableCell>
                    <TableCell>{reason.description ?? "-"}</TableCell>
                    <TableCell>
                      {reason.isDefault ? (
                        <Badge variant="outline">{tCrm("crm.common.yes", locale)}</Badge>
                      ) : (
                        "-"
                      )}
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>
        )}

        {canManageLostReasons ? (
          <>
            <form
              action={createPipelineLostReasonFormAction}
              className="space-y-3 rounded-md border border-dashed bg-background p-4"
            >
              <Text className="text-sm font-medium">
                {tCrm("crm.pipeline.lostReasons.createTitle", locale)}
              </Text>
              <Field>
                <FieldLabel htmlFor="pipeline-lost-reason-name">
                  {tCrm("crm.pipeline.fields.name", locale)}
                </FieldLabel>
                <FieldContent>
                  <Input id="pipeline-lost-reason-name" name="name" />
                </FieldContent>
              </Field>
              <Field>
                <FieldLabel htmlFor="pipeline-lost-reason-description">
                  {tCrm("crm.pipeline.fields.description", locale)}
                </FieldLabel>
                <FieldContent>
                  <Textarea id="pipeline-lost-reason-description" name="description" rows={2} />
                </FieldContent>
              </Field>
              <label className="flex items-center gap-2 text-sm">
                <input name="isDefault" type="checkbox" value="true" />
                {tCrm("crm.pipeline.fields.isDefault", locale)}
              </label>
              <Button type="submit">{tCrm("crm.pipeline.actions.createLostReason", locale)}</Button>
            </form>

            {lostReasons[0] ? (
              <form
                action={updatePipelineLostReasonFormAction}
                className="space-y-3 rounded-md border bg-muted/20 p-4"
              >
                <Text className="text-sm font-medium">
                  {tCrm("crm.pipeline.lostReasons.updateTitle", locale)}
                </Text>
                <input type="hidden" name="lostReasonId" value={lostReasons[0].id} />
                <input type="hidden" name="rowVersion" value={lostReasons[0].rowVersion ?? ""} />
                <Field>
                  <FieldLabel htmlFor="pipeline-lost-reason-update-name">
                    {tCrm("crm.pipeline.fields.name", locale)}
                  </FieldLabel>
                  <FieldContent>
                    <Input
                      defaultValue={lostReasons[0].name}
                      id="pipeline-lost-reason-update-name"
                      name="name"
                    />
                  </FieldContent>
                </Field>
                <Field>
                  <FieldLabel htmlFor="pipeline-lost-reason-update-description">
                    {tCrm("crm.pipeline.fields.description", locale)}
                  </FieldLabel>
                  <FieldContent>
                    <Textarea
                      defaultValue={lostReasons[0].description ?? ""}
                      id="pipeline-lost-reason-update-description"
                      name="description"
                      rows={2}
                    />
                  </FieldContent>
                </Field>
                <label className="flex items-center gap-2 text-sm">
                  <input
                    defaultChecked={lostReasons[0].isDefault}
                    name="isDefault"
                    type="checkbox"
                    value="true"
                  />
                  {tCrm("crm.pipeline.fields.isDefault", locale)}
                </label>
                <Button type="submit" variant="outline">
                  {tCrm("crm.pipeline.actions.updateLostReason", locale)}
                </Button>
              </form>
            ) : null}
          </>
        ) : null}
      </div>
    </CrmSectionCard>
  );
}
