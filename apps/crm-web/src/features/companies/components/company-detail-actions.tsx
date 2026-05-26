"use client";

import { useActionState } from "react";
import { useFormStatus } from "react-dom";
import { Button, Card, CardContent, CardDescription, CardHeader, CardTitle } from "@netmetric/ui";

import { CrmMutationResult } from "@/components/forms/crm-mutation-result";
import {
  initialCrmMutationState,
  type CrmMutationState,
} from "@/features/shared/actions/mutation-state";
import { tCrmClient } from "@/lib/i18n/crm-i18n";

function SubmitButton({ label }: Readonly<{ label: string }>) {
  const { pending } = useFormStatus();
  return (
    <Button type="submit" disabled={pending} aria-busy={pending}>
      {pending ? tCrmClient("crm.forms.actions.processing") : label}
    </Button>
  );
}

export function CompanyDetailActions({
  isActive,
  activateAction,
  deactivateAction,
}: Readonly<{
  isActive: boolean;
  activateAction: (state: CrmMutationState, formData: FormData) => Promise<CrmMutationState>;
  deactivateAction: (state: CrmMutationState, formData: FormData) => Promise<CrmMutationState>;
}>) {
  const [activateState, activateFormAction] = useActionState(
    activateAction,
    initialCrmMutationState,
  );
  const [deactivateState, deactivateFormAction] = useActionState(
    deactivateAction,
    initialCrmMutationState,
  );

  return (
    <Card>
      <CardHeader>
        <CardTitle>{tCrmClient("crm.companies.pages.detail.actionsTitle")}</CardTitle>
        <CardDescription>
          {tCrmClient("crm.companies.pages.detail.actionsDescription")}
        </CardDescription>
      </CardHeader>
      <CardContent className="space-y-3">
        <CrmMutationResult state={isActive ? deactivateState : activateState} />
        {isActive ? (
          <form action={deactivateFormAction}>
            <SubmitButton label={tCrmClient("crm.companies.actions.deactivate")} />
          </form>
        ) : (
          <form action={activateFormAction}>
            <SubmitButton label={tCrmClient("crm.companies.actions.activate")} />
          </form>
        )}
      </CardContent>
    </Card>
  );
}
