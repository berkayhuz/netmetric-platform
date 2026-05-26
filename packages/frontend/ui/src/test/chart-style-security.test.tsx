import { render } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import { ChartStyle } from "../components/data-display/chart";

describe("ChartStyle CSS serialization", () => {
  it("keeps chart color CSS constrained to safe variable keys and values", () => {
    const { container } = render(
      <ChartStyle
        id="chart-test"
        config={{
          revenue: { color: "var(--chart-1)" },
          "bad;key": { color: "red" },
          margin: { color: "red;} body { color: red" },
        }}
      />,
    );

    const css = container.querySelector("style")?.textContent ?? "";

    expect(css).toContain("--color-revenue: var(--chart-1);");
    expect(css).not.toContain("bad;key");
    expect(css).not.toContain("body {");
  });
});
