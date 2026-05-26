"use client";

import { useActionState } from "react";
import { Alert, AlertDescription, AlertTitle, Button, Input } from "@netmetric/ui";

import {
  deleteToolRunAction,
  initialToolHistoryActionState,
} from "@/features/tools/actions/delete-tool-run-action";
import { tTools } from "@/lib/i18n/tools-i18n";

type ToolHistoryDeleteFormProps = {
  runId: string;
  locale?: string | null | undefined;
};

export function ToolHistoryDeleteForm({ runId, locale }: ToolHistoryDeleteFormProps) {
  const [state, formAction, isPending] = useActionState(
    deleteToolRunAction,
    initialToolHistoryActionState,
  );

  return (
    <form action={formAction} className="space-y-3">
      <input type="hidden" name="runId" value={runId} />

      <div className="space-y-2">
        <label htmlFor={`delete-confirm-${runId}`} className="text-sm font-medium">
          {tTools("tools.history.deleteConfirmPrefix", locale)} <code>delete-tool-run</code>{" "}
          {tTools("tools.history.deleteConfirmSuffix", locale)}
        </label>
        <Input id={`delete-confirm-${runId}`} name="confirm" autoComplete="off" required />
      </div>

      <Button type="submit" variant="destructive" disabled={isPending}>
        {isPending
          ? tTools("tools.actions.deleting", locale)
          : tTools("tools.actions.deleteRun", locale)}
      </Button>

      {state.status !== "idle" ? (
        <Alert role="status" aria-live="polite">
          <AlertTitle>
            {state.status === "success"
              ? tTools("tools.history.deletedTitle", locale)
              : tTools("tools.history.deleteFailedTitle", locale)}
          </AlertTitle>
          <AlertDescription>{state.message}</AlertDescription>
        </Alert>
      ) : null}
    </form>
  );
}
