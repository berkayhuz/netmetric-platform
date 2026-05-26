"use client";

import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@netmetric/ui/client";

export function CrmBulkDeleteConfirmDialog({
  open,
  pending = false,
  title = "Delete selected records?",
  description,
  confirmLabel = "Delete",
  confirmVariant = "destructive",
  onOpenChange,
  onConfirm,
}: Readonly<{
  open: boolean;
  pending?: boolean;
  title?: string;
  description: string;
  confirmLabel?: string;
  confirmVariant?: "default" | "destructive";
  onOpenChange: (open: boolean) => void;
  onConfirm: () => void;
}>) {
  return (
    <AlertDialog
      open={open}
      onOpenChange={(nextOpen) => {
        if (pending) {
          return;
        }
        onOpenChange(nextOpen);
      }}
    >
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle>{title}</AlertDialogTitle>
          <AlertDialogDescription>{description}</AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogCancel size="sm" disabled={pending}>
            Cancel
          </AlertDialogCancel>
          <AlertDialogAction
            size="sm"
            variant={confirmVariant}
            disabled={pending}
            onClick={(event) => {
              event.preventDefault();
              onConfirm();
            }}
          >
            {confirmLabel}
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}
