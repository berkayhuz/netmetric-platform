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
  addOpportunityContactFormAction,
  addOpportunityProductFormAction,
  assignOpportunityOwnerFormAction,
  changeOpportunityStageFormAction,
  createOpportunityQuoteFormAction,
  markOpportunityLostFormAction,
  markOpportunityWonFormAction,
} from "@/features/opportunities/actions/opportunity-lifecycle-actions";
import { opportunityStageOptions } from "@/features/shared/forms/options";
import type {
  ActivityTimelineFeed,
  OpportunityDetailDto,
  OpportunityLostReasonDto,
  OpportunityQuoteDetailDto,
  OpportunityStageHistoryDto,
  OpportunityTimelineEventDto,
  OpportunityWorkspaceDto,
} from "@/lib/crm-api";
import {
  type CrmDateSettings,
  formatCrmDate,
  formatCrmDateTime,
} from "@/lib/date-time/crm-date-time";
import { tCrm, tCrmWithFallback } from "@/lib/i18n/crm-i18n";

type OpportunityDetailWorkspaceProps = {
  opportunity: OpportunityDetailDto;
  workspace: OpportunityWorkspaceDto;
  timeline: OpportunityTimelineEventDto[];
  stageHistory: OpportunityStageHistoryDto[];
  lostReasons: OpportunityLostReasonDto[];
  quotes: OpportunityQuoteDetailDto[];
  canReadActivities: boolean;
  canCreateActivities: boolean;
  unifiedTimeline: ActivityTimelineFeed;
  isUnifiedTimelineUnavailable: boolean;
  canEdit: boolean;
  canManageQuotes: boolean;
  dateSettings: CrmDateSettings;
  locale?: string | null | undefined;
};

