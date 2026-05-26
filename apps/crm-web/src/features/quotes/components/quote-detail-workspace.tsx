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
  acceptQuoteAction,
  approveQuoteAction,
  createQuoteRevisionAction,
  declineQuoteAction,
  expireQuoteAction,
  markQuoteSentAction,
  rejectQuoteAction,
  submitQuoteAction,
} from "@/features/quotes/actions/quote-lifecycle-actions";
import {
  createProposalTemplateFormAction,
  deleteProposalTemplateFormAction,
  updateProposalTemplateFormAction,
  upsertGuidedSellingPlaybookFormAction,
  upsertProductBundleFormAction,
  upsertProductRuleFormAction,
} from "@/features/quotes/actions/quote-cpq-actions";
import { QuoteGuidedSellingPanel } from "@/features/quotes/components/quote-guided-selling-panel";
import { QuoteLifecycleActionPanel } from "@/features/quotes/forms/quote-lifecycle-panels";
import type {
  ActivityTimelineFeed,
  CpqValidationResultDto,
  CpqWorkspaceDto,
  ProductBundleDto,
  ProductRuleDto,
  ProposalTemplateDto,
  QuoteDetailDto,
  QuoteTimelineEventDto,
  QuoteWorkspaceDto,
  GuidedSellingPlaybookDto,
} from "@/lib/crm-api";
import {
  type CrmDateSettings,
  formatCrmDate,
  formatCrmDateTime,
} from "@/lib/date-time/crm-date-time";
import { tCrm, tCrmWithFallback } from "@/lib/i18n/crm-i18n";

type QuoteDetailWorkspaceProps = {
  quoteId: string;
  workspace: QuoteWorkspaceDto;
  timeline: QuoteTimelineEventDto[];
  validation: CpqValidationResultDto;
  cpqWorkspace?: CpqWorkspaceDto | null;
  proposalTemplates?: ProposalTemplateDto[] | null;
  canManageQuote: boolean;
  canManageProposals: boolean;
  canReadActivities: boolean;
  canCreateActivities: boolean;
  unifiedTimeline: ActivityTimelineFeed;
  isUnifiedTimelineUnavailable: boolean;
  dateSettings: CrmDateSettings;
  locale?: string | null | undefined;
};

