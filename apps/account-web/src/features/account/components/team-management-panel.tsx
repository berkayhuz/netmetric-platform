import type React from "react";
import { Badge, Text } from "@netmetric/ui";
import { Mail, ShieldCheck, UserRoundCheck, Users } from "lucide-react";

import type { TeamReadData } from "@/features/account/data/team-data";
import { tAccountClient } from "@/lib/i18n/account-i18n";

import { AccountPagePanel } from "./account-page-panel";
import { InvitationManagementPanel } from "./invitation-management-panel";
import { MemberRoleManagementPanel } from "./member-role-management-panel";
import { RolesCatalogPanel } from "./roles-catalog-panel";

type TeamManagementPanelProps = {
  teamData: TeamReadData;
};

export function TeamManagementPanel({ teamData }: TeamManagementPanelProps) {
  const activeMembers = teamData.members.filter((member) => member.isActive).length;
  const protectedRoles = teamData.rolesCatalog.filter((role) => role.isProtected).length;
  const pendingInvitations = teamData.invitations.filter((invitation) =>
    ["pending", "sent"].includes(invitation.status.trim().toLowerCase()),
  ).length;

  return (
    <AccountPagePanel
      title={tAccountClient("account.team.managementTitle")}
      description={tAccountClient("account.team.managementDescription")}
      contentClassName="space-y-0"
      bodyPadding="none"
    >
      <section className="grid gap-0 border-b border-border/70 sm:grid-cols-2 xl:grid-cols-4">
        <Metric
          icon={<Users aria-hidden="true" className="size-4" />}
          label={tAccountClient("account.team.membersLabel")}
          value={teamData.members.length}
          note={`${activeMembers} ${tAccountClient("account.common.active").toLowerCase()}`}
        />
        <Metric
          icon={<ShieldCheck aria-hidden="true" className="size-4" />}
          label={tAccountClient("account.team.rolesLabel")}
          value={teamData.rolesCatalog.length}
          note={`${protectedRoles} ${tAccountClient("account.roles.protected").toLowerCase()}`}
        />
        <Metric
          icon={<Mail aria-hidden="true" className="size-4" />}
          label={tAccountClient("account.team.invitationsLabel")}
          value={teamData.invitations.length}
          note={`${pendingInvitations} pending`}
        />
        <Metric
          icon={<UserRoundCheck aria-hidden="true" className="size-4" />}
          label="Permissions"
          value={new Set(teamData.rolesCatalog.flatMap((role) => role.permissions)).size}
          note={tAccountClient("account.team.authorizationNote")}
        />
      </section>

      <MemberRoleManagementPanel members={teamData.members} rolesCatalog={teamData.rolesCatalog} />
      <RolesCatalogPanel rolesCatalog={teamData.rolesCatalog} />
      <InvitationManagementPanel invitations={teamData.invitations} />
    </AccountPagePanel>
  );
}

function Metric({
  icon,
  label,
  value,
  note,
}: {
  icon: React.ReactNode;
  label: string;
  value: number;
  note: string;
}) {
  return (
    <div className="min-w-0 border-r border-border/70 px-4 py-3 last:border-r-0">
      <div className="flex items-center justify-between gap-3">
        <div className="flex min-w-0 items-center gap-2">
          <span className="grid size-7 shrink-0 place-items-center rounded-sm bg-muted text-muted-foreground">
            {icon}
          </span>
          <Text className="truncate text-sm text-muted-foreground">{label}</Text>
        </div>
        <Text className="text-lg font-semibold text-foreground">{value}</Text>
      </div>
      <Badge
        variant="outline"
        className="mt-3 max-w-full truncate border-transparent bg-muted px-1.5"
      >
        {note}
      </Badge>
    </div>
  );
}
