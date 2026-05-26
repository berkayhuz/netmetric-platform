import { describe, expect, it, vi } from "vitest";

import {
  executeCustomersPreviewDelete,
  getCustomersPreviewDeleteDescription,
  getCustomersPreviewSelectedIds,
  shouldShowCustomersPreviewBulkDelete,
} from "./customers-entity-data-table-preview";

describe("customers entity data-table preview interaction helpers", () => {
  it("computes selected ids from visible filtered customers only", () => {
    const rowSelection = {
      "visible-1": true,
      "visible-2": true,
      "hidden-1": true,
      "visible-3": false,
    };
    const filteredCustomers = [
      { id: "visible-1" },
      { id: "visible-2" },
      { id: "visible-3" },
    ] as const;

    const selectedIds = getCustomersPreviewSelectedIds(rowSelection, filteredCustomers as never);
    expect(selectedIds).toEqual(["visible-1", "visible-2"]);
  });

  it("shows bulk delete only when delete capability exists and selection is non-empty", () => {
    expect(shouldShowCustomersPreviewBulkDelete(true, 0)).toBe(false);
    expect(shouldShowCustomersPreviewBulkDelete(false, 2)).toBe(false);
    expect(shouldShowCustomersPreviewBulkDelete(true, 2)).toBe(true);
  });

  it("returns single and bulk confirmation copy", () => {
    expect(getCustomersPreviewDeleteDescription(1)).toBe(
      "This customer will be deleted permanently.",
    );
    expect(getCustomersPreviewDeleteDescription(3)).toBe(
      "3 selected customers will be deleted permanently.",
    );
  });

  it("executes single-delete success path and triggers success callback", async () => {
    const deleteSingle = vi.fn().mockResolvedValue({ status: "success" });
    const deleteBulk = vi.fn().mockResolvedValue({ status: "success" });
    const onSuccess = vi.fn();
    const onFailure = vi.fn();

    const result = await executeCustomersPreviewDelete(["a"], {
      deleteSingle,
      deleteBulk,
      onSuccess,
      onFailure,
    });

    expect(result).toBe(true);
    expect(deleteSingle).toHaveBeenCalledTimes(1);
    expect(deleteSingle).toHaveBeenCalledWith("a");
    expect(deleteBulk).not.toHaveBeenCalled();
    expect(onSuccess).toHaveBeenCalledTimes(1);
    expect(onFailure).not.toHaveBeenCalled();
  });

  it("executes bulk-delete success path and triggers success callback", async () => {
    const deleteSingle = vi.fn().mockResolvedValue({ status: "success" });
    const deleteBulk = vi.fn().mockResolvedValue({ status: "success" });
    const onSuccess = vi.fn();
    const onFailure = vi.fn();

    const result = await executeCustomersPreviewDelete(["a", "b"], {
      deleteSingle,
      deleteBulk,
      onSuccess,
      onFailure,
    });

    expect(result).toBe(true);
    expect(deleteSingle).not.toHaveBeenCalled();
    expect(deleteBulk).toHaveBeenCalledTimes(1);
    expect(deleteBulk).toHaveBeenCalledWith(["a", "b"]);
    expect(onSuccess).toHaveBeenCalledTimes(1);
    expect(onFailure).not.toHaveBeenCalled();
  });

  it("keeps failure path safe for single delete (no success callback)", async () => {
    const deleteSingle = vi.fn().mockResolvedValue({ status: "error" });
    const deleteBulk = vi.fn().mockResolvedValue({ status: "success" });
    const onSuccess = vi.fn();
    const onFailure = vi.fn();

    const result = await executeCustomersPreviewDelete(["a"], {
      deleteSingle,
      deleteBulk,
      onSuccess,
      onFailure,
    });

    expect(result).toBe(false);
    expect(onSuccess).not.toHaveBeenCalled();
    expect(onFailure).toHaveBeenCalledTimes(1);
  });

  it("keeps failure path safe for bulk delete (no success callback)", async () => {
    const deleteSingle = vi.fn().mockResolvedValue({ status: "success" });
    const deleteBulk = vi.fn().mockResolvedValue({ status: "error" });
    const onSuccess = vi.fn();
    const onFailure = vi.fn();

    const result = await executeCustomersPreviewDelete(["a", "b"], {
      deleteSingle,
      deleteBulk,
      onSuccess,
      onFailure,
    });

    expect(result).toBe(false);
    expect(onSuccess).not.toHaveBeenCalled();
    expect(onFailure).toHaveBeenCalledTimes(1);
  });

  it("treats empty target ids as a guarded failure", async () => {
    const deleteSingle = vi.fn().mockResolvedValue({ status: "success" });
    const deleteBulk = vi.fn().mockResolvedValue({ status: "success" });
    const onSuccess = vi.fn();
    const onFailure = vi.fn();

    const result = await executeCustomersPreviewDelete([], {
      deleteSingle,
      deleteBulk,
      onSuccess,
      onFailure,
    });

    expect(result).toBe(false);
    expect(deleteSingle).not.toHaveBeenCalled();
    expect(deleteBulk).not.toHaveBeenCalled();
    expect(onSuccess).not.toHaveBeenCalled();
    expect(onFailure).toHaveBeenCalledTimes(1);
  });
});
