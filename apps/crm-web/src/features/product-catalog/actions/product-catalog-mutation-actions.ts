"use server";

import { revalidatePath } from "next/cache";
import { redirect } from "next/navigation";

import { mapCrmMutationErrorToState } from "@/features/shared/actions/mutation-error-map";
import type { CrmMutationState } from "@/features/shared/actions/mutation-state";
import { isGuid } from "@/features/shared/data/guid";
import { emptyToNull } from "@/features/shared/forms/schema-primitives";
import {
  crmApiClient,
  type CrmApiDownloadPayload,
  type CrmApiRequestOptions,
  type ProductCatalogActiveStateRequest,
  type ProductCatalogCategoryActiveStateRequest,
  type ProductCatalogCategoryUpsertRequest,
  type ProductCatalogUpsertRequest,
} from "@/lib/crm-api";
import { joinCrmApiPath } from "@/lib/crm-api/crm-api-config";
import { getCrmApiRequestOptions } from "@/lib/crm-auth/crm-api-request-options";
import { requireCrmActionCapability } from "@/lib/crm-auth/require-crm-action-capability";
import { tCrm } from "@/lib/i18n/crm-i18n";
import { getRequestLocale } from "@/lib/i18n/request-locale";
import { assertSameOriginRequest } from "@/lib/security/csrf";

import {
  productCatalogFormSchema,
  type ProductCatalogFormInput,
  type ProductCatalogFormValues,
} from "../forms/product-catalog-form-schema";
import {
  productCatalogCategoryFormSchema,
  type ProductCatalogCategoryFormInput,
  type ProductCatalogCategoryFormValues,
} from "../forms/product-catalog-category-form-schema";

function mapProductCatalogPayload(input: ProductCatalogFormValues): ProductCatalogUpsertRequest {
  return {
    code: input.code.trim(),
    name: input.name.trim(),
    description: emptyToNull(input.description),
    categoryId: emptyToNull(input.categoryId),
    unitPrice: input.unitPrice ?? null,
    currencyCode: input.currencyCode.trim().toUpperCase(),
    defaultDiscountRate: input.defaultDiscountRate,
    defaultTaxRate: input.defaultTaxRate,
  };
}

function mapProductCatalogCategoryPayload(
  input: ProductCatalogCategoryFormValues,
): ProductCatalogCategoryUpsertRequest {
  return {
    code: input.code.trim(),
    name: input.name.trim(),
    description: emptyToNull(input.description),
  };
}

function localizeFieldErrors(
  fieldErrors: Record<string, string[] | undefined>,
  locale: string,
): Record<string, string[]> {
  return Object.fromEntries(
    Object.entries(fieldErrors).flatMap(([key, errors]) => {
      if (!errors || errors.length === 0) return [];
      return [[key, [tCrm("crm.productCatalog.validation.invalid", locale)]] as const];
    }),
  );
}

function buildApiHeaders(options: CrmApiRequestOptions): Headers {
  const headers = new Headers();
  headers.set("accept", "application/json");

  if (options.authContext?.bearerToken) {
    headers.set("authorization", `Bearer ${options.authContext.bearerToken}`);
  }

  if (options.correlationId) {
    headers.set("x-correlation-id", options.correlationId);
  }

  return headers;
}

function toDownloadDataUrl(payload: CrmApiDownloadPayload): string {
  const buffer = Buffer.from(payload.bytes);
  const base64 = buffer.toString("base64");
  return `data:${payload.contentType};base64,${base64}`;
}

async function uploadCategoryImageInternal(categoryId: string, file: File): Promise<void> {
  const options = await getCrmApiRequestOptions();
  const formData = new FormData();
  formData.set("file", file);

  const response = await fetch(joinCrmApiPath(`/api/catalog/categories/${categoryId}/image`), {
    method: "POST",
    headers: buildApiHeaders(options),
    body: formData,
    cache: "no-store",
  });

  if (!response.ok) {
    throw new Error("Category image upload failed.");
  }
}

async function uploadProductImagesInternal(
  productId: string,
  files: readonly File[],
): Promise<void> {
  const options = await getCrmApiRequestOptions();

  for (const file of files) {
    const formData = new FormData();
    formData.set("file", file);

    const response = await fetch(joinCrmApiPath(`/api/catalog/products/${productId}/images`), {
      method: "POST",
      headers: buildApiHeaders(options),
      body: formData,
      cache: "no-store",
    });

    if (!response.ok) {
      throw new Error("Product image upload failed.");
    }
  }
}

