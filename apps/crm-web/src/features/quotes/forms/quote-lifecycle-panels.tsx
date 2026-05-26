"use client";

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

import { CrmMutationResult } from "@/components/forms/crm-mutation-result";
import {
  initialCrmMutationState,
  type CrmMutationState,
} from "@/features/shared/actions/mutation-state";
import { tCrmClient } from "@/lib/i18n/crm-i18n";

function SubmitButton({ label, pendingLabel }: Readonly<{ label: string; pendingLabel: string }>) {
  const { pending } = useFormStatus();

  return (
    <Button type="submit" variant="outline" disabled={pending} aria-busy={pending}>
      {pending ? pendingLabel : label}
    </Button>
  );
}

export function QuoteLifecycleActionPanel({
  title,
  description,
  confirmValue,
  action,
  rowVersion,
  showReason,
  showDate,
}: Readonly<{
  title: string;
  description: string;
  confirmValue:
    | "submit-quote"
    | "approve-quote"
    | "reject-quote"
    | "send-quote"
    | "accept-quote"
    | "decline-quote"
    | "expire-quote"
    | "revise-quote";
  action: (state: CrmMutationState, formData: FormData) => Promise<CrmMutationState>;
  rowVersion?: string;
  showReason?: boolean;
  showDate?: boolean;
}>) {
  const [state, formAction] = useActionState(action, initialCrmMutationState);

  return (
    <Card className="border-border/80">
      <CardHeader>
        <CardTitle>{title}</CardTitle>
        <CardDescription>{description}</CardDescription>
      </CardHeader>
      <CardContent className="space-y-4">
        <CrmMutationResult state={state} />
        <form action={formAction} className="space-y-3">
          <input type="hidden" name="confirm" value={confirmValue} />
          <input type="hidden" name="rowVersion" value={rowVersion ?? ""} />

          {confirmValue === "revise-quote" ? (
            <Field>
              <FieldLabel htmlFor={`${confirmValue}-newQuoteNumber`}>
                {tCrmClient("crm.quotes.fields.newQuoteNumber")}
              </FieldLabel>
              <FieldContent>
                <Input id={`${confirmValue}-newQuoteNumber`} name="newQuoteNumber" />
                <FieldError>{state.fieldErrors?.newQuoteNumber?.[0]}</FieldError>
              </FieldContent>
            </Field>
          ) : null}

          {showDate ? (
            <Field>
              <FieldLabel htmlFor={`${confirmValue}-at`}>
                {tCrmClient("crm.quotes.fields.occurredOn")}
              </FieldLabel>
              <FieldContent>
                <Input id={`${confirmValue}-at`} name="at" type="date" />
                <FieldError>{state.fieldErrors?.at?.[0]}</FieldError>
              </FieldContent>
            </Field>
          ) : null}

          {showReason ? (
            <Field>
              <FieldLabel htmlFor={`${confirmValue}-reason`}>
                {tCrmClient("crm.quotes.fields.reason")}
              </FieldLabel>
              <FieldContent>
                <Textarea id={`${confirmValue}-reason`} name="reason" rows={3} />
                <FieldError>{state.fieldErrors?.reason?.[0]}</FieldError>
              </FieldContent>
            </Field>
          ) : (
            <Field>
              <FieldLabel htmlFor={`${confirmValue}-note`}>
                {tCrmClient("crm.quotes.fields.note")}
              </FieldLabel>
              <FieldContent>
                <Textarea id={`${confirmValue}-note`} name="note" rows={3} />
                <FieldError>{state.fieldErrors?.note?.[0]}</FieldError>
              </FieldContent>
            </Field>
          )}

          <SubmitButton label={title} pendingLabel={tCrmClient("crm.forms.actions.processing")} />
        </form>
      </CardContent>
    </Card>
  );
}