export function OpportunityDetailWorkspace({
  opportunity,
  workspace,
  timeline,
  stageHistory,
  lostReasons,
  quotes,
  canReadActivities,
  canCreateActivities,
  unifiedTimeline,
  isUnifiedTimelineUnavailable,
  canEdit,
  canManageQuotes,
  dateSettings,
  locale,
}: Readonly<OpportunityDetailWorkspaceProps>) {
  return (
    <section className="space-y-6">
      <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
        <MetricCard
          title={tCrm("crm.opportunities.fields.totalQuoteAmount", locale)}
          value={formatNumber(workspace.totalQuoteAmount, locale)}
        />
        <MetricCard
          title={tCrm("crm.opportunities.fields.quoteCount", locale)}
          value={workspace.quoteCount}
        />
        <MetricCard
          title={tCrm("crm.opportunities.fields.activityCount", locale)}
          value={workspace.activityCount}
        />
        <MetricCard
          title={tCrm("crm.opportunities.fields.stageChangeCount", locale)}
          value={workspace.stageChangeCount}
        />
      </div>

      <OpportunityProfileCard
        opportunity={opportunity}
        dateSettings={dateSettings}
        locale={locale}
      />

      {canEdit ? (
        <>
          <div className="grid gap-4 xl:grid-cols-2">
            <OwnerAndStageCard opportunity={opportunity} locale={locale} />
            <OutcomeCard opportunity={opportunity} lostReasons={lostReasons} locale={locale} />
          </div>
          <div className="grid gap-4 xl:grid-cols-2">
            <RelationshipCard opportunity={opportunity} locale={locale} />
            {canManageQuotes ? <CreateQuoteCard opportunity={opportunity} locale={locale} /> : null}
          </div>
        </>
      ) : null}

      <QuotesCard quotes={quotes} dateSettings={dateSettings} locale={locale} />
      {canReadActivities ? (
        <div className="space-y-4">
          {canCreateActivities ? (
            <ActivityComposer
              primaryRecord={{ entityType: "opportunity", entityId: opportunity.id }}
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
        <TimelineCard timeline={timeline} dateSettings={dateSettings} locale={locale} />
        <StageHistoryCard stageHistory={stageHistory} dateSettings={dateSettings} locale={locale} />
      </div>
    </section>
  );
}

function MetricCard({
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

function OpportunityProfileCard({
  opportunity,
  dateSettings,
  locale,
}: Readonly<{
  opportunity: OpportunityDetailDto;
  dateSettings: CrmDateSettings;
  locale?: string | null | undefined;
}>) {
  const fields = [
    {
      label: tCrm("crm.opportunities.fields.opportunityCode", locale),
      value: opportunity.opportunityCode,
    },
    { label: tCrm("crm.opportunities.fields.name", locale), value: opportunity.name },
    {
      label: tCrm("crm.opportunities.fields.stage", locale),
      value: tCrmWithFallback(
        `crm.opportunities.stage.${opportunity.stage}`,
        String(opportunity.stage),
        locale,
      ),
    },
    {
      label: tCrm("crm.opportunities.fields.status", locale),
      value: tCrmWithFallback(
        `crm.opportunities.status.${opportunity.status}`,
        String(opportunity.status),
        locale,
      ),
    },
    {
      label: tCrm("crm.opportunities.fields.priority", locale),
      value: tCrmWithFallback(
        `crm.common.priority.${opportunity.priority}`,
        String(opportunity.priority),
        locale,
      ),
    },
    {
      label: tCrm("crm.opportunities.fields.estimatedAmount", locale),
      value: formatNumber(opportunity.estimatedAmount, locale),
    },
    {
      label: tCrm("crm.opportunities.fields.expectedRevenue", locale),
      value: formatNumber(opportunity.expectedRevenue, locale),
    },
    {
      label: tCrm("crm.opportunities.fields.probability", locale),
      value: `${opportunity.probability}%`,
    },
    {
      label: tCrm("crm.opportunities.fields.estimatedCloseDate", locale),
      value: formatCrmDate(opportunity.estimatedCloseDate, dateSettings),
    },
    { label: tCrm("crm.opportunities.fields.pipelineId", locale), value: opportunity.pipelineId },
    {
      label: tCrm("crm.opportunities.fields.pipelineStageId", locale),
      value: opportunity.pipelineStageId,
    },
    { label: tCrm("crm.opportunities.fields.leadId", locale), value: opportunity.leadId },
    { label: tCrm("crm.opportunities.fields.customerId", locale), value: opportunity.customerId },
    { label: tCrm("crm.opportunities.fields.ownerUserId", locale), value: opportunity.ownerUserId },
    {
      label: tCrm("crm.opportunities.fields.lostReasonId", locale),
      value: opportunity.lostReasonId,
    },
    { label: tCrm("crm.opportunities.fields.lostNote", locale), value: opportunity.lostNote },
  ];

  return (
    <Card>
      <CardHeader>
        <CardTitle>{tCrm("crm.opportunities.pages.detail.profileTitle", locale)}</CardTitle>
        <CardDescription>
          {tCrm("crm.opportunities.pages.detail.workspaceTitle", locale)}
        </CardDescription>
      </CardHeader>
      <CardContent>
        <dl className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
          {fields.map((field) => (
            <div key={field.label} className="space-y-1">
              <dt className="text-xs font-medium uppercase text-muted-foreground">{field.label}</dt>
              <dd className="break-words text-sm">{field.value ?? "-"}</dd>
            </div>
          ))}
        </dl>
      </CardContent>
    </Card>
  );
}

function OwnerAndStageCard({
  opportunity,
  locale,
}: Readonly<{ opportunity: OpportunityDetailDto; locale?: string | null | undefined }>) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>{tCrm("crm.opportunities.pages.detail.actionsTitle", locale)}</CardTitle>
        <CardDescription>
          {tCrm("crm.opportunities.pages.detail.actionsDescription", locale)}
        </CardDescription>
      </CardHeader>
      <CardContent className="space-y-5">
        <form
          action={assignOpportunityOwnerFormAction.bind(null, opportunity.id)}
          className="space-y-3"
        >
          <Field>
            <FieldLabel htmlFor="opportunity-owner-action">
              {tCrm("crm.opportunities.fields.ownerUserId", locale)}
            </FieldLabel>
            <FieldContent>
              <Input
                defaultValue={opportunity.ownerUserId ?? ""}
                id="opportunity-owner-action"
                name="ownerUserId"
                placeholder="00000000-0000-0000-0000-000000000000"
              />
            </FieldContent>
          </Field>
          <Button type="submit">{tCrm("crm.opportunities.actions.assignOwner", locale)}</Button>
        </form>

        <form
          action={changeOpportunityStageFormAction.bind(null, opportunity.id)}
          className="space-y-3"
        >
          <input type="hidden" name="rowVersion" value={opportunity.rowVersion} />
          <Field>
            <FieldLabel htmlFor="opportunity-stage-action">
              {tCrm("crm.opportunities.fields.stage", locale)}
            </FieldLabel>
            <FieldContent>
              <NativeSelect
                className="w-full"
                defaultValue={enumValue(opportunity.stage)}
                id="opportunity-stage-action"
                name="newStage"
              >
                {opportunityStageOptions.map((option) => (
                  <NativeSelectOption key={option.value} value={option.value}>
                    {tCrm(`crm.opportunities.stage.${option.value}`, locale)}
                  </NativeSelectOption>
                ))}
              </NativeSelect>
            </FieldContent>
          </Field>
          <Field>
            <FieldLabel htmlFor="opportunity-stage-note">
              {tCrm("crm.opportunities.fields.note", locale)}
            </FieldLabel>
            <FieldContent>
              <Textarea id="opportunity-stage-note" name="note" rows={3} />
            </FieldContent>
          </Field>
          <Button type="submit">{tCrm("crm.opportunities.actions.changeStage", locale)}</Button>
        </form>
      </CardContent>
    </Card>
  );
}

function OutcomeCard({
  opportunity,
  lostReasons,
  locale,
}: Readonly<{
  opportunity: OpportunityDetailDto;
  lostReasons: OpportunityLostReasonDto[];
  locale?: string | null | undefined;
}>) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>{tCrm("crm.opportunities.pages.detail.outcomeTitle", locale)}</CardTitle>
        <CardDescription>
          {tCrm("crm.opportunities.pages.detail.outcomeDescription", locale)}
        </CardDescription>
      </CardHeader>
      <CardContent className="space-y-5">
        <form
          action={markOpportunityWonFormAction.bind(null, opportunity.id)}
          className="space-y-3"
        >
          <input type="hidden" name="rowVersion" value={opportunity.rowVersion} />
          <Field>
            <FieldLabel htmlFor="opportunity-won-deal-name">
              {tCrm("crm.opportunities.fields.dealName", locale)}
            </FieldLabel>
            <FieldContent>
              <Input
                defaultValue={opportunity.name}
                id="opportunity-won-deal-name"
                name="dealName"
              />
            </FieldContent>
          </Field>
          <Field>
            <FieldLabel htmlFor="opportunity-won-closed-date">
              {tCrm("crm.opportunities.fields.closedDate", locale)}
            </FieldLabel>
            <FieldContent>
              <Input
                defaultValue={formatDateInput(opportunity.estimatedCloseDate)}
                id="opportunity-won-closed-date"
                name="closedDate"
                type="date"
              />
            </FieldContent>
          </Field>
          <Button type="submit">{tCrm("crm.opportunities.actions.markWon", locale)}</Button>
        </form>

        <form
          action={markOpportunityLostFormAction.bind(null, opportunity.id)}
          className="space-y-3"
        >
          <input type="hidden" name="rowVersion" value={opportunity.rowVersion} />
          <Field>
            <FieldLabel htmlFor="opportunity-lost-reason">
              {tCrm("crm.opportunities.fields.lostReasonId", locale)}
            </FieldLabel>
            <FieldContent>
              <NativeSelect
                className="w-full"
                defaultValue={opportunity.lostReasonId ?? ""}
                id="opportunity-lost-reason"
                name="lostReasonId"
              >
                <NativeSelectOption value="">
                  {tCrm("crm.opportunities.lifecycle.selectLostReason", locale)}
                </NativeSelectOption>
                {lostReasons.map((reason) => (
                  <NativeSelectOption key={reason.id} value={reason.id}>
                    {reason.name}
                  </NativeSelectOption>
                ))}
              </NativeSelect>
            </FieldContent>
          </Field>
          <Field>
            <FieldLabel htmlFor="opportunity-lost-note">
              {tCrm("crm.opportunities.fields.lostNote", locale)}
            </FieldLabel>
            <FieldContent>
              <Textarea
                defaultValue={opportunity.lostNote ?? ""}
                id="opportunity-lost-note"
                name="lostNote"
                rows={3}
              />
            </FieldContent>
          </Field>
          <Button type="submit" variant="outline">
            {tCrm("crm.opportunities.actions.markLost", locale)}
          </Button>
        </form>
      </CardContent>
    </Card>
  );
}

function RelationshipCard({
  opportunity,
  locale,
}: Readonly<{ opportunity: OpportunityDetailDto; locale?: string | null | undefined }>) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>{tCrm("crm.opportunities.pages.detail.relationshipsTitle", locale)}</CardTitle>
        <CardDescription>
          {tCrm("crm.opportunities.pages.detail.relationshipsDescription", locale)}
        </CardDescription>
      </CardHeader>
      <CardContent className="space-y-5">
        <form
          action={addOpportunityContactFormAction.bind(null, opportunity.id)}
          className="space-y-3"
        >
          <Field>
            <FieldLabel htmlFor="opportunity-contact-id">
              {tCrm("crm.opportunities.fields.contactId", locale)}
            </FieldLabel>
            <FieldContent>
              <Input
                id="opportunity-contact-id"
                name="contactId"
                placeholder="00000000-0000-0000-0000-000000000000"
              />
            </FieldContent>
          </Field>
          <label className="flex items-center gap-2 text-sm">
            <input name="isDecisionMaker" type="checkbox" value="true" />
            {tCrm("crm.opportunities.fields.isDecisionMaker", locale)}
          </label>
          <label className="flex items-center gap-2 text-sm">
            <input name="isPrimary" type="checkbox" value="true" />
            {tCrm("crm.opportunities.fields.isPrimary", locale)}
          </label>
          <Button type="submit">{tCrm("crm.opportunities.actions.addContact", locale)}</Button>
        </form>

        <form
          action={addOpportunityProductFormAction.bind(null, opportunity.id)}
          className="grid gap-3 md:grid-cols-2"
        >
          <Field className="md:col-span-2">
            <FieldLabel htmlFor="opportunity-product-id">
              {tCrm("crm.opportunities.fields.productId", locale)}
            </FieldLabel>
            <FieldContent>
              <Input
                id="opportunity-product-id"
                name="productId"
                placeholder="00000000-0000-0000-0000-000000000000"
              />
            </FieldContent>
          </Field>
          <Field>
            <FieldLabel htmlFor="opportunity-product-quantity">
              {tCrm("crm.opportunities.fields.quantity", locale)}
            </FieldLabel>
            <FieldContent>
              <Input defaultValue="1" id="opportunity-product-quantity" name="quantity" />
            </FieldContent>
          </Field>
          <Field>
            <FieldLabel htmlFor="opportunity-product-price">
              {tCrm("crm.opportunities.fields.unitPrice", locale)}
            </FieldLabel>
            <FieldContent>
              <Input id="opportunity-product-price" inputMode="decimal" name="unitPrice" />
            </FieldContent>
          </Field>
          <Field>
            <FieldLabel htmlFor="opportunity-product-discount">
              {tCrm("crm.opportunities.fields.discountRate", locale)}
            </FieldLabel>
            <FieldContent>
              <Input
                defaultValue="0"
                id="opportunity-product-discount"
                inputMode="decimal"
                name="discountRate"
              />
            </FieldContent>
          </Field>
          <Field>
            <FieldLabel htmlFor="opportunity-product-vat">
              {tCrm("crm.opportunities.fields.vatRate", locale)}
            </FieldLabel>
            <FieldContent>
              <Input
                defaultValue="0"
                id="opportunity-product-vat"
                inputMode="decimal"
                name="vatRate"
              />
            </FieldContent>
          </Field>
          <div className="md:col-span-2">
            <Button type="submit">{tCrm("crm.opportunities.actions.addProduct", locale)}</Button>
          </div>
        </form>
      </CardContent>
    </Card>
  );
}

