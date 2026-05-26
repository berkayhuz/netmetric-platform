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

import { upsertDealWinLossReviewFormAction } from "@/features/deals/actions/deal-lifecycle-actions";
import { ActivityTimelinePanel } from "@/features/activities/components/activity-timeline-panel";
import { ActivityComposer } from "@/features/activities/components/activity-composer";
import type {
  ActivityTimelineFeed,
  DealDetailDto,
  DealOutcomeHistoryDto,
  DealWorkspaceDto,
  DealWinLossSummaryDto,
} from "@/lib/crm-api";
import {
  type CrmDateSettings,
  formatCrmDate,
  formatCrmDateTime,
} from "@/lib/date-time/crm-date-time";
import { tCrm } from "@/lib/i18n/crm-i18n";

const reviewOutcomeOptions = ["Pending", "Won", "Lost"] as const;

type DealDetailWorkspaceProps = {
  deal: DealDetailDto;
  workspace: DealWorkspaceDto;
  timeline: DealOutcomeHistoryDto[];
  winLossSummary?: DealWinLossSummaryDto | null;
  canManageWinLoss: boolean;
  canReadActivities: boolean;
  canCreateActivities: boolean;
  unifiedTimeline: ActivityTimelineFeed;
  isUnifiedTimelineUnavailable: boolean;
  dateSettings: CrmDateSettings;
  locale?: string | null | undefined;
};

