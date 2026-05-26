import { Badge, Text } from "@netmetric/ui";

import type { ConsentHistoryItemResponse } from "@/lib/account-api";
import { tAccountClient } from "@/lib/i18n/account-i18n";

import { ConsentAcceptForm } from "./consent-accept-form";

type ConsentStatusCardProps = {
  item: ConsentHistoryItemResponse;
};

function formatDate(value: string, emptyLabel: string): string {
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return emptyLabel;
  }

  return date.toLocaleString();
}

export function ConsentStatusCard({ item }: ConsentStatusCardProps) {
  const status = item.status.trim().toLowerCase();
  const isAccepted = status === "accepted" || status === "current";
  const isActionable =
    !isAccepted &&
    (status === "required" ||
      status === "pending" ||
      status === "declined" ||
      status === "revoked" ||
      status === "expired");

  return (
    <section className="space-y-3 border-b border-border/70 pb-4">
      <div className="flex flex-wrap items-center gap-2">
        <Badge variant="outline">{item.status}</Badge>
        <Badge variant="outline">
          {tAccountClient("account.privacy.versionLabel", { version: item.version })}
        </Badge>
      </div>
      <div className="space-y-1">
        <Text className="text-base font-semibold text-foreground">{item.consentType}</Text>
        <Text className="text-sm text-muted-foreground">
          {tAccountClient("account.privacy.historyEntry")}
        </Text>
      </div>
      <div>
        <Text className="text-sm text-muted-foreground">
          {tAccountClient("account.privacy.decidedAtLabel")}:{" "}
          {formatDate(item.decidedAt, tAccountClient("account.common.notAvailable"))}
        </Text>
        {isActionable ? (
          <div className="mt-3">
            <ConsentAcceptForm consentType={item.consentType} version={item.version} />
          </div>
        ) : (
          <Text className="mt-3 text-xs text-muted-foreground">
            {tAccountClient("account.privacy.actionNotRequired")}
          </Text>
        )}
      </div>
    </section>
  );
}
