import { AuditActivityPanel } from "@/features/account/components/audit-activity-panel";
import { getAccountDateSettingsForPage } from "@/features/account/data/account-read-data";
import { getAuditEntriesForPage } from "@/features/account/data/audit-data";
import { handleAccountApiPageError } from "@/lib/auth/handle-account-api-page-error";
import { requireAccountSession } from "@/lib/auth/require-account-session";

type AuditPageProps = {
  searchParams?: Promise<Record<string, string | string[] | undefined>>;
};

function readStringParam(value: string | string[] | undefined): string | undefined {
  if (typeof value === "string") {
    return value.trim() || undefined;
  }

  if (Array.isArray(value) && typeof value[0] === "string") {
    return value[0].trim() || undefined;
  }

  return undefined;
}

function normalizeLimit(raw: string | undefined): number {
  if (!raw) {
    return 50;
  }

  const parsed = Number(raw);
  if (!Number.isFinite(parsed)) {
    return 50;
  }

  return Math.min(200, Math.max(1, Math.floor(parsed)));
}

export default async function AuditPage({ searchParams }: AuditPageProps) {
  await requireAccountSession("/audit");
  const resolvedSearchParams = searchParams ? await searchParams : undefined;
  const eventType = readStringParam(resolvedSearchParams?.eventType);
  const limit = normalizeLimit(readStringParam(resolvedSearchParams?.limit));

  let auditEntries;
  let dateSettings;
  try {
    [auditEntries, dateSettings] = await Promise.all([
      getAuditEntriesForPage(
        eventType
          ? {
              limit,
              eventType,
            }
          : { limit },
      ),
      getAccountDateSettingsForPage(),
    ]);
  } catch (error) {
    handleAccountApiPageError(error);
  }

  return (
    <AuditActivityPanel
      audit={auditEntries}
      activeEventType={eventType}
      activeLimit={limit}
      dateSettings={dateSettings}
    />
  );
}
