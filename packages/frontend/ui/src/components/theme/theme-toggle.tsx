"use client";

import { Moon, Sun } from "lucide-react";
import * as React from "react";

import { Button } from "../primitives/button";

import { useTheme } from "./theme-provider";

export function ThemeToggle() {
  const { theme, resolvedTheme, setTheme } = useTheme();

  const nextTheme = resolvedTheme === "dark" ? "light" : "dark";
  const ariaLabel =
    theme === "system"
      ? `Switch theme from system (${resolvedTheme}) to ${nextTheme}`
      : `Switch theme to ${nextTheme}`;

  const onToggle = React.useCallback(() => {
    setTheme(nextTheme);
  }, [nextTheme, setTheme]);

  return (
    <Button type="button" variant="outline" size="icon" aria-label={ariaLabel} onClick={onToggle}>
      {resolvedTheme === "dark" ? <Sun /> : <Moon />}
    </Button>
  );
}
