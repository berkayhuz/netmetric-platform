import { CriticalRouteWarmup } from "@netmetric/observability/performance";
import { act, render } from "@testing-library/react";
import { afterEach, describe, expect, it, vi } from "vitest";

describe("CriticalRouteWarmup integration", () => {
  afterEach(() => {
    vi.useRealTimers();
  });

  it("does not prefetch logout, API, side-effect, or current routes", async () => {
    vi.useFakeTimers();
    const prefetch = vi.fn();

    render(
      <CriticalRouteWarmup
        app="account-web"
        currentPath="/profile"
        initialDelayMs={0}
        idleTimeoutMs={0}
        prefetch={prefetch}
        routes={[
          { href: "/profile", label: "Current" },
          { href: "/api/auth/logout", label: "Logout" },
          { href: "/security/sessions/revoke", label: "Revoke" },
          { href: "/security", label: "Security" },
        ]}
        staggerMs={0}
      />,
    );

    await act(async () => {
      await vi.runAllTimersAsync();
    });

    expect(prefetch).toHaveBeenCalledTimes(1);
    expect(prefetch).toHaveBeenCalledWith("/security");
  });
});
