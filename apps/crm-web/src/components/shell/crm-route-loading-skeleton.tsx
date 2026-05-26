import { Skeleton } from "@netmetric/ui";

export type CrmLoadingVariant = "crm" | "dashboard" | "customers" | "catalog" | "settings";

const variantCopy: Record<CrmLoadingVariant, { label: string; cards: number; rows: number }> = {
  crm: { label: "CRM workspace", cards: 4, rows: 6 },
  dashboard: { label: "dashboard", cards: 4, rows: 5 },
  customers: { label: "customers", cards: 3, rows: 7 },
  catalog: { label: "product catalog", cards: 4, rows: 6 },
  settings: { label: "settings", cards: 3, rows: 4 },
};

export function CrmRouteLoadingSkeleton({
  variant = "crm",
}: Readonly<{ variant?: CrmLoadingVariant }>) {
  const copy = variantCopy[variant];

  return (
    <section
      aria-busy="true"
      aria-label={`Loading ${copy.label}`}
      className="min-h-full space-y-5 p-4 sm:p-5 lg:p-6"
    >
      <div className="sr-only" role="status">
        Loading {copy.label}
      </div>

      <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-4">
        {Array.from({ length: copy.cards }).map((_, index) => (
          <div
            className="rounded-md border border-border bg-card/90 p-4 shadow-sm"
            key={`crm-loading-card-${index}`}
          >
            <div className="flex items-center justify-between">
              <Skeleton className="h-4 w-28" />
              <Skeleton className="h-6 w-16" />
            </div>
            <Skeleton className="mt-5 h-8 w-24" />
            <Skeleton className="mt-3 h-3 w-36" />
          </div>
        ))}
      </div>

      <div className="grid min-h-[420px] gap-4 xl:grid-cols-[minmax(0,1fr)_360px]">
        <div className="rounded-md border border-border bg-card/90 shadow-sm">
          <div className="flex flex-col gap-3 border-b border-border p-4 sm:flex-row sm:items-center sm:justify-between">
            <div className="space-y-2">
              <Skeleton className="h-5 w-48" />
              <Skeleton className="h-3 w-72 max-w-full" />
            </div>
            <div className="flex flex-wrap gap-2">
              <Skeleton className="h-9 w-28" />
              <Skeleton className="h-9 w-24" />
              <Skeleton className="h-9 w-9" />
            </div>
          </div>

          <div className="divide-y divide-border px-4">
            {Array.from({ length: copy.rows }).map((_, index) => (
              <div
                className="grid gap-3 py-4 md:grid-cols-[minmax(0,1.3fr)_120px_120px_84px]"
                key={`crm-loading-row-${index}`}
              >
                <div className="space-y-2">
                  <Skeleton className="h-4 w-4/5" />
                  <Skeleton className="h-3 w-1/2" />
                </div>
                <Skeleton className="h-4 w-24" />
                <Skeleton className="h-4 w-28" />
                <Skeleton className="h-8 w-20" />
              </div>
            ))}
          </div>
        </div>

        <aside className="rounded-md border border-border bg-muted/20 p-4">
          <Skeleton className="h-5 w-36" />
          <div className="mt-5 space-y-3">
            <Skeleton className="h-20 w-full" />
            <Skeleton className="h-20 w-full" />
            <Skeleton className="h-20 w-full" />
          </div>
        </aside>
      </div>
    </section>
  );
}
