import { describe, expect, it } from "vitest";

import { accountNavIconColors, accountNavIcons } from "./account-nav-icons";
import { accountRoutes } from "./account-routes";

describe("account nav icons", () => {
  it("assigns a distinct icon and fixed color to every account route", () => {
    const icons = accountRoutes.map((route) => accountNavIcons[route.href]);
    const iconColors = accountRoutes.map((route) => accountNavIconColors[route.href]);

    expect(icons).toHaveLength(accountRoutes.length);
    expect(iconColors).toHaveLength(accountRoutes.length);
    expect(icons.every((iconComponent) => iconComponent !== undefined)).toBe(true);
    expect(iconColors.every((colorClass) => colorClass.startsWith("text-"))).toBe(true);
    expect(new Set(icons).size).toBe(accountRoutes.length);
    expect(new Set(iconColors).size).toBe(accountRoutes.length);
  });
});
