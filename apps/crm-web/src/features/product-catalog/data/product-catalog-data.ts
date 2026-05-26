import "server-only";

import { cache } from "react";

import {
  CrmApiError,
  crmApiClient,
  type CrmPagedResult,
  type ProductCatalogCategoryDto,
  type ProductCatalogItemDto,
  type CrmListQuery,
  type ProductCatalogLookupsDto,
  type ProductImageDto,
  type ProductCatalogMetaDto,
  type ProductCatalogStatsDto,
} from "@/lib/crm-api";
import { getCrmApiRequestOptions } from "@/lib/crm-auth/crm-api-request-options";
import { handleCrmApiPageError } from "@/lib/crm-auth/handle-crm-api-page-error";
import { emptyPagedResult, toListQuery } from "@/features/shared/data/query";

export type ProductCatalogListDataResult = {
  paged: CrmPagedResult<ProductCatalogItemDto>;
  unavailable: boolean;
  forbidden: boolean;
};

export type ProductCatalogItemDetailResult = {
  item: ProductCatalogItemDto | null;
  unavailable: boolean;
  forbidden: boolean;
};

export type ProductCatalogLookupsResult = ProductCatalogLookupsDto & {
  unavailable: boolean;
  forbidden: boolean;
};

export type ProductCatalogMetaResult = {
  meta: ProductCatalogMetaDto | null;
  unavailable: boolean;
  forbidden: boolean;
};

export type ProductCatalogStatsResult = {
  stats: ProductCatalogStatsDto | null;
  unavailable: boolean;
  forbidden: boolean;
};

export type ProductCatalogCategoryListDataResult = {
  paged: CrmPagedResult<ProductCatalogCategoryDto>;
  unavailable: boolean;
  forbidden: boolean;
};

export type ProductCatalogCategoryDetailResult = {
  item: ProductCatalogCategoryDto | null;
  unavailable: boolean;
  forbidden: boolean;
};

function getSearchParam(
  searchParams: Record<string, string | string[] | undefined>,
  key: string,
): string | undefined {
  const value = searchParams[key];
  return Array.isArray(value) ? value[0] : value;
}

function withCatalogFilters(query: CrmListQuery, filters: CrmListQuery["filters"]): CrmListQuery {
  const nextFilters = Object.fromEntries(
    Object.entries(filters ?? {}).filter(
      ([, value]) => value !== undefined && value !== null && value !== "",
    ),
  );

  return Object.keys(nextFilters).length > 0 ? { ...query, filters: nextFilters } : query;
}

export function toProductCatalogListQuery(
  searchParams: Record<string, string | string[] | undefined>,
): CrmListQuery {
  const query = toListQuery(searchParams);
  const categoryId = getSearchParam(searchParams, "categoryId")?.trim();
  const isActiveRaw = getSearchParam(searchParams, "isActive")?.trim();
  const isActive = isActiveRaw === "true" ? true : isActiveRaw === "false" ? false : undefined;

  return withCatalogFilters(query, {
    ...(categoryId ? { categoryId } : {}),
    ...(isActive !== undefined ? { isActive } : {}),
  });
}

export function toProductCatalogCategoryListQuery(
  searchParams: Record<string, string | string[] | undefined>,
): CrmListQuery {
  const query = toListQuery(searchParams);
  const isActiveRaw = getSearchParam(searchParams, "isActive")?.trim();
  const isActive = isActiveRaw === "true" ? true : isActiveRaw === "false" ? false : undefined;

  return withCatalogFilters(query, {
    ...(isActive !== undefined ? { isActive } : {}),
  });
}

export async function getProductCatalogData(
  query: CrmListQuery,
  returnPath: string,
): Promise<ProductCatalogListDataResult> {
  try {
    const options = await getCrmApiRequestOptions();
    const paged = await crmApiClient.listProductCatalogItems(query, options);
    return { paged, unavailable: false, forbidden: false };
  } catch (error) {
    if (
      error instanceof CrmApiError &&
      (error.kind === "unauthorized" || error.kind === "forbidden")
    ) {
      return {
        paged: emptyPagedResult<ProductCatalogItemDto>(query.page ?? 1, query.pageSize ?? 20),
        unavailable: false,
        forbidden: true,
      };
    }

    if (
      error instanceof CrmApiError &&
      (error.kind === "server_error" || error.kind === "upstream_unavailable")
    ) {
      return {
        paged: emptyPagedResult<ProductCatalogItemDto>(query.page ?? 1, query.pageSize ?? 20),
        unavailable: true,
        forbidden: false,
      };
    }

    handleCrmApiPageError(error, returnPath);
  }
}

export async function getProductCatalogCategoriesData(
  query: CrmListQuery,
  returnPath: string,
): Promise<ProductCatalogCategoryListDataResult> {
  try {
    const options = await getCrmApiRequestOptions();
    const paged = await crmApiClient.listProductCatalogCategories(query, options);
    return { paged, unavailable: false, forbidden: false };
  } catch (error) {
    if (
      error instanceof CrmApiError &&
      (error.kind === "unauthorized" || error.kind === "forbidden")
    ) {
      return {
        paged: emptyPagedResult<ProductCatalogCategoryDto>(query.page ?? 1, query.pageSize ?? 20),
        unavailable: false,
        forbidden: true,
      };
    }

    if (
      error instanceof CrmApiError &&
      (error.kind === "server_error" ||
        error.kind === "upstream_unavailable" ||
        error.kind === "not_found")
    ) {
      return {
        paged: emptyPagedResult<ProductCatalogCategoryDto>(query.page ?? 1, query.pageSize ?? 20),
        unavailable: true,
        forbidden: false,
      };
    }

    handleCrmApiPageError(error, returnPath);
  }
}

