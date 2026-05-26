import { describe, expect, it } from "vitest";

type CapabilityStatus =
  | "already-supported-in-preview"
  | "safe-next"
  | "destructive-workflow-phase-required"
  | "intentionally-fallback-only";

type CapabilityMatrixRow = {
  capability: string;
  fallback: boolean;
  preview: boolean;
  risk: "low" | "medium" | "high";
  status: CapabilityStatus;
  recommendedOrder: number;
};

const capabilityMatrix: CapabilityMatrixRow[] = [
  {
    capability: "single-delete",
    fallback: true,
    preview: true,
    risk: "medium",
    status: "safe-next",
    recommendedOrder: 0,
  },
  {
    capability: "bulk-delete",
    fallback: true,
    preview: true,
    risk: "medium",
    status: "safe-next",
    recommendedOrder: 4,
  },
  {
    capability: "delete-confirmation-dialog",
    fallback: true,
    preview: true,
    risk: "medium",
    status: "safe-next",
    recommendedOrder: 1,
  },
  {
    capability: "mutation-pending-state",
    fallback: true,
    preview: true,
    risk: "medium",
    status: "safe-next",
    recommendedOrder: 2,
  },
  {
    capability: "permission-gating-for-delete",
    fallback: true,
    preview: true,
    risk: "medium",
    status: "safe-next",
    recommendedOrder: 3,
  },
  {
    capability: "selected-row-behavior",
    fallback: true,
    preview: true,
    risk: "low",
    status: "already-supported-in-preview",
    recommendedOrder: 0,
  },
  {
    capability: "import-batch-preview-validate-commit-workflows",
    fallback: true,
    preview: false,
    risk: "high",
    status: "intentionally-fallback-only",
    recommendedOrder: 99,
  },
  {
    capability: "drag-drop-column-reorder",
    fallback: true,
    preview: false,
    risk: "medium",
    status: "intentionally-fallback-only",
    recommendedOrder: 98,
  },
];

describe("customers mutation capability matrix", () => {
  it("keeps intentionally fallback-only capabilities out of preview", () => {
    const fallbackOnly = capabilityMatrix.filter(
      (row) => row.status === "intentionally-fallback-only",
    );

    for (const row of fallbackOnly) {
      expect(row.fallback).toBe(true);
      expect(row.preview).toBe(false);
    }
  });

  it("allows single-delete and bulk-delete parity in preview", () => {
    const singleDelete = capabilityMatrix.find((row) => row.capability === "single-delete");
    const bulkDelete = capabilityMatrix.find((row) => row.capability === "bulk-delete");

    expect(singleDelete?.fallback).toBe(true);
    expect(singleDelete?.preview).toBe(true);
    expect(bulkDelete?.fallback).toBe(true);
    expect(bulkDelete?.preview).toBe(true);
  });

  it("keeps selected-row behavior available in preview as a non-destructive parity baseline", () => {
    const selectedRowBehavior = capabilityMatrix.find(
      (row) => row.capability === "selected-row-behavior",
    );

    expect(selectedRowBehavior).toBeDefined();
    expect(selectedRowBehavior?.fallback).toBe(true);
    expect(selectedRowBehavior?.preview).toBe(true);
    expect(selectedRowBehavior?.status).toBe("already-supported-in-preview");
  });
});
