import { describe, expect, it } from "vitest";

import { mapMutationErrorToState } from "./mutation-error-map";

describe("account mutation error mapping", () => {
  it("falls back to shared unknown action error message for non-api errors", () => {
    const state = mapMutationErrorToState(new Error("boom"), "/settings");
    expect(state.status).toBe("error");
    expect(state.message).toBe("We could not complete this action. Please try again.");
  });
});
