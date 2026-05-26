import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import { DataTable, type DataTableColumnDef } from "../client";

type Row = { id: string; name: string; status: string };

const rows: Row[] = [
  { id: "1", name: "Alpha", status: "active" },
  { id: "2", name: "Bravo", status: "inactive" },
];

const columns: DataTableColumnDef<Row>[] = [
  { accessorKey: "name", header: "Name" },
  { accessorKey: "status", header: "Status", meta: { disableReorder: true } },
];

describe("DataTable column reorder behavior", () => {
  it("keeps column reorder disabled by default", () => {
    const { container } = render(
      <DataTable
        data={rows}
        columns={columns}
        enablePagination={false}
        enableGlobalFilter={false}
      />,
    );

    expect(container.querySelectorAll('th[draggable="true"]').length).toBe(0);
  });

  it("supports opt-in column reorder and respects per-column reorder locks", () => {
    render(
      <DataTable
        data={rows}
        columns={columns}
        enablePagination={false}
        enableGlobalFilter={false}
        enableColumnReorder
        enableRowSelection
      />,
    );

    expect(screen.getByRole("columnheader", { name: "Name" })).toHaveAttribute("draggable", "true");
    expect(screen.getByRole("columnheader", { name: "Status" })).not.toHaveAttribute(
      "draggable",
      "true",
    );
  });

  it("does not inject a duplicate selection column when __select is provided", () => {
    const columnsWithSelect: DataTableColumnDef<Row>[] = [
      { id: "__select", header: "Select", cell: () => null, meta: { disableReorder: true } },
      { accessorKey: "name", header: "Name" },
    ];

    render(
      <DataTable
        data={rows}
        columns={columnsWithSelect}
        enablePagination={false}
        enableGlobalFilter={false}
        enableRowSelection
      />,
    );

    expect(screen.getAllByRole("columnheader", { name: "Select" })).toHaveLength(1);
  });
});
