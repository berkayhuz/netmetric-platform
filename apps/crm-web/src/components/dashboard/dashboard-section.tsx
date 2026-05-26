export function DashboardSection({
  title,
  description,
  children,
}: Readonly<{
  title: string;
  description: string;
  children: React.ReactNode;
}>) {
  return (
    <section className="space-y-4" aria-label={title}>
      <header className="flex flex-col gap-1 border-b pb-3">
        <h2 className="text-base font-semibold tracking-tight">{title}</h2>
        <p className="max-w-3xl text-sm leading-6 text-muted-foreground">{description}</p>
      </header>
      {children}
    </section>
  );
}
