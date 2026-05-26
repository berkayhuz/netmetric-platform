import { Badge, Text } from "@netmetric/ui";
import type { AccountDateSettings } from "@/lib/account-date";
import { formatAccountDateTime } from "@/lib/account-date";

import type { AccountAuditEntryResponse } from "@/lib/account-api/account-api-types";
import { tAccountClient } from "@/lib/i18n/account-i18n";

type AuditActivityItemProps = {
  item: AccountAuditEntryResponse;
  dateSettings: AccountDateSettings;
};

export function AuditActivityItem({ item, dateSettings }: AuditActivityItemProps) {
  return (
    <section className="space-y-3 border-b border-border/70 pb-4">
      <div className="flex flex-wrap items-center gap-2">
        <Badge variant="outline">{item.severity}</Badge>
        <Badge variant="outline">{item.eventType}</Badge>
      </div>
      <Text className="text-base font-semibold text-foreground">
        {tAccountClient("account.audit.eventTitle")}
      </Text>
      <div className="space-y-2">
        <Text className="text-sm text-muted-foreground">
          {tAccountClient("account.audit.occurredLabel")}:{" "}
          {formatAccountDateTime(item.occurredAt, dateSettings)}
        </Text>
        <Text className="text-sm text-muted-foreground">
          {tAccountClient("account.audit.correlationIdLabel")}:{" "}
          {item.correlationId?.trim()
            ? item.correlationId
            : tAccountClient("account.common.notAvailable")}
        </Text>
      </div>
    </section>
  );
}
