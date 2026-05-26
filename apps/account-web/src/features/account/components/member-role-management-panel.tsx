import {
  Badge,
  Empty,
  EmptyDescription,
  EmptyHeader,
  EmptyTitle,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
  Text,
} from "@netmetric/ui";

import type { AccountMemberResponse, AccountRoleCatalogResponse } from "@/lib/account-api";

import { AccountSection } from "./account-section";
import { MemberRoleUpdateForm } from "./member-role-update-form";
import { tAccountClient } from "@/lib/i18n/account-i18n";

type MemberRoleManagementPanelProps = {
  members: AccountMemberResponse[];
  rolesCatalog: AccountRoleCatalogResponse[];
};

export function MemberRoleManagementPanel({
  members,
  rolesCatalog,
}: MemberRoleManagementPanelProps) {
  return (
    <AccountSection
      title={tAccountClient("account.team.membersTitle")}
      description={tAccountClient("account.team.memberRolesDescription")}
      className="px-4 pt-5"
      contentClassName="overflow-x-auto"
    >
      <Table>
        <TableHeader className="bg-muted/10">
          <TableRow className="hover:bg-transparent">
            <TableHead className="min-w-[260px] text-muted-foreground">Member</TableHead>
            <TableHead className="border-l border-border/50 text-muted-foreground">
              Status
            </TableHead>
            <TableHead className="min-w-[220px] border-l border-border/50 text-muted-foreground">
              Roles
            </TableHead>
            <TableHead className="min-w-[220px] border-l border-border/50 text-muted-foreground">
              Permissions
            </TableHead>
            <TableHead className="min-w-[280px] border-l border-border/50 text-muted-foreground">
              Actions
            </TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {members.length === 0 ? (
            <TableRow className="hover:bg-transparent">
              <TableCell colSpan={5}>
                <Empty className="border-none py-10" role="status" aria-live="polite">
                  <EmptyHeader>
                    <EmptyTitle>{tAccountClient("account.team.noMembers")}</EmptyTitle>
                    <EmptyDescription>
                      {tAccountClient("account.team.memberRolesDescription")}
                    </EmptyDescription>
                  </EmptyHeader>
                </Empty>
              </TableCell>
            </TableRow>
          ) : (
            members.map((member) => (
              <TableRow key={member.userId} className="align-top hover:bg-muted/30">
                <TableCell>
                  <div className="min-w-0 space-y-1">
                    <Text className="truncate text-sm font-medium">
                      {member.userName || member.email}
                    </Text>
                    <Text className="truncate text-xs text-muted-foreground">{member.email}</Text>
                  </div>
                </TableCell>
                <TableCell className="border-l border-border/50">
                  <Badge variant={member.isActive ? "secondary" : "outline"}>
                    {member.isActive
                      ? tAccountClient("account.common.active")
                      : tAccountClient("account.common.inactive")}
                  </Badge>
                </TableCell>
                <TableCell className="border-l border-border/50">
                  <BadgeList
                    values={member.roles}
                    emptyLabel={tAccountClient("account.team.noRolesAssigned")}
                  />
                </TableCell>
                <TableCell className="border-l border-border/50">
                  <BadgeList values={member.permissions} emptyLabel="No permissions" />
                </TableCell>
                <TableCell className="border-l border-border/50">
                  {rolesCatalog.length === 0 ? (
                    <Text className="text-xs text-muted-foreground">
                      {tAccountClient("account.team.roleUpdatesUnavailable")}
                    </Text>
                  ) : (
                    <details className="group">
                      <summary className="cursor-pointer select-none text-sm font-medium text-foreground transition-colors hover:text-primary">
                        {tAccountClient("account.team.updateRoles")}
                      </summary>
                      <div className="mt-3">
                        <MemberRoleUpdateForm member={member} rolesCatalog={rolesCatalog} />
                      </div>
                    </details>
                  )}
                </TableCell>
              </TableRow>
            ))
          )}
        </TableBody>
      </Table>
    </AccountSection>
  );
}

function BadgeList({ values, emptyLabel }: { values: string[]; emptyLabel: string }) {
  if (values.length === 0) {
    return <Text className="text-xs text-muted-foreground">{emptyLabel}</Text>;
  }

  return (
    <div className="flex max-w-xl flex-wrap gap-1.5">
      {values.map((value) => (
        <Badge key={value} variant="outline" className="max-w-48 truncate px-1.5">
          {value}
        </Badge>
      ))}
    </div>
  );
}
