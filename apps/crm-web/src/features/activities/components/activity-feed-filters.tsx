import Link from "next/link";
import { Button, Input } from "@netmetric/ui";

import type { ActivitiesListQuery } from "@/features/activities/data/activities-data";
import { tCrm } from "@/lib/i18n/crm-i18n";

const sourceModuleOptions = [
  "work-management",
  "customer-management",
  "lead-management",
  "deal-management",
  "opportunity-management",
  "quote-management",
  "ticket-management",
] as const;

export function ActivityFeedFilters({
  locale,
  query,
}: Readonly<{
  locale: string;
  query: ActivitiesListQuery;
}>) {
  return (
    <form method="get" className="rounded-lg border border-border/60 bg-background/60 p-3">
      <div className="grid gap-2 sm:grid-cols-2 lg:grid-cols-6">
        <Input
          name="type"
          defaultValue={query.type ?? ""}
          placeholder={tCrm("crm.activities.filters.typePlaceholder", locale)}
          className="h-8"
          aria-label={tCrm("crm.activities.filters.type", locale)}
        />
        <select
          name="sourceModule"
          defaultValue={query.sourceModule ?? ""}
          className="h-8 rounded-sm border border-input bg-background px-2 text-sm"
          aria-label={tCrm("crm.activities.filters.sourceModule", locale)}
        >
          <option value="">{tCrm("crm.activities.filters.allSources", locale)}</option>
          {sourceModuleOptions.map((option) => (
            <option key={option} value={option}>
              {option}
            </option>
          ))}
        </select>
        <Input
          name="from"
          type="date"
          defaultValue={query.from ?? ""}
          className="h-8"
          aria-label={tCrm("crm.activities.filters.from", locale)}
        />
        <Input
          name="to"
          type="date"
          defaultValue={query.to ?? ""}
          className="h-8"
          aria-label={tCrm("crm.activities.filters.to", locale)}
        />
        <select
          name="pageSize"
          defaultValue={String(query.pageSize)}
          className="h-8 rounded-sm border border-input bg-background px-2 text-sm"
          aria-label={tCrm("crm.activities.filters.pageSize", locale)}
        >
          <option value="10">10</option>
          <option value="20">20</option>
          <option value="50">50</option>
        </select>
        <input type="hidden" name="page" value="1" />
        <div className="flex items-center gap-2">
          <Button type="submit" size="sm" className="h-8">
            {tCrm("crm.activities.filters.apply", locale)}
          </Button>
          <Button asChild type="button" size="sm" variant="ghost" className="h-8">
            <Link href="/activities">{tCrm("crm.activities.filters.clear", locale)}</Link>
          </Button>
        </div>
      </div>
    </form>
  );
}
