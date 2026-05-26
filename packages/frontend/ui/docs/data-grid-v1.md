# DataGrid V1

## Decision Note

New table work should use DataTable/EntityDataTable. DataGrid is a legacy/deprecated candidate until removed after usage verification.

## Scope

- Headless + composable client-side data grid foundation for CRM/SaaS lists.
- Generic and type-safe API (`DataGrid<TData>`).
- Supports:
  - sorting
  - column filters/global filter state wiring
  - pagination
  - row selection
  - column visibility
  - row actions and bulk actions render hooks
  - loading/empty/error rendering hooks
  - client mode and server mode contract

## Out of Scope

- virtualization
- pinned columns
- resize/reorder
- CSV export
- grouping/tree data
- infinite scroll
- CRM domain presets

## Server vs Client Mode

- `mode="client"`:
  - local sorting/filtering/pagination row models are used.
- `mode="server"`:
  - table state is controlled externally.
  - `totalRows` should represent backend total for pagination display.
  - if `totalRows` is omitted, DataGrid falls back to `data.length` and emits a development warning.
  - data fetching and query orchestration stays in app layer.

## Performance Notes

- Keep `columns` and `data` references memoized when possible to reduce avoidable rerenders.
- Prefer `mode="server"` for large datasets and backend-driven query workflows.
- Virtualization remains out of V1 scope.

## Export Usage

- Runtime DataGrid components are exported from `@netmetric/ui/client`.
- Type exports are available from `@netmetric/ui` and `@netmetric/ui/client`.
- Internal helpers under `data-grid-internal.ts` are not public API.

## Minimal Example

```tsx
import type { ColumnDef } from "@tanstack/react-table";
import { DataGrid } from "@netmetric/ui/client";

type UserRow = { id: string; name: string; email: string };

const columns: ColumnDef<UserRow, unknown>[] = [
  { accessorKey: "name", header: "Name" },
  { accessorKey: "email", header: "Email" },
];

export function UsersGrid({ rows }: { rows: UserRow[] }) {
  return <DataGrid columns={columns} data={rows} enableRowSelection />;
}
```
