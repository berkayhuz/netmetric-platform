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
  TextareaField,
} from "@netmetric/ui";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@netmetric/ui/client";
import { useForm, useWatch } from "react-hook-form";

import { CrmFormFeedback } from "@/components/forms/crm-form-feedback";
import { initialCrmMutationState } from "@/features/shared/actions/mutation-state";
import type { CrmReferenceOption } from "@/features/shared/data/form-reference-data";
import {
  opportunityStageOptions,
  opportunityStatusOptions,
  priorityOptions,
} from "@/features/shared/forms/options";
import { getSelectDisplayLabel } from "@/features/shared/forms/select-display";
import { tCrmClient } from "@/lib/i18n/crm-i18n";

import {
  createOpportunityAction,
  updateOpportunityAction,
} from "../actions/opportunity-mutation-actions";
import { opportunityFormSchema, type OpportunityFormInput } from "./opportunity-form-schema";

type OpportunityFormProps = {
  mode: "create" | "edit";
  opportunityId?: string;
  initialValues?: Partial<OpportunityFormInput>;
  leadOptions?: CrmReferenceOption[];
  customerOptions?: CrmReferenceOption[];
  ownerUserOptions?: CrmReferenceOption[];
};

const defaults: OpportunityFormInput = {
  opportunityCode: "",
  name: "",
  description: "",
  estimatedAmount: "",
  expectedRevenue: "",
  probability: 0,
  estimatedCloseDate: "",
  stage: 0,
  status: 0,
  priority: 1,
  leadId: "",
  customerId: "",
  ownerUserId: "",
  notes: "",
  rowVersion: "",
};

