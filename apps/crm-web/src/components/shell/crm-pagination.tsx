import { LinkPagination, type LinkPaginationItem } from "@netmetric/ui";

function withPage(url: URL, page: number): string {
  const copy = new URL(url.toString());
  copy.searchParams.set("page", String(page));
  return `${copy.pathname}${copy.search}`;
}

function buildPageItems(currentPage: number, totalPages: number): Array<number | "ellipsis"> {
  if (totalPages <= 7) {
    return Array.from({ length: totalPages }, (_, index) => index + 1);
  }

  const pages = new Set<number>([1, totalPages, currentPage, currentPage - 1, currentPage + 1]);
  for (const value of [...pages]) {
    if (value < 1 || value > totalPages) pages.delete(value);
  }

  const sorted = [...pages].sort((a, b) => a - b);
  const output: Array<number | "ellipsis"> = [];
  for (let index = 0; index < sorted.length; index += 1) {
    const current = sorted[index]!;
    const previous = sorted[index - 1];
    if (previous && current - previous > 1) {
      output.push("ellipsis");
    }
    output.push(current);
  }

  return output;
}

export function CrmPagination({
  currentPage,
  totalPages,
  basePath,
  currentQuery,
}: Readonly<{
  currentPage: number;
  totalPages: number;
  basePath: string;
  currentQuery: URLSearchParams;
}>) {
  if (totalPages <= 1) {
    return null;
  }

  const baseUrl = new URL(`http://localhost${basePath}`);
  for (const [key, value] of currentQuery.entries()) {
    if (key === "page") {
      continue;
    }

    baseUrl.searchParams.set(key, value);
  }

  const prevPage = Math.max(1, currentPage - 1);
  const nextPage = Math.min(totalPages, currentPage + 1);
  const pageItems = buildPageItems(currentPage, totalPages);
  const items: LinkPaginationItem[] = pageItems.map((item, index) =>
    item === "ellipsis"
      ? {
          key: `ellipsis-${index}`,
          label: "...",
          kind: "ellipsis",
        }
      : {
          key: `page-${item}`,
          label: item,
          href: withPage(baseUrl, item),
          active: item === currentPage,
          kind: "page",
        },
  );

  return (
    <LinkPagination
      {...(currentPage > 1 ? { previous: { href: withPage(baseUrl, prevPage) } } : {})}
      {...(currentPage < totalPages ? { next: { href: withPage(baseUrl, nextPage) } } : {})}
      items={items}
    />
  );
}