function CreateQuoteCard({
  opportunity,
  locale,
}: Readonly<{
  opportunity: OpportunityDetailDto;
  locale?: string | null | undefined;
}>) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>{tCrm("crm.opportunities.actions.createQuote", locale)}</CardTitle>
        <CardDescription>
          {tCrm("crm.opportunities.pages.detail.quoteDescription", locale)}
        </CardDescription>
      </CardHeader>
      <CardContent>
        <form
          action={createOpportunityQuoteFormAction.bind(null, opportunity.id)}
          className="grid gap-3 md:grid-cols-2"
        >
          <Field>
            <FieldLabel htmlFor="opportunity-quote-number">
              {tCrm("crm.opportunities.fields.quoteNumber", locale)}
            </FieldLabel>
            <FieldContent>
              <Input id="opportunity-quote-number" name="quoteNumber" />
            </FieldContent>
          </Field>
          <Field>
            <FieldLabel htmlFor="opportunity-quote-date">
              {tCrm("crm.opportunities.fields.quoteDate", locale)}
            </FieldLabel>
            <FieldContent>
              <Input
                defaultValue={formatDateInput(undefined)}
                id="opportunity-quote-date"
                name="quoteDate"
                type="date"
              />
            </FieldContent>
          </Field>
          <Field>
            <FieldLabel htmlFor="opportunity-quote-valid-until">
              {tCrm("crm.opportunities.fields.validUntil", locale)}
            </FieldLabel>
            <FieldContent>
              <Input id="opportunity-quote-valid-until" name="validUntil" type="date" />
            </FieldContent>
          </Field>
          <Field>
            <FieldLabel htmlFor="opportunity-quote-currency">
              {tCrm("crm.opportunities.fields.currencyCode", locale)}
            </FieldLabel>
            <FieldContent>
              <Input defaultValue="TRY" id="opportunity-quote-currency" name="currencyCode" />
            </FieldContent>
          </Field>
          <Field>
            <FieldLabel htmlFor="opportunity-quote-rate">
              {tCrm("crm.opportunities.fields.exchangeRate", locale)}
            </FieldLabel>
            <FieldContent>
              <Input
                defaultValue="1"
                id="opportunity-quote-rate"
                inputMode="decimal"
                name="exchangeRate"
              />
            </FieldContent>
          </Field>
          <Field>
            <FieldLabel htmlFor="opportunity-quote-owner">
              {tCrm("crm.opportunities.fields.ownerUserId", locale)}
            </FieldLabel>
            <FieldContent>
              <Input
                defaultValue={opportunity.ownerUserId ?? ""}
                id="opportunity-quote-owner"
                name="ownerUserId"
              />
            </FieldContent>
          </Field>
          <Field className="md:col-span-2">
            <FieldLabel htmlFor="opportunity-quote-product-id">
              {tCrm("crm.opportunities.fields.productId", locale)}
            </FieldLabel>
            <FieldContent>
              <Input
                id="opportunity-quote-product-id"
                name="productId"
                placeholder="00000000-0000-0000-0000-000000000000"
              />
            </FieldContent>
          </Field>
          <Field>
            <FieldLabel htmlFor="opportunity-quote-quantity">
              {tCrm("crm.opportunities.fields.quantity", locale)}
            </FieldLabel>
            <FieldContent>
              <Input defaultValue="1" id="opportunity-quote-quantity" name="quantity" />
            </FieldContent>
          </Field>
          <Field>
            <FieldLabel htmlFor="opportunity-quote-unit-price">
              {tCrm("crm.opportunities.fields.unitPrice", locale)}
            </FieldLabel>
            <FieldContent>
              <Input id="opportunity-quote-unit-price" inputMode="decimal" name="unitPrice" />
            </FieldContent>
          </Field>
          <Field>
            <FieldLabel htmlFor="opportunity-quote-discount">
              {tCrm("crm.opportunities.fields.discountRate", locale)}
            </FieldLabel>
            <FieldContent>
              <Input
                defaultValue="0"
                id="opportunity-quote-discount"
                inputMode="decimal"
                name="discountRate"
              />
            </FieldContent>
          </Field>
          <Field>
            <FieldLabel htmlFor="opportunity-quote-tax">
              {tCrm("crm.opportunities.fields.taxRate", locale)}
            </FieldLabel>
            <FieldContent>
              <Input
                defaultValue="0"
                id="opportunity-quote-tax"
                inputMode="decimal"
                name="taxRate"
              />
            </FieldContent>
          </Field>
          <Field className="md:col-span-2">
            <FieldLabel htmlFor="opportunity-quote-description">
              {tCrm("crm.opportunities.fields.description", locale)}
            </FieldLabel>
            <FieldContent>
              <Textarea id="opportunity-quote-description" name="description" rows={2} />
            </FieldContent>
          </Field>
          <Field className="md:col-span-2">
            <FieldLabel htmlFor="opportunity-quote-terms">
              {tCrm("crm.opportunities.fields.termsAndConditions", locale)}
            </FieldLabel>
            <FieldContent>
              <Textarea id="opportunity-quote-terms" name="termsAndConditions" rows={3} />
            </FieldContent>
          </Field>
          <div className="md:col-span-2">
            <Button type="submit">{tCrm("crm.opportunities.actions.createQuote", locale)}</Button>
          </div>
        </form>
      </CardContent>
    </Card>
  );
}