export function DealDetailWorkspace({
  deal,
  workspace,
  timeline,
  winLossSummary,
  canManageWinLoss,
  canReadActivities,
  canCreateActivities,
  unifiedTimeline,
  isUnifiedTimelineUnavailable,
  dateSettings,
  locale,
}: Readonly<DealDetailWorkspaceProps>) {
  const lostReasons = workspace.lostReasons;

  return (
    <section className="space-y-6">
      <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
        <DealMetricCard
          title={tCrm("crm.deals.fields.totalAmount", locale)}
          value={formatNumber(deal.totalAmount, locale)}
        />
        <DealMetricCard
          title={tCrm("crm.deals.fields.closedDate", locale)}
          value={formatCrmDate(deal.closedDate, dateSettings)}
        />
        <DealMetricCard title={tCrm("crm.deals.fields.stage", locale)} value={String(deal.stage)} />
        <DealMetricCard
          title={tCrm("crm.deals.fields.outcome", locale)}
          value={String(deal.outcome)}
        />
      </div>

      <DealProfileCard deal={deal} dateSettings={dateSettings} locale={locale} />

      <div className="grid gap-4 xl:grid-cols-2">
        <DealReviewCard deal={deal} canManageWinLoss={canManageWinLoss} locale={locale} />
        <DealLostReasonsCard lostReasons={lostReasons} locale={locale} />
      </div>

      {winLossSummary ? <DealWinLossSummaryCard summary={winLossSummary} locale={locale} /> : null}

      <DealTimelineCard timeline={timeline} dateSettings={dateSettings} locale={locale} />
      {canReadActivities ? (
        <div className="space-y-4">
          {canCreateActivities ? (
            <ActivityComposer
              primaryRecord={{ entityType: "deal", entityId: deal.id }}
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
    </section>
  );
}

function DealMetricCard({
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

function DealProfileCard({
  deal,
  dateSettings,
  locale,
}: Readonly<{
  deal: DealDetailDto;
  dateSettings: CrmDateSettings;
  locale?: string | null | undefined;
}>) {
  const fields = [
    { label: tCrm("crm.deals.fields.dealCode", locale), value: deal.dealCode },
    { label: tCrm("crm.deals.fields.name", locale), value: deal.name },
    {
      label: tCrm("crm.deals.fields.totalAmount", locale),
      value: formatNumber(deal.totalAmount, locale),
    },
    {
      label: tCrm("crm.deals.fields.closedDate", locale),
      value: formatCrmDate(deal.closedDate, dateSettings),
    },
    { label: tCrm("crm.deals.fields.opportunityId", locale), value: deal.opportunityId },
    { label: tCrm("crm.deals.fields.companyId", locale), value: deal.companyId },
    { label: tCrm("crm.deals.fields.ownerUserId", locale), value: deal.ownerUserId },
    { label: tCrm("crm.deals.fields.lostReasonId", locale), value: deal.lostReasonId },
    { label: tCrm("crm.deals.fields.lostNote", locale), value: deal.lostNote },
    {
      label: tCrm("crm.deals.fields.state", locale),
      value: deal.isActive
        ? tCrm("crm.common.active", locale)
        : tCrm("crm.common.inactive", locale),
    },
  ];

  return (
    <Card>
      <CardHeader>
        <CardTitle>{tCrm("crm.deals.pages.detail.profileTitle", locale)}</CardTitle>
        <CardDescription>{tCrm("crm.deals.pages.detail.workspaceTitle", locale)}</CardDescription>
      </CardHeader>
      <CardContent>
        <dl className="grid gap-4 md:grid-cols-2 xl:grid-cols-3">
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

function DealReviewCard({
  deal,
  canManageWinLoss,
  locale,
}: Readonly<{
  deal: DealDetailDto;
  canManageWinLoss: boolean;
  locale?: string | null | undefined;
}>) {
  const review = deal.review;

  return (
    <Card>
      <CardHeader>
        <CardTitle>{tCrm("crm.deals.pages.detail.reviewTitle", locale)}</CardTitle>
        <CardDescription>
          {tCrm("crm.deals.pages.detail.reviewDescription", locale)}
        </CardDescription>
      </CardHeader>
      <CardContent className="space-y-5">
        {review ? (
          <dl className="grid gap-3 md:grid-cols-2">
            <ReviewField label={tCrm("crm.deals.fields.outcome", locale)} value={review.outcome} />
            <ReviewField
              label={tCrm("crm.deals.fields.competitorName", locale)}
              value={review.competitorName}
            />
            <ReviewField
              label={tCrm("crm.deals.fields.competitorPrice", locale)}
              value={formatNumber(review.competitorPrice, locale)}
            />
            <ReviewField
              label={tCrm("crm.deals.fields.reviewedByUserId", locale)}
              value={review.reviewedByUserId}
            />
            <ReviewField
              label={tCrm("crm.deals.fields.summary", locale)}
              value={review.summary}
              wide
            />
            <ReviewField
              label={tCrm("crm.deals.fields.customerFeedback", locale)}
              value={review.customerFeedback}
              wide
            />
          </dl>
        ) : (
          <Text className="text-sm text-muted-foreground">
            {tCrm("crm.deals.states.noReview", locale)}
          </Text>
        )}

        {canManageWinLoss ? (
          <form
            action={upsertDealWinLossReviewFormAction.bind(null, deal.id)}
            className="grid gap-4 md:grid-cols-2"
          >
            <input type="hidden" name="rowVersion" value={review?.rowVersion ?? ""} />
            <Field>
              <FieldLabel htmlFor="deal-review-outcome">
                {tCrm("crm.deals.fields.outcome", locale)}
              </FieldLabel>
              <FieldContent>
                <NativeSelect
                  className="w-full"
                  defaultValue={review?.outcome ?? String(deal.outcome)}
                  id="deal-review-outcome"
                  name="outcome"
                >
                  {reviewOutcomeOptions.map((option) => (
                    <NativeSelectOption key={option} value={option}>
                      {option}
                    </NativeSelectOption>
                  ))}
                </NativeSelect>
              </FieldContent>
            </Field>
            <Field>
              <FieldLabel htmlFor="deal-review-competitor">
                {tCrm("crm.deals.fields.competitorName", locale)}
              </FieldLabel>
              <FieldContent>
                <Input
                  defaultValue={review?.competitorName ?? ""}
                  id="deal-review-competitor"
                  name="competitorName"
                />
              </FieldContent>
            </Field>
            <Field>
              <FieldLabel htmlFor="deal-review-competitor-price">
                {tCrm("crm.deals.fields.competitorPrice", locale)}
              </FieldLabel>
              <FieldContent>
                <Input
                  defaultValue={review?.competitorPrice?.toString() ?? ""}
                  id="deal-review-competitor-price"
                  inputMode="decimal"
                  name="competitorPrice"
                />
              </FieldContent>
            </Field>
            <Field>
              <FieldLabel htmlFor="deal-review-strengths">
                {tCrm("crm.deals.fields.strengths", locale)}
              </FieldLabel>
              <FieldContent>
                <Textarea
                  defaultValue={review?.strengths ?? ""}
                  id="deal-review-strengths"
                  name="strengths"
                  rows={3}
                />
              </FieldContent>
            </Field>
            <Field>
              <FieldLabel htmlFor="deal-review-risks">
                {tCrm("crm.deals.fields.risks", locale)}
              </FieldLabel>
              <FieldContent>
                <Textarea
                  defaultValue={review?.risks ?? ""}
                  id="deal-review-risks"
                  name="risks"
                  rows={3}
                />
              </FieldContent>
            </Field>
            <Field>
              <FieldLabel htmlFor="deal-review-summary">
                {tCrm("crm.deals.fields.summary", locale)}
              </FieldLabel>
              <FieldContent>
                <Textarea
                  defaultValue={review?.summary ?? ""}
                  id="deal-review-summary"
                  name="summary"
                  rows={3}
                />
              </FieldContent>
            </Field>
            <Field className="md:col-span-2">
              <FieldLabel htmlFor="deal-review-feedback">
                {tCrm("crm.deals.fields.customerFeedback", locale)}
              </FieldLabel>
              <FieldContent>
                <Textarea
                  defaultValue={review?.customerFeedback ?? ""}
                  id="deal-review-feedback"
                  name="customerFeedback"
                  rows={3}
                />
              </FieldContent>
            </Field>
            <div className="md:col-span-2">
              <Button type="submit">{tCrm("crm.deals.actions.saveReview", locale)}</Button>
            </div>
          </form>
        ) : null}
      </CardContent>
    </Card>
  );
}

function ReviewField({
  label,
  value,
  wide,
}: Readonly<{ label: string; value?: string | number | null | undefined; wide?: boolean }>) {
  return (
    <div className={wide ? "space-y-1 md:col-span-2" : "space-y-1"}>
      <dt className="text-xs font-medium uppercase text-muted-foreground">{label}</dt>
      <dd className="break-words text-sm">{value ?? "-"}</dd>
    </div>
  );
}

function DealLostReasonsCard({
  lostReasons,
  locale,
}: Readonly<{
  lostReasons: DealWorkspaceDto["lostReasons"];
  locale?: string | null | undefined;
}>) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>{tCrm("crm.deals.pages.detail.lostReasonsTitle", locale)}</CardTitle>
        <CardDescription>{tCrm("crm.deals.fields.lostReasonId", locale)}</CardDescription>
      </CardHeader>
      <CardContent>
        {lostReasons.length === 0 ? (
          <Text className="text-sm text-muted-foreground">
            {tCrm("crm.deals.states.noLostReasons", locale)}
          </Text>
        ) : (
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>{tCrm("crm.deals.fields.name", locale)}</TableHead>
                <TableHead>{tCrm("crm.deals.fields.description", locale)}</TableHead>
                <TableHead>{tCrm("crm.deals.fields.default", locale)}</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {lostReasons.map((reason) => (
                <TableRow key={reason.id}>
                  <TableCell>{reason.name}</TableCell>
                  <TableCell className="max-w-sm whitespace-normal break-words">
                    {reason.description ?? "-"}
                  </TableCell>
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
        )}
      </CardContent>
    </Card>
  );
}

function DealWinLossSummaryCard({
  summary,
  locale,
}: Readonly<{
  summary: DealWinLossSummaryDto;
  locale?: string | null | undefined;
}>) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>{tCrm("crm.deals.pages.detail.winLossSummaryTitle", locale)}</CardTitle>
        <CardDescription>
          {tCrm("crm.deals.pages.detail.winLossSummaryDescription", locale)}
        </CardDescription>
      </CardHeader>
      <CardContent className="space-y-5">
        <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-5">
          <SummaryMetric
            title={tCrm("crm.deals.fields.totalDeals", locale)}
            value={summary.totalDeals}
          />
          <SummaryMetric
            title={tCrm("crm.deals.fields.wonDeals", locale)}
            value={summary.wonDeals}
          />
          <SummaryMetric
            title={tCrm("crm.deals.fields.lostDeals", locale)}
            value={summary.lostDeals}
          />
          <SummaryMetric
            title={tCrm("crm.deals.fields.wonAmount", locale)}
            value={formatNumber(summary.wonAmount, locale)}
          />
          <SummaryMetric
            title={tCrm("crm.deals.fields.lostAmount", locale)}
            value={formatNumber(summary.lostAmount, locale)}
          />
        </div>
        {summary.lostReasonBreakdown.length === 0 ? null : (
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>{tCrm("crm.deals.fields.lostReasonId", locale)}</TableHead>
                <TableHead>{tCrm("crm.deals.fields.count", locale)}</TableHead>
                <TableHead>{tCrm("crm.deals.fields.totalAmount", locale)}</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {summary.lostReasonBreakdown.map((reason) => (
                <TableRow key={`${reason.lostReasonId ?? "none"}-${reason.label}`}>
                  <TableCell>{reason.label}</TableCell>
                  <TableCell>{reason.count}</TableCell>
                  <TableCell>{formatNumber(reason.totalAmount, locale)}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        )}
      </CardContent>
    </Card>
  );
}

function SummaryMetric({
  title,
  value,
}: Readonly<{ title: string; value: string | number | null | undefined }>) {
  return (
    <div className="space-y-1 rounded-md border p-3">
      <div className="text-xs font-medium uppercase text-muted-foreground">{title}</div>
      <div className="text-2xl font-semibold">{value ?? "-"}</div>
    </div>
  );
}

function DealTimelineCard({
  timeline,
  dateSettings,
  locale,
}: Readonly<{
  timeline: DealOutcomeHistoryDto[];
  dateSettings: CrmDateSettings;
  locale?: string | null | undefined;
}>) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>{tCrm("crm.deals.pages.detail.timelineTitle", locale)}</CardTitle>
        <CardDescription>{tCrm("crm.deals.pages.detail.workspaceTitle", locale)}</CardDescription>
      </CardHeader>
      <CardContent>
        {timeline.length === 0 ? (
          <Text className="text-sm text-muted-foreground">
            {tCrm("crm.deals.states.noTimeline", locale)}
          </Text>
        ) : (
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>{tCrm("crm.deals.fields.occurredAt", locale)}</TableHead>
                <TableHead>{tCrm("crm.deals.fields.outcome", locale)}</TableHead>
                <TableHead>{tCrm("crm.deals.fields.stage", locale)}</TableHead>
                <TableHead>{tCrm("crm.deals.fields.note", locale)}</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {timeline.map((event) => (
                <TableRow key={event.id}>
                  <TableCell>{formatCrmDateTime(event.occurredAt, dateSettings)}</TableCell>
                  <TableCell>
                    <Badge variant="outline">{event.outcome}</Badge>
                  </TableCell>
                  <TableCell>{event.stage}</TableCell>
                  <TableCell className="max-w-md whitespace-normal break-words">
                    {event.note ?? "-"}
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

function formatNumber(value: number | null | undefined, locale?: string | null | undefined) {
  if (value === null || value === undefined) {
    return null;
  }

  return new Intl.NumberFormat(locale ?? undefined, {
    maximumFractionDigits: 2,
  }).format(value);
}
