"use client";

import { useRouter } from "next/navigation";
import { useState, useTransition } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import {
  Field,
  FieldContent,
  FieldError,
  FieldLabel,
  FormGrid,
  Input,
  SubmitBar,
  Textarea,
} from "@netmetric/ui";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@netmetric/ui/client";
import { useForm } from "react-hook-form";

import { CrmFormFeedback } from "@/components/forms/crm-form-feedback";
import { initialCrmMutationState } from "@/features/shared/actions/mutation-state";
import type { CrmReferenceOption } from "@/features/shared/data/form-reference-data";
import { getSelectDisplayLabel } from "@/features/shared/forms/select-display";
import { tCrmClient } from "@/lib/i18n/crm-i18n";

import { createDealAction, updateDealAction } from "../actions/deal-mutation-actions";
import { dealFormSchema, type DealFormInput } from "./deal-form-schema";

type DealFormProps = {
  mode: "create" | "edit";
  dealId?: string;
  initialValues?: Partial<DealFormInput>;
  companyOptions?: CrmReferenceOption[];
  opportunityOptions?: CrmReferenceOption[];
  ownerUserOptions?: CrmReferenceOption[];
};

const defaults: DealFormInput = {
  dealCode: "",
  name: "",
  totalAmount: "",
  closedDate: "",
  opportunityId: "",
  companyId: "",
  ownerUserId: "",
  notes: "",
  rowVersion: "",
};

export function DealForm({
  mode,
  dealId,
  initialValues,
  companyOptions = [],
  opportunityOptions = [],
  ownerUserOptions = [],
}: Readonly<DealFormProps>) {
  const router = useRouter();
  const [isPending, startTransition] = useTransition();
  const [result, setResult] = useState(initialCrmMutationState);
  const form = useForm<DealFormInput>({
    resolver: zodResolver(dealFormSchema),
    defaultValues: { ...defaults, ...initialValues },
  });
  const opportunityDisplayOptions = [{ value: "__none__", label: "-" }, ...opportunityOptions];
  const companyDisplayOptions = [{ value: "__none__", label: "-" }, ...companyOptions];
  const ownerDisplayOptions = [{ value: "__none__", label: "-" }, ...ownerUserOptions];

  const onSubmit = (values: DealFormInput) => {
    setResult(initialCrmMutationState);
    startTransition(async () => {
      const response =
        mode === "create"
          ? await createDealAction(values)
          : await updateDealAction(dealId ?? "", values);
      setResult(response);
      if (response.fieldErrors) {
        for (const [field, errors] of Object.entries(response.fieldErrors)) {
          const first = errors[0];
          if (first) form.setError(field as keyof DealFormInput, { message: first });
        }
      }
      if (response.status === "success" && response.redirectTo) {
        router.push(response.redirectTo);
        router.refresh();
      }
    });
  };

  return (
    <form className="space-y-6" noValidate onSubmit={form.handleSubmit(onSubmit)}>
      <CrmFormFeedback state={result} />

      <FormGrid columns={2}>
        <Field>
          <FieldLabel htmlFor="deal-code">{tCrmClient("crm.deals.fields.dealCode")}</FieldLabel>
          <FieldContent>
            <Input id="deal-code" {...form.register("dealCode")} />
            <FieldError>{form.formState.errors.dealCode?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="deal-name">{tCrmClient("crm.deals.fields.name")}</FieldLabel>
          <FieldContent>
            <Input id="deal-name" {...form.register("name")} />
            <FieldError>{form.formState.errors.name?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="deal-totalAmount">
            {tCrmClient("crm.deals.fields.totalAmount")}
          </FieldLabel>
          <FieldContent>
            <Input id="deal-totalAmount" inputMode="decimal" {...form.register("totalAmount")} />
            <FieldError>{form.formState.errors.totalAmount?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="deal-closedDate">
            {tCrmClient("crm.deals.fields.closedDate")}
          </FieldLabel>
          <FieldContent>
            <Input id="deal-closedDate" type="date" {...form.register("closedDate")} />
            <FieldError>{form.formState.errors.closedDate?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="deal-opportunityId">
            {tCrmClient("crm.deals.fields.opportunityId")}
          </FieldLabel>
          <FieldContent>
            <Select
              value={form.watch("opportunityId") ?? "__none__"}
              onValueChange={(value) =>
                form.setValue("opportunityId", value && value !== "__none__" ? value : "", {
                  shouldDirty: true,
                  shouldValidate: true,
                })
              }
            >
              <SelectTrigger id="deal-opportunityId">
                <SelectValue>
                  {getSelectDisplayLabel(
                    form.watch("opportunityId") ?? "__none__",
                    opportunityDisplayOptions,
                  )}
                </SelectValue>
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="__none__">-</SelectItem>
                {opportunityOptions.map((option) => (
                  <SelectItem key={`deal-opportunity-${option.value}`} value={option.value}>
                    {option.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <FieldError>{form.formState.errors.opportunityId?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="deal-companyId">
            {tCrmClient("crm.deals.fields.companyId")}
          </FieldLabel>
          <FieldContent>
            <Select
              value={form.watch("companyId") ?? "__none__"}
              onValueChange={(value) =>
                form.setValue("companyId", value && value !== "__none__" ? value : "", {
                  shouldDirty: true,
                  shouldValidate: true,
                })
              }
            >
              <SelectTrigger id="deal-companyId">
                <SelectValue>
                  {getSelectDisplayLabel(
                    form.watch("companyId") ?? "__none__",
                    companyDisplayOptions,
                  )}
                </SelectValue>
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="__none__">-</SelectItem>
                {companyOptions.map((option) => (
                  <SelectItem key={`deal-company-${option.value}`} value={option.value}>
                    {option.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <FieldError>{form.formState.errors.companyId?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="deal-ownerUserId">
            {tCrmClient("crm.deals.fields.ownerUserId")}
          </FieldLabel>
          <FieldContent>
            <Select
              value={form.watch("ownerUserId") ?? "__none__"}
              onValueChange={(value) =>
                form.setValue("ownerUserId", value && value !== "__none__" ? value : "", {
                  shouldDirty: true,
                  shouldValidate: true,
                })
              }
            >
              <SelectTrigger id="deal-ownerUserId">
                <SelectValue>
                  {getSelectDisplayLabel(
                    form.watch("ownerUserId") ?? "__none__",
                    ownerDisplayOptions,
                  )}
                </SelectValue>
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="__none__">-</SelectItem>
                {ownerUserOptions.map((option) => (
                  <SelectItem key={`deal-owner-${option.value}`} value={option.value}>
                    {option.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <FieldError>{form.formState.errors.ownerUserId?.message}</FieldError>
          </FieldContent>
        </Field>
      </FormGrid>

      <FormGrid>
        <Field>
          <FieldLabel htmlFor="deal-notes">{tCrmClient("crm.deals.fields.notes")}</FieldLabel>
          <FieldContent>
            <Textarea id="deal-notes" rows={4} {...form.register("notes")} />
            <FieldError>{form.formState.errors.notes?.message}</FieldError>
          </FieldContent>
        </Field>
      </FormGrid>

      {mode === "edit" ? <input type="hidden" {...form.register("rowVersion")} /> : null}

      <SubmitBar
        isPending={isPending}
        cancelLabel={tCrmClient("crm.forms.actions.cancel")}
        pendingLabel={tCrmClient("crm.forms.actions.saving")}
        submitLabel={
          mode === "create"
            ? tCrmClient("crm.deals.actions.create")
            : tCrmClient("crm.deals.actions.save")
        }
        onCancel={() => router.back()}
      />
    </form>
  );
}
