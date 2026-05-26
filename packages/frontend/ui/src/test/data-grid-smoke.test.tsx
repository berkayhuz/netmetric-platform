import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

import { DataGrid } from "../client";

import type { ColumnDef } from "@tanstack/react-table";

type GridRow = {
  id: string;
  name: string;
  team: string;
};

const rows: GridRow[] = [
  { id: "1", name: "Charlie", team: "Support" },
  { id: "2", name: "Alice", team: "Sales" },
  { id: "3", name: "Bob", team: "Ops" },
];

const columns: ColumnDef<GridRow, unknown>[] = [
  {
    accessorKey: "name",
    header: "Name",
  },
  {
    accessorKey: "team",
    header: "Team",
  },
];

describe("DataGrid smoke", () => {
  it("renders rows", () => {
    render(<DataGrid columns={columns} data={rows} />);

    expect(screen.getByRole("table")).toBeInTheDocument();
    expect(screen.getByText("Charlie")).toBeInTheDocument();
    expect(screen.getByText("Alice")).toBeInTheDocument();
  });

  it("supports sorting interaction", () => {
    render(<DataGrid columns={columns} data={rows} />);

    fireEvent.click(screen.getByRole("button", { name: /name/i }));

    const renderedNames = screen
      .getAllByRole("cell")
      .map((cell) => cell.textContent)
      .filter((value): value is string => Boolean(value));

    expect(renderedNames.indexOf("Alice")).toBeLessThan(renderedNames.indexOf("Charlie"));
  });

  it("calls controlled sorting callback", () => {
    const onSortingChange = vi.fn();
    render(
      <DataGrid columns={columns} data={rows} sorting={[]} onSortingChange={onSortingChange} />,
    );

    fireEvent.click(screen.getByRole("button", { name: /name/i }));

    expect(onSortingChange).toHaveBeenCalled();
  });

  it("supports row selection", () => {
    render(<DataGrid columns={columns} data={rows} enableRowSelection />);

    const rowCheckbox = screen.getAllByRole("checkbox", { name: /select row /i }).at(0);
    expect(rowCheckbox).toBeDefined();
    if (!rowCheckbox) {
      return;
    }
    fireEvent.click(rowCheckbox);

    expect(rowCheckbox).toHaveAttribute("aria-checked", "true");
  });

  it("renders row actions", () => {
    render(
      <DataGrid
        columns={columns}
        data={rows}
        renderRowActions={(row) => <button type="button">Actions {row.original.name}</button>}
      />,
    );

    expect(screen.getByText("Actions Charlie")).toBeInTheDocument();
    expect(screen.getByRole("columnheader", { name: "Actions" })).toBeInTheDocument();
  });

  it("renders bulk actions with selected rows", () => {
    render(
      <DataGrid
        columns={columns}
        data={rows}
        enableRowSelection
        renderBulkActions={(selectedRows) => (
          <button type="button">Bulk ({selectedRows.length})</button>
        )}
      />,
    );

    const rowCheckbox = screen.getAllByRole("checkbox", { name: /select row /i }).at(0);
    expect(rowCheckbox).toBeDefined();
    if (!rowCheckbox) {
      return;
    }

    fireEvent.click(rowCheckbox);
    expect(screen.getByRole("button", { name: "Bulk (1)" })).toBeInTheDocument();
  });

  it("renders loading, empty and error states", () => {
    const { rerender } = render(<DataGrid columns={columns} data={[]} loading />);
    expect(screen.getByRole("textbox", { name: /search rows/i })).toBeInTheDocument();

    rerender(<DataGrid columns={columns} data={[]} />);
    expect(screen.getByText("No results")).toBeInTheDocument();

    rerender(<DataGrid columns={columns} data={[]} error="Request failed" />);
    expect(screen.getByText("Could not load data")).toBeInTheDocument();
    expect(screen.getByText("Request failed")).toBeInTheDocument();
  });

  it("exposes sortable aria-sort semantics", () => {
    render(<DataGrid columns={columns} data={rows} />);

    const nameHeader = screen.getByRole("columnheader", { name: /name/i });
    expect(nameHeader).toHaveAttribute("aria-sort", "none");

    fireEvent.click(screen.getByRole("button", { name: /name/i }));
    expect(nameHeader).toHaveAttribute("aria-sort", "ascending");
  });

  it("exposes loading and error semantics", () => {
    const { rerender } = render(<DataGrid columns={columns} data={[]} loading />);

    expect(screen.getByRole("table")).toHaveAttribute("aria-busy", "true");
    expect(screen.getByRole("status")).toBeInTheDocument();

    rerender(<DataGrid columns={columns} data={[]} error="Failed to load" />);
    expect(screen.getByRole("alert")).toBeInTheDocument();
  });

  it("supports controlled server-mode pagination contract", () => {
    const onPaginationChange = vi.fn();
    render(
      <DataGrid
        columns={columns}
        data={[{ id: "server-1", name: "Server Row", team: "Ops" }]}
        mode="server"
        totalRows={3}
        pagination={{ pageIndex: 0, pageSize: 1 }}
        onPaginationChange={onPaginationChange}
      />,
    );

    fireEvent.click(screen.getByRole("link", { name: /go to next page/i }));
    expect(onPaginationChange).toHaveBeenCalled();
  });
});
