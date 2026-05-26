import { render, screen } from "@testing-library/react";
import { beforeAll, describe, expect, it, vi } from "vitest";

import { DataGrid } from "../components/data-display/data-grid";
import {
  Sidebar,
  SidebarContent,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarProvider,
} from "../components/navigation/sidebar";
import { Dialog, DialogContent, DialogTitle } from "../components/overlay/dialog";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "../components/overlay/dropdown-menu";
import { Button } from "../components/primitives/button";
import { Input } from "../components/primitives/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "../components/primitives/select";
import { ThemeProvider, useTheme } from "../components/theme/theme-provider";

import type { ColumnDef } from "@tanstack/react-table";

beforeAll(() => {
  Object.defineProperty(window, "matchMedia", {
    writable: true,
    value: vi.fn().mockImplementation(() => ({
      matches: false,
      media: "(prefers-color-scheme: dark)",
      onchange: null,
      addListener: vi.fn(),
      removeListener: vi.fn(),
      addEventListener: vi.fn(),
      removeEventListener: vi.fn(),
      dispatchEvent: vi.fn(),
    })),
  });
});

type Row = { id: string; name: string };

const columns: ColumnDef<Row, unknown>[] = [{ accessorKey: "name", header: "Name" }];
const data: Row[] = [{ id: "1", name: "Alice" }];

function ThemeProbe() {
  const { theme } = useTheme();
  return <span data-testid="theme-probe">{theme}</span>;
}

describe("coverage target components", () => {
  it("renders Button and Input", () => {
    render(
      <div>
        <Button>Save</Button>
        <Input aria-label="Email" />
      </div>,
    );

    expect(screen.getByRole("button", { name: "Save" })).toBeInTheDocument();
    expect(screen.getByRole("textbox", { name: "Email" })).toBeInTheDocument();
  });

  it("renders Select primitive composition", () => {
    render(
      <Select defaultValue="istanbul">
        <SelectTrigger aria-label="City">
          <SelectValue placeholder="Select a city" />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value="istanbul">Istanbul</SelectItem>
        </SelectContent>
      </Select>,
    );

    expect(screen.getByRole("combobox", { name: "City" })).toBeInTheDocument();
  });

  it("renders Dialog content", () => {
    render(
      <Dialog open>
        <DialogContent>
          <DialogTitle>Dialog title</DialogTitle>
        </DialogContent>
      </Dialog>,
    );

    expect(screen.getByText("Dialog title")).toBeInTheDocument();
  });

  it("renders Dropdown menu content", () => {
    render(
      <DropdownMenu open>
        <DropdownMenuTrigger>Open menu</DropdownMenuTrigger>
        <DropdownMenuContent>
          <DropdownMenuItem>Profile</DropdownMenuItem>
        </DropdownMenuContent>
      </DropdownMenu>,
    );

    expect(screen.getByText("Profile")).toBeInTheDocument();
  });

  it("renders Sidebar layout", () => {
    render(
      <SidebarProvider>
        <Sidebar collapsible="none">
          <SidebarContent>
            <SidebarMenu>
              <SidebarMenuItem>
                <SidebarMenuButton>Dashboard</SidebarMenuButton>
              </SidebarMenuItem>
            </SidebarMenu>
          </SidebarContent>
        </Sidebar>
      </SidebarProvider>,
    );

    expect(screen.getByText("Dashboard")).toBeInTheDocument();
  });

  it("renders ThemeProvider hook consumer", () => {
    render(
      <ThemeProvider defaultTheme="light">
        <ThemeProbe />
      </ThemeProvider>,
    );

    expect(screen.getByTestId("theme-probe")).toHaveTextContent("light");
  });

  it("honors forcedTheme over defaultTheme", () => {
    render(
      <ThemeProvider defaultTheme="dark" forcedTheme="light">
        <ThemeProbe />
      </ThemeProvider>,
    );

    expect(screen.getByTestId("theme-probe")).toHaveTextContent("light");
  });

  it("renders DataGrid", () => {
    render(<DataGrid columns={columns} data={data} />);

    expect(screen.getByRole("table")).toBeInTheDocument();
    expect(screen.getByText("Alice")).toBeInTheDocument();
  });
});