export async function createProductCatalogItemAction(
  input: ProductCatalogFormInput,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability("/product-catalog/new", "productCatalog.manage");
  const locale = await getRequestLocale();

  const parsed = productCatalogFormSchema.safeParse(input);
  if (!parsed.success) {
    return {
      status: "error",
      message: tCrm("crm.forms.errors.reviewTitle", locale),
      fieldErrors: localizeFieldErrors(parsed.error.flatten().fieldErrors, locale),
    };
  }

  try {
    const options = await getCrmApiRequestOptions();
    const created = await crmApiClient.createProductCatalogItem(
      mapProductCatalogPayload(parsed.data),
      options,
    );

    if (!parsed.data.isActive) {
      const inactivePayload: ProductCatalogActiveStateRequest = { isActive: false };
      await crmApiClient.setProductCatalogItemActiveState(created.id, inactivePayload, options);
    }

    revalidatePath("/product-catalog");
    revalidatePath(`/product-catalog/${created.id}`);

    return {
      status: "success",
      message: tCrm("crm.productCatalog.result.created", locale),
      redirectTo: `/product-catalog/${created.id}`,
    };
  } catch (error) {
    return mapCrmMutationErrorToState(error, "/product-catalog/new");
  }
}

export async function uploadProductCatalogImagesAction(
  productId: string,
  files: readonly File[],
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/product-catalog/${productId}`, "productCatalog.manage");
  const locale = await getRequestLocale();

  if (!isGuid(productId)) {
    return { status: "error", message: tCrm("crm.productCatalog.validation.invalidId", locale) };
  }

  if (files.length > 10) {
    return { status: "error", message: "A product can have at most 10 images." };
  }

  try {
    await uploadProductImagesInternal(productId, files);
    revalidatePath(`/product-catalog/${productId}`);
    return { status: "success", message: tCrm("crm.productCatalog.result.updated", locale) };
  } catch {
    return { status: "error", message: tCrm("crm.forms.errors.saveFailed", locale) };
  }
}

export async function updateProductCatalogItemAction(
  productId: string,
  input: ProductCatalogFormInput,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/product-catalog/${productId}`, "productCatalog.manage");
  const locale = await getRequestLocale();

  if (!isGuid(productId)) {
    return { status: "error", message: tCrm("crm.productCatalog.validation.invalidId", locale) };
  }

  const parsed = productCatalogFormSchema.safeParse(input);
  if (!parsed.success) {
    return {
      status: "error",
      message: tCrm("crm.forms.errors.reviewTitle", locale),
      fieldErrors: localizeFieldErrors(parsed.error.flatten().fieldErrors, locale),
    };
  }

  try {
    const options = await getCrmApiRequestOptions();
    await crmApiClient.updateProductCatalogItem(
      productId,
      mapProductCatalogPayload(parsed.data),
      options,
    );
    await crmApiClient.setProductCatalogItemActiveState(
      productId,
      { isActive: parsed.data.isActive },
      options,
    );

    revalidatePath("/product-catalog");
    revalidatePath(`/product-catalog/${productId}`);
    revalidatePath(`/product-catalog/${productId}/edit`);

    return {
      status: "success",
      message: tCrm("crm.productCatalog.result.updated", locale),
      redirectTo: `/product-catalog/${productId}`,
    };
  } catch (error) {
    return mapCrmMutationErrorToState(error, `/product-catalog/${productId}/edit`);
  }
}

