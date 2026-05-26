"use client";

import { useActionState, useEffect, useRef } from "react";
import { useFormStatus } from "react-dom";
import { FileImage, Trash2, Upload } from "lucide-react";
import { Button, FieldError, Input, TextTitle, cn } from "@netmetric/ui";
import { toast } from "@netmetric/ui/client";

import type { UserPreferenceResponse } from "@/lib/account-api";
import { tAccountClient } from "@/lib/i18n/account-i18n";

import { initialMutationState } from "../actions/mutation-state";
import { removeFaviconAction, uploadFaviconAction } from "../actions/preferences-actions";

type FaviconManagementPanelProps = {
  preferences: UserPreferenceResponse;
  className?: string;
};

const clientFaviconUploadLimitBytes = 1024 * 1024;

function UploadButton({ onClick }: { onClick: () => void }) {
  const { pending } = useFormStatus();
  return (
    <Button type="button" variant="outline" size="xs" disabled={pending} onClick={onClick}>
      <Upload />
      {pending
        ? tAccountClient("account.preferences.favicon.uploading")
        : tAccountClient("account.preferences.favicon.upload")}
    </Button>
  );
}

function DeleteButton({ disabled }: { disabled: boolean }) {
  const { pending } = useFormStatus();
  return (
    <Button type="submit" variant="outline" size="xs" disabled={disabled || pending}>
      <Trash2 />
      {pending
        ? tAccountClient("account.common.removing")
        : tAccountClient("account.preferences.favicon.delete")}
    </Button>
  );
}

export function FaviconManagementPanel({ preferences, className }: FaviconManagementPanelProps) {
  const [uploadState, uploadFormAction] = useActionState(uploadFaviconAction, initialMutationState);
  const [deleteState, deleteFormAction] = useActionState(removeFaviconAction, initialMutationState);
  const uploadFormRef = useRef<HTMLFormElement | null>(null);
  const fileInputRef = useRef<HTMLInputElement | null>(null);
  const lastUploadToastKeyRef = useRef<string | null>(null);
  const lastDeleteToastKeyRef = useRef<string | null>(null);
  const faviconUrl = preferences.faviconUrl?.trim() || null;

  function openFilePicker() {
    fileInputRef.current?.click();
  }

  function handleFileChange() {
    if (!fileInputRef.current?.files || fileInputRef.current.files.length === 0) {
      return;
    }

    const nextFile = fileInputRef.current.files[0];
    if (!nextFile) {
      return;
    }

    if (nextFile.size > clientFaviconUploadLimitBytes) {
      toast.error(tAccountClient("account.preferences.favicon.uploadFailed"), {
        description: "Please choose a favicon under 1 MB.",
      });
      fileInputRef.current.value = "";
      return;
    }

    uploadFormRef.current?.requestSubmit();
  }

  useEffect(() => {
    if (uploadState.status !== "success" && uploadState.status !== "error") {
      return;
    }

    const toastMessage = uploadState.message ?? "";
    const nextToastKey = `${uploadState.status}:${toastMessage}`;
    if (nextToastKey === lastUploadToastKeyRef.current) {
      return;
    }

    lastUploadToastKeyRef.current = nextToastKey;
    if (uploadState.status === "success") {
      toast.success(tAccountClient("account.preferences.favicon.updated"), {
        description: toastMessage || undefined,
      });
      return;
    }

    toast.error(tAccountClient("account.preferences.favicon.uploadFailed"), {
      description: toastMessage || undefined,
    });
  }, [uploadState.status, uploadState.message]);

  useEffect(() => {
    if (deleteState.status !== "success" && deleteState.status !== "error") {
      return;
    }

    const toastMessage = deleteState.message ?? "";
    const nextToastKey = `${deleteState.status}:${toastMessage}`;
    if (nextToastKey === lastDeleteToastKeyRef.current) {
      return;
    }

    lastDeleteToastKeyRef.current = nextToastKey;
    if (deleteState.status === "success") {
      toast.success(tAccountClient("account.preferences.favicon.removed"), {
        description: toastMessage || undefined,
      });
      return;
    }

    toast.error(tAccountClient("account.common.deleteFailed"), {
      description: toastMessage || undefined,
    });
  }, [deleteState.status, deleteState.message]);

  return (
    <section className={cn("space-y-2", className)}>
      <TextTitle className="text-sm font-semibold">
        {tAccountClient("account.preferences.favicon.managementTitle")}
      </TextTitle>
      <div className="flex items-start gap-4">
        <button
          type="button"
          onClick={openFilePicker}
          aria-label={tAccountClient("account.preferences.favicon.fileLabel")}
          className={cn(
            "flex h-16 w-16 shrink-0 items-center justify-center overflow-hidden rounded-sm border border-input bg-muted/50 transition-colors",
            "hover:bg-muted focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring/50",
          )}
        >
          {faviconUrl ? (
            // eslint-disable-next-line @next/next/no-img-element
            <img
              src={faviconUrl}
              alt={tAccountClient("account.preferences.favicon.current")}
              className="h-10 w-10 object-contain"
            />
          ) : (
            <FileImage className="text-muted-foreground" />
          )}
        </button>

        <div className="min-w-0 flex-1 space-y-2">
          <div className="flex flex-col items-center gap-2 md:flex-row">
            <form ref={uploadFormRef} action={uploadFormAction} className="contents" noValidate>
              <Input
                ref={fileInputRef}
                id="faviconFile"
                name="faviconFile"
                type="file"
                accept="image/x-icon,image/vnd.microsoft.icon,image/png,image/svg+xml,.ico,.png,.svg"
                onChange={handleFileChange}
                aria-invalid={Boolean(uploadState.fieldErrors?.faviconFile?.[0])}
                aria-describedby={
                  uploadState.fieldErrors?.faviconFile?.[0] ? "faviconFile-error" : undefined
                }
                className="sr-only"
              />
              <div className="flex flex-wrap items-center gap-2">
                <UploadButton onClick={openFilePicker} />
              </div>
            </form>

            <form action={deleteFormAction} className="contents">
              <input type="hidden" name="confirm" value="delete-favicon" />
              <div className="flex flex-wrap items-center gap-2">
                <DeleteButton disabled={!faviconUrl} />
              </div>
            </form>
          </div>
          <TextTitle className="text-xs text-muted-foreground">
            {tAccountClient("account.preferences.favicon.help")}
          </TextTitle>
          <FieldError id="faviconFile-error">
            {uploadState.fieldErrors?.faviconFile?.[0]}
          </FieldError>
        </div>
      </div>
    </section>
  );
}
