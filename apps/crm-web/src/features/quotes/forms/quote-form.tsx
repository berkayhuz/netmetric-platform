"use client";

import { useRouter } from "next/navigation";
import { useState, useTransition } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import {
  Button,
  Field,
  FieldContent,
  FieldError,
  FieldSet,
  FieldLabel,
  FormGrid,
  Input,
  SubmitBar,
  Textarea,
  TextareaField,
} from "@netmetric/ui";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@netmetric/ui/client";
import { useFieldArray, useForm } from "react-hook-form";

import { CrmFormFeedback } from "@/components/forms/crm-form-feedback";
import { initialCrmMutationState } from "@/features/shared/actions/mutation-state";
import type {
  CrmProductReferenceOption,
  CrmReferenceOption,
} from "@/features/shared/data/form-reference-data";
import { getSelectDisplayLabel } from "@/features/shared/forms/select-display";
import { tCrmClient } from "@/lib/i18n/crm-i18n";

import { createQuoteAction, updateQuoteAction } from "../actions/quote-mutation-actions";
import { quoteFormSchema, type QuoteFormInput } from "./quote-form-schema";

type QuoteFormProps = {
  mode: "create" | "edit";
  quoteId?: string;
  initialValues?: Partial<QuoteFormInput>;
  opportunityOptions?: CrmReferenceOption[];
  customerOptions?: CrmReferenceOption[];
  ownerUserOptions?: CrmReferenceOption[];
  proposalTemplateOptions?: CrmReferenceOption[];
  productOptions?: CrmProductReferenceOption[];
};

const defaultLine: QuoteFormInput["items"][number] = {
  productId: "",
  description: "",
  quantity: 1,
  unitPrice: "0",
  discountRate: 0,
  taxRate: 0,
};

const defaults: QuoteFormInput = {
  quoteNumber: "",
  proposalTitle: "",
  proposalSummary: "",
  proposalBody: "",
  quoteDate: "",
  validUntil: "",
  opportunityId: "",
  customerId: "",
  ownerUserId: "",
  currencyCode: "TRY",
  exchangeRate: "1",
  termsAndConditions: "",
  proposalTemplateId: "",
  items: [defaultLine],
  rowVersion: "",
};

