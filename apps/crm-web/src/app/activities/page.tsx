import { CrmPageShell } from "@/components/shell/crm-page-shell";
import { CrmPagination } from "@/components/shell/crm-pagination";
import { ActivityFeedFilters } from "@/features/activities/components/activity-feed-filters";
import { ActivityFeedList } from "@/features/activities/components/activity-feed-list";
import {
  getActivitiesData,
  parseActivitiesListQuery,
} from "@/features/activities/data/activities-data";
import { requireCrmSession } from "@/lib/crm-auth/require-crm-session";
import { tCrm } from "@/lib/i18n/crm-i18n";
import { getRequestDateSettings } from "@/lib/i18n/request-date-settings";

export default async function ActivitiesPage({
  searchParams,
}: {
  searchParams: Promise<Record<string, string | string[] | undefined>>;
}) {
  await requireCrmSession("/activities");
  const dateSettings = await getRequestDateSettings();
  const locale = dateSettings.locale;
  const params = await searchParams;
  const query = parseActivitiesListQuery(params);
  const feed = await getActivitiesData(query, "/activities");

  const currentQuery = new URLSearchParams();
  for (const [key, value] of Object.entries(params)) {
    if (typeof value === "string") currentQuery.set(key, value);
    if (Array.isArray(value) && value[0]) currentQuery.set(key, value[0]);
  }

  const totalPages = Math.ceil(feed.totalCount / Math.max(1, feed.pageSize));

  return (
    <CrmPageShell
      title={tCrm("crm.activities.page.title", locale)}
      description={tCrm("crm.activities.page.description", locale)}
    >
      <div className="space-y-4">
        <ActivityFeedFilters locale={locale} query={query} />
        <ActivityFeedList activities={feed.items} dateSettings={dateSettings} locale={locale} />
        <CrmPagination
          currentPage={feed.page}
          totalPages={totalPages}
          basePath="/activities"
          currentQuery={currentQuery}
        />
      </div>
    </CrmPageShell>
  );
}
