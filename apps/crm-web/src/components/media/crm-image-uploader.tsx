"use client";

import { useActionState, useRef } from "react";
import type { ReactNode } from "react";
import { useFormStatus } from "react-dom";
import { Trash2, Upload, Camera, Loader2 } from "lucide-react";
import { Button } from "@netmetric/ui";
import { Avatar, AvatarImage, AvatarFallback } from "@netmetric/ui/client";

import { CrmMutationResult } from "@/components/forms/crm-mutation-result";
import {
  initialCrmMutationState,
  type CrmMutationState,
} from "@/features/shared/actions/mutation-state";
import { tCrmClient } from "@/lib/i18n/crm-i18n";

type CrmImageUploaderProps = {
  title: string;
  description: string;
  imageUrl?: string | null | undefined;
  altText: string;
  uploadLabel?: string;
  removeLabel?: string;

  // Direct Server Action Mode (e.g. details pages where actions are immediately saved)
  uploadAction?: (state: CrmMutationState, formData: FormData) => Promise<CrmMutationState>;
  removeAction?: (state: CrmMutationState, formData: FormData) => Promise<CrmMutationState>;

  // Local Form Mode (e.g. create/edit forms where files are stored locally in state first)
  onChange?: (file: File | null) => void;
  onRemove?: () => void;
};

function SubmitButton({
  label,
  variant = "default",
  icon,
}: Readonly<{
  label: string;
  variant?: "default" | "outline";
  icon: ReactNode;
}>) {
  const { pending } = useFormStatus();
  return (
    <Button
      type="submit"
      variant={variant}
      size="sm"
      className="h-8 text-xs gap-1.5"
      disabled={pending}
      aria-busy={pending}
    >
      {pending ? (
        <>
          <Loader2 className="size-3.5 animate-spin" />
          <span>{tCrmClient("crm.forms.actions.processing")}</span>
        </>
      ) : (
        <>
          {icon}
          <span>{label}</span>
        </>
      )}
    </Button>
  );
}

function SubmitButtonTrigger({
  fileInputRef,
  label,
}: Readonly<{
  fileInputRef: React.RefObject<HTMLInputElement | null>;
  label: string;
}>) {
  const { pending } = useFormStatus();
  return (
    <Button
      type="button"
      onClick={() => fileInputRef.current?.click()}
      variant="outline"
      size="sm"
      disabled={pending}
      className="h-8 text-xs gap-1.5 cursor-pointer"
    >
      {pending ? (
        <>
          <Loader2 className="size-3.5 animate-spin" />
          <span>{tCrmClient("crm.forms.actions.processing")}</span>
        </>
      ) : (
        <>
          <Upload className="size-3.5 text-muted-foreground" />
          <span>{label}</span>
        </>
      )}
    </Button>
  );
}

