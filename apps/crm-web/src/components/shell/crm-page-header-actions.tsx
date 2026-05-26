import Link from "next/link";
import type { ComponentProps, ReactNode } from "react";

import { Button, cn } from "@netmetric/ui";

type CrmHeaderActionVariant = "default" | "outline" | "secondary";

const CRM_HEADER_ACTION_BUTTON_SIZE_CLASS =
  "[&_[data-slot='button']]:h-8 [&_[data-slot='button']]:gap-1 [&_[data-slot='button']]:px-2.5 [&_[data-slot='button']]:text-[0.8rem] [&_[data-slot='button']]:[&_svg:not([class*='size-'])]:size-3.5";

export function CrmPageHeaderActions({
  children,
  className,
}: Readonly<{
  children?: ReactNode;
  className?: string;
}>) {
  return (
    <div
      className={cn(
        "flex flex-wrap items-center justify-end gap-2",
        CRM_HEADER_ACTION_BUTTON_SIZE_CLASS,
        className,
      )}
    >
      {children}
    </div>
  );
}

export function CrmPageHeaderActionScope({
  children,
  className,
}: Readonly<{
  children?: ReactNode;
  className?: string;
}>) {
  return (
    <div
      className={cn(
        "flex items-center justify-end",
        CRM_HEADER_ACTION_BUTTON_SIZE_CLASS,
        className,
      )}
    >
      {children}
    </div>
  );
}

type CrmPageHeaderActionButtonProps = Omit<ComponentProps<typeof Button>, "size" | "variant"> & {
  variant?: CrmHeaderActionVariant;
};

export function CrmPageHeaderActionButton({
  children,
  variant = "default",
  className,
  ...props
}: Readonly<CrmPageHeaderActionButtonProps>) {
  return (
    <Button size="sm" variant={variant} className={className} {...props}>
      {children}
    </Button>
  );
}

export function CrmPageHeaderActionLink({
  href,
  label,
  icon,
  variant = "default",
  className,
  ...props
}: Readonly<{
  href: string;
  label: string;
  icon?: ReactNode;
  variant?: CrmHeaderActionVariant;
  className?: string;
}>) {
  return (
    <CrmPageHeaderActionButton asChild variant={variant} className={className} {...props}>
      <Link href={href} prefetch={false}>
        {icon}
        {label}
      </Link>
    </CrmPageHeaderActionButton>
  );
}
