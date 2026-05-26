import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import {
  AccessDeniedState,
  EmptyState,
  ErrorState,
  LoadingState,
  NotFoundState,
} from "../components/feedback/state";

describe("shared state components", () => {
  it("renders semantic roles for loading and error states", () => {
    render(
      <>
        <LoadingState title="Loading" description="Please wait" />
        <ErrorState title="Error" description="Retry later" />
      </>,
    );

    expect(screen.getAllByRole("status").length).toBeGreaterThan(0);
    expect(screen.getByRole("alert")).toBeTruthy();
  });

  it("renders actions when links are provided", () => {
    render(
      <>
        <EmptyState
          title="Empty"
          description="No records"
          actionLabel="Retry"
          actionHref="/retry"
        />
        <AccessDeniedState
          title="Denied"
          description="No permission"
          actionLabel="Back"
          actionHref="/"
        />
        <NotFoundState title="Not found" description="Missing" actionLabel="Home" actionHref="/" />
      </>,
    );

    expect(screen.getByRole("link", { name: "Retry" })).toHaveAttribute("href", "/retry");
    expect(screen.getByRole("link", { name: "Back" })).toHaveAttribute("href", "/");
    expect(screen.getByRole("link", { name: "Home" })).toHaveAttribute("href", "/");
  });
});
