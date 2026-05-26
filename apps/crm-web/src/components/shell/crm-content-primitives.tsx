import type { ReactNode } from "react";

import {
  MetricCard,
  MetricGrid,
  SectionCard,
  ToolbarSurface,
  type MetricItem,
  type MetricTone,
} from "@netmetric/ui";

export type CrmMetricItem = {
  label: string;
  value: string | number;
  description?: string | undefined;
  icon?: ReactNode | undefined;
  tone?: MetricTone | undefined;
  badge?: string | undefined;
};

export function CrmMetricGrid({
  items,
  columns = "auto",
}: Readonly<{
  items: CrmMetricItem[];
  columns?: "auto" | "three" | "four";
}>) {
  return <MetricGrid items={items as MetricItem[]} columns={columns} />;
}

export function CrmMetricCard({ item }: Readonly<{ item: CrmMetricItem }>) {
  return <MetricCard item={item as MetricItem} />;
}

export function CrmSectionCard({
  title,
  description,
  actions,
  children,
  className,
  contentClassName,
}: Readonly<{
  title: string;
  description?: string | undefined;
  actions?: ReactNode | undefined;
  children: ReactNode;
  className?: string | undefined;
  contentClassName?: string | undefined;
}>) {
  return (
    <SectionCard
      title={title}
      description={description}
      actions={actions}
      className={className}
      contentClassName={contentClassName}
    >
      {children}
    </SectionCard>
  );
}

export function CrmToolbarSurface({
  title,
  description,
  children,
}: Readonly<{
  title: string;
  description: string;
  children: ReactNode;
}>) {
  return (
    <ToolbarSurface title={title} description={description}>
      {children}
    </ToolbarSurface>
  );
}
