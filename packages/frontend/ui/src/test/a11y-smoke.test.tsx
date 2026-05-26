import { render } from "@testing-library/react";
import { axe } from "jest-axe";
import { describe, expect, it } from "vitest";

import {
  DataTable,
  DataGrid,
  Dialog,
  DialogContent,
  DialogDescription,
  DialogTitle,
  DialogTrigger,
} from "../client";
import { AccessDeniedState } from "../components/feedback/state";
import { Button } from "../components/primitives/button";
import { Input } from "../components/primitives/input";

import type { ColumnDef } from "@tanstack/react-table";

type DataGridRow = { id: string; name: string };

const dataGridColumns: ColumnDef<DataGridRow, unknown>[] = [
  {
    accessorKey: "name",
    header: "Name",
  },
];

const dataTableColumns = [
  {
    accessorKey: "name",
    header: "Name",
  },
] satisfies Array<{ accessorKey: "name"; header: string }>;

describe("UI accessibility smoke", () => {
  it("Button has no obvious a11y violations", async () => {
    const { container } = render(<Button>Submit</Button>);
    const results = await axe(container);

    expect(results.violations).toHaveLength(0);
  });

  it("Input has no obvious a11y violations", async () => {
    const { container } = render(<Input aria-label="Username" />);
    const results = await axe(container);

    expect(results.violations).toHaveLength(0);
  });

  it("DataGrid has no obvious a11y violations", async () => {
    const { container } = render(
      <DataGrid
        columns={dataGridColumns}
        data={[
          { id: "1", name: "Acme" },
          { id: "2", name: "Globex" },
        ]}
      />,
    );
    const results = await axe(container);

    expect(results.violations).toHaveLength(0);
  });

  it("DataTable has no obvious a11y violations", async () => {
    const { container } = render(
      <DataTable
        columns={dataTableColumns}
        data={[
          { id: "1", name: "Acme" },
          { id: "2", name: "Globex" },
        ]}
      />,
    );
    const results = await axe(container);

    expect(results.violations).toHaveLength(0);
  });

  it("Dialog and state primitives have no obvious a11y violations", async () => {
    const { container } = render(
      <>
        <Dialog defaultOpen>
          <DialogTrigger>Open</DialogTrigger>
          <DialogContent>
            <DialogTitle>Dialog title</DialogTitle>
            <DialogDescription>Dialog description</DialogDescription>
          </DialogContent>
        </Dialog>
        <AccessDeniedState title="Access denied" description="You do not have permission." />
      </>,
    );
    const results = await axe(container);

    expect(results.violations).toHaveLength(0);
  });
});
