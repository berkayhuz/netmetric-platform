import { beforeEach, describe, expect, it, vi } from "vitest";

const mocks = vi.hoisted(() => ({
  createActivity: vi.fn(),
  requireCrmActionCapability: vi.fn(async () => ({})),
  requireCrmSession: vi.fn(async () => ({})),
}));

vi.mock("next/cache", () => ({ revalidatePath: vi.fn() }));
vi.mock("@/lib/security/csrf", () => ({ assertSameOriginRequest: vi.fn(async () => {}) }));
vi.mock("@/lib/i18n/request-locale", () => ({ getRequestLocale: vi.fn(async () => "en") }));
vi.mock("@/lib/i18n/crm-i18n", () => ({ tCrm: vi.fn((key: string) => key) }));
vi.mock("@/lib/crm-auth/crm-api-request-options", () => ({
  getCrmApiRequestOptions: vi.fn(async () => ({})),
}));
vi.mock("@/lib/crm-auth/require-crm-action-capability", () => ({
  requireCrmActionCapability: mocks.requireCrmActionCapability,
}));
vi.mock("@/lib/crm-auth/require-crm-session", () => ({
  requireCrmSession: mocks.requireCrmSession,
}));
vi.mock("@/lib/crm-api", () => ({
  crmApiClient: {
    createActivity: mocks.createActivity,
  },
}));

import { createActivityAction } from "./create-activity-action";