export function QuoteForm({
  mode,
  quoteId,
  initialValues,
  opportunityOptions = [],
  customerOptions = [],
  ownerUserOptions = [],
  proposalTemplateOptions = [],
  productOptions = [],
}: Readonly<QuoteFormProps>) {
  const router = useRouter();
  const [isPending, startTransition] = useTransition();
  const [result, setResult] = useState(initialCrmMutationState);
  const form = useForm<QuoteFormInput>({
    resolver: zodResolver(quoteFormSchema),
    defaultValues: {
      ...defaults,
      ...initialValues,
      items: initialValues?.items?.length ? initialValues.items : defaults.items,
    },
  });

  const lineItems = useFieldArray({
    control: form.control,
    name: "items",
  });
  const opportunityDisplayOptions = [{ value: "__none__", label: "-" }, ...opportunityOptions];
  const customerDisplayOptions = [{ value: "__none__", label: "-" }, ...customerOptions];
  const ownerDisplayOptions = [{ value: "__none__", label: "-" }, ...ownerUserOptions];
  const proposalTemplateDisplayOptions = [
    { value: "__none__", label: "-" },
    ...proposalTemplateOptions,
  ];
  const productDisplayOptions = [{ value: "__none__", label: "-" }, ...productOptions];

  const onSubmit = (values: QuoteFormInput) => {
    setResult(initialCrmMutationState);
    startTransition(async () => {
      const response =
        mode === "create"
          ? await createQuoteAction(values)
          : await updateQuoteAction(quoteId ?? "", values);

      setResult(response);
      if (response.fieldErrors) {
        for (const [field, errors] of Object.entries(response.fieldErrors)) {
          const first = errors[0];
          if (first) form.setError(field as keyof QuoteFormInput, { message: first });
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
          <FieldLabel htmlFor="quote-number">
            {tCrmClient("crm.quotes.fields.quoteNumber")}
          </FieldLabel>
          <FieldContent>
            <Input id="quote-number" {...form.register("quoteNumber")} />
            <FieldError>{form.formState.errors.quoteNumber?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="quote-title">
            {tCrmClient("crm.quotes.fields.proposalTitle")}
          </FieldLabel>
          <FieldContent>
            <Input id="quote-title" {...form.register("proposalTitle")} />
            <FieldError>{form.formState.errors.proposalTitle?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="quote-date">{tCrmClient("crm.quotes.fields.quoteDate")}</FieldLabel>
          <FieldContent>
            <Input id="quote-date" type="date" {...form.register("quoteDate")} />
            <FieldError>{form.formState.errors.quoteDate?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="quote-validUntil">
            {tCrmClient("crm.quotes.fields.validUntil")}
          </FieldLabel>
          <FieldContent>
            <Input id="quote-validUntil" type="date" {...form.register("validUntil")} />
            <FieldError>{form.formState.errors.validUntil?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="quote-currency">
            {tCrmClient("crm.quotes.fields.currencyCode")}
          </FieldLabel>
          <FieldContent>
            <Input id="quote-currency" maxLength={3} {...form.register("currencyCode")} />
            <FieldError>{form.formState.errors.currencyCode?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="quote-exchangeRate">
            {tCrmClient("crm.quotes.fields.exchangeRate")}
          </FieldLabel>
          <FieldContent>
            <Input id="quote-exchangeRate" inputMode="decimal" {...form.register("exchangeRate")} />
            <FieldError>{form.formState.errors.exchangeRate?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="quote-opportunityId">
            {tCrmClient("crm.quotes.fields.opportunityId")}
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
              <SelectTrigger id="quote-opportunityId">
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
                  <SelectItem key={`quote-opportunity-${option.value}`} value={option.value}>
                    {option.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <FieldError>{form.formState.errors.opportunityId?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="quote-customerId">
            {tCrmClient("crm.quotes.fields.customerId")}
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
              <SelectTrigger id="quote-customerId">
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
                  <SelectItem key={`quote-customer-${option.value}`} value={option.value}>
                    {option.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <FieldError>{form.formState.errors.customerId?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="quote-ownerUserId">
            {tCrmClient("crm.quotes.fields.ownerUserId")}
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
              <SelectTrigger id="quote-ownerUserId">
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
                  <SelectItem key={`quote-owner-${option.value}`} value={option.value}>
                    {option.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <FieldError>{form.formState.errors.ownerUserId?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="quote-proposalTemplateId">
            {tCrmClient("crm.quotes.fields.proposalTemplateId")}
          </FieldLabel>
          <FieldContent>
            {proposalTemplateOptions.length > 0 ? (
              <Select
                value={form.watch("proposalTemplateId") ?? "__none__"}
                onValueChange={(value) =>
                  form.setValue("proposalTemplateId", value && value !== "__none__" ? value : "", {
                    shouldDirty: true,
                    shouldValidate: true,
                  })
                }
              >
                <SelectTrigger id="quote-proposalTemplateId">
                  <SelectValue>
                    {getSelectDisplayLabel(
                      form.watch("proposalTemplateId") ?? "__none__",
                      proposalTemplateDisplayOptions,
                    )}
                  </SelectValue>
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="__none__">-</SelectItem>
                  {proposalTemplateOptions.map((option) => (
                    <SelectItem
                      key={`quote-proposal-template-${option.value}`}
                      value={option.value}
                    >
                      {option.label}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            ) : (
              <Input
                id="quote-proposalTemplateId"
                value=""
                readOnly
                disabled
                placeholder="-"
                aria-label={tCrmClient("crm.quotes.fields.proposalTemplateId")}
              />
            )}
            <FieldError>{form.formState.errors.proposalTemplateId?.message}</FieldError>
          </FieldContent>
        </Field>
      </FormGrid>

      <FormGrid>
        <TextareaField
          id="quote-summary"
          label={tCrmClient("crm.quotes.fields.proposalSummary")}
          rows={3}
          error={form.formState.errors.proposalSummary?.message}
          {...form.register("proposalSummary")}
        />
        <TextareaField
          id="quote-body"
          label={tCrmClient("crm.quotes.fields.proposalBody")}
          rows={5}
          error={form.formState.errors.proposalBody?.message}
          {...form.register("proposalBody")}
        />
        <TextareaField
          id="quote-terms"
          label={tCrmClient("crm.quotes.fields.termsAndConditions")}
          rows={4}
          error={form.formState.errors.termsAndConditions?.message}
          {...form.register("termsAndConditions")}
        />
      </FormGrid>

      <FieldSet className="grid gap-4">
        <div className="flex items-center justify-between">
          <h2 className="text-sm font-medium">{tCrmClient("crm.quotes.lineItems.title")}</h2>
          <Button
            type="button"
            variant="outline"
            onClick={() => lineItems.append({ ...defaultLine })}
          >
            {tCrmClient("crm.quotes.lineItems.addLine")}
          </Button>
        </div>

        {lineItems.fields.map((line, index) => (
          <div key={line.id} className="grid gap-3 rounded-sm border p-4 sm:grid-cols-2">
            <Field>
              <FieldLabel htmlFor={`quote-item-${index}-productId`}>
                {tCrmClient("crm.quotes.fields.productId")}
              </FieldLabel>
              <FieldContent>
                <Select
                  value={form.watch(`items.${index}.productId`) || "__none__"}
                  onValueChange={(value) => {
                    const productId = value && value !== "__none__" ? value : "";
                    form.setValue(`items.${index}.productId`, productId, {
                      shouldDirty: true,
                      shouldValidate: true,
                    });

                    const selected = productOptions.find((option) => option.value === productId);
                    if (!selected) {
                      return;
                    }

                    if (selected.unitPrice != null) {
                      form.setValue(`items.${index}.unitPrice`, String(selected.unitPrice), {
                        shouldDirty: true,
                        shouldValidate: true,
                      });
                    }

                    if (selected.defaultDiscountRate != null) {
                      form.setValue(`items.${index}.discountRate`, selected.defaultDiscountRate, {
                        shouldDirty: true,
                        shouldValidate: true,
                      });
                    }

                    if (selected.defaultTaxRate != null) {
                      form.setValue(`items.${index}.taxRate`, selected.defaultTaxRate, {
                        shouldDirty: true,
                        shouldValidate: true,
                      });
                    }

                    if (selected.description) {
                      form.setValue(`items.${index}.description`, selected.description, {
                        shouldDirty: true,
                        shouldValidate: true,
                      });
                    }
                  }}
                >
                  <SelectTrigger id={`quote-item-${index}-productId`}>
                    <SelectValue>
                      {getSelectDisplayLabel(
                        form.watch(`items.${index}.productId`) || "__none__",
                        productDisplayOptions,
                      )}
                    </SelectValue>
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="__none__">-</SelectItem>
                    {productOptions.map((option) => (
                      <SelectItem key={`quote-item-product-${option.value}`} value={option.value}>
                        {option.label}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                <FieldError>{form.formState.errors.items?.[index]?.productId?.message}</FieldError>
              </FieldContent>
            </Field>
            <Field>
              <FieldLabel htmlFor={`quote-item-${index}-quantity`}>
                {tCrmClient("crm.quotes.fields.quantity")}
              </FieldLabel>
              <FieldContent>
                <Input
                  id={`quote-item-${index}-quantity`}
                  type="number"
                  min={1}
                  {...form.register(`items.${index}.quantity`)}
                />
                <FieldError>{form.formState.errors.items?.[index]?.quantity?.message}</FieldError>
              </FieldContent>
            </Field>
            <Field>
              <FieldLabel htmlFor={`quote-item-${index}-unitPrice`}>
                {tCrmClient("crm.quotes.fields.unitPrice")}
              </FieldLabel>
              <FieldContent>
                <Input
                  id={`quote-item-${index}-unitPrice`}
                  inputMode="decimal"
                  {...form.register(`items.${index}.unitPrice`)}
                />
                <FieldError>{form.formState.errors.items?.[index]?.unitPrice?.message}</FieldError>
              </FieldContent>
            </Field>
            <Field>
              <FieldLabel htmlFor={`quote-item-${index}-discountRate`}>
                {tCrmClient("crm.quotes.fields.discountRate")}
              </FieldLabel>
              <FieldContent>
                <Input
                  id={`quote-item-${index}-discountRate`}
                  type="number"
                  min={0}
                  max={100}
                  step="0.01"
                  {...form.register(`items.${index}.discountRate`)}
                />
                <FieldError>
                  {form.formState.errors.items?.[index]?.discountRate?.message}
                </FieldError>
              </FieldContent>
            </Field>
            <Field>
              <FieldLabel htmlFor={`quote-item-${index}-taxRate`}>
                {tCrmClient("crm.quotes.fields.taxRate")}
              </FieldLabel>
              <FieldContent>
                <Input
                  id={`quote-item-${index}-taxRate`}
                  type="number"
                  min={0}
                  max={100}
                  step="0.01"
                  {...form.register(`items.${index}.taxRate`)}
                />
                <FieldError>{form.formState.errors.items?.[index]?.taxRate?.message}</FieldError>
              </FieldContent>
            </Field>
            <Field className="sm:col-span-2">
              <FieldLabel htmlFor={`quote-item-${index}-description`}>
                {tCrmClient("crm.quotes.fields.description")}
              </FieldLabel>
              <FieldContent>
                <Textarea
                  id={`quote-item-${index}-description`}
                  rows={2}
                  {...form.register(`items.${index}.description`)}
                />
                <FieldError>
                  {form.formState.errors.items?.[index]?.description?.message}
                </FieldError>
              </FieldContent>
            </Field>
            <div className="sm:col-span-2 flex justify-end">
              <Button
                type="button"
                variant="ghost"
                disabled={lineItems.fields.length <= 1}
                onClick={() => lineItems.remove(index)}
              >
                {tCrmClient("crm.quotes.lineItems.removeLine")}
              </Button>
            </div>
          </div>
        ))}
      </FieldSet>

      {mode === "edit" ? <input type="hidden" {...form.register("rowVersion")} /> : null}

      <SubmitBar
        isPending={isPending}
        cancelLabel={tCrmClient("crm.forms.actions.cancel")}
        pendingLabel={tCrmClient("crm.forms.actions.saving")}
        submitLabel={
          mode === "create"
            ? tCrmClient("crm.quotes.actions.create")
            : tCrmClient("crm.quotes.actions.save")
        }
        onCancel={() => router.back()}
      />
    </form>
  );
}
