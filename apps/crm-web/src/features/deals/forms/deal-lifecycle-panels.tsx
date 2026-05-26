"use client";

import { useState } from "react";
import { useActionState } from "react";
import { useFormStatus } from "react-dom";
import {
  Button,
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
  Field,
  FieldContent,
  FieldError,
  FieldLabel,
  Input,
  Textarea,
} from "@netmetric/ui";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@netmetric/ui/client";

import { CrmMutationResult } from "@/components/forms/crm-mutation-result";
import {
  initialCrmMutationState,
  type CrmMutationState,
} from "@/features/shared/actions/mutation-state";
import { getSelectDisplayLabel } from "@/features/shared/forms/select-display";
import { tCrmClient } from "@/lib/i18n/crm-i18n";
import type { DealLostReasonDto } from "@/lib/crm-api";

function SubmitButton({ label, pendingLabel }: Readonly<{ label: string; pendingLabel: string }>) {
  const { pending } = useFormStatus();

  return (
    <Button type="submit" variant="outline" disabled={pending} aria-busy={pending}>
      {pending ? pendingLabel : label}
    </Button>
  );
}

export function DealOwnerActionPanel({
  dealId,
  ownerUserId,
  action,
}: Readonly<{
  dealId: string;
  ownerUserId?: string | null;
  action: (state: CrmMutationState, formData: FormData) => Promise<CrmMutationState>;
}>) {
  const [state, formAction] = useActionState(action, initialCrmMutationState);

  return (
    <Card>
      <CardHeader>
        <CardTitle>{tCrmClient("crm.deals.lifecycle.ownerTitle")}</CardTitle>
        <CardDescription>{tCrmClient("crm.deals.lifecycle.ownerDescription")}</CardDescription>
      </CardHeader>
      <CardContent className="space-y-4">
        <CrmMutationResult state={state} />
        <form action={formAction} className="space-y-3">
          <Field>
            <FieldLabel htmlFor={`deal-owner-${dealId}`}>
              {tCrmClient("crm.deals.fields.ownerUserId")}
            </FieldLabel>
            <FieldContent>
              <Input
                id={`deal-owner-${dealId}`}
                name="ownerUserId"
                defaultValue={ownerUserId ?? ""}
              />
              <FieldError>{state.fieldErrors?.ownerUserId?.[0]}</FieldError>
            </FieldContent>
          </Field>
          <SubmitButton
            label={tCrmClient("crm.deals.actions.changeOwner")}
            pendingLabel={tCrmClient("crm.deals.actions.updatingOwner")}
          />
        </form>
      </CardContent>
    </Card>
  );
}

export function DealLifecycleActionPanel({
  title,
  description,
  confirmValue,
  action,
  showLostReason,
  lostReasons,
  rowVersion,
}: Readonly<{
  title: string;
  description: string;
  confirmValue: "mark-deal-won" | "mark-deal-lost" | "reopen-deal";
  action: (state: CrmMutationState, formData: FormData) => Promise<CrmMutationState>;
  showLostReason?: boolean;
  lostReasons?: DealLostReasonDto[];
  rowVersion?: string;
}>) {
  const [state, formAction] = useActionState(action, initialCrmMutationState);
  const lostReasonNone = "__none__";
  const [lostReasonId, setLostReasonId] = useState(lostReasonNone);
  const lostReasonDisplayOptions = [
    { value: lostReasonNone, label: tCrmClient("crm.deals.lifecycle.selectLostReason") },
    ...(lostReasons ?? []).map((reason) => ({ value: reason.id, label: reason.name })),
  ];

  return (
    <Card className="border-destructive/30">
      <CardHeader>
        <CardTitle>{title}</CardTitle>
        <CardDescription>{description}</CardDescription>
      </CardHeader>
      <CardContent className="space-y-4">
        <CrmMutationResult state={state} />
        <form action={formAction} className="space-y-3">
          <input type="hidden" name="confirm" value={confirmValue} />
          <input type="hidden" name="rowVersion" value={rowVersion ?? ""} />
          <Field>
            <FieldLabel htmlFor={`${confirmValue}-occurredAt`}>
              {tCrmClient("crm.deals.lifecycle.occurredAt")}
            </FieldLabel>
            <FieldContent>
              <Input id={`${confirmValue}-occurredAt`} name="occurredAt" type="datetime-local" />
              <FieldError>{state.fieldErrors?.occurredAt?.[0]}</FieldError>
            </FieldContent>
          </Field>
          {showLostReason ? (
            <Field>
              <FieldLabel htmlFor={`${confirmValue}-lostReasonId`}>
                {tCrmClient("crm.deals.fields.lostReasonId")}
              </FieldLabel>
              <FieldContent>
                <input
                  type="hidden"
                  name="lostReasonId"
                  value={lostReasonId === lostReasonNone ? "" : lostReasonId}
                />
                <Select
                  value={lostReasonId}
                  onValueChange={(value) => setLostReasonId(value ?? lostReasonNone)}
                >
                  <SelectTrigger id={`${confirmValue}-lostReasonId`}>
                    <SelectValue placeholder={tCrmClient("crm.deals.lifecycle.selectLostReason")}>
                      {getSelectDisplayLabel(lostReasonId, lostReasonDisplayOptions)}
                    </SelectValue>
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value={lostReasonNone}>
                      {tCrmClient("crm.deals.lifecycle.selectLostReason")}
                    </SelectItem>
                    {(lostReasons ?? []).map((reason) => (
                      <SelectItem key={reason.id} value={reason.id}>
                        {reason.name}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                <FieldError>{state.fieldErrors?.lostReasonId?.[0]}</FieldError>
              </FieldContent>
            </Field>
          ) : null}
          <Field>
            <FieldLabel htmlFor={`${confirmValue}-note`}>
              {tCrmClient("crm.deals.fields.note")}
            </FieldLabel>
            <FieldContent>
              <Textarea id={`${confirmValue}-note`} name="note" rows={3} />
              <FieldError>{state.fieldErrors?.note?.[0]}</FieldError>
            </FieldContent>
          </Field>
          <SubmitButton label={title} pendingLabel={tCrmClient("crm.deals.actions.processing")} />
        </form>
      </CardContent>
    </Card>
  );
}