export function QuoteDetailWorkspace({
  quoteId,
  workspace,
  timeline,
  validation,
  cpqWorkspace,
  proposalTemplates,
  canManageQuote,
  canManageProposals,
  canReadActivities,
  canCreateActivities,
  unifiedTimeline,
  isUnifiedTimelineUnavailable,
  dateSettings,
  locale,
}: Readonly<QuoteDetailWorkspaceProps>) {
  const quote = workspace.quote;

  return (
    <section className="space-y-6">
      <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
        <QuoteMetricCard
          title={tCrm("crm.quotes.fields.grandTotal", locale)}
          value={formatNumber(quote.grandTotal, locale)}
        />
        <QuoteMetricCard
          title={tCrm("crm.quotes.fields.status", locale)}
          value={formatQuoteStatus(quote.status, locale)}
        />
        <QuoteMetricCard
          title={tCrm("crm.quotes.fields.validUntil", locale)}
          value={formatCrmDate(quote.validUntil, dateSettings)}
        />
        <QuoteMetricCard
          title={tCrm("crm.quotes.cpq.validation.title", locale)}
          value={
            validation.isValid
              ? tCrm("crm.quotes.cpq.validation.valid", locale)
              : tCrm("crm.quotes.cpq.validation.invalid", locale)
          }
        />
      </div>

      <QuoteProfileCard quote={quote} dateSettings={dateSettings} locale={locale} />

      <QuoteValidationCard validation={validation} locale={locale} />

      {canManageQuote ? (
        <QuoteLifecycleCards
          quoteId={quoteId}
          quote={quote}
          workspace={workspace}
          locale={locale}
        />
      ) : null}

      <div className="grid gap-4 xl:grid-cols-2">
        <QuoteLineItemsCard quote={quote} locale={locale} />
        <QuoteStatusHistoryCard quote={quote} dateSettings={dateSettings} locale={locale} />
      </div>

      <QuoteTimelineCard timeline={timeline} dateSettings={dateSettings} locale={locale} />
      {canReadActivities ? (
        <div className="space-y-4">
          {canCreateActivities ? (
            <ActivityComposer
              primaryRecord={{ entityType: "quote", entityId: quoteId }}
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

      {cpqWorkspace ? (
        <QuoteCpqWorkspaceCard
          quoteId={quoteId}
          cpqWorkspace={cpqWorkspace}
          canManageQuote={canManageQuote}
          locale={locale}
        />
      ) : null}

      {proposalTemplates ? (
        <ProposalTemplatesCard
          quoteId={quoteId}
          templates={proposalTemplates}
          canManageProposals={canManageProposals}
          locale={locale}
        />
      ) : null}
    </section>
  );
}

function QuoteMetricCard({
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

function QuoteProfileCard({
  quote,
  dateSettings,
  locale,
}: Readonly<{
  quote: QuoteDetailDto;
  dateSettings: CrmDateSettings;
  locale?: string | null | undefined;
}>) {
  const fields = [
    { label: tCrm("crm.quotes.fields.quoteNumber", locale), value: quote.quoteNumber },
    { label: tCrm("crm.quotes.fields.proposalTitle", locale), value: quote.proposalTitle },
    {
      label: tCrm("crm.quotes.fields.status", locale),
      value: formatQuoteStatus(quote.status, locale),
    },
    {
      label: tCrm("crm.quotes.fields.quoteDate", locale),
      value: formatCrmDate(quote.quoteDate, dateSettings),
    },
    {
      label: tCrm("crm.quotes.fields.validUntil", locale),
      value: formatCrmDate(quote.validUntil, dateSettings),
    },
    { label: tCrm("crm.quotes.fields.currency", locale), value: quote.currencyCode },
    {
      label: tCrm("crm.quotes.fields.exchangeRate", locale),
      value: formatNumber(quote.exchangeRate, locale),
    },
    { label: tCrm("crm.quotes.fields.revision", locale), value: quote.revisionNumber },
    { label: tCrm("crm.quotes.fields.opportunityId", locale), value: quote.opportunityId },
    { label: tCrm("crm.quotes.fields.customerId", locale), value: quote.customerId },
    { label: tCrm("crm.quotes.fields.ownerUserId", locale), value: quote.ownerUserId },
    {
      label: tCrm("crm.quotes.fields.proposalTemplateId", locale),
      value: quote.proposalTemplateId,
    },
    {
      label: tCrm("crm.quotes.fields.subTotal", locale),
      value: formatNumber(quote.subTotal, locale),
    },
    {
      label: tCrm("crm.quotes.fields.discountTotal", locale),
      value: formatNumber(quote.discountTotal, locale),
    },
    {
      label: tCrm("crm.quotes.fields.taxTotal", locale),
      value: formatNumber(quote.taxTotal, locale),
    },
    {
      label: tCrm("crm.quotes.fields.grandTotal", locale),
      value: formatNumber(quote.grandTotal, locale),
    },
  ];

  return (
    <Card>
      <CardHeader>
        <CardTitle>{tCrm("crm.quotes.detail.profileTitle", locale)}</CardTitle>
        <CardDescription>{tCrm("crm.quotes.detail.description", locale)}</CardDescription>
      </CardHeader>
      <CardContent className="space-y-5">
        <dl className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
          {fields.map((field) => (
            <div key={field.label} className="space-y-1">
              <dt className="text-xs font-medium uppercase text-muted-foreground">{field.label}</dt>
              <dd className="break-words text-sm">{field.value ?? "-"}</dd>
            </div>
          ))}
        </dl>
        <div className="grid gap-4 lg:grid-cols-2">
          <LongTextBlock
            title={tCrm("crm.quotes.fields.proposalSummary", locale)}
            value={quote.proposalSummary}
          />
          <LongTextBlock
            title={tCrm("crm.quotes.fields.termsAndConditions", locale)}
            value={quote.termsAndConditions}
          />
        </div>
      </CardContent>
    </Card>
  );
}

function LongTextBlock({
  title,
  value,
}: Readonly<{ title: string; value?: string | null | undefined }>) {
  return (
    <div className="space-y-2">
      <h3 className="text-sm font-medium">{title}</h3>
      <Text className="whitespace-pre-wrap text-sm text-muted-foreground">{value ?? "-"}</Text>
    </div>
  );
}

function QuoteValidationCard({
  validation,
  locale,
}: Readonly<{
  validation: CpqValidationResultDto;
  locale?: string | null | undefined;
}>) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>{tCrm("crm.quotes.cpq.validation.title", locale)}</CardTitle>
        <CardDescription>
          {validation.isValid
            ? tCrm("crm.quotes.cpq.validation.validDescription", locale)
            : tCrm("crm.quotes.cpq.validation.invalidDescription", locale)}
        </CardDescription>
      </CardHeader>
      <CardContent>
        {validation.violations.length > 0 ? (
          <ul className="grid gap-2">
            {validation.violations.map((violation) => (
              <li key={violation} className="rounded border border-border p-3 text-sm">
                {violation}
              </li>
            ))}
          </ul>
        ) : (
          <Text className="text-sm text-muted-foreground">
            {tCrm("crm.quotes.cpq.validation.noViolations", locale)}
          </Text>
        )}
      </CardContent>
    </Card>
  );
}

function QuoteLifecycleCards({
  quoteId,
  quote,
  workspace,
  locale,
}: Readonly<{
  quoteId: string;
  quote: QuoteDetailDto;
  workspace: QuoteWorkspaceDto;
  locale?: string | null | undefined;
}>) {
  return (
    <div className="grid gap-4 lg:grid-cols-2">
      {workspace.canSubmit ? (
        <QuoteLifecycleActionPanel
          title={tCrm("crm.quotes.lifecycle.submit.title", locale)}
          description={tCrm("crm.quotes.lifecycle.submit.description", locale)}
          confirmValue="submit-quote"
          action={submitQuoteAction.bind(null, quoteId)}
          rowVersion={quote.rowVersion}
        />
      ) : null}
      {workspace.canApprove ? (
        <QuoteLifecycleActionPanel
          title={tCrm("crm.quotes.lifecycle.approve.title", locale)}
          description={tCrm("crm.quotes.lifecycle.approve.description", locale)}
          confirmValue="approve-quote"
          action={approveQuoteAction.bind(null, quoteId)}
          rowVersion={quote.rowVersion}
        />
      ) : null}
      {workspace.canReject ? (
        <QuoteLifecycleActionPanel
          title={tCrm("crm.quotes.lifecycle.reject.title", locale)}
          description={tCrm("crm.quotes.lifecycle.reject.description", locale)}
          confirmValue="reject-quote"
          action={rejectQuoteAction.bind(null, quoteId)}
          rowVersion={quote.rowVersion}
          showReason
        />
      ) : null}
      {workspace.canSend ? (
        <QuoteLifecycleActionPanel
          title={tCrm("crm.quotes.lifecycle.send.title", locale)}
          description={tCrm("crm.quotes.lifecycle.send.description", locale)}
          confirmValue="send-quote"
          action={markQuoteSentAction.bind(null, quoteId)}
          rowVersion={quote.rowVersion}
          showDate
        />
      ) : null}
      {workspace.canAccept ? (
        <QuoteLifecycleActionPanel
          title={tCrm("crm.quotes.lifecycle.accept.title", locale)}
          description={tCrm("crm.quotes.lifecycle.accept.description", locale)}
          confirmValue="accept-quote"
          action={acceptQuoteAction.bind(null, quoteId)}
          rowVersion={quote.rowVersion}
          showDate
        />
      ) : null}
      {workspace.canDecline ? (
        <QuoteLifecycleActionPanel
          title={tCrm("crm.quotes.lifecycle.decline.title", locale)}
          description={tCrm("crm.quotes.lifecycle.decline.description", locale)}
          confirmValue="decline-quote"
          action={declineQuoteAction.bind(null, quoteId)}
          rowVersion={quote.rowVersion}
          showReason
        />
      ) : null}
      {workspace.canExpire ? (
        <QuoteLifecycleActionPanel
          title={tCrm("crm.quotes.lifecycle.expire.title", locale)}
          description={tCrm("crm.quotes.lifecycle.expire.description", locale)}
          confirmValue="expire-quote"
          action={expireQuoteAction.bind(null, quoteId)}
          rowVersion={quote.rowVersion}
          showDate
        />
      ) : null}
      {workspace.canEdit ? (
        <QuoteLifecycleActionPanel
          title={tCrm("crm.quotes.lifecycle.revise.title", locale)}
          description={tCrm("crm.quotes.lifecycle.revise.description", locale)}
          confirmValue="revise-quote"
          action={createQuoteRevisionAction.bind(null, quoteId)}
          rowVersion={quote.rowVersion}
        />
      ) : null}
    </div>
  );
}

function QuoteLineItemsCard({
  quote,
  locale,
}: Readonly<{ quote: QuoteDetailDto; locale?: string | null | undefined }>) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>{tCrm("crm.quotes.lineItems.title", locale)}</CardTitle>
        <CardDescription>
          {tCrm("crm.quotes.lineItems.summaryTitle", locale)}: {quote.items.length}
        </CardDescription>
      </CardHeader>
      <CardContent className="overflow-x-auto">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>{tCrm("crm.quotes.fields.productId", locale)}</TableHead>
              <TableHead>{tCrm("crm.quotes.fields.quantity", locale)}</TableHead>
              <TableHead>{tCrm("crm.quotes.fields.unitPrice", locale)}</TableHead>
              <TableHead>{tCrm("crm.quotes.fields.discountRate", locale)}</TableHead>
              <TableHead>{tCrm("crm.quotes.fields.taxRate", locale)}</TableHead>
              <TableHead>{tCrm("crm.quotes.fields.grandTotal", locale)}</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {quote.items.map((item) => (
              <TableRow key={item.id}>
                <TableCell className="font-mono text-xs">{item.productId}</TableCell>
                <TableCell>{item.quantity}</TableCell>
                <TableCell>{formatNumber(item.unitPrice, locale)}</TableCell>
                <TableCell>{formatPercent(item.discountRate, locale)}</TableCell>
                <TableCell>{formatPercent(item.taxRate, locale)}</TableCell>
                <TableCell>{formatNumber(item.lineTotal, locale)}</TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </CardContent>
    </Card>
  );
}

function QuoteStatusHistoryCard({
  quote,
  dateSettings,
  locale,
}: Readonly<{
  quote: QuoteDetailDto;
  dateSettings: CrmDateSettings;
  locale?: string | null | undefined;
}>) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>{tCrm("crm.quotes.history.title", locale)}</CardTitle>
        <CardDescription>{tCrm("crm.quotes.history.description", locale)}</CardDescription>
      </CardHeader>
      <CardContent className="overflow-x-auto">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>{tCrm("crm.quotes.fields.status", locale)}</TableHead>
              <TableHead>{tCrm("crm.quotes.history.changedAt", locale)}</TableHead>
              <TableHead>{tCrm("crm.quotes.history.changedBy", locale)}</TableHead>
              <TableHead>{tCrm("crm.quotes.fields.note", locale)}</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {quote.history.map((item) => (
              <TableRow key={item.id}>
                <TableCell>{formatQuoteStatus(item.newStatus, locale)}</TableCell>
                <TableCell>{formatCrmDateTime(item.changedAt, dateSettings)}</TableCell>
                <TableCell className="font-mono text-xs">{item.changedByUserId ?? "-"}</TableCell>
                <TableCell>{item.note ?? "-"}</TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </CardContent>
    </Card>
  );
}

function QuoteTimelineCard({
  timeline,
  dateSettings,
  locale,
}: Readonly<{
  timeline: QuoteTimelineEventDto[];
  dateSettings: CrmDateSettings;
  locale?: string | null | undefined;
}>) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>{tCrm("crm.quotes.timeline.title", locale)}</CardTitle>
        <CardDescription>{tCrm("crm.quotes.timeline.description", locale)}</CardDescription>
      </CardHeader>
      <CardContent className="space-y-3">
        {timeline.length > 0 ? (
          timeline.map((item) => (
            <div
              key={`${item.occurredAt}-${item.title}`}
              className="rounded border border-border p-4"
            >
              <div className="flex flex-wrap items-center justify-between gap-3">
                <div>
                  <p className="text-sm font-medium">{item.title}</p>
                  <p className="text-xs text-muted-foreground">
                    {formatCrmDateTime(item.occurredAt, dateSettings)}
                  </p>
                </div>
                <Badge variant="outline">{item.eventType}</Badge>
              </div>
              {item.description ? (
                <p className="mt-3 text-sm text-muted-foreground">{item.description}</p>
              ) : null}
            </div>
          ))
        ) : (
          <Text className="text-sm text-muted-foreground">
            {tCrm("crm.quotes.timeline.empty", locale)}
          </Text>
        )}
      </CardContent>
    </Card>
  );
}

function QuoteCpqWorkspaceCard({
  quoteId,
  cpqWorkspace,
  canManageQuote,
  locale,
}: Readonly<{
  quoteId: string;
  cpqWorkspace: CpqWorkspaceDto;
  canManageQuote: boolean;
  locale?: string | null | undefined;
}>) {
  const firstPlaybook = cpqWorkspace.guidedSellingPlaybooks[0] ?? null;
  const firstBundle = cpqWorkspace.productBundles[0] ?? null;
  const firstRule = cpqWorkspace.productRules[0] ?? null;

  return (
    <Card>
      <CardHeader>
        <CardTitle>{tCrm("crm.quotes.cpq.workspace.title", locale)}</CardTitle>
        <CardDescription>{tCrm("crm.quotes.cpq.workspace.description", locale)}</CardDescription>
      </CardHeader>
      <CardContent className="space-y-6">
        <div className="grid gap-4 md:grid-cols-3">
          <CpqStatBox
            title={tCrm("crm.quotes.cpq.playbooks.title", locale)}
            value={cpqWorkspace.guidedSellingPlaybooks.length}
          />
          <CpqStatBox
            title={tCrm("crm.quotes.cpq.bundles.title", locale)}
            value={cpqWorkspace.productBundles.length}
          />
          <CpqStatBox
            title={tCrm("crm.quotes.cpq.rules.title", locale)}
            value={cpqWorkspace.productRules.length}
          />
        </div>

        {canManageQuote ? (
          <div className="space-y-4">
            <SectionTitle
              title={tCrm("crm.quotes.cpq.guidedSelling.title", locale)}
              description={tCrm("crm.quotes.cpq.guidedSelling.description", locale)}
            />
            <QuoteGuidedSellingPanel quoteId={quoteId} />
          </div>
        ) : null}

        <div className="grid gap-4 xl:grid-cols-3">
          <CpqCollectionTable
            title={tCrm("crm.quotes.cpq.playbooks.title", locale)}
            items={cpqWorkspace.guidedSellingPlaybooks.map((item) => ({
              id: item.id,
              name: item.name,
              meta: item.segment ?? item.industry ?? "-",
              active: item.isActive,
            }))}
            locale={locale}
          />
          <CpqCollectionTable
            title={tCrm("crm.quotes.cpq.bundles.title", locale)}
            items={cpqWorkspace.productBundles.map((item) => ({
              id: item.id,
              name: item.name,
              meta: item.code,
              active: item.isActive,
            }))}
            locale={locale}
          />
          <CpqCollectionTable
            title={tCrm("crm.quotes.cpq.rules.title", locale)}
            items={cpqWorkspace.productRules.map((item) => ({
              id: item.id,
              name: item.name,
              meta: item.severity,
              active: item.isActive,
            }))}
            locale={locale}
          />
        </div>

        {canManageQuote ? (
          <div className="grid gap-4 xl:grid-cols-3">
            <PlaybookForm quoteId={quoteId} playbook={firstPlaybook} locale={locale} />
            <ProductBundleForm quoteId={quoteId} bundle={firstBundle} locale={locale} />
            <ProductRuleForm quoteId={quoteId} rule={firstRule} locale={locale} />
          </div>
        ) : null}
      </CardContent>
    </Card>
  );
}

function CpqStatBox({ title, value }: Readonly<{ title: string; value: string | number }>) {
  return (
    <div className="rounded border border-border p-4">
      <p className="text-sm text-muted-foreground">{title}</p>
      <p className="mt-2 text-2xl font-semibold">{value}</p>
    </div>
  );
}

function CpqCollectionTable({
  title,
  items,
  locale,
}: Readonly<{
  title: string;
  items: Array<{ id: string; name: string; meta: string; active: boolean }>;
  locale?: string | null | undefined;
}>) {
  return (
    <div className="overflow-x-auto rounded border border-border">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>{title}</TableHead>
            <TableHead>{tCrm("crm.common.status", locale)}</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {items.map((item) => (
            <TableRow key={item.id}>
              <TableCell>
                <div className="space-y-1">
                  <p className="text-sm font-medium">{item.name}</p>
                  <p className="font-mono text-xs text-muted-foreground">{item.meta}</p>
                </div>
              </TableCell>
              <TableCell>
                {item.active
                  ? tCrm("crm.common.active", locale)
                  : tCrm("crm.common.inactive", locale)}
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  );
}

function ProposalTemplatesCard({
  quoteId,
  templates,
  canManageProposals,
  locale,
}: Readonly<{
  quoteId: string;
  templates: ProposalTemplateDto[];
  canManageProposals: boolean;
  locale?: string | null | undefined;
}>) {
  const firstTemplate = templates[0] ?? null;

  return (
    <Card>
      <CardHeader>
        <CardTitle>{tCrm("crm.quotes.proposals.title", locale)}</CardTitle>
        <CardDescription>{tCrm("crm.quotes.proposals.description", locale)}</CardDescription>
      </CardHeader>
      <CardContent className="space-y-6">
        <div className="overflow-x-auto">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>{tCrm("crm.quotes.proposals.fields.name", locale)}</TableHead>
                <TableHead>{tCrm("crm.quotes.proposals.fields.default", locale)}</TableHead>
                <TableHead>{tCrm("crm.common.status", locale)}</TableHead>
                <TableHead>{tCrm("crm.quotes.proposals.fields.notes", locale)}</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {templates.map((template) => (
                <TableRow key={template.id}>
                  <TableCell>{template.name}</TableCell>
                  <TableCell>{formatBoolean(template.isDefault, locale)}</TableCell>
                  <TableCell>
                    {template.isActive
                      ? tCrm("crm.common.active", locale)
                      : tCrm("crm.common.inactive", locale)}
                  </TableCell>
                  <TableCell>{template.notes ?? "-"}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>

        {canManageProposals ? (
          <div className="grid gap-4 xl:grid-cols-2">
            <ProposalTemplateForm quoteId={quoteId} template={null} locale={locale} />
            {firstTemplate ? (
              <div className="space-y-4">
                <ProposalTemplateForm quoteId={quoteId} template={firstTemplate} locale={locale} />
                <form
                  action={deleteProposalTemplateFormAction.bind(null, quoteId, firstTemplate.id)}
                >
                  <input type="hidden" name="confirm" value="delete-template" />
                  <Button type="submit" variant="destructive">
                    {tCrm("crm.quotes.proposals.deleteFirst", locale)}
                  </Button>
                </form>
              </div>
            ) : null}
          </div>
        ) : null}
      </CardContent>
    </Card>
  );
}

function ProposalTemplateForm({
  quoteId,
  template,
  locale,
}: Readonly<{
  quoteId: string;
  template: ProposalTemplateDto | null;
  locale?: string | null | undefined;
}>) {
  const action = template
    ? updateProposalTemplateFormAction.bind(null, quoteId, template.id)
    : createProposalTemplateFormAction.bind(null, quoteId);

  return (
    <form action={action} className="grid gap-4">
      <SectionTitle
        title={
          template
            ? tCrm("crm.quotes.proposals.updateTitle", locale)
            : tCrm("crm.quotes.proposals.createTitle", locale)
        }
        description={tCrm("crm.quotes.proposals.formDescription", locale)}
      />
      <Field>
        <FieldLabel htmlFor={template ? "proposal-template-name-edit" : "proposal-template-name"}>
          {tCrm("crm.quotes.proposals.fields.name", locale)}
        </FieldLabel>
        <FieldContent>
          <Input
            defaultValue={template?.name ?? ""}
            id={template ? "proposal-template-name-edit" : "proposal-template-name"}
            name="name"
            required
          />
        </FieldContent>
      </Field>
      <Field>
        <FieldLabel
          htmlFor={template ? "proposal-template-subject-edit" : "proposal-template-subject"}
        >
          {tCrm("crm.quotes.proposals.fields.subject", locale)}
        </FieldLabel>
        <FieldContent>
          <Input
            defaultValue={template?.subjectTemplate ?? ""}
            id={template ? "proposal-template-subject-edit" : "proposal-template-subject"}
            name="subjectTemplate"
          />
        </FieldContent>
      </Field>
      <Field>
        <FieldLabel htmlFor={template ? "proposal-template-body-edit" : "proposal-template-body"}>
          {tCrm("crm.quotes.proposals.fields.body", locale)}
        </FieldLabel>
        <FieldContent>
          <Textarea
            defaultValue={template?.bodyTemplate ?? ""}
            id={template ? "proposal-template-body-edit" : "proposal-template-body"}
            name="bodyTemplate"
            required
            rows={4}
          />
        </FieldContent>
      </Field>
      <Field>
        <FieldLabel htmlFor={template ? "proposal-template-notes-edit" : "proposal-template-notes"}>
          {tCrm("crm.quotes.proposals.fields.notes", locale)}
        </FieldLabel>
        <FieldContent>
          <Textarea
            defaultValue={template?.notes ?? ""}
            id={template ? "proposal-template-notes-edit" : "proposal-template-notes"}
            name="notes"
            rows={2}
          />
        </FieldContent>
      </Field>
      <BinaryInputs
        defaultActive={template?.isActive ?? true}
        defaultPrimary={template?.isDefault ?? false}
        primaryName="isDefault"
        primaryLabel={tCrm("crm.quotes.proposals.fields.default", locale)}
        activeLabel={tCrm("crm.common.active", locale)}
      />
      <Button type="submit">
        {template
          ? tCrm("crm.quotes.proposals.updateAction", locale)
          : tCrm("crm.quotes.proposals.createAction", locale)}
      </Button>
    </form>
  );
}

function PlaybookForm({
  quoteId,
  playbook,
  locale,
}: Readonly<{
  quoteId: string;
  playbook: GuidedSellingPlaybookDto | null;
  locale?: string | null | undefined;
}>) {
  return (
    <form
      action={upsertGuidedSellingPlaybookFormAction.bind(null, quoteId, playbook?.id ?? null)}
      className="space-y-4"
    >
      <input type="hidden" name="rowVersion" value={playbook?.rowVersion ?? ""} />
      <SectionTitle
        title={
          playbook
            ? tCrm("crm.quotes.cpq.playbooks.updateTitle", locale)
            : tCrm("crm.quotes.cpq.playbooks.createTitle", locale)
        }
        description={tCrm("crm.quotes.cpq.playbooks.description", locale)}
      />
      <TextInput
        defaultValue={playbook?.name}
        label={tCrm("crm.quotes.cpq.fields.name", locale)}
        name="name"
        required
      />
      <TextInput
        defaultValue={playbook?.segment}
        label={tCrm("crm.quotes.cpq.fields.segment", locale)}
        name="segment"
      />
      <TextInput
        defaultValue={playbook?.industry}
        label={tCrm("crm.quotes.cpq.fields.industry", locale)}
        name="industry"
      />
      <TextInput
        defaultValue={playbook?.minimumBudget?.toString()}
        inputMode="decimal"
        label={tCrm("crm.quotes.cpq.fields.minimumBudget", locale)}
        name="minimumBudget"
      />
      <TextInput
        defaultValue={playbook?.maximumBudget?.toString()}
        inputMode="decimal"
        label={tCrm("crm.quotes.cpq.fields.maximumBudget", locale)}
        name="maximumBudget"
      />
      <TextareaInput
        defaultValue={playbook?.requiredCapabilities}
        label={tCrm("crm.quotes.cpq.fields.requiredCapabilities", locale)}
        name="requiredCapabilities"
      />
      <TextareaInput
        defaultValue={playbook?.recommendedBundleCodes.join(", ")}
        label={tCrm("crm.quotes.cpq.fields.recommendedBundleCodes", locale)}
        name="recommendedBundleCodes"
        required
      />
      <TextareaInput
        defaultValue={playbook?.qualificationJson}
        label={tCrm("crm.quotes.cpq.fields.qualificationJson", locale)}
        name="qualificationJson"
      />
      <Button type="submit">{tCrm("crm.common.save", locale)}</Button>
    </form>
  );
}

function ProductBundleForm({
  quoteId,
  bundle,
  locale,
}: Readonly<{
  quoteId: string;
  bundle: ProductBundleDto | null;
  locale?: string | null | undefined;
}>) {
  return (
    <form
      action={upsertProductBundleFormAction.bind(null, quoteId, bundle?.id ?? null)}
      className="space-y-4"
    >
      <input type="hidden" name="rowVersion" value={bundle?.rowVersion ?? ""} />
      {bundle ? (
        <input type="hidden" name="itemsJson" value={JSON.stringify(bundle.items)} />
      ) : null}
      <SectionTitle
        title={
          bundle
            ? tCrm("crm.quotes.cpq.bundles.updateTitle", locale)
            : tCrm("crm.quotes.cpq.bundles.createTitle", locale)
        }
        description={tCrm("crm.quotes.cpq.bundles.description", locale)}
      />
      <TextInput
        defaultValue={bundle?.code}
        label={tCrm("crm.quotes.cpq.fields.code", locale)}
        name="code"
        required
      />
      <TextInput
        defaultValue={bundle?.name}
        label={tCrm("crm.quotes.cpq.fields.name", locale)}
        name="name"
        required
      />
      <TextareaInput
        defaultValue={bundle?.description}
        label={tCrm("crm.quotes.cpq.fields.description", locale)}
        name="description"
      />
      <TextInput
        defaultValue={bundle?.segment}
        label={tCrm("crm.quotes.cpq.fields.segment", locale)}
        name="segment"
      />
      <TextInput
        defaultValue={bundle?.industry}
        label={tCrm("crm.quotes.cpq.fields.industry", locale)}
        name="industry"
      />
      <TextInput
        defaultValue={bundle?.discountRate?.toString() ?? "0"}
        inputMode="decimal"
        label={tCrm("crm.quotes.cpq.fields.discountRate", locale)}
        name="discountRate"
      />
      <TextInput
        defaultValue={bundle?.minimumBudget?.toString()}
        inputMode="decimal"
        label={tCrm("crm.quotes.cpq.fields.minimumBudget", locale)}
        name="minimumBudget"
      />
      {!bundle ? (
        <div className="grid gap-4 md:grid-cols-2">
          <TextInput
            label={tCrm("crm.quotes.cpq.fields.productId", locale)}
            name="productId"
            required
          />
          <TextInput
            defaultValue="1"
            inputMode="numeric"
            label={tCrm("crm.quotes.fields.quantity", locale)}
            name="quantity"
            required
          />
        </div>
      ) : null}
      <Button type="submit">{tCrm("crm.common.save", locale)}</Button>
    </form>
  );
}

function ProductRuleForm({
  quoteId,
  rule,
  locale,
}: Readonly<{
  quoteId: string;
  rule: ProductRuleDto | null;
  locale?: string | null | undefined;
}>) {
  return (
    <form
      action={upsertProductRuleFormAction.bind(null, quoteId, rule?.id ?? null)}
      className="space-y-4"
    >
      <input type="hidden" name="rowVersion" value={rule?.rowVersion ?? ""} />
      <SectionTitle
        title={
          rule
            ? tCrm("crm.quotes.cpq.rules.updateTitle", locale)
            : tCrm("crm.quotes.cpq.rules.createTitle", locale)
        }
        description={tCrm("crm.quotes.cpq.rules.description", locale)}
      />
      <TextInput
        defaultValue={rule?.name}
        label={tCrm("crm.quotes.cpq.fields.name", locale)}
        name="name"
        required
      />
      <TextInput
        defaultValue={rule?.ruleType}
        label={tCrm("crm.quotes.cpq.fields.ruleType", locale)}
        name="ruleType"
        required
      />
      <TextInput
        defaultValue={rule?.severity ?? "Warning"}
        label={tCrm("crm.quotes.cpq.fields.severity", locale)}
        name="severity"
        required
      />
      <TextareaInput
        defaultValue={rule?.message}
        label={tCrm("crm.quotes.cpq.fields.message", locale)}
        name="message"
        required
      />
      <TextInput
        defaultValue={rule?.triggerProductId}
        label={tCrm("crm.quotes.cpq.fields.triggerProductId", locale)}
        name="triggerProductId"
      />
      <TextInput
        defaultValue={rule?.targetProductId}
        label={tCrm("crm.quotes.cpq.fields.targetProductId", locale)}
        name="targetProductId"
      />
      <TextInput
        defaultValue={rule?.minimumQuantity?.toString()}
        inputMode="numeric"
        label={tCrm("crm.quotes.cpq.fields.minimumQuantity", locale)}
        name="minimumQuantity"
      />
      <TextInput
        defaultValue={rule?.maximumDiscountRate?.toString()}
        inputMode="decimal"
        label={tCrm("crm.quotes.cpq.fields.maximumDiscountRate", locale)}
        name="maximumDiscountRate"
      />
      <TextareaInput
        defaultValue={rule?.criteriaJson}
        label={tCrm("crm.quotes.cpq.fields.criteriaJson", locale)}
        name="criteriaJson"
      />
      <Button type="submit">{tCrm("crm.common.save", locale)}</Button>
    </form>
  );
}

function SectionTitle({ title, description }: Readonly<{ title: string; description: string }>) {
  return (
    <div className="space-y-1">
      <h3 className="text-sm font-medium">{title}</h3>
      <p className="text-sm text-muted-foreground">{description}</p>
    </div>
  );
}

function TextInput({
  defaultValue,
  inputMode,
  label,
  name,
  required,
}: Readonly<{
  defaultValue?: string | null | undefined;
  inputMode?: "decimal" | "numeric";
  label: string;
  name: string;
  required?: boolean;
}>) {
  return (
    <Field>
      <FieldLabel htmlFor={`quote-cpq-${name}`}>{label}</FieldLabel>
      <FieldContent>
        <Input
          defaultValue={defaultValue ?? ""}
          id={`quote-cpq-${name}`}
          inputMode={inputMode}
          name={name}
          required={required}
        />
      </FieldContent>
    </Field>
  );
}

function TextareaInput({
  defaultValue,
  label,
  name,
  required,
}: Readonly<{
  defaultValue?: string | null | undefined;
  label: string;
  name: string;
  required?: boolean;
}>) {
  return (
    <Field>
      <FieldLabel htmlFor={`quote-cpq-${name}`}>{label}</FieldLabel>
      <FieldContent>
        <Textarea
          defaultValue={defaultValue ?? ""}
          id={`quote-cpq-${name}`}
          name={name}
          required={required}
          rows={3}
        />
      </FieldContent>
    </Field>
  );
}

function BinaryInputs({
  activeLabel,
  defaultActive,
  defaultPrimary,
  primaryLabel,
  primaryName,
}: Readonly<{
  activeLabel: string;
  defaultActive: boolean;
  defaultPrimary: boolean;
  primaryLabel: string;
  primaryName: string;
}>) {
  return (
    <div className="grid gap-3 md:grid-cols-2">
      <label className="flex items-center gap-2 text-sm">
        <input defaultChecked={defaultPrimary} name={primaryName} type="checkbox" value="true" />
        {primaryLabel}
      </label>
      <label className="flex items-center gap-2 text-sm">
        <input type="hidden" name="isActive" value="false" />
        <input defaultChecked={defaultActive} name="isActive" type="checkbox" value="true" />
        {activeLabel}
      </label>
    </div>
  );
}

function formatQuoteStatus(status: string | number, locale?: string | null | undefined): string {
  return tCrmWithFallback(`crm.quotes.status.${status}`, String(status), locale);
}

function formatBoolean(value: boolean, locale?: string | null | undefined): string {
  return value ? tCrm("crm.common.yes", locale) : tCrm("crm.common.no", locale);
}

function formatNumber(value: number | null | undefined, locale?: string | null | undefined) {
  if (value === null || value === undefined) {
    return "-";
  }

  return new Intl.NumberFormat(locale ?? undefined, { maximumFractionDigits: 2 }).format(value);
}

function formatPercent(value: number | null | undefined, locale?: string | null | undefined) {
  if (value === null || value === undefined) {
    return "-";
  }

  return new Intl.NumberFormat(locale ?? undefined, {
    maximumFractionDigits: 2,
    style: "percent",
  }).format(value);
}
