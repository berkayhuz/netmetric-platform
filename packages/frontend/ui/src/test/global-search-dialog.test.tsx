import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";

import { GlobalSearchDialog } from "../client";

const originalFetch = global.fetch;

describe("GlobalSearchDialog", () => {
  beforeEach(() => {
    Element.prototype.scrollIntoView = vi.fn();
    globalThis.ResizeObserver = class ResizeObserver {
      observe() {}
      unobserve() {}
      disconnect() {}
    };
  });

  afterEach(() => {
    global.fetch = originalFetch;
    vi.restoreAllMocks();
  });

  it("renders search results without exposing content fields", async () => {
    global.fetch = vi.fn(async () =>
      Response.json({
        query: "pricing",
        page: 1,
        pageSize: 8,
        totalCount: 1,
        items: [
          {
            id: "public-page-pricing",
            source: "Public",
            type: "page",
            title: "Pricing",
            summary: "Plan details",
            content: "Sensitive indexed content should stay hidden",
            url: "/pricing",
          },
        ],
      }),
    ) as typeof fetch;

    render(
      <GlobalSearchDialog
        open
        debounceMs={1}
        onOpenChange={() => {}}
        placeholder="Search"
        onNavigate={() => {}}
      />,
    );

    fireEvent.change(screen.getByTestId("global-search-input"), {
      target: {
        value: "pricing",
      },
    });

    await waitFor(() =>
      expect(screen.getByTestId("global-search-results")).toHaveAttribute(
        "data-search-state",
        "ready",
      ),
    );
    await waitFor(() => expect(screen.getByText("Pricing")).toBeInTheDocument());
    expect(screen.getByText("Plan details")).toBeInTheDocument();
    expect(
      screen.queryByText("Sensitive indexed content should stay hidden"),
    ).not.toBeInTheDocument();
  });

  it("does not navigate when a result URL is unsafe", async () => {
    const onNavigate = vi.fn();
    global.fetch = vi.fn(async () =>
      Response.json({
        query: "pricing",
        page: 1,
        pageSize: 8,
        totalCount: 1,
        items: [
          {
            id: "unsafe-pricing",
            source: "Public",
            type: "page",
            title: "Pricing",
            summary: "Plan details",
            url: "https://example.com/pricing",
          },
        ],
      }),
    ) as typeof fetch;

    render(
      <GlobalSearchDialog
        open
        debounceMs={1}
        onOpenChange={() => {}}
        placeholder="Search"
        onNavigate={onNavigate}
      />,
    );

    fireEvent.change(screen.getByTestId("global-search-input"), {
      target: {
        value: "pricing",
      },
    });

    await waitFor(() =>
      expect(screen.getByTestId("global-search-results")).toHaveAttribute(
        "data-search-state",
        "ready",
      ),
    );
    await waitFor(() => expect(screen.getByText("Pricing")).toBeInTheDocument());
    expect(screen.getByText("Unsupported public link")).toBeInTheDocument();
    expect(screen.getByTestId("global-search-result-disabled")).toBeInTheDocument();

    fireEvent.click(screen.getByText("Pricing"));
    expect(onNavigate).not.toHaveBeenCalled();
  });

  it("shows loading state while the request is in progress", async () => {
    let resolveFetch: ((response: Response) => void) | undefined;
    global.fetch = vi.fn(
      () =>
        new Promise<Response>((resolve) => {
          resolveFetch = resolve;
        }),
    ) as typeof fetch;

    render(
      <GlobalSearchDialog
        open
        debounceMs={1}
        onOpenChange={() => {}}
        placeholder="Search"
        onNavigate={() => {}}
      />,
    );

    fireEvent.change(screen.getByTestId("global-search-input"), {
      target: {
        value: "pricing",
      },
    });

    await waitFor(() =>
      expect(screen.getByTestId("global-search-results")).toHaveAttribute(
        "data-search-state",
        "loading",
      ),
    );
    expect(screen.getByTestId("global-search-loading")).toBeInTheDocument();

    if (!resolveFetch) {
      throw new Error("Fetch resolver was not assigned.");
    }

    resolveFetch(
      Response.json({
        query: "pricing",
        page: 1,
        pageSize: 8,
        totalCount: 1,
        items: [
          {
            id: "public-page-pricing",
            source: "Public",
            type: "page",
            title: "Pricing",
            summary: "Plan details",
            url: "/pricing",
          },
        ],
      }),
    );

    await waitFor(() =>
      expect(screen.getByTestId("global-search-results")).toHaveAttribute(
        "data-search-state",
        "ready",
      ),
    );
  });

  it("shows empty state when the response has no items", async () => {
    global.fetch = vi.fn(async () =>
      Response.json({
        query: "missing",
        page: 1,
        pageSize: 8,
        totalCount: 0,
        items: [],
      }),
    ) as typeof fetch;

    render(
      <GlobalSearchDialog
        open
        debounceMs={1}
        onOpenChange={() => {}}
        placeholder="Search"
        onNavigate={() => {}}
      />,
    );

    fireEvent.change(screen.getByTestId("global-search-input"), {
      target: {
        value: "missing",
      },
    });

    await waitFor(() =>
      expect(screen.getByTestId("global-search-results")).toHaveAttribute(
        "data-search-state",
        "empty",
      ),
    );
    expect(screen.getByTestId("global-search-empty")).toBeInTheDocument();
  });

  it("shows error state when search request fails", async () => {
    global.fetch = vi.fn(async () => new Response("unavailable", { status: 503 })) as typeof fetch;

    render(
      <GlobalSearchDialog
        open
        debounceMs={1}
        onOpenChange={() => {}}
        placeholder="Search"
        onNavigate={() => {}}
      />,
    );

    fireEvent.change(screen.getByTestId("global-search-input"), {
      target: {
        value: "pricing",
      },
    });

    await waitFor(() =>
      expect(screen.getByTestId("global-search-results")).toHaveAttribute(
        "data-search-state",
        "error",
      ),
    );
    expect(screen.getByTestId("global-search-error")).toBeInTheDocument();
  });

  it("appends the canonical active locale to search requests", async () => {
    global.fetch = vi.fn(async () =>
      Response.json({
        query: "customers",
        page: 1,
        pageSize: 8,
        totalCount: 0,
        items: [],
      }),
    ) as typeof fetch;

    render(
      <GlobalSearchDialog
        open
        debounceMs={1}
        locale="tr"
        onOpenChange={() => {}}
        placeholder="Search"
        onNavigate={() => {}}
      />,
    );

    fireEvent.change(screen.getByTestId("global-search-input"), {
      target: {
        value: "customers",
      },
    });

    await waitFor(() => expect(global.fetch).toHaveBeenCalled());
    const [requestUrl] = vi.mocked(global.fetch).mock.calls[0] ?? [];
    expect(String(requestUrl)).toContain("locale=tr-TR");
  });
});
