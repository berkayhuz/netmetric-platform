import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

import { DataTable, type DataTableColumnDef } from "../client";

type TableRow = {
  id: string;
  name: string;
  team: string;
  active: boolean;
};

const rows: TableRow[] = [
  { id: "1", name: "Charlie", team: "Support", active: true },
  { id: "2", name: "Alice", team: "Sales", active: false },
  { id: "3", name: "Bob", team: "Ops", active: true },
];

const columns: DataTableColumnDef<TableRow>[] = [
  {
    accessorKey: "name",
    header: "Name",
    meta: { label: "Name" },
  },
  {
    accessorKey: "team",
    header: "Team",
    meta: { label: "Team" },
  },
  {
    id: "active",
    accessorFn: (row) => String(row.active),
    header: "Status",
    cell: ({ row }) => (row.original.active ? "Active" : "Inactive"),
    filterFn: (row, id, value) => {
      const values = Array.isArray(value) ? value : [];
      return values.length === 0 || values.includes(row.getValue(id));
    },
    meta: { label: "Status" },
  },
];

describe("DataTable smoke", () => {
  it("renders rows and supports global filtering", () => {
    render(<DataTable columns={columns} data={rows} />);

    expect(screen.getByRole("table")).toBeInTheDocument();
    expect(screen.getByText("Charlie")).toBeInTheDocument();

    fireEvent.change(screen.getByRole("textbox", { name: /search rows/i }), {
      target: { value: "Alice" },
    });

    expect(screen.getByText("Alice")).toBeInTheDocument();
    expect(screen.queryByText("Charlie")).not.toBeInTheDocument();
  });

  it("supports sorting interaction", () => {
    render(<DataTable columns={columns} data={rows} />);

    fireEvent.click(screen.getByRole("button", { name: /sort by name/i }));

    const renderedCells = screen
      .getAllByRole("cell")
      .map((cell) => cell.textContent)
      .filter((value): value is string => Boolean(value));

    expect(renderedCells.indexOf("Alice")).toBeLessThan(renderedCells.indexOf("Charlie"));
  });

  it("supports controlled server-mode callbacks", () => {
    const onSortingChange = vi.fn();
    const onPaginationChange = vi.fn();

    render(
      <DataTable
        columns={columns}
        data={rows.slice(0, 1)}
        mode="server"
        totalRows={3}
        sorting={[]}
        pagination={{ pageIndex: 0, pageSize: 1 }}
        onSortingChange={onSortingChange}
        onPaginationChange={onPaginationChange}
      />,
    );

    fireEvent.click(screen.getByRole("button", { name: /sort by name/i }));
    fireEvent.click(screen.getByRole("button", { name: /go to next page/i }));

    expect(onSortingChange).toHaveBeenCalled();
    expect(onPaginationChange).toHaveBeenCalled();
  });

  it("renders selection, faceted filters, and view options controls", () => {
    render(
      <DataTable
        columns={columns}
        data={rows}
        enableRowSelection
        facetedFilters={[
          {
            columnId: "active",
            title: "Status",
            options: [
              { label: "Active", value: "true" },
              { label: "Inactive", value: "false" },
            ],
          },
        ]}
      />,
    );

    expect(screen.getByRole("button", { name: /^status$/i })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /view columns/i })).toBeInTheDocument();
    expect(screen.getAllByRole("checkbox", { name: /select row/i })).toHaveLength(3);
  });

  it("renders loading, empty, and error states", () => {
    const { rerender } = render(<DataTable columns={columns} data={[]} loading />);
    expect(screen.getByRole("table")).toHaveAttribute("aria-busy", "true");

    rerender(<DataTable columns={columns} data={[]} />);
    expect(screen.getByText("No results")).toBeInTheDocument();

    rerender(<DataTable columns={columns} data={[]} error="Request failed" />);
    expect(screen.getByRole("alert")).toBeInTheDocument();
    expect(screen.getByText("Request failed")).toBeInTheDocument();
  });
});
