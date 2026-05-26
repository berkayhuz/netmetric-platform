import React from "react";
import { renderToStaticMarkup } from "react-dom/server";
import { describe, expect, it } from "vitest";

import Loading from "./loading";

describe("CRM loading skeleton", () => {
  it("renders an accessible busy state instead of a blank page", () => {
    const markup = renderToStaticMarkup(React.createElement(Loading));

    expect(markup).toContain('aria-busy="true"');
    expect(markup).toContain("Loading CRM workspace");
    expect(markup).toContain('role="status"');
  });
});
