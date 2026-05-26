"use client";

import { useActionState } from "react";
import { useFormStatus } from "react-dom";
import { Button, Field, FieldContent, FieldLabel, Input, Textarea } from "@netmetric/ui";

import { CrmMutationResult } from "@/components/forms/crm-mutation-result";
import { runGuidedSellingFormAction } from "@/features/quotes/actions/quote-cpq-actions";
import {
  initialCrmMutationState,
  type CrmMutationState,
} from "@/features/shared/actions/mutation-state";
import { tCrmClient } from "@/lib/i18n/crm-i18n";

function SubmitButton() {
  const { pending } = useFormStatus();

  return (
    <Button type="submit" disabled={pending} aria-busy={pending}>
      {pending
        ? tCrmClient("crm.forms.actions.processing")
        : tCrmClient("crm.quotes.cpq.guidedSelling.run")}
    </Button>
  );
}

export function QuoteGuidedSellingPanel({ quoteId }: Readonly<{ quoteId: string }>) {
  const action = runGuidedSellingFormAction.bind(null, quoteId);
  const [state, formAction] = useActionState<CrmMutationState, FormData>(
    action,
    initialCrmMutationState,
  );

  return (
    <div className="space-y-4">
      <CrmMutationResult state={state} />
      <form action={formAction} className="grid gap-4 md:grid-cols-2">
        <Field>
          <FieldLabel htmlFor="guided-selling-segment">
            {tCrmClient("crm.quotes.cpq.fields.segment")}
          </FieldLabel>
          <FieldContent>
            <Input id="guided-selling-segment" name="segment" />
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="guided-selling-industry">
            {tCrmClient("crm.quotes.cpq.fields.industry")}
          </FieldLabel>
          <FieldContent>
            <Input id="guided-selling-industry" name="industry" />
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="guided-selling-budget">
            {tCrmClient("crm.quotes.cpq.fields.budget")}
          </FieldLabel>
          <FieldContent>
            <Input id="guided-selling-budget" inputMode="decimal" name="budget" />
          </FieldContent>
        </Field>
        <Field className="md:col-span-2">
          <FieldLabel htmlFor="guided-selling-capabilities">
            {tCrmClient("crm.quotes.cpq.fields.requiredCapabilities")}
          </FieldLabel>
          <FieldContent>
            <Textarea id="guided-selling-capabilities" name="requiredCapabilities" rows={3} />
          </FieldContent>
        </Field>
        <div className="md:col-span-2">
          <SubmitButton />
        </div>
      </form>
    </div>
  );
}