export async function deleteProductCatalogItemAction(
  productId: string,
  _previous: CrmMutationState,
  formData: FormData,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/product-catalog/${productId}`, "productCatalog.manage");
  const locale = await getRequestLocale();

  if (!isGuid(productId)) {
    return { status: "error", message: tCrm("crm.productCatalog.validation.invalidId", locale) };
  }

  if (formData.get("confirm") !== "delete-product-catalog-item") {
    return { status: "error", message: tCrm("crm.delete.invalidConfirmation", locale) };
  }

  try {
    const options = await getCrmApiRequestOptions();
    await crmApiClient.deleteProductCatalogItem(productId, options);

    revalidatePath("/product-catalog");
    redirect("/product-catalog");
  } catch (error) {
    return mapCrmMutationErrorToState(error, `/product-catalog/${productId}`);
  }
}

export type ProductCatalogDownloadResult = {
  status: "success" | "error";
  message?: string;
  fileName?: string;
  contentType?: string;
  downloadUrl?: string;
};

export async function downloadProductCatalogExportAction(
  searchParams: Record<string, string | string[] | undefined>,
): Promise<ProductCatalogDownloadResult> {
  await assertSameOriginRequest();
  await requireCrmActionCapability("/product-catalog", "productCatalog.read");
  const locale = await getRequestLocale();

  try {
    const options = await getCrmApiRequestOptions();
    const search = Array.isArray(searchParams.search)
      ? searchParams.search[0]
      : searchParams.search;
    const payload = await crmApiClient.downloadProductCatalogExport(
      search ? { search } : {},
      options,
    );

    return {
      status: "success",
      fileName: payload.fileName,
      contentType: payload.contentType,
      downloadUrl: toDownloadDataUrl(payload),
    };
  } catch {
    return { status: "error", message: tCrm("crm.forms.errors.saveFailed", locale) };
  }
}

export async function downloadProductCatalogTemplateAction(): Promise<ProductCatalogDownloadResult> {
  await assertSameOriginRequest();
  await requireCrmActionCapability("/product-catalog", "productCatalog.read");
  const locale = await getRequestLocale();

  try {
    const options = await getCrmApiRequestOptions();
    const payload = await crmApiClient.downloadProductCatalogTemplate(options);

    return {
      status: "success",
      fileName: payload.fileName,
      contentType: payload.contentType,
      downloadUrl: toDownloadDataUrl(payload),
    };
  } catch {
    return { status: "error", message: tCrm("crm.forms.errors.saveFailed", locale) };
  }
}

export async function bulkDeleteProductCatalogItemsAction(
  productIds: readonly string[],
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability("/product-catalog", "productCatalog.manage");
  const locale = await getRequestLocale();

  const ids = productIds.filter(isGuid);
  if (ids.length === 0) {
    return { status: "error", message: tCrm("crm.productCatalog.validation.invalidId", locale) };
  }

  try {
    const options = await getCrmApiRequestOptions();
    await crmApiClient.bulkDeleteProductCatalogItems({ ids }, options);
    revalidatePath("/product-catalog");
    return { status: "success", message: tCrm("crm.productCatalog.result.updated", locale) };
  } catch (error) {
    return mapCrmMutationErrorToState(error, "/product-catalog");
  }
}

export async function bulkSetProductCatalogItemsActiveStateAction(
  productIds: readonly string[],
  isActive: boolean,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability("/product-catalog", "productCatalog.manage");
  const locale = await getRequestLocale();

  const ids = productIds.filter(isGuid);
  if (ids.length === 0) {
    return { status: "error", message: tCrm("crm.productCatalog.validation.invalidId", locale) };
  }

  try {
    const options = await getCrmApiRequestOptions();
    await crmApiClient.bulkSetProductCatalogItemsActiveState({ ids, isActive }, options);
    revalidatePath("/product-catalog");
    return { status: "success", message: tCrm("crm.productCatalog.result.updated", locale) };
  } catch (error) {
    return mapCrmMutationErrorToState(error, "/product-catalog");
  }
}

export async function setProductCatalogImagePrimaryAction(
  productId: string,
  productImageId: string,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/product-catalog/${productId}/edit`, "productCatalog.manage");
  const locale = await getRequestLocale();

  if (!isGuid(productId) || !isGuid(productImageId)) {
    return { status: "error", message: tCrm("crm.productCatalog.validation.invalidId", locale) };
  }

  try {
    const options = await getCrmApiRequestOptions();
    await crmApiClient.setProductCatalogImagePrimary(productId, productImageId, options);
    revalidatePath(`/product-catalog/${productId}`);
    revalidatePath(`/product-catalog/${productId}/edit`);
    return { status: "success", message: tCrm("crm.productCatalog.result.updated", locale) };
  } catch (error) {
    return mapCrmMutationErrorToState(error, `/product-catalog/${productId}/edit`);
  }
}

