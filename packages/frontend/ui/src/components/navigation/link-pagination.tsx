import * as React from "react";

import {
  Pagination,
  PaginationContent,
  PaginationEllipsis,
  PaginationItem,
  PaginationLink,
  PaginationNext,
  PaginationPrevious,
} from "./pagination";

export type LinkPaginationControl = {
  href: string;
  ariaLabel?: string;
  text?: string;
};

export type LinkPaginationItem = {
  key: string;
  label: React.ReactNode;
  href?: string;
  active?: boolean;
  kind?: "page" | "ellipsis";
};

export type LinkPaginationProps = Readonly<{
  previous?: LinkPaginationControl;
  next?: LinkPaginationControl;
  items: LinkPaginationItem[];
}>;

export function LinkPagination({ previous, next, items }: LinkPaginationProps) {
  if (items.length === 0 && !previous && !next) {
    return null;
  }

  return (
    <Pagination>
      <PaginationContent>
        {previous ? (
          <PaginationItem>
            <PaginationPrevious
              href={previous.href}
              aria-label={previous.ariaLabel ?? "Go to previous page"}
              {...(previous.text !== undefined ? { text: previous.text } : {})}
            />
          </PaginationItem>
        ) : null}

        {items.map((item) => (
          <PaginationItem key={item.key}>
            {item.kind === "ellipsis" ? (
              <PaginationEllipsis />
            ) : (
              <PaginationLink
                href={item.href ?? "#"}
                {...(item.active !== undefined ? { isActive: item.active } : {})}
              >
                {item.label}
              </PaginationLink>
            )}
          </PaginationItem>
        ))}

        {next ? (
          <PaginationItem>
            <PaginationNext
              href={next.href}
              aria-label={next.ariaLabel ?? "Go to next page"}
              {...(next.text !== undefined ? { text: next.text } : {})}
            />
          </PaginationItem>
        ) : null}
      </PaginationContent>
    </Pagination>
  );
}
