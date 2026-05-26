"use client";

import { useActionState } from "react";
import { useFormStatus } from "react-dom";
import { Badge, Button, Text } from "@netmetric/ui";
import type { AccountDateSettings } from "@/lib/account-date";
import { formatAccountDateTime } from "@/lib/account-date";

import type { AccountNotificationResponse } from "@/lib/account-api/account-api-types";

import {
  deleteNotificationAction,
  markNotificationAsReadAction,
} from "../actions/notification-actions";
import { initialMutationState } from "../actions/mutation-state";
import { SecurityActionResult } from "./security-action-result";
import { tAccountClient } from "@/lib/i18n/account-i18n";

type NotificationItemProps = {
  item: AccountNotificationResponse;
  dateSettings: AccountDateSettings;
};

function ActionButton({
  idleLabel,
  pendingLabel,
  variant = "outline",
}: {
  idleLabel: string;
  pendingLabel: string;
  variant?: "outline" | "destructive";
}) {
  const { pending } = useFormStatus();
  return (
    <Button type="submit" size="xs" variant={variant} disabled={pending}>
      {pending ? pendingLabel : idleLabel}
    </Button>
  );
}

export function NotificationItem({ item, dateSettings }: NotificationItemProps) {
  const [readState, readAction] = useActionState(
    markNotificationAsReadAction,
    initialMutationState,
  );
  const [deleteState, deleteAction] = useActionState(
    deleteNotificationAction,
    initialMutationState,
  );

  return (
    <section className="space-y-3 border-b border-border/70 pb-4">
      <div className="flex flex-wrap items-center gap-2">
        <Badge variant={item.isRead ? "outline" : "secondary"}>
          {item.isRead ? "Read" : "Unread"}
        </Badge>
        <Badge variant="outline">{item.category}</Badge>
        <Badge variant="outline">{item.severity}</Badge>
      </div>
      <Text className="text-base font-semibold text-foreground">{item.title}</Text>
      <div className="space-y-4">
        <Text className="text-sm">{item.description}</Text>
        <Text className="text-xs text-muted-foreground">
          Occurred: {formatAccountDateTime(item.occurredAt, dateSettings)}
        </Text>

        {readState.status !== "idle" ? (
          <SecurityActionResult
            state={readState}
            successTitle={tAccountClient("account.notifications.updatedOne")}
            errorTitle={tAccountClient("account.notifications.readUpdateFailed")}
          />
        ) : null}
        {deleteState.status !== "idle" ? (
          <SecurityActionResult
            state={deleteState}
            successTitle={tAccountClient("account.notifications.removed")}
            errorTitle={tAccountClient("account.common.removeFailed")}
          />
        ) : null}

        <div className="flex flex-wrap gap-2">
          {!item.isRead ? (
            <form action={readAction}>
              <input type="hidden" name="notificationId" value={item.id} />
              <ActionButton
                idleLabel={tAccountClient("account.notifications.markRead")}
                pendingLabel={tAccountClient("account.common.marking")}
              />
            </form>
          ) : null}
          <form action={deleteAction}>
            <input type="hidden" name="notificationId" value={item.id} />
            <input type="hidden" name="confirm" value="delete-notification" />
            <ActionButton
              idleLabel={tAccountClient("account.common.remove")}
              pendingLabel={tAccountClient("account.common.removing")}
              variant="destructive"
            />
          </form>
        </div>
      </div>
    </section>
  );
}
