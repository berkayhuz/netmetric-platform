"use client";

import { useState, useTransition } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import {
  Button,
  Field,
  FieldContent,
  FieldError,
  FieldLabel,
  FieldSet,
  Input,
} from "@netmetric/ui";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@netmetric/ui/client";
import { useForm, useWatch } from "react-hook-form";

import { AddressActionResult } from "@/components/address/address-action-result";
import { CrmFormErrorSummary } from "@/components/forms/crm-form-error-summary";
import { initialCrmMutationState } from "@/features/shared/actions/mutation-state";
import { addressTypeOptions, booleanOptions } from "@/features/shared/forms/options";
import { tCrmClient } from "@/lib/i18n/crm-i18n";

import {
  addressFormSchema,
  type AddressFormInput,
} from "@/features/addresses/forms/address-form-schema";
import type { CrmMutationState } from "@/features/shared/actions/mutation-state";

const defaults: AddressFormInput = {
  id: "",
  addressType: 0,
  line1: "",
  line2: "",
  district: "",
  city: "",
  state: "",
  country: "",
  zipCode: "",
  isDefault: false,
  rowVersion: "",
};

export function AddressForm({
  mode,
  initialValues,
  action,
}: Readonly<{
  mode: "create" | "edit";
  initialValues?: Partial<AddressFormInput>;
  action: (input: AddressFormInput) => Promise<CrmMutationState>;
}>) {
  const [isPending, startTransition] = useTransition();
  const [result, setResult] = useState(initialCrmMutationState);

  const form = useForm<AddressFormInput>({
    resolver: zodResolver(addressFormSchema),
    defaultValues: { ...defaults, ...initialValues },
  });
  const addressType = useWatch({ control: form.control, name: "addressType" });
  const isDefault = useWatch({ control: form.control, name: "isDefault" });

  const onSubmit = (values: AddressFormInput) => {
    setResult(initialCrmMutationState);

    startTransition(async () => {
      const response = await action(values);
      setResult(response);

      if (response.fieldErrors) {
        for (const [field, errors] of Object.entries(response.fieldErrors)) {
          const first = errors[0];
          if (first) {
            form.setError(field as keyof AddressFormInput, { message: first });
          }
        }
      }

      if (response.status === "success" && mode === "create") {
        form.reset(defaults);
      }
    });
  };

  return (
    <form className="space-y-4" noValidate onSubmit={form.handleSubmit(onSubmit)}>
      <CrmFormErrorSummary
        {...(result.status === "error" && result.message ? { message: result.message } : {})}
        {...(result.fieldErrors ? { errors: result.fieldErrors } : {})}
      />
      <AddressActionResult state={result} />

      <FieldSet className="grid gap-4 sm:grid-cols-2">
        <Field>
          <FieldLabel htmlFor={`${mode}-address-line1`}>
            {tCrmClient("crm.address.fields.line1")}
          </FieldLabel>
          <FieldContent>
            <Input id={`${mode}-address-line1`} {...form.register("line1")} />
            <FieldError>{form.formState.errors.line1?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor={`${mode}-address-line2`}>
            {tCrmClient("crm.address.fields.line2")}
          </FieldLabel>
          <FieldContent>
            <Input id={`${mode}-address-line2`} {...form.register("line2")} />
            <FieldError>{form.formState.errors.line2?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor={`${mode}-address-district`}>
            {tCrmClient("crm.address.fields.district")}
          </FieldLabel>
          <FieldContent>
            <Input id={`${mode}-address-district`} {...form.register("district")} />
            <FieldError>{form.formState.errors.district?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor={`${mode}-address-city`}>
            {tCrmClient("crm.address.fields.city")}
          </FieldLabel>
          <FieldContent>
            <Input id={`${mode}-address-city`} {...form.register("city")} />
            <FieldError>{form.formState.errors.city?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor={`${mode}-address-state`}>
            {tCrmClient("crm.address.fields.stateRegion")}
          </FieldLabel>
          <FieldContent>
            <Input id={`${mode}-address-state`} {...form.register("state")} />
            <FieldError>{form.formState.errors.state?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor={`${mode}-address-country`}>
            {tCrmClient("crm.address.fields.country")}
          </FieldLabel>
          <FieldContent>
            <Input id={`${mode}-address-country`} {...form.register("country")} />
            <FieldError>{form.formState.errors.country?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor={`${mode}-address-zipCode`}>
            {tCrmClient("crm.address.fields.postalCode")}
          </FieldLabel>
          <FieldContent>
            <Input id={`${mode}-address-zipCode`} {...form.register("zipCode")} />
            <FieldError>{form.formState.errors.zipCode?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor={`${mode}-address-addressType`}>
            {tCrmClient("crm.address.fields.addressType")}
          </FieldLabel>
          <FieldContent>
            <Select
              value={String(addressType)}
              onValueChange={(value) =>
                form.setValue("addressType", Number(value), {
                  shouldDirty: true,
                  shouldValidate: true,
                })
              }
            >
              <SelectTrigger id={`${mode}-address-addressType`}>
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                {addressTypeOptions.map((option) => (
                  <SelectItem key={`address-type-${option.value}`} value={String(option.value)}>
                    {option.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <FieldError>{form.formState.errors.addressType?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor={`${mode}-address-isDefault`}>
            {tCrmClient("crm.address.fields.defaultAddress")}
          </FieldLabel>
          <FieldContent>
            <Select
              value={String(isDefault)}
              onValueChange={(value) =>
                form.setValue("isDefault", value === "true", {
                  shouldDirty: true,
                  shouldValidate: true,
                })
              }
            >
              <SelectTrigger id={`${mode}-address-isDefault`}>
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                {booleanOptions.map((option) => (
                  <SelectItem key={`address-default-${option.value}`} value={option.value}>
                    {option.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <FieldError>{form.formState.errors.isDefault?.message}</FieldError>
          </FieldContent>
        </Field>
      </FieldSet>

      {mode === "edit" ? <input type="hidden" {...form.register("id")} /> : null}
      {mode === "edit" ? <input type="hidden" {...form.register("rowVersion")} /> : null}

      <div className="flex justify-end">
        <Button type="submit" disabled={isPending} aria-busy={isPending}>
          {isPending
            ? tCrmClient("crm.address.actions.saving")
            : mode === "create"
              ? tCrmClient("crm.address.actions.add")
              : tCrmClient("crm.address.actions.update")}
        </Button>
      </div>
    </form>
  );
}
