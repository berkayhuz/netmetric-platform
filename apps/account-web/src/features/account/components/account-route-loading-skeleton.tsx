import { Skeleton } from "@netmetric/ui";

export type AccountLoadingVariant = "account" | "profile" | "settings" | "security" | "team";

const variantCopy: Record<AccountLoadingVariant, { label: string; cards: number; rows: number }> = {
  account: { label: "account workspace", cards: 3, rows: 5 },
  profile: { label: "profile", cards: 3, rows: 4 },
  settings: { label: "settings", cards: 4, rows: 5 },
  security: { label: "security controls", cards: 3, rows: 4 },
  team: { label: "team settings", cards: 4, rows: 6 },
};

export function AccountRouteLoadingSkeleton({
  variant = "account",
}: Readonly<{ variant?: AccountLoadingVariant }>) {
  const copy = variantCopy[variant];

  return (
    <section
      aria-busy="true"
      aria-label={`Loading ${copy.label}`}
      className="min-h-full space-y-6 p-4 sm:p-6 lg:p-8"
    >
      <div className="sr-only" role="status">
        Loading {copy.label}
      </div>

      <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-4">
        {Array.from({ length: copy.cards }).map((_, index) => (
          <div
            className="rounded-md border border-border bg-card/90 p-4 shadow-sm"
            key={`account-loading-card-${index}`}
          >
            <div className="flex items-center justify-between gap-3">
              <Skeleton className="h-4 w-24" />
              <Skeleton className="size-8 rounded-sm" />
            </div>
            <Skeleton className="mt-5 h-8 w-20" />
            <Skeleton className="mt-3 h-3 w-32" />
          </div>
        ))}
      </div>

      <div className="grid gap-4 xl:grid-cols-[minmax(0,1fr)_320px]">
        <div className="rounded-md border border-border bg-card/90 p-4 shadow-sm">
          <div className="flex flex-col gap-3 border-b border-border pb-4 sm:flex-row sm:items-center sm:justify-between">
            <div className="space-y-2">
              <Skeleton className="h-5 w-44" />
              <Skeleton className="h-3 w-64 max-w-full" />
            </div>
            <div className="flex gap-2">
              <Skeleton className="h-9 w-24" />
              <Skeleton className="h-9 w-28" />
            </div>
          </div>

          <div className="divide-y divide-border">
            {Array.from({ length: copy.rows }).map((_, index) => (
              <div
                className="grid gap-3 py-4 sm:grid-cols-[minmax(0,1.5fr)_minmax(120px,0.7fr)_96px]"
                key={`account-loading-row-${index}`}
              >
                <div className="space-y-2">
                  <Skeleton className="h-4 w-3/4" />
                  <Skeleton className="h-3 w-1/2" />
                </div>
                <Skeleton className="h-4 w-28" />
                <Skeleton className="h-8 w-20" />
              </div>
            ))}
          </div>
        </div>

        <aside className="rounded-md border border-border bg-muted/20 p-4">
          <Skeleton className="h-5 w-32" />
          <div className="mt-5 space-y-3">
            <Skeleton className="h-16 w-full" />
            <Skeleton className="h-16 w-full" />
            <Skeleton className="h-10 w-2/3" />
          </div>
        </aside>
      </div>
    </section>
  );
}