describe("createActivityAction", () => {
  const leadId = "7f9619ff-8b86-4a11-8b2d-00cf4fc964ff";
  const opportunityId = "7c6c8937-c7a1-4f24-9b63-17fddb0a95c0";
  const dealId = "c21ee526-2a5b-453d-908d-a0b4cd138cf9";
  const quoteId = "9150d647-7444-4846-a53d-35064f568d08";
  const ticketId = "f04ec7f4-43a4-4ab9-a6c0-04c892023924";
  const customerId = "4ea7d131-e22f-4cc4-a474-1a4f23885f98";
  const companyId = "2fb94bf7-4e56-4b6c-87ec-e342ac7b528c";
  const contactId = "93f3bc39-86a6-4bfa-b3b6-71d7caec64a1";

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("creates note activity with primary lead relation", async () => {
    mocks.createActivity.mockResolvedValueOnce({
      activityId: "6f9619ff-8b86-d011-b42d-00cf4fc964fe",
      type: "note",
      createdAtUtc: "2026-05-24T12:00:00Z",
      sourceEntityType: "lead",
      sourceEntityId: leadId,
      timelineItem: {
        id: "6f9619ff-8b86-d011-b42d-00cf4fc964fe",
        occurredAtUtc: "2026-05-24T12:00:00Z",
        type: "note",
        title: "title",
        description: null,
        status: null,
        sourceModule: "work-management",
        sourceEntityType: "lead",
        sourceEntityId: leadId,
        actorUserId: null,
        ownerUserId: null,
        relatedRecords: [],
        metadata: {},
      },
    });

    const state = await createActivityAction({
      primaryRecord: { entityType: "lead", entityId: leadId },
      type: "note",
      noteBody: "note",
      title: "title",
    });

    expect(state.status).toBe("success");
    expect(mocks.createActivity).toHaveBeenCalledWith(
      expect.objectContaining({
        type: "note",
        relatedRecords: [
          {
            entityType: "lead",
            entityId: leadId,
            relationRole: "primary",
          },
        ],
      }),
      {},
    );
  });

  it("returns validation error for missing note body", async () => {
    const state = await createActivityAction({
      primaryRecord: { entityType: "lead", entityId: leadId },
      type: "note",
      noteBody: "  ",
    });

    expect(state.status).toBe("error");
    expect(mocks.createActivity).not.toHaveBeenCalled();
  });

  it("requires call direction and outcome", async () => {
    const state = await createActivityAction({
      primaryRecord: { entityType: "lead", entityId: leadId },
      type: "call",
      callSummary: "test",
    });

    expect(state.status).toBe("error");
    expect(mocks.createActivity).not.toHaveBeenCalled();
  });

  it("creates note activity for opportunity primary relation", async () => {
    mocks.createActivity.mockResolvedValueOnce({
      activityId: "f885f6f0-6ba4-4f13-b4cb-d02f73eb4d81",
      type: "note",
      createdAtUtc: "2026-05-24T13:00:00Z",
      sourceEntityType: "opportunity",
      sourceEntityId: opportunityId,
      timelineItem: {
        id: "f885f6f0-6ba4-4f13-b4cb-d02f73eb4d81",
        occurredAtUtc: "2026-05-24T13:00:00Z",
        type: "note",
        title: "opportunity note",
        description: null,
        status: null,
        sourceModule: "work-management",
        sourceEntityType: "opportunity",
        sourceEntityId: opportunityId,
        actorUserId: null,
        ownerUserId: null,
        relatedRecords: [],
        metadata: {},
      },
    });

    const state = await createActivityAction({
      primaryRecord: { entityType: "opportunity", entityId: opportunityId },
      type: "note",
      noteBody: "opportunity note",
    });

    expect(state.status).toBe("success");
    expect(mocks.createActivity).toHaveBeenCalledWith(
      expect.objectContaining({
        relatedRecords: [
          {
            entityType: "opportunity",
            entityId: opportunityId,
            relationRole: "primary",
          },
        ],
      }),
      {},
    );
  });

  it("creates note activity for deal primary relation", async () => {
    mocks.createActivity.mockResolvedValueOnce({
      activityId: "6321bc59-4c07-4ac5-9e7f-0b8865cbef6f",
      type: "note",
      createdAtUtc: "2026-05-24T14:00:00Z",
      sourceEntityType: "deal",
      sourceEntityId: dealId,
      timelineItem: {
        id: "6321bc59-4c07-4ac5-9e7f-0b8865cbef6f",
        occurredAtUtc: "2026-05-24T14:00:00Z",
        type: "note",
        title: "deal note",
        description: null,
        status: null,
        sourceModule: "work-management",
        sourceEntityType: "deal",
        sourceEntityId: dealId,
        actorUserId: null,
        ownerUserId: null,
        relatedRecords: [],
        metadata: {},
      },
    });

    const state = await createActivityAction({
      primaryRecord: { entityType: "deal", entityId: dealId },
      type: "note",
      noteBody: "deal note",
    });

    expect(state.status).toBe("success");
    expect(mocks.createActivity).toHaveBeenCalledWith(
      expect.objectContaining({
        relatedRecords: [
          {
            entityType: "deal",
            entityId: dealId,
            relationRole: "primary",
          },
        ],
      }),
      {},
    );
  });

  it("creates note activity for quote primary relation", async () => {
    mocks.createActivity.mockResolvedValueOnce({
      activityId: "0f93fbca-c909-4736-8c35-6ac7f5b69da6",
      type: "note",
      createdAtUtc: "2026-05-24T15:00:00Z",
      sourceEntityType: "quote",
      sourceEntityId: quoteId,
      timelineItem: {
        id: "0f93fbca-c909-4736-8c35-6ac7f5b69da6",
        occurredAtUtc: "2026-05-24T15:00:00Z",
        type: "note",
        title: "quote note",
        description: null,
        status: null,
        sourceModule: "work-management",
        sourceEntityType: "quote",
        sourceEntityId: quoteId,
        actorUserId: null,
        ownerUserId: null,
        relatedRecords: [],
        metadata: {},
      },
    });

    const state = await createActivityAction({
      primaryRecord: { entityType: "quote", entityId: quoteId },
      type: "note",
      noteBody: "quote note",
    });

    expect(state.status).toBe("success");
    expect(mocks.createActivity).toHaveBeenCalledWith(
      expect.objectContaining({
        relatedRecords: [
          {
            entityType: "quote",
            entityId: quoteId,
            relationRole: "primary",
          },
        ],
      }),
      {},
    );
  });

  it("creates note activity for ticket primary relation", async () => {
    mocks.createActivity.mockResolvedValueOnce({
      activityId: "86808dfa-d6bb-4f8f-a9fb-26b54dbafba8",
      type: "note",
      createdAtUtc: "2026-05-24T16:00:00Z",
      sourceEntityType: "ticket",
      sourceEntityId: ticketId,
      timelineItem: {
        id: "86808dfa-d6bb-4f8f-a9fb-26b54dbafba8",
        occurredAtUtc: "2026-05-24T16:00:00Z",
        type: "note",
        title: "ticket note",
        description: null,
        status: null,
        sourceModule: "work-management",
        sourceEntityType: "ticket",
        sourceEntityId: ticketId,
        actorUserId: null,
        ownerUserId: null,
        relatedRecords: [],
        metadata: {},
      },
    });

    const state = await createActivityAction({
      primaryRecord: { entityType: "ticket", entityId: ticketId },
      type: "note",
      noteBody: "ticket note",
    });

    expect(state.status).toBe("success");
    expect(mocks.createActivity).toHaveBeenCalledWith(
      expect.objectContaining({
        relatedRecords: [
          {
            entityType: "ticket",
            entityId: ticketId,
            relationRole: "primary",
          },
        ],
      }),
      {},
    );
  });

  it("creates note activity for customer primary relation", async () => {
    mocks.createActivity.mockResolvedValueOnce({
      activityId: "a1098af5-3320-48d3-869c-d3371658e9bf",
      type: "note",
      createdAtUtc: "2026-05-24T17:00:00Z",
      sourceEntityType: "customer",
      sourceEntityId: customerId,
      timelineItem: {
        id: "a1098af5-3320-48d3-869c-d3371658e9bf",
        occurredAtUtc: "2026-05-24T17:00:00Z",
        type: "note",
        title: "customer note",
        description: null,
        status: null,
        sourceModule: "work-management",
        sourceEntityType: "customer",
        sourceEntityId: customerId,
        actorUserId: null,
        ownerUserId: null,
        relatedRecords: [],
        metadata: {},
      },
    });

    const state = await createActivityAction({
      primaryRecord: { entityType: "customer", entityId: customerId },
      type: "note",
      noteBody: "customer note",
    });

    expect(state.status).toBe("success");
    expect(mocks.createActivity).toHaveBeenCalledWith(
      expect.objectContaining({
        relatedRecords: [
          {
            entityType: "customer",
            entityId: customerId,
            relationRole: "primary",
          },
        ],
      }),
      {},
    );
  });

  it("creates note activity for company primary relation", async () => {
    mocks.createActivity.mockResolvedValueOnce({
      activityId: "3e228914-fc0a-4e01-ab21-080b69771bc2",
      type: "note",
      createdAtUtc: "2026-05-24T18:00:00Z",
      sourceEntityType: "company",
      sourceEntityId: companyId,
      timelineItem: {
        id: "3e228914-fc0a-4e01-ab21-080b69771bc2",
        occurredAtUtc: "2026-05-24T18:00:00Z",
        type: "note",
        title: "company note",
        description: null,
        status: null,
        sourceModule: "work-management",
        sourceEntityType: "company",
        sourceEntityId: companyId,
        actorUserId: null,
        ownerUserId: null,
        relatedRecords: [],
        metadata: {},
      },
    });

    const state = await createActivityAction({
      primaryRecord: { entityType: "company", entityId: companyId },
      type: "note",
      noteBody: "company note",
    });

    expect(state.status).toBe("success");
    expect(mocks.createActivity).toHaveBeenCalledWith(
      expect.objectContaining({
        relatedRecords: [
          {
            entityType: "company",
            entityId: companyId,
            relationRole: "primary",
          },
        ],
      }),
      {},
    );
  });

  it("creates note activity for contact primary relation", async () => {
    mocks.createActivity.mockResolvedValueOnce({
      activityId: "6f19fbe1-8dba-4d70-8c7b-c7adce0f5133",
      type: "note",
      createdAtUtc: "2026-05-24T19:00:00Z",
      sourceEntityType: "contact",
      sourceEntityId: contactId,
      timelineItem: {
        id: "6f19fbe1-8dba-4d70-8c7b-c7adce0f5133",
        occurredAtUtc: "2026-05-24T19:00:00Z",
        type: "note",
        title: "contact note",
        description: null,
        status: null,
        sourceModule: "work-management",
        sourceEntityType: "contact",
        sourceEntityId: contactId,
        actorUserId: null,
        ownerUserId: null,
        relatedRecords: [],
        metadata: {},
      },
    });

    const state = await createActivityAction({
      primaryRecord: { entityType: "contact", entityId: contactId },
      type: "note",
      noteBody: "contact note",
    });

    expect(state.status).toBe("success");
    expect(mocks.createActivity).toHaveBeenCalledWith(
      expect.objectContaining({
        relatedRecords: [
          {
            entityType: "contact",
            entityId: contactId,
            relationRole: "primary",
          },
        ],
      }),
      {},
    );
  });
});
