import type { ReactNode } from "react";

export function ToolbarSurface({
  title,
  description,
  children,
}: Readonly<{
  title: string;
  description: string;
  children: ReactNode;
}>) {
  return (
    <section
      className="rounded-2xl border border-border/70 bg-card/95 p-4 shadow-[0_18px_55px_rgb(15_23_42_/_0.08)] backdrop-blur"
      aria-label={title}
    >
      <div className="mb-4 flex flex-col gap-1">
        <h2 className="text-sm font-semibold">{title}</h2>
        <p className="text-sm text-muted-foreground">{description}</p>
      </div>
      {children}
    </section>
  );
}