export function CrmImageUploader({
  title,
  description,
  imageUrl,
  altText,
  uploadLabel,
  removeLabel,
  uploadAction,
  removeAction,
  onChange,
  onRemove,
}: Readonly<CrmImageUploaderProps>) {
  const fileInputRef = useRef<HTMLInputElement>(null);
  const uploadFormRef = useRef<HTMLFormElement>(null);

  const defaultUploadLabel = uploadLabel ?? tCrmClient("crm.media.actions.upload");
  const defaultRemoveLabel = removeLabel ?? tCrmClient("crm.media.actions.remove");

  // Hook up useActionState only if Server Actions are provided
  const [uploadState, uploadFormAction] = useActionState(
    uploadAction ?? (async (state) => state),
    initialCrmMutationState,
  );
  const [removeState, removeFormAction] = useActionState(
    removeAction ?? (async (state) => state),
    initialCrmMutationState,
  );

  const isServerActionMode = Boolean(uploadAction && removeAction);

  const handleAvatarClick = () => {
    fileInputRef.current?.click();
  };

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0] ?? null;
    if (isServerActionMode) {
      if (file) {
        uploadFormRef.current?.requestSubmit();
      }
    } else {
      onChange?.(file);
      // Clear file input value to allow choosing the same file again
      if (fileInputRef.current) {
        fileInputRef.current.value = "";
      }
    }
  };

  const handleLocalRemove = () => {
    if (onRemove) {
      onRemove();
    } else {
      onChange?.(null);
    }
    if (fileInputRef.current) {
      fileInputRef.current.value = "";
    }
  };

  return (
    <div className="w-full">
      <div className="flex flex-col sm:flex-row sm:items-center gap-6">
        {/* Profile Image Squircle/Circle container with hover overlay trigger */}
        <div
          onClick={handleAvatarClick}
          className="relative group size-20 rounded-full cursor-pointer overflow-hidden flex items-center justify-center shrink-0 border border-border/40 shadow-xs transition-colors hover:border-primary/50"
        >
          <Avatar className="size-full">
            {imageUrl ? <AvatarImage src={imageUrl} alt={altText} /> : null}
            <AvatarFallback>
              <Camera className="size-7 text-muted-foreground/45 stroke-[1.5]" />
            </AvatarFallback>
          </Avatar>

          {/* Hover Overlay */}
          <div className="absolute inset-0 bg-black/60 opacity-0 group-hover:opacity-100 flex flex-col items-center justify-center transition-opacity duration-200 gap-1 text-white select-none">
            <Upload className="size-3.5 text-white/90" />
            <span className="text-[9px] font-bold tracking-wider uppercase text-white/90">
              Change
            </span>
          </div>
        </div>

        {/* Info & Actions */}
        <div className="space-y-2 flex-1 min-w-0">
          <div>
            <h3 className="text-sm font-semibold text-foreground tracking-tight">{title}</h3>
            <p className="text-xs text-muted-foreground mt-0.5">{description}</p>
          </div>

          <div className="flex flex-wrap items-center gap-2 pt-1">
            {isServerActionMode ? (
              <>
                {/* Hidden upload form for direct/immediate server action */}
                <form ref={uploadFormRef} action={uploadFormAction} className="inline-block">
                  <input
                    ref={fileInputRef}
                    accept="image/png,image/jpeg,image/webp"
                    name="file"
                    type="file"
                    className="hidden"
                    onChange={handleFileChange}
                  />
                  <SubmitButtonTrigger fileInputRef={fileInputRef} label={defaultUploadLabel} />
                </form>

                {/* Remove form for direct/immediate server action */}
                {imageUrl ? (
                  <form action={removeFormAction} className="inline-block">
                    <SubmitButton
                      icon={<Trash2 aria-hidden className="size-3.5 text-destructive" />}
                      label={defaultRemoveLabel}
                      variant="outline"
                    />
                  </form>
                ) : null}
              </>
            ) : (
              <>
                {/* Local uploader */}
                <input
                  ref={fileInputRef}
                  type="file"
                  accept="image/png,image/jpeg,image/webp"
                  className="hidden"
                  onChange={handleFileChange}
                />
                <Button
                  type="button"
                  onClick={handleAvatarClick}
                  variant="outline"
                  size="sm"
                  className="h-8 text-xs gap-1.5 cursor-pointer"
                >
                  <Upload className="size-3.5 text-muted-foreground" />
                  <span>{defaultUploadLabel}</span>
                </Button>

                {imageUrl ? (
                  <Button
                    type="button"
                    onClick={handleLocalRemove}
                    variant="outline"
                    size="sm"
                    className="h-8 text-xs gap-1.5 text-destructive cursor-pointer hover:bg-destructive/10 focus-visible:ring-destructive"
                  >
                    <Trash2 className="size-3.5" />
                    <span>{defaultRemoveLabel}</span>
                  </Button>
                ) : null}
              </>
            )}
          </div>

          {isServerActionMode && (
            <CrmMutationResult state={uploadState.status === "idle" ? removeState : uploadState} />
          )}
        </div>
      </div>
    </div>
  );
}
