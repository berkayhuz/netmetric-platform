import {
  Badge,
  Button,
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
  Field,
  FieldContent,
  FieldLabel,
  Input,
  NativeSelect,
  NativeSelectOption,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
  Text,
  Textarea,
} from "@netmetric/ui";

import { ActivityTimelinePanel } from "@/features/activities/components/activity-timeline-panel";
import { ActivityComposer } from "@/features/activities/components/activity-composer";
import {
  assignLeadOwnerFormAction,
  changeLeadStatusFormAction,
  convertLeadToCustomerFormAction,
  scheduleLeadNextContactFormAction,
  upsertLeadQualificationFormAction,
  upsertLeadScoreFormAction,
} from "@/features/leads/actions/lead-mutation-actions";
import { customerTypeOptions, leadStatusOptions } from "@/features/shared/forms/options";
import type {
  ActivityTimelineFeed,
  LeadDetailDto,
  LeadTimelineEventDto,
  LeadWorkspaceDto,
} from "@/lib/crm-api";
import {
  type CrmDateSettings,
  formatCrmDate,
  formatCrmDateTime,
} from "@/lib/date-time/crm-date-time";
import { tCrm } from "@/lib/i18n/crm-i18n";

const qualificationFrameworkOptions = [
  { value: 0, label: "None" },
  { value: 1, label: "BANT" },
  { value: 2, label: "MEDDIC" },
  { value: 3, label: "CHAMP" },
  { value: 4, label: "FAINT" },
] as const;

type LeadDetailWorkspaceProps = {
  lead: LeadDetailDto;
  workspace: LeadWorkspaceDto;
  timeline: LeadTimelineEventDto[];
  unifiedTimeline: ActivityTimelineFeed;
  canReadActivities: boolean;
  canCreateActivities: boolean;
  isUnifiedTimelineUnavailable: boolean;
  canEdit: boolean;
  dateSettings: CrmDateSettings;
  locale?: string | null | undefined;
};

export function LeadDetailWorkspace({
  lead,
  workspace,
  timeline,
  unifiedTimeline,
  canReadActivities,
  canCreateActivities,
  isUnifiedTimelineUnavailable,
  canEdit,
  dateSettings,
  locale,
}: Readonly<LeadDetailWorkspaceProps>) {
  return (
    <section className="space-y-6">
      <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
        <LeadMetricCard
          title={tCrm("crm.leads.fields.scoreHistoryCount", locale)}
          value={workspace.scoreHistoryCount}
        />
        <LeadMetricCard
          title={tCrm("crm.leads.fields.latestScore", locale)}
          value={workspace.latestScore ?? lead.totalScore}
        />
        <LeadMetricCard
          title={tCrm("crm.leads.fields.relatedOpenOpportunityCount", locale)}
          value={workspace.relatedOpenOpportunityCount}
        />
        <LeadMetricCard
          title={tCrm("crm.leads.fields.nextContactDate", locale)}
          value={formatCrmDate(lead.nextContactDate, dateSettings)}
        />
      </div>

      {canEdit ? (
        <>
          <div className="grid gap-4 xl:grid-cols-2">
            <LeadStatusActionCard lead={lead} locale={locale} />
            <LeadOwnerActionCard lead={lead} locale={locale} />
            <LeadScoreActionCard lead={lead} workspace={workspace} locale={locale} />
            <LeadQualificationActionCard lead={lead} locale={locale} />
          </div>
          <LeadConversionCard lead={lead} locale={locale} />
        </>
      ) : null}

      <LeadTimelineCard timeline={timeline} dateSettings={dateSettings} locale={locale} />
      {canReadActivities ? (
        <div className="space-y-4">
          {canCreateActivities ? (
            <ActivityComposer
              primaryRecord={{ entityType: "lead", entityId: lead.id }}
              locale={locale}
            />
          ) : null}
          <ActivityTimelinePanel
            activities={unifiedTimeline.items}
            dateSettings={dateSettings}
            locale={locale ?? "en"}
            title={tCrm("crm.activities.sections.unifiedTimelinePreviewTitle", locale)}
            description={tCrm("crm.activities.sections.unifiedTimelinePreviewDescription", locale)}
            unavailable={isUnifiedTimelineUnavailable}
          />
        </div>
      ) : null}

      <div className="grid gap-4 xl:grid-cols-2">
        <LeadScoreHistoryCard lead={lead} dateSettings={dateSettings} locale={locale} />
        <LeadOwnershipHistoryCard lead={lead} dateSettings={dateSettings} locale={locale} />
      </div>
    </section>
  );
}

function LeadMetricCard({
  title,
  value,
}: Readonly<{ title: string; value: string | number | null | undefined }>) {
  return (
    <Card>
      <CardHeader>
        <CardDescription>{title}</CardDescription>
        <CardTitle className="text-3xl">{value ?? "-"}</CardTitle>
      </CardHeader>
    </Card>
  );
}

