import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import { EntityDataTable, type DataTableColumnDef } from "../client";

type Row = {
  id: string;
  name: string;
  status: string;
};

const rows: Row[] = [
  { id: "1", name: "Alpha", status: "active" },
  { id: "2", name: "Bravo", status: "inactive" },
];

const columns: DataTableColumnDef<Row>[] = [
  { accessorKey: "name", header: "Name", meta: { label: "Name" } },
  { accessorKey: "status", header: "Status", meta: { label: "Status" } },
];

describe("EntityDataTable smoke", () => {
  it("renders the wrapped DataTable and supports search by default", () => {
    render(<EntityDataTable columns={columns} data={rows} />);

    fireEvent.change(screen.getByRole("textbox", { name: /search rows/i }), {
      target: { value: "Bravo" },
    });

    expect(screen.getByText("Bravo")).toBeInTheDocument();
    expect(screen.queryByText("Alpha")).not.toBeInTheDocument();
  });

  it("supports feature opt-outs", () => {
    render(
      <EntityDataTable
        columns={columns}
        data={[]}
        enableSearch={false}
        enablePagination={false}
        enableColumnVisibility={false}
        showEmptyState={false}
      />,
    );

    expect(screen.queryByRole("textbox", { name: /search rows/i })).not.toBeInTheDocument();
    expect(screen.queryByText("No results")).not.toBeInTheDocument();
    expect(screen.queryByRole("button", { name: /view columns/i })).not.toBeInTheDocument();
  });
});
