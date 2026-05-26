"use client";

import Image from "next/image";
import { useActionState, useEffect, useRef } from "react";
import { useFormStatus } from "react-dom";
import { ImageUp, Trash2, Upload } from "lucide-react";
import { Button, FieldError, Input, TextTitle, cn } from "@netmetric/ui";
import { toast } from "@netmetric/ui/client";

import type { MyProfileResponse } from "@/lib/account-api";
import { normalizeMediaUrl, shouldUseUnoptimizedAvatar } from "@/lib/media/avatar-media";

import { removeAvatarAction, uploadAvatarAction } from "../actions/profile-actions";
import { initialMutationState } from "../actions/mutation-state";
import { tAccountClient } from "@/lib/i18n/account-i18n";

type AvatarManagementPanelProps = {
  profile: MyProfileResponse;
  className?: string;
};

const clientAvatarUploadLimitBytes = 1024 * 1024;

function UploadButton({ onClick }: { onClick: () => void }) {
  const { pending } = useFormStatus();
  return (
    <Button type="button" variant="outline" size="xs" disabled={pending} onClick={onClick}>
      <Upload />
      {pending
        ? tAccountClient("account.profile.avatar.uploading")
        : tAccountClient("account.profile.avatar.upload")}
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
        : tAccountClient("account.profile.avatar.delete")}
    </Button>
  );
}

export function AvatarManagementPanel({ profile, className }: AvatarManagementPanelProps) {
  const [uploadState, uploadFormAction] = useActionState(uploadAvatarAction, initialMutationState);
  const [deleteState, deleteFormAction] = useActionState(removeAvatarAction, initialMutationState);
  const uploadFormRef = useRef<HTMLFormElement | null>(null);
  const fileInputRef = useRef<HTMLInputElement | null>(null);
  const lastUploadToastKeyRef = useRef<string | null>(null);
  const lastDeleteToastKeyRef = useRef<string | null>(null);
  const normalizedAvatarUrl = normalizeMediaUrl(profile.avatarUrl);

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

    if (nextFile.size > clientAvatarUploadLimitBytes) {
      toast.error(tAccountClient("account.profile.avatar.uploadFailed"), {
        description: "Please choose an image under 1 MB.",
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
      toast.success(tAccountClient("account.profile.avatar.updated"), {
        description: toastMessage || undefined,
      });
      return;
    }

    toast.error(tAccountClient("account.profile.avatar.uploadFailed"), {
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
      toast.success(tAccountClient("account.profile.avatar.removed"), {
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
        {tAccountClient("account.profile.avatar.managementTitle")}
      </TextTitle>
      <div className="flex items-start gap-4">
        <button
          type="button"
          onClick={openFilePicker}
          aria-label={tAccountClient("account.profile.avatar.fileLabel")}
          className={cn(
            "flex h-20 w-20 shrink-0 items-center justify-center overflow-hidden rounded-sm border border-input bg-muted/50 transition-colors",
            "hover:bg-muted focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring/50",
          )}
        >
          {normalizedAvatarUrl ? (
            <Image
              src={normalizedAvatarUrl}
              alt={`${profile.displayName} avatar`}
              width={80}
              height={80}
              unoptimized={shouldUseUnoptimizedAvatar(normalizedAvatarUrl)}
              className="h-full w-full object-cover"
            />
          ) : (
            <ImageUp className="text-muted-foreground" />
          )}
        </button>

        <div className="min-w-0 flex-1 space-y-2">
          <div className="flex flex-col md:flex-row gap-2 items-center">
            <form ref={uploadFormRef} action={uploadFormAction} className="contents" noValidate>
              <Input
                ref={fileInputRef}
                id="avatarFile"
                name="avatarFile"
                type="file"
                accept="image/png,image/jpeg,image/webp"
                onChange={handleFileChange}
                aria-invalid={Boolean(uploadState.fieldErrors?.avatarFile?.[0])}
                aria-describedby={
                  uploadState.fieldErrors?.avatarFile?.[0] ? "avatarFile-error" : undefined
                }
                className="sr-only"
              />
              <div className="flex flex-wrap items-center gap-2">
                <UploadButton onClick={openFilePicker} />
              </div>
            </form>

            <form action={deleteFormAction} className="contents">
              <input type="hidden" name="confirm" value="delete-avatar" />
              <div className="flex flex-wrap items-center gap-2">
                <DeleteButton disabled={!normalizedAvatarUrl} />
              </div>
            </form>
          </div>
          <FieldError id="avatarFile-error">{uploadState.fieldErrors?.avatarFile?.[0]}</FieldError>
        </div>
      </div>
    </section>
  );
}