export async function deleteProductCatalogImageAction(
  productId: string,
  productImageId: string,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/product-catalog/${productId}/edit`, "productCatalog.manage");
  const locale = await getRequestLocale();

  if (!isGuid(productId) || !isGuid(productImageId)) {
    return { status: "error", message: tCrm("crm.productCatalog.validation.invalidId", locale) };
  }

  try {
    const options = await getCrmApiRequestOptions();
    await crmApiClient.deleteProductCatalogImage(productId, productImageId, options);
    revalidatePath(`/product-catalog/${productId}`);
    revalidatePath(`/product-catalog/${productId}/edit`);
    return { status: "success", message: tCrm("crm.productCatalog.result.updated", locale) };
  } catch (error) {
    return mapCrmMutationErrorToState(error, `/product-catalog/${productId}/edit`);
  }
}

export async function createProductCatalogCategoryAction(
  input: ProductCatalogCategoryFormInput,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability("/product-catalog/categories/new", "productCatalog.manage");
  const locale = await getRequestLocale();

  const parsed = productCatalogCategoryFormSchema.safeParse(input);
  if (!parsed.success) {
    return {
      status: "error",
      message: tCrm("crm.forms.errors.reviewTitle", locale),
      fieldErrors: localizeFieldErrors(parsed.error.flatten().fieldErrors, locale),
    };
  }

  try {
    const options = await getCrmApiRequestOptions();
    const created = await crmApiClient.createProductCatalogCategory(
      mapProductCatalogCategoryPayload(parsed.data),
      options,
    );

    if (!parsed.data.isActive) {
      const inactivePayload: ProductCatalogCategoryActiveStateRequest = { isActive: false };
      await crmApiClient.setProductCatalogCategoryActiveState(created.id, inactivePayload, options);
    }

    revalidatePath("/product-catalog");
    revalidatePath("/product-catalog/categories");
    revalidatePath(`/product-catalog/categories/${created.id}`);

    return {
      status: "success",
      message: tCrm("crm.productCatalog.categories.result.created", locale),
      redirectTo: `/product-catalog/categories/${created.id}`,
    };
  } catch (error) {
    return mapCrmMutationErrorToState(error, "/product-catalog/categories/new");
  }
}

export async function uploadProductCatalogCategoryImageAction(
  categoryId: string,
  file: File,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(
    `/product-catalog/categories/${categoryId}`,
    "productCatalog.manage",
  );
  const locale = await getRequestLocale();

  if (!isGuid(categoryId)) {
    return {
      status: "error",
      message: tCrm("crm.productCatalog.categories.validation.invalidId", locale),
    };
  }

  try {
    await uploadCategoryImageInternal(categoryId, file);
    revalidatePath(`/product-catalog/categories/${categoryId}`);
    revalidatePath("/product-catalog/categories");
    return {
      status: "success",
      message: tCrm("crm.productCatalog.categories.result.updated", locale),
    };
  } catch {
    return { status: "error", message: tCrm("crm.forms.errors.saveFailed", locale) };
  }
}

export async function updateProductCatalogCategoryAction(
  categoryId: string,
  input: ProductCatalogCategoryFormInput,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(
    `/product-catalog/categories/${categoryId}`,
    "productCatalog.manage",
  );
  const locale = await getRequestLocale();

  if (!isGuid(categoryId)) {
    return {
      status: "error",
      message: tCrm("crm.productCatalog.categories.validation.invalidId", locale),
    };
  }

  const parsed = productCatalogCategoryFormSchema.safeParse(input);
  if (!parsed.success) {
    return {
      status: "error",
      message: tCrm("crm.forms.errors.reviewTitle", locale),
      fieldErrors: localizeFieldErrors(parsed.error.flatten().fieldErrors, locale),
    };
  }

  try {
    const options = await getCrmApiRequestOptions();
    await crmApiClient.updateProductCatalogCategory(
      categoryId,
      mapProductCatalogCategoryPayload(parsed.data),
      options,
    );
    await crmApiClient.setProductCatalogCategoryActiveState(
      categoryId,
      { isActive: parsed.data.isActive },
      options,
    );

    revalidatePath("/product-catalog");
    revalidatePath("/product-catalog/categories");
    revalidatePath(`/product-catalog/categories/${categoryId}`);
    revalidatePath(`/product-catalog/categories/${categoryId}/edit`);

    return {
      status: "success",
      message: tCrm("crm.productCatalog.categories.result.updated", locale),
      redirectTo: `/product-catalog/categories/${categoryId}`,
    };
  } catch (error) {
    return mapCrmMutationErrorToState(error, `/product-catalog/categories/${categoryId}/edit`);
  }
}

export async function deleteProductCatalogCategoryAction(
  categoryId: string,
  _previous: CrmMutationState,
  formData: FormData,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(
    `/product-catalog/categories/${categoryId}`,
    "productCatalog.manage",
  );
  const locale = await getRequestLocale();

  if (!isGuid(categoryId)) {
    return {
      status: "error",
      message: tCrm("crm.productCatalog.categories.validation.invalidId", locale),
    };
  }

  if (formData.get("confirm") !== "delete-product-catalog-category") {
    return { status: "error", message: tCrm("crm.delete.invalidConfirmation", locale) };
  }

  try {
    const options = await getCrmApiRequestOptions();
    await crmApiClient.deleteProductCatalogCategory(categoryId, options);

    revalidatePath("/product-catalog");
    revalidatePath("/product-catalog/categories");
    redirect("/product-catalog/categories");
  } catch (error) {
    return mapCrmMutationErrorToState(error, `/product-catalog/categories/${categoryId}`);
  }
}
