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

import type { AccountRoleCatalogResponse } from "@/lib/account-api";
import { tAccountClient } from "@/lib/i18n/account-i18n";
import { AccountSection } from "./account-section";

type RolesCatalogPanelProps = {
  rolesCatalog: AccountRoleCatalogResponse[];
};

export function RolesCatalogPanel({ rolesCatalog }: RolesCatalogPanelProps) {
  return (
    <AccountSection
      title={tAccountClient("account.roles.title")}
      description={tAccountClient("account.roles.description")}
      className="px-4 pt-5"
      contentClassName="overflow-x-auto"
    >
      <Table>
        <TableHeader className="bg-muted/10">
          <TableRow className="hover:bg-transparent">
            <TableHead className="min-w-[220px] text-muted-foreground">Role</TableHead>
            <TableHead className="border-l border-border/50 text-muted-foreground">Rank</TableHead>
            <TableHead className="border-l border-border/50 text-muted-foreground">
              Protection
            </TableHead>
            <TableHead className="min-w-[420px] border-l border-border/50 text-muted-foreground">
              Permissions
            </TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {rolesCatalog.length === 0 ? (
            <TableRow className="hover:bg-transparent">
              <TableCell colSpan={4}>
                <Empty className="border-none py-10" role="status" aria-live="polite">
                  <EmptyHeader>
                    <EmptyTitle>{tAccountClient("account.roles.emptyTitle")}</EmptyTitle>
                    <EmptyDescription>
                      {tAccountClient("account.roles.description")}
                    </EmptyDescription>
                  </EmptyHeader>
                </Empty>
              </TableCell>
            </TableRow>
          ) : (
            rolesCatalog.map((role) => (
              <TableRow key={role.name} className="align-top hover:bg-muted/30">
                <TableCell className="font-medium">{role.name}</TableCell>
                <TableCell className="border-l border-border/50">
                  <Badge variant="outline">
                    {tAccountClient("account.roles.rankLabel", { rank: role.rank })}
                  </Badge>
                </TableCell>
                <TableCell className="border-l border-border/50">
                  {role.isProtected ? (
                    <Badge variant="secondary">{tAccountClient("account.roles.protected")}</Badge>
                  ) : (
                    <Text className="text-xs text-muted-foreground">Editable</Text>
                  )}
                </TableCell>
                <TableCell className="border-l border-border/50">
                  {role.permissions.length === 0 ? (
                    <Text className="text-xs text-muted-foreground">
                      {tAccountClient("account.roles.noPermissions")}
                    </Text>
                  ) : (
                    <div className="flex flex-wrap gap-1.5">
                      {role.permissions.map((permission) => (
                        <Badge
                          key={permission}
                          variant="outline"
                          className="max-w-64 truncate px-1.5"
                        >
                          {permission}
                        </Badge>
                      ))}
                    </div>
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
