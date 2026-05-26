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

import type { AccountInvitationSummaryResponse } from "@/lib/account-api";
import { tAccountClient } from "@/lib/i18n/account-i18n";

import { resendInvitationAction, revokeInvitationAction } from "../actions/invitation-actions";
import { AccountSection } from "./account-section";
import { InvitationConfirmActionForm } from "./invitation-confirm-action-form";
import { InvitationCreateForm } from "./invitation-create-form";

type InvitationManagementPanelProps = {
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

function isPendingStatus(status: string): boolean {
  const normalized = status.trim().toLowerCase();
  return normalized === "pending" || normalized === "sent";
}

export function InvitationManagementPanel({ invitations }: InvitationManagementPanelProps) {
  return (
    <AccountSection
      title={tAccountClient("account.invitations.title")}
      description={tAccountClient("account.invitations.manageDescription")}
      className="border-b-0 px-4 pt-5"
      contentClassName="space-y-4"
    >
      <InvitationCreateForm />

      <div className="overflow-x-auto">
        <Table>
          <TableHeader className="bg-muted/10">
            <TableRow className="hover:bg-transparent">
              <TableHead className="min-w-[260px] text-muted-foreground">Invitation</TableHead>
              <TableHead className="border-l border-border/50 text-muted-foreground">
                Status
              </TableHead>
              <TableHead className="min-w-[190px] border-l border-border/50 text-muted-foreground">
                {tAccountClient("account.invitations.createdLabel")}
              </TableHead>
              <TableHead className="min-w-[190px] border-l border-border/50 text-muted-foreground">
                {tAccountClient("account.invitations.expiresLabel")}
              </TableHead>
              <TableHead className="min-w-[260px] border-l border-border/50 text-muted-foreground">
                Actions
              </TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {invitations.length === 0 ? (
              <TableRow className="hover:bg-transparent">
                <TableCell colSpan={5}>
                  <Empty className="border-none py-10" role="status" aria-live="polite">
                    <EmptyHeader>
                      <EmptyTitle>{tAccountClient("account.invitations.emptyTitle")}</EmptyTitle>
                      <EmptyDescription>
                        {tAccountClient("account.invitations.manageDescription")}
                      </EmptyDescription>
                    </EmptyHeader>
                  </Empty>
                </TableCell>
              </TableRow>
            ) : (
              invitations.map((invitation) => (
                <TableRow key={invitation.invitationId} className="align-top hover:bg-muted/30">
                  <TableCell>
                    <div className="min-w-0 space-y-1">
                      <Text className="truncate text-sm font-medium">{invitation.email}</Text>
                      <Text className="truncate text-xs text-muted-foreground">
                        {[invitation.firstName, invitation.lastName].filter(Boolean).join(" ") ||
                          tAccountClient("account.common.notAvailable")}
                      </Text>
                    </div>
                  </TableCell>
                  <TableCell className="border-l border-border/50">
                    <Badge variant={isPendingStatus(invitation.status) ? "secondary" : "outline"}>
                      {invitation.status}
                    </Badge>
                  </TableCell>
                  <TableCell className="border-l border-border/50 text-sm">
                    {formatDate(
                      invitation.createdAtUtc,
                      tAccountClient("account.common.notAvailable"),
                    )}
                  </TableCell>
                  <TableCell className="border-l border-border/50 text-sm">
                    {formatDate(
                      invitation.expiresAtUtc,
                      tAccountClient("account.common.notAvailable"),
                    )}
                  </TableCell>
                  <TableCell className="border-l border-border/50">
                    {isPendingStatus(invitation.status) ? (
                      <div className="grid gap-2 xl:grid-cols-2">
                        <InvitationConfirmActionForm
                          invitationId={invitation.invitationId}
                          confirmValue="resend-invitation"
                          label={tAccountClient("account.invitations.resend")}
                          pendingLabel={tAccountClient("account.invitations.resending")}
                          variant="outline"
                          successTitle={tAccountClient("account.invitations.resent")}
                          errorTitle={tAccountClient("account.invitations.resendFailed")}
                          action={resendInvitationAction}
                        />
                        <InvitationConfirmActionForm
                          invitationId={invitation.invitationId}
                          confirmValue="revoke-invitation"
                          label={tAccountClient("account.invitations.revoke")}
                          pendingLabel={tAccountClient("account.common.revoking")}
                          variant="destructive"
                          successTitle={tAccountClient("account.invitations.revoked")}
                          errorTitle={tAccountClient("account.invitations.revokeFailed")}
                          action={revokeInvitationAction}
                        />
                      </div>
                    ) : (
                      <Text className="text-xs text-muted-foreground">
                        {tAccountClient("account.invitations.actionsUnavailable")}
                      </Text>
                    )}
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </div>
    </AccountSection>
  );
}
