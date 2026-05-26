import { describe, expect, it } from "vitest";

import { mapCrmMutationErrorToState } from "./mutation-error-map";

describe("crm mutation error mapping", () => {
  it("falls back to shared unknown action error message for non-api errors", () => {
    const state = mapCrmMutationErrorToState(new Error("boom"), "/");
    expect(state.status).toBe("error");
    expect(state.message).toBe("We could not complete this action. Please try again.");
  });
});