function LeadStatusActionCard({
  lead,
  locale,
}: Readonly<{ lead: LeadDetailDto; locale?: string | null | undefined }>) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>{tCrm("crm.leads.actions.changeStatus", locale)}</CardTitle>
        <CardDescription>{tCrm("crm.leads.pages.detail.actionsTitle", locale)}</CardDescription>
      </CardHeader>
      <CardContent className="space-y-5">
        <form action={changeLeadStatusFormAction.bind(null, lead.id)} className="space-y-3">
          <Field>
            <FieldLabel htmlFor="lead-status-action">
              {tCrm("crm.leads.fields.status", locale)}
            </FieldLabel>
            <FieldContent>
              <NativeSelect
                className="w-full"
                defaultValue={enumSelectValue(lead.status, leadStatusOptions)}
                id="lead-status-action"
                name="status"
              >
                {leadStatusOptions.map((option) => (
                  <NativeSelectOption key={option.value} value={option.value}>
                    {tCrm(`crm.leads.status.${option.value}`, locale)}
                  </NativeSelectOption>
                ))}
              </NativeSelect>
            </FieldContent>
          </Field>
          <Button type="submit">{tCrm("crm.leads.actions.changeStatus", locale)}</Button>
        </form>

        <form action={scheduleLeadNextContactFormAction.bind(null, lead.id)} className="space-y-3">
          <Field>
            <FieldLabel htmlFor="lead-next-contact-action">
              {tCrm("crm.leads.fields.nextContactDate", locale)}
            </FieldLabel>
            <FieldContent>
              <Input
                defaultValue={toDateInputValue(lead.nextContactDate)}
                id="lead-next-contact-action"
                name="nextContactDate"
                type="date"
              />
            </FieldContent>
          </Field>
          <Button type="submit">{tCrm("crm.leads.actions.scheduleNextContact", locale)}</Button>
        </form>
      </CardContent>
    </Card>
  );
}

function LeadOwnerActionCard({
  lead,
  locale,
}: Readonly<{ lead: LeadDetailDto; locale?: string | null | undefined }>) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>{tCrm("crm.leads.actions.assignOwner", locale)}</CardTitle>
        <CardDescription>{tCrm("crm.leads.fields.ownerUserId", locale)}</CardDescription>
      </CardHeader>
      <CardContent>
        <form action={assignLeadOwnerFormAction.bind(null, lead.id)} className="space-y-3">
          <Field>
            <FieldLabel htmlFor="lead-owner-action">
              {tCrm("crm.leads.fields.ownerUserId", locale)}
            </FieldLabel>
            <FieldContent>
              <Input
                defaultValue={lead.ownerUserId ?? ""}
                id="lead-owner-action"
                name="ownerUserId"
                placeholder="00000000-0000-0000-0000-000000000000"
              />
            </FieldContent>
          </Field>
          <Button type="submit">{tCrm("crm.leads.actions.assignOwner", locale)}</Button>
        </form>
      </CardContent>
    </Card>
  );
}

function LeadScoreActionCard({
  lead,
  workspace,
  locale,
}: Readonly<{
  lead: LeadDetailDto;
  workspace: LeadWorkspaceDto;
  locale?: string | null | undefined;
}>) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>{tCrm("crm.leads.actions.upsertScore", locale)}</CardTitle>
        <CardDescription>{tCrm("crm.leads.fields.latestScore", locale)}</CardDescription>
      </CardHeader>
      <CardContent>
        <form action={upsertLeadScoreFormAction.bind(null, lead.id)} className="space-y-3">
          <Field>
            <FieldLabel htmlFor="lead-score-action">
              {tCrm("crm.leads.fields.score", locale)}
            </FieldLabel>
            <FieldContent>
              <Input
                defaultValue={String(workspace.latestScore ?? lead.totalScore ?? "")}
                id="lead-score-action"
                inputMode="decimal"
                name="score"
              />
            </FieldContent>
          </Field>
          <Field>
            <FieldLabel htmlFor="lead-score-reason-action">
              {tCrm("crm.leads.fields.scoreReason", locale)}
            </FieldLabel>
            <FieldContent>
              <Textarea id="lead-score-reason-action" name="scoreReason" rows={3} />
            </FieldContent>
          </Field>
          <Button type="submit">{tCrm("crm.leads.actions.upsertScore", locale)}</Button>
        </form>
      </CardContent>
    </Card>
  );
}

