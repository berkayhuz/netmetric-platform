import { describe, expect, it } from "vitest";

import { crmModuleIconKeys, crmModuleRegistry } from "@/features/modules/module-registry";

import { crmNavIconColors, crmNavIcons } from "./crm-nav-icons";

describe("crm nav icons", () => {
  it("assigns an icon and fixed color for every CRM module icon key and registry item", () => {
    const keyedIcons = crmModuleIconKeys.map((iconKey) => crmNavIcons[iconKey]);
    const keyedIconColors = crmModuleIconKeys.map((iconKey) => crmNavIconColors[iconKey]);
    const icons = crmModuleRegistry.map((moduleItem) => crmNavIcons[moduleItem.iconKey]);
    const iconColors = crmModuleRegistry.map((moduleItem) => crmNavIconColors[moduleItem.iconKey]);

    expect(keyedIcons).toHaveLength(crmModuleIconKeys.length);
    expect(keyedIconColors).toHaveLength(crmModuleIconKeys.length);
    expect(keyedIcons.every((iconComponent) => iconComponent !== undefined)).toBe(true);
    expect(keyedIconColors.every((colorClass) => colorClass.startsWith("text-"))).toBe(true);
    expect(icons).toHaveLength(crmModuleRegistry.length);
    expect(iconColors).toHaveLength(crmModuleRegistry.length);
    expect(icons.every((iconComponent) => iconComponent !== undefined)).toBe(true);
    expect(iconColors.every((colorClass) => colorClass.startsWith("text-"))).toBe(true);
  });
});
