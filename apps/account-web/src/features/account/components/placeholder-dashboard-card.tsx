import { Badge, Text } from "@netmetric/ui";

import { tAccountClient } from "@/lib/i18n/account-i18n";

type PlaceholderDashboardCardProps = {
  title: string;
  description: string;
};

export function PlaceholderDashboardCard({ title, description }: PlaceholderDashboardCardProps) {
  return (
    <section className="space-y-4 border-b border-border/70 pb-5">
      <div className="flex items-start justify-between gap-3 sm:flex-row sm:items-center">
        <div className="space-y-1">
          <Text className="text-base font-semibold text-foreground">{title}</Text>
          <Text className="text-sm text-muted-foreground">{description}</Text>
        </div>
        <Badge variant="outline">{tAccountClient("account.scaffold.shortLabel")}</Badge>
      </div>
      <Text className="text-sm text-muted-foreground">
        {tAccountClient("account.scaffold.placeholderDescription")}
      </Text>
    </section>
  );
}
