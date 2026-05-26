import { beforeEach, describe, expect, it, vi } from "vitest";

const mocks = vi.hoisted(() => ({
  mutateOperationalEndpoint: vi.fn(),
  mapCrmMutationErrorToState: vi.fn(),
  requireCrmSession: vi.fn(),
  crmCapabilityAllows: vi.fn(),
}));

vi.mock("next/cache", () => ({ revalidatePath: vi.fn() }));
vi.mock("@/lib/security/csrf", () => ({ assertSameOriginRequest: vi.fn(async () => {}) }));
vi.mock("@/lib/crm-auth/require-crm-session", () => ({
  requireCrmSession: mocks.requireCrmSession,
}));
vi.mock("@/lib/i18n/request-locale", () => ({ getRequestLocale: vi.fn(async () => "en") }));
vi.mock("@/lib/i18n/crm-i18n", () => ({ tCrm: vi.fn(() => "ok") }));
vi.mock("@/lib/crm-auth/crm-api-request-options", () => ({
  getCrmApiRequestOptions: vi.fn(async () => ({})),
}));
vi.mock("@/features/shared/actions/mutation-error-map", () => ({
  mapCrmMutationErrorToState: mocks.mapCrmMutationErrorToState,
}));
vi.mock("@/lib/crm-auth/crm-capabilities", () => ({
  crmCapabilityAllows: mocks.crmCapabilityAllows,
}));
vi.mock("@/lib/crm-api", () => ({
  crmApiClient: {
    mutateOperationalEndpoint: mocks.mutateOperationalEndpoint,
  },
}));

import {
  appendActivityFormAction,
  detectDuplicatesFormAction,
  mergeEntitiesFormAction,
  resolveIdentityFormAction,
  trackCdpEventFormAction,
  upsertRelationshipFormAction,
} from "./customer-intelligence-mutation-actions";

describe("customer intelligence mutation actions", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mocks.mutateOperationalEndpoint.mockResolvedValue({});
    mocks.mapCrmMutationErrorToState.mockReturnValue({
      status: "error",
      message: "mapped-error",
    });
    mocks.requireCrmSession.mockResolvedValue({ capabilities: {} });
    mocks.crmCapabilityAllows.mockReturnValue(true);
  });

  it("posts detect duplicates payload to the correct endpoint", async () => {
    const formData = new FormData();
    formData.set("subjectId", "11111111-1111-4111-8111-111111111111");
    formData.set("entityType", "Customer");

    const result = await detectDuplicatesFormAction(undefined, formData);

    expect(mocks.mutateOperationalEndpoint).toHaveBeenCalledWith(
      "POST",
      "/api/customer-intelligence/duplicates/detect",
      {
        subjectId: "11111111-1111-4111-8111-111111111111",
        entityType: "Customer",
      },
      {},
    );
    expect(result.status).toBe("success");
  });

  it("posts append activity payload to the correct endpoint", async () => {
    const formData = new FormData();
    formData.set("subjectId", "22222222-2222-4222-8222-222222222222");
    formData.set("name", "Ticket Follow-up");
    formData.set("category", "Ticket");
    formData.set("channel", "Email");

    const result = await appendActivityFormAction(undefined, formData);

    expect(mocks.mutateOperationalEndpoint).toHaveBeenCalledWith(
      "POST",
      "/api/customer-intelligence/activities",
      {
        subjectType: "Customer",
        subjectId: "22222222-2222-4222-8222-222222222222",
        name: "Ticket Follow-up",
        category: "Ticket",
        channel: "Email",
        entityType: "Customer",
        relatedEntityId: "22222222-2222-4222-8222-222222222222",
        dataJson: null,
        occurredAtUtc: null,
      },
      {},
    );
    expect(result.status).toBe("success");
  });

  it("returns field validation error when resolve identity form is invalid", async () => {
    const formData = new FormData();
    formData.set("subjectId", "not-a-guid");
    formData.set("identityType", "Email");
    formData.set("identityValue", "foo@bar.com");

    const result = await resolveIdentityFormAction(undefined, formData);

    expect(mocks.mapCrmMutationErrorToState).not.toHaveBeenCalled();
    expect(result.status).toBe("error");
    expect(result.fieldErrors?.subjectId).toBeDefined();
  });

  it("posts merge entities payload to the correct endpoint", async () => {
    const formData = new FormData();
    formData.set("primaryEntityId", "33333333-3333-4333-8333-333333333333");
    formData.set("secondaryEntityId", "44444444-4444-4444-8444-444444444444");
    formData.set("primaryEntityType", "Customer");
    formData.set("secondaryEntityType", "Customer");
    formData.set("reason", "Duplicate profile");

    const result = await mergeEntitiesFormAction(undefined, formData);

    expect(mocks.mutateOperationalEndpoint).toHaveBeenCalledWith(
      "POST",
      "/api/customer-intelligence/merges",
      {
        primaryEntityType: "Customer",
        primaryEntityId: "33333333-3333-4333-8333-333333333333",
        secondaryEntityType: "Customer",
        secondaryEntityId: "44444444-4444-4444-8444-444444444444",
        reason: "Duplicate profile",
      },
      {},
    );
    expect(result.status).toBe("success");
  });

  it("posts upsert relationship payload to the correct endpoint", async () => {
    const formData = new FormData();
    formData.set("sourceEntityId", "55555555-5555-4555-8555-555555555555");
    formData.set("targetEntityId", "66666666-6666-4666-8666-666666666666");
    formData.set("name", "Decision Maker");
    formData.set("relationshipType", "Influencer");
    formData.set("strengthScore", "0.75");

    const result = await upsertRelationshipFormAction(undefined, formData);

    expect(mocks.mutateOperationalEndpoint).toHaveBeenCalledWith(
      "PUT",
      "/api/customer-intelligence/relationships",
      {
        sourceEntityType: "Customer",
        sourceEntityId: "55555555-5555-4555-8555-555555555555",
        targetEntityType: "Customer",
        targetEntityId: "66666666-6666-4666-8666-666666666666",
        name: "Decision Maker",
        relationshipType: "Influencer",
        strengthScore: 0.75,
        isBidirectional: true,
        dataJson: null,
      },
      {},
    );
    expect(result.status).toBe("success");
  });

  it("posts track cdp event payload to the correct endpoint", async () => {
    const formData = new FormData();
    formData.set("subjectId", "77777777-7777-4777-8777-777777777777");
    formData.set("source", "Portal");
    formData.set("eventName", "PageViewed");
    formData.set("channel", "Web");
    formData.set("identityKey", "user-1");
    formData.set("propertiesJson", '{"page":"billing"}');

    const result = await trackCdpEventFormAction(undefined, formData);

    expect(mocks.mutateOperationalEndpoint).toHaveBeenCalledWith(
      "POST",
      "/api/customer-intelligence/cdp/events",
      {
        source: "Portal",
        eventName: "PageViewed",
        subjectType: "Customer",
        subjectId: "77777777-7777-4777-8777-777777777777",
        identityKey: "user-1",
        channel: "Web",
        propertiesJson: '{"page":"billing"}',
        occurredAtUtc: null,
      },
      {},
    );
    expect(result.status).toBe("success");
  });

  it("returns permission error when capability check fails", async () => {
    mocks.crmCapabilityAllows.mockReturnValue(false);
    const formData = new FormData();
    formData.set("subjectId", "11111111-1111-4111-8111-111111111111");
    formData.set("entityType", "Customer");

    const result = await detectDuplicatesFormAction(undefined, formData);

    expect(mocks.mutateOperationalEndpoint).not.toHaveBeenCalled();
    expect(result.status).toBe("error");
  });
});
