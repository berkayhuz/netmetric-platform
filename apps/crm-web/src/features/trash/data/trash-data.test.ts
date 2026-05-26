import { beforeEach, describe, expect, it, vi } from "vitest";

const { getCrmApiRequestOptions, listTrashItems, handleCrmApiPageError } = vi.hoisted(() => ({
  getCrmApiRequestOptions: vi.fn(),
  listTrashItems: vi.fn(),
  handleCrmApiPageError: vi.fn((error: unknown) => {
    throw error;
  }),
}));

vi.mock("@/lib/crm-auth/crm-api-request-options", () => ({
  getCrmApiRequestOptions,
}));

vi.mock("@/lib/crm-api", () => ({
  CrmApiError: class CrmApiError extends Error {
    kind: string;

    constructor(params: { message: string; status: number; kind?: string }) {
      super(params.message);
      this.kind = params.kind ?? "unknown";
    }
  },
  crmApiClient: {
    listTrashItems,
  },
}));

vi.mock("@/lib/crm-auth/handle-crm-api-page-error", () => ({
  handleCrmApiPageError,
}));

describe("getTrashData", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("returns trash data when API succeeds", async () => {
    const payload = {
      items: [],
      totalCount: 0,
      pageNumber: 1,
      pageSize: 20,
      totalPages: 0,
    };
    getCrmApiRequestOptions.mockResolvedValueOnce({ authContext: { bearerToken: "token" } });
    listTrashItems.mockResolvedValueOnce(payload);

    const { getTrashData } = await import("./trash-data");
    const result = await getTrashData({ page: 1, pageSize: 20 }, "/trash");

    expect(result).toEqual(payload);
    expect(handleCrmApiPageError).not.toHaveBeenCalled();
  });

  it("returns an empty paged result when trash endpoint responds not_found", async () => {
    const { CrmApiError } = await import("@/lib/crm-api");
    const notFoundError = new CrmApiError({ message: "Not Found", status: 404, kind: "not_found" });
    getCrmApiRequestOptions.mockResolvedValueOnce({ authContext: { bearerToken: "token" } });
    listTrashItems.mockRejectedValueOnce(notFoundError);

    const { getTrashData } = await import("./trash-data");
    const result = await getTrashData({ page: 1, pageSize: 20 }, "/trash");
    expect(result).toEqual({
      items: [],
      totalCount: 0,
      pageNumber: 1,
      pageSize: 20,
      totalPages: 0,
    });
    expect(handleCrmApiPageError).not.toHaveBeenCalled();
  });
});
