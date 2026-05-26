"use client";

import Link from "next/link";
import {
  Badge,
  Button,
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@netmetric/ui";

import { CrmEmptyState } from "@/components/shell/crm-empty-state";
import type { ActivityTimelineItem } from "@/lib/crm-api";
import { type CrmDateSettings, formatCrmDateTime } from "@/lib/date-time/crm-date-time";
import { tCrm } from "@/lib/i18n/crm-i18n";

function toReadableLabel(value: string): string {
  return value
    .replaceAll(".", " ")
    .replaceAll("-", " ")
    .split(" ")
    .filter(Boolean)
    .map((token) => token.charAt(0).toUpperCase() + token.slice(1))
    .join(" ");
}

function renderRelatedSummary(activity: ActivityTimelineItem): string {
  if (activity.relatedRecords.length === 0) {
    return "-";
  }

  return activity.relatedRecords
    .slice(0, 3)
    .map((record) => record.displayName ?? `${record.entityType}:${record.entityId}`)
    .join(", ");
}

export function ActivityTimelinePanel({
  title,
  description,
  activities,
  dateSettings,
  locale,
  unavailable = false,
  className,
}: Readonly<{
  title: string;
  description?: string;
  activities: ActivityTimelineItem[];
  dateSettings: CrmDateSettings;
  locale: string;
  unavailable?: boolean;
  className?: string;
}>) {
  return (
    <Card className={className}>
      <CardHeader>
        <CardTitle>{title}</CardTitle>
        {description ? <CardDescription>{description}</CardDescription> : null}
      </CardHeader>
      <CardContent>
        {unavailable ? (
          <CrmEmptyState
            compact
            title={tCrm("crm.activities.states.unavailableTitle", locale)}
            description={tCrm("crm.activities.states.unavailableDescription", locale)}
          />
        ) : activities.length === 0 ? (
          <CrmEmptyState
            compact
            title={tCrm("crm.activities.states.emptyTitle", locale)}
            description={tCrm("crm.activities.states.emptyDescription", locale)}
          />
        ) : (
          <div className="space-y-3">
            {activities.map((activity) => (
              <article
                key={activity.id}
                className="space-y-3 rounded-lg border border-border/60 bg-background/60 p-4"
              >
                <div className="flex flex-wrap items-start justify-between gap-2">
                  <div className="space-y-1">
                    <p className="text-sm font-semibold leading-tight">{activity.title}</p>
                    <p className="text-xs text-muted-foreground">
                      {formatCrmDateTime(activity.occurredAtUtc, dateSettings)}
                    </p>
                  </div>
                  <div className="flex flex-wrap items-center gap-2">
                    <Badge variant="secondary">{toReadableLabel(activity.type)}</Badge>
                    {activity.status ? (
                      <Badge variant="outline">{toReadableLabel(activity.status)}</Badge>
                    ) : null}
                  </div>
                </div>

                {activity.description ? (
                  <p className="text-sm text-muted-foreground">{activity.description}</p>
                ) : null}

                <dl className="grid gap-2 text-xs sm:grid-cols-2 lg:grid-cols-4">
                  <div>
                    <dt className="text-muted-foreground">
                      {tCrm("crm.activities.fields.source", locale)}
                    </dt>
                    <dd className="font-medium">
                      {activity.sourceModule} · {activity.sourceEntityType}
                    </dd>
                  </div>
                  <div>
                    <dt className="text-muted-foreground">
                      {tCrm("crm.activities.fields.sourceEntityId", locale)}
                    </dt>
                    <dd className="font-medium">{activity.sourceEntityId ?? "-"}</dd>
                  </div>
                  <div>
                    <dt className="text-muted-foreground">
                      {tCrm("crm.activities.fields.actor", locale)}
                    </dt>
                    <dd className="font-medium">{activity.actorUserId ?? "-"}</dd>
                  </div>
                  <div>
                    <dt className="text-muted-foreground">
                      {tCrm("crm.activities.fields.owner", locale)}
                    </dt>
                    <dd className="font-medium">{activity.ownerUserId ?? "-"}</dd>
                  </div>
                </dl>

                <div className="text-xs">
                  <span className="text-muted-foreground">
                    {tCrm("crm.activities.fields.relatedRecords", locale)}:{" "}
                  </span>
                  <span className="font-medium">{renderRelatedSummary(activity)}</span>
                </div>

                <Button asChild variant="outline" size="sm" className="h-8">
                  <Link href={`/activities/${activity.id}`}>
                    {tCrm("crm.activities.actions.viewActivity", locale)}
                  </Link>
                </Button>
              </article>
            ))}
          </div>
        )}
      </CardContent>
    </Card>
  );
}
