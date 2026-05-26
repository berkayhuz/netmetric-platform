import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import { LinkPagination } from "../index";

describe("LinkPagination", () => {
  it("renders page links, active state, and previous/next controls", () => {
    render(
      <LinkPagination
        previous={{ href: "/items?page=1" }}
        next={{ href: "/items?page=3" }}
        items={[
          { key: "page-1", label: "1", href: "/items?page=1" },
          { key: "page-2", label: "2", href: "/items?page=2", active: true },
          { key: "ellipsis-0", label: "...", kind: "ellipsis" },
          { key: "page-9", label: "9", href: "/items?page=9" },
        ]}
      />,
    );

    expect(screen.getByRole("link", { name: "1" })).toHaveAttribute("href", "/items?page=1");
    expect(screen.getByRole("link", { name: "2" })).toHaveAttribute("aria-current", "page");
    expect(screen.getByRole("link", { name: /go to previous page/i })).toHaveAttribute(
      "href",
      "/items?page=1",
    );
    expect(screen.getByRole("link", { name: /go to next page/i })).toHaveAttribute(
      "href",
      "/items?page=3",
    );
    expect(screen.getByText("More pages")).toBeInTheDocument();
  });
});
