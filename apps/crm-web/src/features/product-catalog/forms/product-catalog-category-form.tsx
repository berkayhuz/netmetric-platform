"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import Image from "next/image";
import { useRouter } from "next/navigation";
import { useEffect, useMemo, useRef, useState, useTransition } from "react";
import { useForm, useWatch } from "react-hook-form";

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

import { CrmFormErrorSummary } from "@/components/forms/crm-form-error-summary";
import { CrmMutationResult } from "@/components/forms/crm-mutation-result";
import { initialCrmMutationState } from "@/features/shared/actions/mutation-state";
import { getSelectDisplayLabel } from "@/features/shared/forms/select-display";
import { tCrmClient } from "@/lib/i18n/crm-i18n";
import { CrmImageUploader } from "@/components/media/crm-image-uploader";

import {
  createProductCatalogCategoryAction,
  uploadProductCatalogCategoryImageAction,
  updateProductCatalogCategoryAction,
} from "../actions/product-catalog-mutation-actions";
import {
  productCatalogCategoryFormSchema,
  type ProductCatalogCategoryFormInput,
} from "./product-catalog-category-form-schema";

type ProductCatalogCategoryFormProps = {
  mode: "create" | "edit";
  categoryId?: string;
  initialValues?: Partial<ProductCatalogCategoryFormInput>;
  initialImageUrl?: string | null;
};

const defaults: ProductCatalogCategoryFormInput = {
  code: "",
  name: "",
  description: "",
  isActive: true,
};

export function ProductCatalogCategoryForm({
  mode,
  categoryId,
  initialValues,
  initialImageUrl = null,
}: Readonly<ProductCatalogCategoryFormProps>) {
  const router = useRouter();
  const [isPending, startTransition] = useTransition();
  const [result, setResult] = useState(initialCrmMutationState);
  const [imageFile, setImageFile] = useState<File | null>(null);
  const previewUrl = useMemo(
    () => (imageFile ? URL.createObjectURL(imageFile) : null),
    [imageFile],
  );

  useEffect(() => {
    return () => {
      if (previewUrl) {
        URL.revokeObjectURL(previewUrl);
      }
    };
  }, [previewUrl]);

  const form = useForm<ProductCatalogCategoryFormInput>({
    resolver: zodResolver(productCatalogCategoryFormSchema),
    defaultValues: { ...defaults, ...initialValues },
  });

  const isActive = useWatch({ control: form.control, name: "isActive" });
  const statusDisplayOptions = [
    { value: "true", label: tCrmClient("crm.common.active") },
    { value: "false", label: tCrmClient("crm.common.inactive") },
  ];

  const onSubmit = (values: ProductCatalogCategoryFormInput) => {
    setResult(initialCrmMutationState);

    startTransition(async () => {
      const response =
        mode === "create"
          ? await createProductCatalogCategoryAction(values)
          : await updateProductCatalogCategoryAction(categoryId ?? "", values);

      setResult(response);

      if (response.fieldErrors) {
        for (const [field, errors] of Object.entries(response.fieldErrors)) {
          const first = errors[0];
          if (first) {
            form.setError(field as keyof ProductCatalogCategoryFormInput, { message: first });
          }
        }
      }

      if (response.status === "success" && response.redirectTo) {
        const selectedFile = imageFile;
        if (selectedFile) {
          const categoryId = response.redirectTo.split("/").pop() ?? "";
          const imageUploadResult = await uploadProductCatalogCategoryImageAction(
            categoryId,
            selectedFile,
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

      <div className="pb-6 border-b border-border/30">
        <CrmImageUploader
          altText="Category image"
          title="Category image"
          description="JPG, PNG or WebP. Max 5MB."
          imageUrl={previewUrl ?? initialImageUrl}
          onChange={(file) => setImageFile(file)}
        />
      </div>

      <FieldSet className="grid gap-4 sm:grid-cols-2">
        <Field>
          <FieldLabel htmlFor="catalog-category-code">
            {tCrmClient("crm.productCatalog.fields.code")}
          </FieldLabel>
          <FieldContent>
            <Input id="catalog-category-code" {...form.register("code")} />
            <FieldError>{form.formState.errors.code?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="catalog-category-name">
            {tCrmClient("crm.productCatalog.fields.name")}
          </FieldLabel>
          <FieldContent>
            <Input id="catalog-category-name" {...form.register("name")} />
            <FieldError>{form.formState.errors.name?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="catalog-category-status">
            {tCrmClient("crm.productCatalog.fields.status")}
          </FieldLabel>
          <FieldContent>
            <Select
              value={String(isActive)}
              onValueChange={(value) =>
                form.setValue("isActive", value === "true", { shouldDirty: true })
              }
            >
              <SelectTrigger id="catalog-category-status">
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
      </FieldSet>

      <Field>
        <FieldLabel htmlFor="catalog-category-description">
          {tCrmClient("crm.productCatalog.fields.description")}
        </FieldLabel>
        <FieldContent>
          <Textarea id="catalog-category-description" rows={5} {...form.register("description")} />
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
              ? tCrmClient("crm.productCatalog.categories.actions.create")
              : tCrmClient("crm.productCatalog.categories.actions.save")}
        </Button>
      </div>
    </form>
  );
}
