import { Badge, Text } from "@netmetric/ui";

import type { AccountInvitationSummaryResponse } from "@/lib/account-api";
import { tAccountClient } from "@/lib/i18n/account-i18n";
import { AccountSection } from "./account-section";

type InvitationsReadOnlyPanelProps = {
  invitations: AccountInvitationSummaryResponse[];
};

function formatDate(value: string | null | undefined, emptyLabel: string): string {
  if (!value) {
    return emptyLabel;
  }
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return emptyLabel;
  }
  return date.toLocaleString();
}

export function InvitationsReadOnlyPanel({ invitations }: InvitationsReadOnlyPanelProps) {
  return (
    <AccountSection
      title={tAccountClient("account.invitations.title")}
      description={tAccountClient("account.invitations.description")}
      contentClassName="space-y-3"
    >
      {invitations.length === 0 ? (
        <Text className="text-sm text-muted-foreground">
          {tAccountClient("account.invitations.emptyTitle")}
        </Text>
      ) : (
        invitations.map((invitation) => (
          <section
            key={invitation.invitationId}
            className="space-y-2 border-b border-border/70 pb-4"
          >
            <div className="flex flex-wrap items-center gap-2">
              <Text className="font-medium">{invitation.email}</Text>
              <Badge variant="outline">{invitation.status}</Badge>
            </div>
            <Text className="text-xs text-muted-foreground">
              {tAccountClient("account.invitations.createdLabel")}:{" "}
              {formatDate(invitation.createdAtUtc, tAccountClient("account.common.notAvailable"))}
            </Text>
            <Text className="text-xs text-muted-foreground">
              {tAccountClient("account.invitations.expiresLabel")}:{" "}
              {formatDate(invitation.expiresAtUtc, tAccountClient("account.common.notAvailable"))}
            </Text>
          </section>
        ))
      )}
    </AccountSection>
  );
}
