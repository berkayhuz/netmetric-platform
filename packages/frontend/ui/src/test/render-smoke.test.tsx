import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import * as clientEntry from "../client";
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from "../components/overlay/tooltip";
import { Button } from "../components/primitives/button";
import { Input } from "../components/primitives/input";

describe("UI render smoke", () => {
  it("renders Button", () => {
    render(<Button>Save</Button>);

    expect(screen.getByRole("button", { name: "Save" })).toBeInTheDocument();
  });

  it("renders Input", () => {
    render(<Input aria-label="Email" placeholder="Email" />);

    expect(screen.getByRole("textbox", { name: "Email" })).toBeInTheDocument();
  });

  it("renders Tooltip primitives", () => {
    render(
      <TooltipProvider>
        <Tooltip>
          <TooltipTrigger>More</TooltipTrigger>
          <TooltipContent>Details</TooltipContent>
        </Tooltip>
      </TooltipProvider>,
    );

    expect(screen.getByRole("button", { name: "More" })).toBeInTheDocument();
  });
});

describe("client entry smoke", () => {
  it("exports representative client symbols", () => {
    expect("Tooltip" in clientEntry).toBe(true);
    expect("Checkbox" in clientEntry).toBe(true);
    expect("DataGrid" in clientEntry).toBe(true);
    expect("Button" in clientEntry).toBe(false);
    expect("Input" in clientEntry).toBe(false);
  });
});
