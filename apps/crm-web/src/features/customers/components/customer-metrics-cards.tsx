"use client";

import { useMemo, useState } from "react";
import { Text } from "@netmetric/ui";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@netmetric/ui/client";
import { MoreHorizontal } from "lucide-react";

import type {
  CustomerMetricsByPeriod,
  CustomerMetricsPeriod,
} from "@/features/customers/data/customers-data";

const periods: CustomerMetricsPeriod[] = ["daily", "weekly", "monthly"];

function formatPercent(value: number): string {
  return `${value >= 0 ? "+" : ""}${value.toFixed(1)}%`;
}

function periodLabel(period: CustomerMetricsPeriod): string {
  if (period === "daily") return "Daily";
  if (period === "weekly") return "Weekly";
  return "Monthly";
}

export function CustomerMetricsCards({ metrics }: Readonly<{ metrics: CustomerMetricsByPeriod }>) {
  const [period, setPeriod] = useState<CustomerMetricsPeriod>("weekly");
  const selected = metrics[period];
  const cards = useMemo(
    () => [
      {
        label: "Corporate Customers",
        value: selected.corporateCount,
        detail: `${periodLabel(period)} growth ${formatPercent(selected.corporateGrowthPercent)}`,
      },
      {
        label: "Individual Customers",
        value: selected.individualCount,
        detail: `${periodLabel(period)} growth ${formatPercent(selected.individualGrowthPercent)}`,
      },
      {
        label: "Active Customers",
        value: selected.activeCount,
        detail: `${periodLabel(period)} active customer count`,
      },
    ],
    [period, selected],
  );

  return (
    <div className="grid gap-3 md:grid-cols-3">
      {cards.map((card) => (
        <article key={card.label} className="min-h-28 rounded-lg border bg-card p-4 shadow-xs">
          <div className="flex items-start justify-between gap-3">
            <div>
              <Text className="text-xs font-medium uppercase text-muted-foreground">
                {card.label}
              </Text>
              <Text className="mt-2 text-2xl font-semibold tracking-tight text-foreground">
                {card.value}
              </Text>
            </div>
            <DropdownMenu>
              <DropdownMenuTrigger>
                <span
                  className="inline-flex h-8 w-8 items-center justify-center rounded-sm text-muted-foreground transition-colors hover:bg-muted hover:text-foreground"
                  aria-label={`${card.label} period options`}
                >
                  <MoreHorizontal className="size-4" aria-hidden="true" />
                </span>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end">
                {periods.map((item) => (
                  <DropdownMenuItem key={item} onSelect={() => setPeriod(item)}>
                    {periodLabel(item)}
                  </DropdownMenuItem>
                ))}
              </DropdownMenuContent>
            </DropdownMenu>
          </div>
          <Text className="mt-3 text-xs text-muted-foreground">{card.detail}</Text>
        </article>
      ))}
    </div>
  );
}
