import { NotificationsManagementPanel } from "@/features/account/components/notifications-management-panel";
import { getAccountDateSettingsForPage } from "@/features/account/data/account-read-data";
import {
  getNotificationPreferencesForPage,
  getNotificationsForPage,
} from "@/features/account/data/notifications-data";
import { handleAccountApiPageError } from "@/lib/auth/handle-account-api-page-error";
import { requireAccountSession } from "@/lib/auth/require-account-session";

type NotificationsPageProps = {
  searchParams?: Promise<Record<string, string | string[] | undefined>>;
};

function normalizeFilter(input: string | undefined): "all" | "unread" | "read" {
  if (input === "unread" || input === "read") {
    return input;
  }

  return "all";
}

export default async function NotificationsPage({ searchParams }: NotificationsPageProps) {
  await requireAccountSession("/notifications");

  const resolvedSearchParams = searchParams ? await searchParams : undefined;
  const rawFilter = resolvedSearchParams?.filter;
  const filterValue =
    typeof rawFilter === "string"
      ? rawFilter
      : Array.isArray(rawFilter) && typeof rawFilter[0] === "string"
        ? rawFilter[0]
        : undefined;
  const activeFilter = normalizeFilter(filterValue);

  let notifications;
  let preferences;
  let dateSettings;

  try {
    [notifications, preferences, dateSettings] = await Promise.all([
      getNotificationsForPage(activeFilter === "all" ? undefined : activeFilter),
      getNotificationPreferencesForPage(),
      getAccountDateSettingsForPage(),
    ]);
  } catch (error) {
    handleAccountApiPageError(error);
  }

  return (
    <NotificationsManagementPanel
      notifications={notifications}
      preferences={preferences}
      activeFilter={activeFilter}
      dateSettings={dateSettings}
    />
  );
}