function LeadQualificationActionCard({
  lead,
  locale,
}: Readonly<{ lead: LeadDetailDto; locale?: string | null | undefined }>) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>{tCrm("crm.leads.actions.upsertQualification", locale)}</CardTitle>
        <CardDescription>
          {tCrm("crm.leads.pages.detail.qualificationTitle", locale)}
        </CardDescription>
      </CardHeader>
      <CardContent>
        <form action={upsertLeadQualificationFormAction.bind(null, lead.id)} className="space-y-3">
          <Field>
            <FieldLabel htmlFor="lead-qualification-framework-action">
              {tCrm("crm.leads.fields.qualificationFramework", locale)}
            </FieldLabel>
            <FieldContent>
              <NativeSelect
                className="w-full"
                defaultValue={enumSelectValue(
                  lead.qualificationFramework,
                  qualificationFrameworkOptions,
                )}
                id="lead-qualification-framework-action"
                name="frameworkType"
              >
                {qualificationFrameworkOptions.map((option) => (
                  <NativeSelectOption key={option.value} value={option.value}>
                    {tCrm(`crm.leads.qualificationFramework.${option.value}`, locale)}
                  </NativeSelectOption>
                ))}
              </NativeSelect>
            </FieldContent>
          </Field>
          <Field>
            <FieldLabel htmlFor="lead-qualification-json-action">
              {tCrm("crm.leads.fields.qualificationDataJson", locale)}
            </FieldLabel>
            <FieldContent>
              <Textarea
                defaultValue={lead.qualificationData ?? "{}"}
                id="lead-qualification-json-action"
                name="qualificationDataJson"
                rows={5}
              />
            </FieldContent>
          </Field>
          <Button type="submit">{tCrm("crm.leads.actions.upsertQualification", locale)}</Button>
        </form>
      </CardContent>
    </Card>
  );
}

function LeadConversionCard({
  lead,
  locale,
}: Readonly<{ lead: LeadDetailDto; locale?: string | null | undefined }>) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>{tCrm("crm.leads.actions.convert", locale)}</CardTitle>
        <CardDescription>{tCrm("crm.leads.pages.detail.conversionTitle", locale)}</CardDescription>
      </CardHeader>
      <CardContent>
        <form
          action={convertLeadToCustomerFormAction.bind(null, lead.id)}
          className="grid gap-4 md:grid-cols-2"
        >
          <Field>
            <FieldLabel htmlFor="lead-convert-customer-type">
              {tCrm("crm.leads.fields.customerType", locale)}
            </FieldLabel>
            <FieldContent>
              <NativeSelect
                className="w-full"
                defaultValue="0"
                id="lead-convert-customer-type"
                name="customerType"
              >
                {customerTypeOptions.map((option) => (
                  <NativeSelectOption key={option.value} value={option.value}>
                    {customerTypeLabel(option.value, locale)}
                  </NativeSelectOption>
                ))}
              </NativeSelect>
            </FieldContent>
          </Field>
          <Field>
            <FieldLabel htmlFor="lead-convert-company-id">
              {tCrm("crm.leads.fields.companyId", locale)}
            </FieldLabel>
            <FieldContent>
              <Input
                defaultValue={lead.companyId ?? ""}
                id="lead-convert-company-id"
                name="companyId"
                placeholder="00000000-0000-0000-0000-000000000000"
              />
            </FieldContent>
          </Field>
          <Field>
            <FieldLabel htmlFor="lead-convert-opportunity-name">
              {tCrm("crm.leads.fields.opportunityName", locale)}
            </FieldLabel>
            <FieldContent>
              <Input id="lead-convert-opportunity-name" name="opportunityName" />
            </FieldContent>
          </Field>
          <Field>
            <FieldLabel htmlFor="lead-convert-estimated-amount">
              {tCrm("crm.leads.fields.estimatedBudget", locale)}
            </FieldLabel>
            <FieldContent>
              <Input
                id="lead-convert-estimated-amount"
                inputMode="decimal"
                name="estimatedAmount"
              />
            </FieldContent>
          </Field>
          <label className="flex items-center gap-2 text-sm">
            <input name="markCustomerAsVip" type="checkbox" value="true" />
            {tCrm("crm.leads.fields.markCustomerAsVip", locale)}
          </label>
          <label className="flex items-center gap-2 text-sm">
            <input name="createOpportunity" type="checkbox" value="true" />
            {tCrm("crm.leads.fields.createOpportunity", locale)}
          </label>
          <div className="md:col-span-2">
            <Button type="submit">{tCrm("crm.leads.actions.convert", locale)}</Button>
          </div>
        </form>
      </CardContent>
    </Card>
  );
}