export async function getProductCatalogItemDetail(
  productId: string,
  returnPath: string,
): Promise<ProductCatalogItemDetailResult> {
  try {
    const options = await getCrmApiRequestOptions();
    const item = await crmApiClient.getProductCatalogItemById(productId, options);
    return { item, unavailable: false, forbidden: false };
  } catch (error) {
    if (
      error instanceof CrmApiError &&
      (error.kind === "unauthorized" || error.kind === "forbidden")
    ) {
      return { item: null, unavailable: false, forbidden: true };
    }

    if (
      error instanceof CrmApiError &&
      (error.kind === "server_error" || error.kind === "upstream_unavailable")
    ) {
      return { item: null, unavailable: true, forbidden: false };
    }

    handleCrmApiPageError(error, returnPath);
  }
}

export async function getProductCatalogCategoryDetail(
  categoryId: string,
  returnPath: string,
): Promise<ProductCatalogCategoryDetailResult> {
  try {
    const options = await getCrmApiRequestOptions();
    const item = await crmApiClient.getProductCatalogCategoryById(categoryId, options);
    return { item, unavailable: false, forbidden: false };
  } catch (error) {
    if (
      error instanceof CrmApiError &&
      (error.kind === "unauthorized" || error.kind === "forbidden")
    ) {
      return { item: null, unavailable: false, forbidden: true };
    }

    if (
      error instanceof CrmApiError &&
      (error.kind === "server_error" || error.kind === "upstream_unavailable")
    ) {
      return { item: null, unavailable: true, forbidden: false };
    }

    handleCrmApiPageError(error, returnPath);
  }
}

const getProductCatalogLookupsCached = cache(async function getProductCatalogLookupsCached(
  returnPath: string,
): Promise<ProductCatalogLookupsResult> {
  try {
    const options = await getCrmApiRequestOptions();
    const lookups = await crmApiClient.getProductCatalogLookups(options);
    return {
      ...lookups,
      currencies: lookups.currencies.length > 0 ? lookups.currencies : ["USD"],
      unavailable: false,
      forbidden: false,
    };
  } catch (error) {
    if (
      error instanceof CrmApiError &&
      (error.kind === "unauthorized" || error.kind === "forbidden")
    ) {
      return {
        products: [],
        categories: [],
        priceLists: [],
        discountMatrices: [],
        productBindings: [],
        currencies: ["USD"],
        unavailable: false,
        forbidden: true,
      };
    }

    if (
      error instanceof CrmApiError &&
      (error.kind === "server_error" || error.kind === "upstream_unavailable")
    ) {
      return {
        products: [],
        categories: [],
        priceLists: [],
        discountMatrices: [],
        productBindings: [],
        currencies: ["USD"],
        unavailable: true,
        forbidden: false,
      };
    }

    handleCrmApiPageError(error, returnPath);
  }
});

export async function getProductCatalogLookups(
  returnPath: string,
): Promise<ProductCatalogLookupsResult> {
  return getProductCatalogLookupsCached(returnPath);
}

export async function getProductCatalogMeta(returnPath: string): Promise<ProductCatalogMetaResult> {
  try {
    const options = await getCrmApiRequestOptions();
    const meta = await crmApiClient.getProductCatalogMeta(options);
    return { meta, unavailable: false, forbidden: false };
  } catch (error) {
    if (
      error instanceof CrmApiError &&
      (error.kind === "unauthorized" || error.kind === "forbidden")
    ) {
      return { meta: null, unavailable: false, forbidden: true };
    }

    if (
      error instanceof CrmApiError &&
      (error.kind === "server_error" || error.kind === "upstream_unavailable")
    ) {
      return { meta: null, unavailable: true, forbidden: false };
    }

    handleCrmApiPageError(error, returnPath);
  }
}

export async function getProductCatalogStats(
  returnPath: string,
): Promise<ProductCatalogStatsResult> {
  try {
    const options = await getCrmApiRequestOptions();
    const stats = await crmApiClient.getProductCatalogStats(options);
    return { stats, unavailable: false, forbidden: false };
  } catch (error) {
    if (
      error instanceof CrmApiError &&
      (error.kind === "unauthorized" || error.kind === "forbidden")
    ) {
      return { stats: null, unavailable: false, forbidden: true };
    }

    if (
      error instanceof CrmApiError &&
      (error.kind === "server_error" || error.kind === "upstream_unavailable")
    ) {
      return { stats: null, unavailable: true, forbidden: false };
    }

    handleCrmApiPageError(error, returnPath);
  }
}

export async function getProductCatalogItemImages(
  productId: string,
  returnPath: string,
): Promise<ProductImageDto[]> {
  try {
    const options = await getCrmApiRequestOptions();
    return await crmApiClient.listProductCatalogItemImages(productId, options);
  } catch (error) {
    if (
      error instanceof CrmApiError &&
      (error.kind === "unauthorized" || error.kind === "forbidden")
    ) {
      return [];
    }

    if (
      error instanceof CrmApiError &&
      (error.kind === "server_error" ||
        error.kind === "upstream_unavailable" ||
        error.kind === "not_found")
    ) {
      return [];
    }

    handleCrmApiPageError(error, returnPath);
  }
}

export function toProductCatalogLookupOptions(
  items: ProductCatalogLookupsDto["categories"],
): Array<{ value: string; label: string }> {
  return items.map((item) => ({
    value: item.id,
    label: `${item.code} - ${item.name}`,
  }));
}
