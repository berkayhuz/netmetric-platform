import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import { EmptyTitle } from "../components/data-display/empty";

describe("EmptyTitle", () => {
  it("does not render when children are missing", () => {
    const { container } = render(<EmptyTitle />);
    expect(container.querySelector("[data-slot='empty-title']")).not.toBeNull();
  });

  it("does not render for empty text content", () => {
    const { container } = render(<EmptyTitle>{"   "}</EmptyTitle>);
    expect(container.querySelector("[data-slot='empty-title']")).not.toBeNull();
  });

  it("renders title text content", () => {
    render(<EmptyTitle>Nothing here yet</EmptyTitle>);
    expect(screen.getByText("Nothing here yet")).toBeInTheDocument();
  });

  it("forwards aria-label", () => {
    render(
      <EmptyTitle aria-label="No notifications">
        <span aria-hidden="true">!</span>
      </EmptyTitle>,
    );

    expect(screen.getByLabelText("No notifications")).toBeInTheDocument();
  });

  it("forwards title without text content", () => {
    render(
      <EmptyTitle title="No results yet">
        <span aria-hidden="true">!</span>
      </EmptyTitle>,
    );

    expect(screen.getByTitle("No results yet")).toBeInTheDocument();
  });

  it("keeps className", () => {
    render(<EmptyTitle className="custom-empty-title">Nothing here yet</EmptyTitle>);
    expect(screen.getByText("Nothing here yet")).toHaveClass("custom-empty-title");
  });
});