export function OpportunityForm({
  mode,
  opportunityId,
  initialValues,
  leadOptions = [],
  customerOptions = [],
  ownerUserOptions = [],
}: Readonly<OpportunityFormProps>) {
  const router = useRouter();
  const [isPending, startTransition] = useTransition();
  const [result, setResult] = useState(initialCrmMutationState);
  const form = useForm<OpportunityFormInput>({
    resolver: zodResolver(opportunityFormSchema),
    defaultValues: { ...defaults, ...initialValues },
  });
  const stage = useWatch({ control: form.control, name: "stage" });
  const status = useWatch({ control: form.control, name: "status" });
  const priority = useWatch({ control: form.control, name: "priority" });
  const stageDisplayOptions = opportunityStageOptions.map((option) => ({
    value: String(option.value),
    label: tCrmClient(`crm.opportunities.stage.${option.value}`),
  }));
  const statusDisplayOptions = opportunityStatusOptions.map((option) => ({
    value: String(option.value),
    label: tCrmClient(`crm.opportunities.status.${option.value}`),
  }));
  const priorityDisplayOptions = priorityOptions.map((option) => ({
    value: String(option.value),
    label: tCrmClient(`crm.common.priority.${option.value}`),
  }));
  const leadDisplayOptions = [{ value: "__none__", label: "-" }, ...leadOptions];
  const customerDisplayOptions = [{ value: "__none__", label: "-" }, ...customerOptions];
  const ownerDisplayOptions = [{ value: "__none__", label: "-" }, ...ownerUserOptions];

  const onSubmit = (values: OpportunityFormInput) => {
    setResult(initialCrmMutationState);
    startTransition(async () => {
      const response =
        mode === "create"
          ? await createOpportunityAction(values)
          : await updateOpportunityAction(opportunityId ?? "", values);
      setResult(response);
      if (response.fieldErrors) {
        for (const [field, errors] of Object.entries(response.fieldErrors)) {
          const first = errors[0];
          if (first) form.setError(field as keyof OpportunityFormInput, { message: first });
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
          <FieldLabel htmlFor="opportunity-code">
            {tCrmClient("crm.opportunities.fields.opportunityCode")}
          </FieldLabel>
          <FieldContent>
            <Input id="opportunity-code" {...form.register("opportunityCode")} />
            <FieldError>{form.formState.errors.opportunityCode?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="opportunity-name">
            {tCrmClient("crm.opportunities.fields.name")}
          </FieldLabel>
          <FieldContent>
            <Input id="opportunity-name" {...form.register("name")} />
            <FieldError>{form.formState.errors.name?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="opportunity-estimatedAmount">
            {tCrmClient("crm.opportunities.fields.estimatedAmount")}
          </FieldLabel>
          <FieldContent>
            <Input
              id="opportunity-estimatedAmount"
              inputMode="decimal"
              {...form.register("estimatedAmount")}
            />
            <FieldError>{form.formState.errors.estimatedAmount?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="opportunity-expectedRevenue">
            {tCrmClient("crm.opportunities.fields.expectedRevenue")}
          </FieldLabel>
          <FieldContent>
            <Input
              id="opportunity-expectedRevenue"
              inputMode="decimal"
              {...form.register("expectedRevenue")}
            />
            <FieldError>{form.formState.errors.expectedRevenue?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="opportunity-probability">
            {tCrmClient("crm.opportunities.fields.probability")}
          </FieldLabel>
          <FieldContent>
            <Input
              id="opportunity-probability"
              type="number"
              min={0}
              max={100}
              {...form.register("probability")}
            />
            <FieldError>{form.formState.errors.probability?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="opportunity-estimatedCloseDate">
            {tCrmClient("crm.opportunities.fields.estimatedCloseDate")}
          </FieldLabel>
          <FieldContent>
            <Input
              id="opportunity-estimatedCloseDate"
              type="date"
              {...form.register("estimatedCloseDate")}
            />
            <FieldError>{form.formState.errors.estimatedCloseDate?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="opportunity-stage">
            {tCrmClient("crm.opportunities.fields.stage")}
          </FieldLabel>
          <FieldContent>
            <Select
              value={String(stage)}
              onValueChange={(value) =>
                form.setValue("stage", Number(value), { shouldDirty: true, shouldValidate: true })
              }
            >
              <SelectTrigger id="opportunity-stage">
                <SelectValue>
                  {getSelectDisplayLabel(String(stage), stageDisplayOptions)}
                </SelectValue>
              </SelectTrigger>
              <SelectContent>
                {opportunityStageOptions.map((o) => (
                  <SelectItem key={`opportunity-stage-${o.value}`} value={String(o.value)}>
                    {tCrmClient(`crm.opportunities.stage.${o.value}`)}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <FieldError>{form.formState.errors.stage?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="opportunity-status">
            {tCrmClient("crm.opportunities.fields.status")}
          </FieldLabel>
          <FieldContent>
            <Select
              value={String(status)}
              onValueChange={(value) =>
                form.setValue("status", Number(value), { shouldDirty: true, shouldValidate: true })
              }
            >
              <SelectTrigger id="opportunity-status">
                <SelectValue>
                  {getSelectDisplayLabel(String(status), statusDisplayOptions)}
                </SelectValue>
              </SelectTrigger>
              <SelectContent>
                {opportunityStatusOptions.map((o) => (
                  <SelectItem key={`opportunity-status-${o.value}`} value={String(o.value)}>
                    {tCrmClient(`crm.opportunities.status.${o.value}`)}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <FieldError>{form.formState.errors.status?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="opportunity-priority">
            {tCrmClient("crm.opportunities.fields.priority")}
          </FieldLabel>
          <FieldContent>
            <Select
              value={String(priority)}
              onValueChange={(value) =>
                form.setValue("priority", Number(value), {
                  shouldDirty: true,
                  shouldValidate: true,
                })
              }
            >
              <SelectTrigger id="opportunity-priority">
                <SelectValue>
                  {getSelectDisplayLabel(String(priority), priorityDisplayOptions)}
                </SelectValue>
              </SelectTrigger>
              <SelectContent>
                {priorityOptions.map((o) => (
                  <SelectItem key={`opportunity-priority-${o.value}`} value={String(o.value)}>
                    {tCrmClient(`crm.common.priority.${o.value}`)}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <FieldError>{form.formState.errors.priority?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="opportunity-leadId">
            {tCrmClient("crm.opportunities.fields.leadId")}
          </FieldLabel>
          <FieldContent>
            <Select
              value={form.watch("leadId") ?? "__none__"}
              onValueChange={(value) =>
                form.setValue("leadId", value && value !== "__none__" ? value : "", {
                  shouldDirty: true,
                  shouldValidate: true,
                })
              }
            >
              <SelectTrigger id="opportunity-leadId">
                <SelectValue>
                  {getSelectDisplayLabel(form.watch("leadId") ?? "__none__", leadDisplayOptions)}
                </SelectValue>
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="__none__">-</SelectItem>
                {leadOptions.map((option) => (
                  <SelectItem key={`opportunity-lead-${option.value}`} value={option.value}>
                    {option.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <FieldError>{form.formState.errors.leadId?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="opportunity-customerId">
            {tCrmClient("crm.opportunities.fields.customerId")}
          </FieldLabel>
          <FieldContent>
            <Select
              value={form.watch("customerId") ?? "__none__"}
              onValueChange={(value) =>
                form.setValue("customerId", value && value !== "__none__" ? value : "", {
                  shouldDirty: true,
                  shouldValidate: true,
                })
              }
            >
              <SelectTrigger id="opportunity-customerId">
                <SelectValue>
                  {getSelectDisplayLabel(
                    form.watch("customerId") ?? "__none__",
                    customerDisplayOptions,
                  )}
                </SelectValue>
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="__none__">-</SelectItem>
                {customerOptions.map((option) => (
                  <SelectItem key={`opportunity-customer-${option.value}`} value={option.value}>
                    {option.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <FieldError>{form.formState.errors.customerId?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="opportunity-ownerUserId">
            {tCrmClient("crm.opportunities.fields.ownerUserId")}
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
              <SelectTrigger id="opportunity-ownerUserId">
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
                  <SelectItem key={`opportunity-owner-${option.value}`} value={option.value}>
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
        <TextareaField
          id="opportunity-description"
          label={tCrmClient("crm.opportunities.fields.description")}
          rows={4}
          error={form.formState.errors.description?.message}
          {...form.register("description")}
        />
        <TextareaField
          id="opportunity-notes"
          label={tCrmClient("crm.opportunities.fields.notes")}
          rows={4}
          error={form.formState.errors.notes?.message}
          {...form.register("notes")}
        />
      </FormGrid>

      {mode === "edit" ? <input type="hidden" {...form.register("rowVersion")} /> : null}

      <SubmitBar
        isPending={isPending}
        cancelLabel={tCrmClient("crm.forms.actions.cancel")}
        pendingLabel={tCrmClient("crm.forms.actions.saving")}
        submitLabel={
          mode === "create"
            ? tCrmClient("crm.opportunities.actions.create")
            : tCrmClient("crm.opportunities.actions.save")
        }
        onCancel={() => router.back()}
      />
    </form>
  );
}
