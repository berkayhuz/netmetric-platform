import { Badge, Text } from "@netmetric/ui";

import type { AccountMemberResponse } from "@/lib/account-api";
import { tAccountClient } from "@/lib/i18n/account-i18n";
import { AccountSection } from "./account-section";

type MembersReadOnlyPanelProps = {
  members: AccountMemberResponse[];
};

export function MembersReadOnlyPanel({ members }: MembersReadOnlyPanelProps) {
  return (
    <AccountSection
      title={tAccountClient("account.team.membersTitle")}
      description={tAccountClient("account.team.membersDescription")}
      contentClassName="space-y-3"
    >
      {members.length === 0 ? (
        <Text className="text-sm text-muted-foreground">
          {tAccountClient("account.team.noMembers")}
        </Text>
      ) : (
        members.map((member) => (
          <section key={member.userId} className="space-y-2 border-b border-border/70 pb-4">
            <div className="flex flex-wrap items-center justify-between gap-2">
              <Text className="font-medium">{member.userName || member.email}</Text>
              <Badge variant={member.isActive ? "secondary" : "outline"}>
                {member.isActive ? "Active" : "Inactive"}
              </Badge>
            </div>
            <Text className="text-sm text-muted-foreground">{member.email}</Text>
            <div className="flex flex-wrap gap-2">
              {member.roles.length === 0 ? (
                <Text className="text-xs text-muted-foreground">
                  {tAccountClient("account.team.noRolesAssigned")}
                </Text>
              ) : (
                member.roles.map((role) => (
                  <Badge key={role} variant="outline">
                    {role}
                  </Badge>
                ))
              )}
            </div>
          </section>
        ))
      )}
    </AccountSection>
  );
}
