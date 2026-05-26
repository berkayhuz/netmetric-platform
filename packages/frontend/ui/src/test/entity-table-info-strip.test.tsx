import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import { EntityTableInfoStrip } from "../index";

describe("EntityTableInfoStrip", () => {
  it("renders as a compact shared summary strip shell", () => {
    const { container } = render(
      <EntityTableInfoStrip>
        <span>Products: 12</span>
      </EntityTableInfoStrip>,
    );

    expect(screen.getByText("Products: 12")).toBeInTheDocument();
    expect(container.firstChild).toHaveClass("flex");
    expect(container.firstChild).toHaveClass("gap-x-3");
    expect(container.firstChild).toHaveClass("text-xs");
  });
});
