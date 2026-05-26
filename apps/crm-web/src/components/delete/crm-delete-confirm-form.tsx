"use client";

import { useActionState } from "react";
import { useFormStatus } from "react-dom";
import {
  Button,
  Field,
  FieldContent,
  FieldDescription,
  FieldError,
  FieldLabel,
  Input,
} from "@netmetric/ui";
import { tCrmClient } from "@/lib/i18n/crm-i18n";

import {
  initialCrmMutationState,
  type CrmMutationState,
} from "@/features/shared/actions/mutation-state";

import { CrmDeleteActionResult } from "./crm-delete-action-result";

function SubmitButton({
  entityLabel,
  deletingLabel,
  deleteLabel,
}: Readonly<{ entityLabel: string; deletingLabel?: string; deleteLabel?: string }>) {
  const { pending } = useFormStatus();

  return (
    <Button type="submit" variant="destructive" disabled={pending} aria-busy={pending}>
      {pending
        ? (deletingLabel ??
          tCrmClient("crm.delete.deletingEntity", { entity: entityLabel.toLowerCase() }))
        : (deleteLabel ?? tCrmClient("crm.delete.deleteEntity", { entity: entityLabel }))}
    </Button>
  );
}

export function CrmDeleteConfirmForm({
  entityLabel,
  entityName,
  confirmValue,
  action,
  confirmationLabel,
  confirmationHelpText,
  deletingLabel,
  deleteLabel,
}: Readonly<{
  entityLabel: string;
  entityName: string;
  confirmValue: string;
  action: (state: CrmMutationState, formData: FormData) => Promise<CrmMutationState>;
  confirmationLabel?: string;
  confirmationHelpText?: string;
  deletingLabel?: string;
  deleteLabel?: string;
}>) {
  const [state, formAction] = useActionState(action, initialCrmMutationState);

  return (
    <form action={formAction} className="space-y-4" noValidate>
      <input type="hidden" name="confirm" value={confirmValue} />
      <CrmDeleteActionResult state={state} />
      <Field>
        <FieldLabel htmlFor={`delete-confirm-${confirmValue}`}>
          {confirmationLabel ?? tCrmClient("crm.delete.confirmation")}
        </FieldLabel>
        <FieldContent>
          <Input
            id={`delete-confirm-${confirmValue}`}
            name="confirmText"
            defaultValue=""
            placeholder={entityName}
            aria-invalid={Boolean(state.fieldErrors?.confirmText?.[0])}
            aria-describedby={
              state.fieldErrors?.confirmText?.[0]
                ? `delete-confirm-${confirmValue}-error`
                : undefined
            }
          />
          <FieldDescription>
            {confirmationHelpText ?? (
              <>
                {tCrmClient("crm.delete.typeToConfirmPrefix")} <strong>{entityName}</strong>{" "}
                {tCrmClient("crm.delete.typeToConfirmSuffix")}
              </>
            )}
          </FieldDescription>
          <FieldError id={`delete-confirm-${confirmValue}-error`}>
            {state.fieldErrors?.confirmText?.[0]}
          </FieldError>
        </FieldContent>
      </Field>
      <SubmitButton
        entityLabel={entityLabel}
        {...(deletingLabel ? { deletingLabel } : {})}
        {...(deleteLabel ? { deleteLabel } : {})}
      />
    </form>
  );
}
