import type { ActivityTimelineItem } from "@/lib/crm-api";
import type { CrmDateSettings } from "@/lib/date-time/crm-date-time";
import { tCrm } from "@/lib/i18n/crm-i18n";

import { ActivityTimelinePanel } from "./activity-timeline-panel";

export function ActivityFeedList({
  activities,
  dateSettings,
  locale,
}: Readonly<{
  activities: ActivityTimelineItem[];
  dateSettings: CrmDateSettings;
  locale: string;
}>) {
  return (
    <ActivityTimelinePanel
      activities={activities}
      dateSettings={dateSettings}
      locale={locale}
      title={tCrm("crm.activities.page.title", locale)}
      description={tCrm("crm.activities.page.description", locale)}
      className="border-0 bg-transparent shadow-none"
    />
  );
}
