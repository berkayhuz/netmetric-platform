"use client";

import Image from "next/image";
import { useRouter } from "next/navigation";
import { useEffect, useMemo, useRef, useState, useTransition } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { Upload, Trash2 } from "lucide-react";
import {
  Button,
  Field,
  FieldContent,
  FieldError,
  FieldLabel,
  FieldSet,
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
import { useForm, useWatch } from "react-hook-form";

import { CrmFormErrorSummary } from "@/components/forms/crm-form-error-summary";
import { CrmMutationResult } from "@/components/forms/crm-mutation-result";
import { initialCrmMutationState } from "@/features/shared/actions/mutation-state";
import { getSelectDisplayLabel } from "@/features/shared/forms/select-display";
import { tCrmClient } from "@/lib/i18n/crm-i18n";

import {
  deleteProductCatalogImageAction,
  createProductCatalogItemAction,
  setProductCatalogImagePrimaryAction,
  uploadProductCatalogImagesAction,
  updateProductCatalogItemAction,
} from "../actions/product-catalog-mutation-actions";
import type { ProductImageDto } from "@/lib/crm-api";
import {
  productCatalogFormSchema,
  type ProductCatalogFormInput,
} from "./product-catalog-form-schema";

type ProductCatalogFormProps = {
  mode: "create" | "edit";
  productId?: string;
  initialValues?: Partial<ProductCatalogFormInput>;
  initialImages?: ProductImageDto[];
  categoryOptions: Array<{ value: string; label: string }>;
  currencyOptions: string[];
};

const defaults: ProductCatalogFormInput = {
  code: "",
  name: "",
  description: "",
  isActive: true,
  categoryId: undefined,
  unitPrice: undefined,
  currencyCode: "USD",
  defaultDiscountRate: 0,
  defaultTaxRate: 0,
};

export function ProductCatalogForm({
  mode,
  productId,
  initialValues,
  initialImages = [],
  categoryOptions,
  currencyOptions,
}: Readonly<ProductCatalogFormProps>) {
  const router = useRouter();
  const [isPending, startTransition] = useTransition();
  const [result, setResult] = useState(initialCrmMutationState);
  const [imageFiles, setImageFiles] = useState<File[]>([]);
  const fileInputRef = useRef<HTMLInputElement>(null);
  const previewUrls = useMemo(
    () => imageFiles.map((file) => URL.createObjectURL(file)),
    [imageFiles],
  );

  const handleRemoveImageAt = (indexToRemove: number) => {
    setImageFiles((prev) => prev.filter((_, idx) => idx !== indexToRemove));
  };

  useEffect(() => {
    return () => {
      for (const url of previewUrls) {
        URL.revokeObjectURL(url);
      }
    };
  }, [previewUrls]);

  const form = useForm<ProductCatalogFormInput>({
    resolver: zodResolver(productCatalogFormSchema),
    defaultValues: { ...defaults, ...initialValues },
  });

  const isActive = useWatch({ control: form.control, name: "isActive" });
  const categoryId = useWatch({ control: form.control, name: "categoryId" });
  const currencyCode = useWatch({ control: form.control, name: "currencyCode" });
  const categoryDisplayOptions = [{ value: "__none__", label: "-" }, ...categoryOptions];
  const statusDisplayOptions = [
    { value: "true", label: tCrmClient("crm.common.active") },
    { value: "false", label: tCrmClient("crm.common.inactive") },
  ];
  const currencyDisplayOptions = currencyOptions.map((code) => ({ value: code, label: code }));

  const onSubmit = (values: ProductCatalogFormInput) => {
    setResult(initialCrmMutationState);

    startTransition(async () => {
      const response =
        mode === "create"
          ? await createProductCatalogItemAction(values)
          : await updateProductCatalogItemAction(productId ?? "", values);

      setResult(response);

      if (response.fieldErrors) {
        for (const [field, errors] of Object.entries(response.fieldErrors)) {
          const first = errors[0];
          if (first) {
            form.setError(field as keyof ProductCatalogFormInput, { message: first });
          }
        }
      }

      if (response.status === "success" && response.redirectTo) {
        const selectedFiles = imageFiles;
        if (selectedFiles.length > 10) {
          setResult({ status: "error", message: "A product can have at most 10 images." });
          return;
        }

        if (selectedFiles.length > 0) {
          const productId = response.redirectTo.split("/").pop() ?? "";
          const imageUploadResult = await uploadProductCatalogImagesAction(
            productId,
            selectedFiles,
          );
          if (imageUploadResult.status === "error") {
            setResult(imageUploadResult);
            return;
          }
        }

        router.push(response.redirectTo);
        router.refresh();
      }
    });
  };

  return (
    <form className="space-y-6" noValidate onSubmit={form.handleSubmit(onSubmit)}>
      <CrmFormErrorSummary
        {...(result.status === "error" && result.message ? { message: result.message } : {})}
        {...(result.fieldErrors ? { errors: result.fieldErrors } : {})}
      />
      <CrmMutationResult state={result} />

      {/* Premium Multiple Image Uploader Gallery */}
      <div className="pb-6 border-b border-border/30">
        <h3 className="text-sm font-semibold text-foreground tracking-tight mb-3">
          Product images
        </h3>
        <div className="flex flex-wrap gap-3">
          {/* Dash trigger block */}
          <div
            onClick={() => fileInputRef.current?.click()}
            className="size-20 rounded-xl border border-dashed border-border/60 hover:border-primary/50 bg-muted/10 hover:bg-muted/20 cursor-pointer flex flex-col items-center justify-center gap-1 transition-all select-none"
          >
            <Upload className="size-4 text-muted-foreground/60" />
            <span className="text-[9px] font-bold tracking-wider uppercase text-muted-foreground/60">
              Upload
            </span>
          </div>

          {/* Existing images */}
          {initialImages.map((image, index) => (
            <div
              key={`existing-${image.id}-${index}`}
              className="relative size-20 rounded-xl border border-border/40 overflow-hidden bg-muted/15 shadow-xs"
            >
              <Image
                unoptimized
                src={image.publicUrl}
                alt={`Product ${index + 1}`}
                fill
                className="object-cover"
              />
              {mode === "edit" && productId ? (
                <div className="absolute inset-x-1 bottom-1 flex items-center gap-1">
                  <button
                    type="button"
                    className="rounded bg-background/90 px-1 py-0.5 text-[10px]"
                    onClick={() =>
                      startTransition(async () => {
                        const response = await setProductCatalogImagePrimaryAction(
                          productId,
                          image.id,
                        );
                        setResult(response);
                        if (response.status === "success") {
                          router.refresh();
                        }
                      })
                    }
                  >
                    Primary
                  </button>
                  <button
                    type="button"
                    className="rounded bg-destructive/90 px-1 py-0.5 text-[10px] text-white"
                    onClick={() =>
                      startTransition(async () => {
                        const response = await deleteProductCatalogImageAction(productId, image.id);
                        setResult(response);
                        if (response.status === "success") {
                          router.refresh();
                        }
                      })
                    }
                  >
                    Delete
                  </button>
                </div>
              ) : null}
            </div>
          ))}

          {/* Newly selected images preview */}
          {previewUrls.map((url, index) => (
            <div
              key={`selected-${url}-${index}`}
              className="relative size-20 rounded-xl border border-border/40 overflow-hidden bg-muted/15 shadow-xs group"
            >
              <Image
                unoptimized
                src={url}
                alt={`Selected ${index + 1}`}
                fill
                className="object-cover"
              />
              <button
                type="button"
                onClick={() => handleRemoveImageAt(index)}
                className="absolute inset-0 bg-black/60 opacity-0 group-hover:opacity-100 flex items-center justify-center text-white transition-opacity cursor-pointer border-0"
                aria-label="Remove image"
              >
                <Trash2 className="size-4 text-white" />
              </button>
            </div>
          ))}
        </div>
        <p className="text-[10px] text-muted-foreground mt-2">
          JPG, PNG or WebP. Max 10 images. Max 5MB each.
        </p>

        <input
          ref={fileInputRef}
          id="catalog-images"
          type="file"
          accept="image/png,image/jpeg,image/webp"
          multiple
          className="hidden"
          onChange={(event) => setImageFiles(Array.from(event.target.files ?? []))}
        />
      </div>

      <FieldSet className="grid gap-4 sm:grid-cols-2">
        <Field>
          <FieldLabel htmlFor="catalog-code">
            {tCrmClient("crm.productCatalog.fields.code")}
          </FieldLabel>
          <FieldContent>
            <Input id="catalog-code" {...form.register("code")} />
            <FieldError>{form.formState.errors.code?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="catalog-name">
            {tCrmClient("crm.productCatalog.fields.name")}
          </FieldLabel>
          <FieldContent>
            <Input id="catalog-name" {...form.register("name")} />
            <FieldError>{form.formState.errors.name?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="catalog-category">
            {tCrmClient("crm.productCatalog.fields.category")}
          </FieldLabel>
          <FieldContent>
            <Select
              value={categoryId || "__none__"}
              onValueChange={(value) =>
                form.setValue(
                  "categoryId",
                  value == null || value === "__none__" ? undefined : value,
                  {
                    shouldDirty: true,
                  },
                )
              }
            >
              <SelectTrigger id="catalog-category">
                <SelectValue
                  placeholder={tCrmClient("crm.productCatalog.fields.categoryPlaceholder")}
                >
                  {getSelectDisplayLabel(categoryId || "__none__", categoryDisplayOptions)}
                </SelectValue>
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="__none__">-</SelectItem>
                {categoryOptions.map((option) => (
                  <SelectItem key={option.value} value={option.value}>
                    {option.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <FieldError>{form.formState.errors.categoryId?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="catalog-status">
            {tCrmClient("crm.productCatalog.fields.status")}
          </FieldLabel>
          <FieldContent>
            <Select
              value={String(isActive)}
              onValueChange={(value) =>
                form.setValue("isActive", value === "true", { shouldDirty: true })
              }
            >
              <SelectTrigger id="catalog-status">
                <SelectValue>
                  {getSelectDisplayLabel(String(isActive), statusDisplayOptions)}
                </SelectValue>
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="true">{tCrmClient("crm.common.active")}</SelectItem>
                <SelectItem value="false">{tCrmClient("crm.common.inactive")}</SelectItem>
              </SelectContent>
            </Select>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="catalog-price">
            {tCrmClient("crm.productCatalog.fields.price")}
          </FieldLabel>
          <FieldContent>
            <Input
              id="catalog-price"
              type="number"
              step="0.01"
              {...form.register("unitPrice", { valueAsNumber: true })}
            />
            <FieldError>{form.formState.errors.unitPrice?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="catalog-currency">
            {tCrmClient("crm.productCatalog.fields.currencyCode")}
          </FieldLabel>
          <FieldContent>
            <Select
              value={currencyCode || "USD"}
              onValueChange={(value) =>
                form.setValue("currencyCode", (value ?? "USD").toUpperCase(), {
                  shouldDirty: true,
                  shouldValidate: true,
                })
              }
            >
              <SelectTrigger id="catalog-currency">
                <SelectValue>
                  {getSelectDisplayLabel(currencyCode || "USD", currencyDisplayOptions)}
                </SelectValue>
              </SelectTrigger>
              <SelectContent>
                {currencyOptions.map((currencyCode) => (
                  <SelectItem key={currencyCode} value={currencyCode}>
                    {currencyCode}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <FieldError>{form.formState.errors.currencyCode?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="catalog-defaultDiscountRate">
            {tCrmClient("crm.quotes.fields.discountRate")}
          </FieldLabel>
          <FieldContent>
            <Input
              id="catalog-defaultDiscountRate"
              type="number"
              min={0}
              max={100}
              step="0.01"
              {...form.register("defaultDiscountRate", { valueAsNumber: true })}
            />
            <FieldError>{form.formState.errors.defaultDiscountRate?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="catalog-defaultTaxRate">
            {tCrmClient("crm.quotes.fields.taxRate")}
          </FieldLabel>
          <FieldContent>
            <Input
              id="catalog-defaultTaxRate"
              type="number"
              min={0}
              max={100}
              step="0.01"
              {...form.register("defaultTaxRate", { valueAsNumber: true })}
            />
            <FieldError>{form.formState.errors.defaultTaxRate?.message}</FieldError>
          </FieldContent>
        </Field>
      </FieldSet>

      <Field>
        <FieldLabel htmlFor="catalog-description">
          {tCrmClient("crm.productCatalog.fields.description")}
        </FieldLabel>
        <FieldContent>
          <Textarea id="catalog-description" rows={5} {...form.register("description")} />
          <FieldError>{form.formState.errors.description?.message}</FieldError>
        </FieldContent>
      </Field>

      <div className="flex flex-col-reverse gap-3 sm:flex-row sm:justify-end">
        <Button type="button" variant="outline" onClick={() => router.back()}>
          {tCrmClient("crm.forms.actions.cancel")}
        </Button>
        <Button type="submit" disabled={isPending} aria-busy={isPending}>
          {isPending
            ? tCrmClient("crm.forms.actions.saving")
            : mode === "create"
              ? tCrmClient("crm.productCatalog.actions.create")
              : tCrmClient("crm.productCatalog.actions.save")}
        </Button>
      </div>
    </form>
  );
}
