import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import Loading from "./loading";

describe("account loading skeleton", () => {
  it("renders an accessible busy state instead of a blank page", () => {
    render(<Loading />);

    const skeleton = screen.getByLabelText("Loading account workspace");
    expect(skeleton.getAttribute("aria-busy")).toBe("true");
    expect(screen.getByRole("status").textContent).toContain("Loading account workspace");
  });
});
