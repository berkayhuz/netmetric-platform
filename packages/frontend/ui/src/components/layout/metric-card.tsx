import { cn } from "../../lib/utils";
import { Badge } from "../data-display/badge";
import { Text } from "../typography/text";

import type { ReactNode } from "react";

export type MetricTone = "neutral" | "success" | "warning" | "danger" | "info";

const metricToneClasses: Record<MetricTone, string> = {
  neutral: "border-border/70 bg-card/90 text-foreground",
  success: "border-emerald-500/25 bg-emerald-500/10 text-emerald-700 dark:text-emerald-300",
  warning: "border-amber-500/25 bg-amber-500/10 text-amber-700 dark:text-amber-300",
  danger: "border-rose-500/25 bg-rose-500/10 text-rose-700 dark:text-rose-300",
  info: "border-sky-500/25 bg-sky-500/10 text-sky-700 dark:text-sky-300",
};

export type MetricItem = {
  label: string;
  value: string | number;
  description?: string | undefined;
  icon?: ReactNode | undefined;
  tone?: MetricTone | undefined;
  badge?: string | undefined;
};

export function MetricGrid({
  items,
  columns = "auto",
}: Readonly<{
  items: MetricItem[];
  columns?: "auto" | "three" | "four";
}>) {
  return (
    <div
      className={cn(
        "grid gap-3",
        columns === "three" ? "md:grid-cols-3" : null,
        columns === "four" ? "md:grid-cols-2 xl:grid-cols-4" : null,
        columns === "auto" ? "sm:grid-cols-2 xl:grid-cols-4" : null,
      )}
    >
      {items.map((item) => (
        <MetricCard item={item} key={item.label} />
      ))}
    </div>
  );
}

export function MetricCard({ item }: Readonly<{ item: MetricItem }>) {
  const tone = item.tone ?? "neutral";

  return (
    <div
      className={cn(
        "group relative min-h-28 overflow-hidden rounded-2xl border p-4 shadow-[0_14px_40px_rgb(15_23_42_/_0.07)] transition-all hover:-translate-y-0.5 hover:shadow-[0_18px_54px_rgb(15_23_42_/_0.11)]",
        metricToneClasses[tone],
      )}
    >
      <div className="pointer-events-none absolute inset-x-0 top-0 h-px bg-gradient-to-r from-transparent via-foreground/20 to-transparent" />
      <div className="flex items-start justify-between gap-3">
        <div className="min-w-0">
          <Text className="truncate text-xs font-medium uppercase text-muted-foreground">
            {item.label}
          </Text>
          <Text className="mt-2 text-2xl font-semibold tracking-tight text-foreground">
            {item.value}
          </Text>
        </div>
        {item.icon ? (
          <div className="rounded-xl border bg-background/80 p-2 text-muted-foreground shadow-xs">
            {item.icon}
          </div>
        ) : null}
      </div>
      <div className="mt-3 flex min-h-5 items-center gap-2">
        {item.badge ? (
          <Badge variant="secondary" className="rounded-sm">
            {item.badge}
          </Badge>
        ) : null}
        {item.description ? (
          <Text className="line-clamp-2 text-xs text-muted-foreground">{item.description}</Text>
        ) : null}
      </div>
    </div>
  );
}