function LeadTimelineCard({
  timeline,
  dateSettings,
  locale,
}: Readonly<{
  timeline: LeadTimelineEventDto[];
  dateSettings: CrmDateSettings;
  locale?: string | null | undefined;
}>) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>{tCrm("crm.leads.pages.detail.timelineTitle", locale)}</CardTitle>
        <CardDescription>{tCrm("crm.leads.pages.detail.workspaceTitle", locale)}</CardDescription>
      </CardHeader>
      <CardContent>
        {timeline.length === 0 ? (
          <Text className="text-sm text-muted-foreground">
            {tCrm("crm.leads.states.noTimeline", locale)}
          </Text>
        ) : (
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>{tCrm("crm.leads.fields.occurredAt", locale)}</TableHead>
                <TableHead>{tCrm("crm.leads.fields.eventType", locale)}</TableHead>
                <TableHead>{tCrm("crm.leads.fields.eventTitle", locale)}</TableHead>
                <TableHead>{tCrm("crm.leads.fields.description", locale)}</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {timeline.map((event) => (
                <TableRow key={`${event.occurredAt}-${event.eventType}-${event.title}`}>
                  <TableCell>{formatCrmDateTime(event.occurredAt, dateSettings)}</TableCell>
                  <TableCell>
                    <Badge variant="outline">{event.eventType}</Badge>
                  </TableCell>
                  <TableCell>{event.title}</TableCell>
                  <TableCell className="max-w-md whitespace-normal break-words">
                    {event.description ?? "-"}
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        )}
      </CardContent>
    </Card>
  );
}

function LeadScoreHistoryCard({
  lead,
  dateSettings,
  locale,
}: Readonly<{
  lead: LeadDetailDto;
  dateSettings: CrmDateSettings;
  locale?: string | null | undefined;
}>) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>{tCrm("crm.leads.pages.detail.scoreHistoryTitle", locale)}</CardTitle>
        <CardDescription>{tCrm("crm.leads.fields.score", locale)}</CardDescription>
      </CardHeader>
      <CardContent>
        {lead.scores.length === 0 ? (
          <Text className="text-sm text-muted-foreground">
            {tCrm("crm.leads.states.noScoreHistory", locale)}
          </Text>
        ) : (
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>{tCrm("crm.leads.fields.score", locale)}</TableHead>
                <TableHead>{tCrm("crm.leads.fields.scoreReason", locale)}</TableHead>
                <TableHead>{tCrm("crm.leads.fields.occurredAt", locale)}</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {lead.scores.map((score) => (
                <TableRow key={score.id}>
                  <TableCell>{score.score}</TableCell>
                  <TableCell className="max-w-sm whitespace-normal break-words">
                    {score.scoreReason ?? "-"}
                  </TableCell>
                  <TableCell>{formatCrmDateTime(score.scoredAt, dateSettings)}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        )}
      </CardContent>
    </Card>
  );
}

function LeadOwnershipHistoryCard({
  lead,
  dateSettings,
  locale,
}: Readonly<{
  lead: LeadDetailDto;
  dateSettings: CrmDateSettings;
  locale?: string | null | undefined;
}>) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>{tCrm("crm.leads.pages.detail.ownershipHistoryTitle", locale)}</CardTitle>
        <CardDescription>{tCrm("crm.leads.fields.ownerUserId", locale)}</CardDescription>
      </CardHeader>
      <CardContent>
        {lead.ownershipHistories.length === 0 ? (
          <Text className="text-sm text-muted-foreground">
            {tCrm("crm.leads.states.noOwnershipHistory", locale)}
          </Text>
        ) : (
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>{tCrm("crm.leads.fields.assignedAt", locale)}</TableHead>
                <TableHead>{tCrm("crm.leads.fields.previousOwnerId", locale)}</TableHead>
                <TableHead>{tCrm("crm.leads.fields.newOwnerId", locale)}</TableHead>
                <TableHead>{tCrm("crm.leads.fields.assignedByUserId", locale)}</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {lead.ownershipHistories.map((history) => (
                <TableRow key={history.id}>
                  <TableCell>{formatCrmDateTime(history.assignedAt, dateSettings)}</TableCell>
                  <TableCell>{history.previousOwnerId ?? "-"}</TableCell>
                  <TableCell>{history.newOwnerId ?? "-"}</TableCell>
                  <TableCell>{history.assignedByUserId ?? "-"}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        )}
      </CardContent>
    </Card>
  );
}

function enumSelectValue<TOption extends { value: number; label: string }>(
  value: string | number,
  options: readonly TOption[],
): string {
  const raw = String(value);
  const option = options.find(
    (candidate) =>
      String(candidate.value) === raw || candidate.label.toLowerCase() === raw.toLowerCase(),
  );

  return String(option?.value ?? options[0]?.value ?? "");
}

function toDateInputValue(value: string | null | undefined): string {
  return value ? value.slice(0, 10) : "";
}

function customerTypeLabel(value: number, locale?: string | null | undefined): string {
  return value === 1
    ? tCrm("crm.customers.options.customerType.corporate", locale)
    : tCrm("crm.customers.options.customerType.individual", locale);
}
