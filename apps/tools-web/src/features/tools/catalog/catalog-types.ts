export type ToolCategory = {
  slug: string;
  title: string;
  description: string;
  sortOrder: number;
};

export type ToolCatalogItem = {
  slug: string;
  title: string;
  description: string;
  categorySlug: string;
  executionMode: string;
  availabilityStatus: string;
  isEnabled: boolean;
  acceptedMimeTypes: readonly string[];
  guestMaxFileBytes: number;
  authenticatedMaxSaveBytes: number;
  seoTitle: string;
  seoDescription: string;
};

export type ToolCatalog = {
  categories: ToolCategory[];
  tools: ToolCatalogItem[];
};