function QuotesCard({
  quotes,
  dateSettings,
  locale,
}: Readonly<{
  quotes: OpportunityQuoteDetailDto[];
  dateSettings: CrmDateSettings;
  locale?: string | null | undefined;
}>) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>{tCrm("crm.opportunities.pages.detail.quotesTitle", locale)}</CardTitle>
        <CardDescription>
          {tCrm("crm.opportunities.pages.detail.quotesDescription", locale)}
        </CardDescription>
      </CardHeader>
      <CardContent>
        {quotes.length === 0 ? (
          <Text className="text-sm text-muted-foreground">
            {tCrm("crm.opportunities.states.noQuotes", locale)}
          </Text>
        ) : (
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>{tCrm("crm.opportunities.fields.quoteNumber", locale)}</TableHead>
                <TableHead>{tCrm("crm.opportunities.fields.quoteDate", locale)}</TableHead>
                <TableHead>{tCrm("crm.opportunities.fields.grandTotal", locale)}</TableHead>
                <TableHead>{tCrm("crm.opportunities.fields.currencyCode", locale)}</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {quotes.map((quote) => (
                <TableRow key={quote.id}>
                  <TableCell>{quote.quoteNumber}</TableCell>
                  <TableCell>{formatCrmDate(quote.quoteDate, dateSettings)}</TableCell>
                  <TableCell>{formatNumber(quote.grandTotal, locale)}</TableCell>
                  <TableCell>{quote.currencyCode}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        )}
      </CardContent>
    </Card>
  );
}

function TimelineCard({
  timeline,
  dateSettings,
  locale,
}: Readonly<{
  timeline: OpportunityTimelineEventDto[];
  dateSettings: CrmDateSettings;
  locale?: string | null | undefined;
}>) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>{tCrm("crm.opportunities.pages.detail.timelineTitle", locale)}</CardTitle>
        <CardDescription>
          {tCrm("crm.opportunities.pages.detail.workspaceTitle", locale)}
        </CardDescription>
      </CardHeader>
      <CardContent>
        {timeline.length === 0 ? (
          <Text className="text-sm text-muted-foreground">
            {tCrm("crm.opportunities.states.noTimeline", locale)}
          </Text>
        ) : (
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>{tCrm("crm.opportunities.fields.occurredAt", locale)}</TableHead>
                <TableHead>{tCrm("crm.opportunities.fields.eventType", locale)}</TableHead>
                <TableHead>{tCrm("crm.opportunities.fields.title", locale)}</TableHead>
                <TableHead>{tCrm("crm.opportunities.fields.description", locale)}</TableHead>
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

function StageHistoryCard({
  stageHistory,
  dateSettings,
  locale,
}: Readonly<{
  stageHistory: OpportunityStageHistoryDto[];
  dateSettings: CrmDateSettings;
  locale?: string | null | undefined;
}>) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>{tCrm("crm.opportunities.pages.detail.stageHistoryTitle", locale)}</CardTitle>
        <CardDescription>{tCrm("crm.opportunities.fields.stage", locale)}</CardDescription>
      </CardHeader>
      <CardContent>
        {stageHistory.length === 0 ? (
          <Text className="text-sm text-muted-foreground">
            {tCrm("crm.opportunities.states.noStageHistory", locale)}
          </Text>
        ) : (
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>{tCrm("crm.opportunities.fields.changedAt", locale)}</TableHead>
                <TableHead>{tCrm("crm.opportunities.fields.oldStage", locale)}</TableHead>
                <TableHead>{tCrm("crm.opportunities.fields.newStage", locale)}</TableHead>
                <TableHead>{tCrm("crm.opportunities.fields.note", locale)}</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {stageHistory.map((history) => (
                <TableRow key={history.id}>
                  <TableCell>{formatCrmDateTime(history.changedAt, dateSettings)}</TableCell>
                  <TableCell>{String(history.oldStage)}</TableCell>
                  <TableCell>{String(history.newStage)}</TableCell>
                  <TableCell className="max-w-md whitespace-normal break-words">
                    {history.note ?? "-"}
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

function enumValue(value: string | number): string {
  const raw = String(value);
  const match = opportunityStageOptions.find(
    (option) => String(option.value) === raw || option.label.toLowerCase() === raw.toLowerCase(),
  );
  return String(match?.value ?? opportunityStageOptions[0]?.value ?? "");
}

function formatDateInput(value: string | null | undefined): string {
  if (value) {
    return value.slice(0, 10);
  }

  return new Date().toISOString().slice(0, 10);
}

function formatNumber(value: number | null | undefined, locale?: string | null | undefined) {
  if (value === null || value === undefined) {
    return null;
  }

  return new Intl.NumberFormat(locale ?? undefined, {
    maximumFractionDigits: 2,
  }).format(value);
}
