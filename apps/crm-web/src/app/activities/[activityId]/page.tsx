import { CrmPageShell } from "@/components/shell/crm-page-shell";
import { getActivityByIdData } from "@/features/activities/data/activities-data";
import { requireCrmSession } from "@/lib/crm-auth/require-crm-session";
import { formatCrmDateTime } from "@/lib/date-time/crm-date-time";
import { tCrm } from "@/lib/i18n/crm-i18n";
import { getRequestDateSettings } from "@/lib/i18n/request-date-settings";

function stringifyMetadataValue(value: string | number | boolean | null): string {
  if (value === null) {
    return "-";
  }

  return String(value);
}

export default async function ActivityDetailPage({
  params,
}: {
  params: Promise<{ activityId: string }>;
}) {
  const { activityId } = await params;
  await requireCrmSession(`/activities/${activityId}`);
  const dateSettings = await getRequestDateSettings();
  const locale = dateSettings.locale;
  const activity = await getActivityByIdData(activityId, `/activities/${activityId}`);

  return (
    <CrmPageShell
      title={tCrm("crm.activities.detail.title", locale)}
      description={tCrm("crm.activities.detail.description", locale)}
    >
      <section className="space-y-4 rounded-lg border border-border/60 bg-background/60 p-4">
        <div className="space-y-1">
          <h1 className="text-xl font-semibold">{activity.title}</h1>
          <p className="text-sm text-muted-foreground">{activity.description || "-"}</p>
        </div>

        <dl className="grid gap-3 text-sm sm:grid-cols-2 lg:grid-cols-3">
          <div>
            <dt className="text-muted-foreground">{tCrm("crm.activities.fields.type", locale)}</dt>
            <dd className="font-medium">{activity.type}</dd>
          </div>
          <div>
            <dt className="text-muted-foreground">
              {tCrm("crm.activities.fields.status", locale)}
            </dt>
            <dd className="font-medium">{activity.status || "-"}</dd>
          </div>
          <div>
            <dt className="text-muted-foreground">
              {tCrm("crm.activities.fields.occurredAt", locale)}
            </dt>
            <dd className="font-medium">
              {formatCrmDateTime(activity.occurredAtUtc, dateSettings)}
            </dd>
          </div>
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
            <dd className="font-medium">{activity.sourceEntityId || "-"}</dd>
          </div>
          <div>
            <dt className="text-muted-foreground">{tCrm("crm.activities.fields.actor", locale)}</dt>
            <dd className="font-medium">{activity.actorUserId || "-"}</dd>
          </div>
          <div>
            <dt className="text-muted-foreground">{tCrm("crm.activities.fields.owner", locale)}</dt>
            <dd className="font-medium">{activity.ownerUserId || "-"}</dd>
          </div>
        </dl>

        <div className="space-y-2">
          <h2 className="text-sm font-semibold">
            {tCrm("crm.activities.fields.relatedRecords", locale)}
          </h2>
          {activity.relatedRecords.length === 0 ? (
            <p className="text-sm text-muted-foreground">-</p>
          ) : (
            <ul className="space-y-1 text-sm">
              {activity.relatedRecords.map((record) => (
                <li
                  key={`${record.entityType}:${record.entityId}`}
                  className="rounded border border-border/60 px-2 py-1"
                >
                  {record.displayName || record.entityType} ({record.entityId})
                </li>
              ))}
            </ul>
          )}
        </div>

        <div className="space-y-2">
          <h2 className="text-sm font-semibold">
            {tCrm("crm.activities.fields.metadata", locale)}
          </h2>
          {Object.keys(activity.metadata).length === 0 ? (
            <p className="text-sm text-muted-foreground">-</p>
          ) : (
            <ul className="space-y-1 text-sm">
              {Object.entries(activity.metadata).map(([key, value]) => (
                <li key={key} className="rounded border border-border/60 px-2 py-1">
                  <span className="font-medium">{key}: </span>
                  <span>{stringifyMetadataValue(value)}</span>
                </li>
              ))}
            </ul>
          )}
        </div>
      </section>
    </CrmPageShell>
  );
}
