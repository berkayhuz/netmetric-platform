"use client";

import Link from "next/link";
import { useState, useTransition } from "react";
import { Alert, AlertDescription, AlertTitle, Button } from "@netmetric/ui";

import {
  initialToolHistoryActionState,
  saveToHistoryAction,
} from "@/features/tools/actions/save-to-history-action";
import type { ToolHistoryActionState } from "@/features/tools/actions/tool-history-action-state";
import { tTools } from "@/lib/i18n/tools-i18n";
import { toolsEnv } from "@/lib/tools-env";

type SavePayload = {
  outputFile: File;
  inputSummaryJson: string;
};

type SaveToHistoryPanelProps = {
  toolSlug: "qr-generator" | "png-to-jpg" | "jpg-to-png";
  isAuthenticated: boolean;
  canSave: boolean;
  locale?: string | null | undefined;
  getPayload: () => Promise<SavePayload | null>;
};

export function SaveToHistoryPanel({
  toolSlug,
  isAuthenticated,
  canSave,
  locale,
  getPayload,
}: SaveToHistoryPanelProps) {
  const [isPending, startTransition] = useTransition();
  const [state, setState] = useState<ToolHistoryActionState>(initialToolHistoryActionState);

  function onSaveClick(): void {
    startTransition(() => {
      void (async () => {
        const payload = await getPayload();
        if (!payload) {
          setState({
            status: "error",
            message: tTools("tools.history.generateBeforeSaving", locale),
          });
          return;
        }

        const formData = new FormData();
        formData.set("toolSlug", toolSlug);
        formData.set("inputSummaryJson", payload.inputSummaryJson);
        formData.set("outputFile", payload.outputFile, payload.outputFile.name);

        const result = await saveToHistoryAction(state, formData);
        setState(result);
      })();
    });
  }

  if (!isAuthenticated) {
    return (
      <Alert className="mt-6">
        <AlertTitle>{tTools("tools.history.signInTitle", locale)}</AlertTitle>
        <AlertDescription>
          {tTools("tools.history.signInDescription", locale)}
          <span className="mt-3 block">
            <Button asChild size="sm">
              <Link href={toolsEnv.authUrl}>{tTools("tools.actions.signIn", locale)}</Link>
            </Button>
          </span>
        </AlertDescription>
      </Alert>
    );
  }

  return (
    <div className="mt-6 space-y-3">
      <Button type="button" onClick={onSaveClick} disabled={!canSave || isPending}>
        {isPending
          ? tTools("tools.actions.saving", locale)
          : tTools("tools.actions.saveToHistory", locale)}
      </Button>

      {state.status !== "idle" ? (
        <Alert role="status" aria-live="polite">
          <AlertTitle>
            {state.status === "success"
              ? tTools("tools.history.savedTitle", locale)
              : tTools("tools.history.saveFailedTitle", locale)}
          </AlertTitle>
          <AlertDescription>
            {state.message}
            {state.status === "success" && state.runId ? (
              <span className="mt-2 block">
                <Link href={`/history/${state.runId}`} className="underline underline-offset-4">
                  {tTools("tools.history.viewSavedRun", locale)}
                </Link>
              </span>
            ) : null}
          </AlertDescription>
        </Alert>
      ) : null}
    </div>
  );
}
