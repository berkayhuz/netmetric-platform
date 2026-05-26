import type { ReactNode } from "react";

export function ToolsShell({ children }: { children: ReactNode }) {
  return <div className="mx-auto w-full max-w-6xl px-4 py-8 sm:px-6 lg:px-8">{children}</div>;
}
