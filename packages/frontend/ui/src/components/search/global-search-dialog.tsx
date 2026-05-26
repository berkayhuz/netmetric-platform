"use client";

import {
  CornerDownLeft,
  Search,
  AlertCircle,
  FileText,
  User,
  Building2,
  Settings,
} from "lucide-react";
import * as React from "react";

import { useDebounce } from "../../hooks/use-debounce";
import { cn } from "../../lib/utils";
import { Badge } from "../data-display/badge";
import {
  Command,
  CommandDialog,
  CommandGroup,
  CommandInput,
  CommandItem,
  CommandList,
} from "../overlay/command";

import {
  defaultGlobalSearchSourceOrder,
  groupGlobalSearchResults,
  isSafeGlobalSearchUrl,
  normalizeGlobalSearchSource,
  resolveGlobalSearchLocale,
  toGlobalSearchSourceQueryValue,
} from "./global-search-utils";

import type {
  GlobalSearchResponse,
  GlobalSearchResultItem,
  GlobalSearchSourceGroup,
  GlobalSearchSourceValue,
} from "./global-search-types";

type GlobalSearchDialogLabels = {
  title: string;
  description: string;
  placeholder: string;
  idle: string;
  loading: string;
  empty: string;
  error: string;
  navigate: string;
  open: string;
  close: string;
  escBadge: string;
  noResultsFor: string;
};

export type GlobalSearchDialogProps = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  searchEndpoint?: string;
  placeholder?: string;
  defaultSources?: readonly GlobalSearchSourceValue[];
  sourceOrder?: readonly GlobalSearchSourceGroup[];
  locale?: string | null;
  pageSize?: number;
  minQueryLength?: number;
  debounceMs?: number;
  className?: string;
  labels?: Partial<GlobalSearchDialogLabels>;
  onNavigate: (url: string, result: GlobalSearchResultItem) => void;
};

const defaultLabels: GlobalSearchDialogLabels = {
  title: "Global Search",
  description: "Search across NetMetric.",
  placeholder: "Search...",
  idle: "Type at least two characters to search.",
  loading: "Searching...",
  empty: "No results found.",
  error: "Search is unavailable right now.",
  navigate: "Navigate",
  open: "Open",
  close: "Close",
  escBadge: "ESC",
  noResultsFor: 'No results for "{query}"',
};

function getItemIcon(type: string, _source: unknown) {
  const t = type.toLowerCase();
  if (t === "page" || t === "view") return FileText;
  if (t === "customer" || t === "contact" || t === "user" || t === "lead") return User;
  if (t === "company" || t === "organization") return Building2;
  if (
    t === "setting" ||
    t === "preference" ||
    t === "profile" ||
    t === "security" ||
    t === "mfa" ||
    t === "session"
  ) {
    return Settings;
  }
  return FileText;
}

function highlightText(text: string, query: string) {
  if (!query.trim()) return text;
  try {
    const escaped = query.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
    const regex = new RegExp(`(${escaped})`, "gi");
    const parts = text.split(regex);
    return (
      <>
        {parts.map((part, i) =>
          regex.test(part) ? (
            <mark key={i} className="bg-primary/10 text-primary font-medium rounded-xs px-0.5">
              {part}
            </mark>
          ) : (
            part
          ),
        )}
      </>
    );
  } catch {
    return text;
  }
}

type SearchState =
  | { status: "idle"; response: null }
  | { status: "loading"; response: GlobalSearchResponse | null }
  | { status: "ready"; response: GlobalSearchResponse }
  | { status: "error"; response: null };

type SearchRenderState = "idle" | "loading" | "ready" | "empty" | "error";

function isAbortError(error: unknown): boolean {
  return error instanceof DOMException && error.name === "AbortError";
}

function buildSearchUrl(
  endpoint: string,
  query: string,
  pageSize: number,
  sources: readonly GlobalSearchSourceValue[] | undefined,
  locale: string,
): string {
  const params = new URLSearchParams();
  params.set("q", query);
  params.set("pageSize", String(pageSize));
  params.set("locale", locale);

  if (sources?.length) {
    params.set("sources", sources.map(toGlobalSearchSourceQueryValue).join(","));
  }

  return `${endpoint}?${params.toString()}`;
}

function getDisplaySummary(result: GlobalSearchResultItem): string | null {
  const highlightedSummary = result.highlightedSummary?.trim();
  if (highlightedSummary) {
    return highlightedSummary;
  }

  const summary = result.summary?.trim();
  return summary || null;
}

export function GlobalSearchDialog({
  open,
  onOpenChange,
  searchEndpoint = "/api/search",
  placeholder,
  defaultSources,
  sourceOrder = defaultGlobalSearchSourceOrder,
  locale,
  pageSize = 8,
  minQueryLength = 2,
  debounceMs = 250,
  className,
  labels: labelOverrides,
  onNavigate,
}: Readonly<GlobalSearchDialogProps>) {
  const labels = React.useMemo(
    () => ({ ...defaultLabels, ...labelOverrides, ...(placeholder ? { placeholder } : {}) }),
    [labelOverrides, placeholder],
  );
  const [query, setQuery] = React.useState("");
  const debouncedQuery = useDebounce(query, debounceMs);
  const [state, setState] = React.useState<SearchState>({ status: "idle", response: null });
  const trimmedQuery = debouncedQuery.trim();
  const resolvedLocale = React.useMemo(() => {
    if (locale) {
      return resolveGlobalSearchLocale(locale);
    }

    if (typeof document !== "undefined") {
      return resolveGlobalSearchLocale(document.documentElement.lang);
    }

    return resolveGlobalSearchLocale(null);
  }, [locale]);

  React.useEffect(() => {
    if (!open) {
      setState({ status: "idle", response: null });
      return;
    }

    if (trimmedQuery.length < minQueryLength) {
      setState({ status: "idle", response: null });
      return;
    }

    const controller = new AbortController();
    setState((current) => ({ status: "loading", response: current.response }));

    void (async () => {
      try {
        const response = await fetch(
          buildSearchUrl(searchEndpoint, trimmedQuery, pageSize, defaultSources, resolvedLocale),
          {
            method: "GET",
            credentials: "same-origin",
            headers: {
              accept: "application/json",
            },
            signal: controller.signal,
          },
        );

        if (!response.ok) {
          throw new Error("Search request failed.");
        }

        const payload = (await response.json()) as GlobalSearchResponse;
        if (!controller.signal.aborted) {
          setState({ status: "ready", response: payload });
        }
      } catch (error) {
        if (controller.signal.aborted || isAbortError(error)) {
          return;
        }

        setState({ status: "error", response: null });
      }
    })();

    return () => controller.abort();
  }, [
    defaultSources,
    minQueryLength,
    open,
    pageSize,
    resolvedLocale,
    searchEndpoint,
    trimmedQuery,
  ]);

  const items = state.response?.items ?? [];
  const groups = groupGlobalSearchResults(items, sourceOrder);
  const renderState: SearchRenderState =
    state.status === "ready" && groups.length === 0 ? "empty" : state.status;

  const handleSelect = React.useCallback(
    (result: GlobalSearchResultItem) => {
      if (!isSafeGlobalSearchUrl(result.url)) {
        return;
      }

      onOpenChange(false);
      onNavigate(result.url, result);
    },
    [onNavigate, onOpenChange],
  );

  return (
    <CommandDialog
      open={open}
      onOpenChange={onOpenChange}
      title={labels.title}
      description={labels.description}
      contentTestId="global-search-dialog"
      className={cn(
        "w-[min(720px,calc(100vw-2rem))] max-w-[720px] w-full sm:max-w-2xl overflow-hidden rounded-xl border border-border/80 bg-popover text-popover-foreground shadow-2xl",
        className,
      )}
    >
      <Command
        shouldFilter={false}
        className="[&_[data-slot=command-input-wrapper]]:p-3.5 [&_[data-slot=command-input-wrapper]]:pb-2.5 [&_[data-slot=input-group]]:h-11! [&_[data-slot=input-group]]:rounded-lg! [&_[data-slot=input-group]]:border-border/60! [&_[data-slot=input-group]]:bg-muted/15! [&_[data-slot=input-group]]:focus-within:border-primary/40! [&_[data-slot=input-group]]:focus-within:ring-2! [&_[data-slot=input-group]]:focus-within:ring-primary/10! [&_[data-slot=command-input]]:text-sm! [&_[data-slot=command-input]]:px-3! [&_[data-slot=command-input]]:pr-12! [&_[data-slot=input-group-addon]]:pl-3.5! border-none shadow-none bg-transparent"
      >
        <div className="relative">
          <CommandInput
            data-testid="global-search-input"
            autoFocus
            value={query}
            onValueChange={setQuery}
            placeholder={labels.placeholder}
          />
          <div className="absolute right-7 top-1/2 -translate-y-1/2 flex items-center pointer-events-none select-none">
            <kbd className="hidden sm:inline-flex h-5 items-center gap-1 rounded border border-border/80 bg-muted/40 px-1.5 font-mono text-[9px] font-medium text-muted-foreground/80 shadow-xs">
              {labels.escBadge}
            </kbd>
          </div>
        </div>
        <div className="mx-3.5 h-px bg-border/40" />
        <CommandList
          data-testid="global-search-results"
          data-search-state={renderState}
          className="max-h-[min(28rem,calc(100vh-14rem))] p-2 space-y-1.5"
        >
          {renderState === "idle" ? (
            <div className="flex flex-col items-center justify-center py-12 px-4 text-center">
              <Search className="size-8 text-muted-foreground/30 mb-3 stroke-[1.25]" />
              <p className="text-sm text-muted-foreground max-w-[320px]">{labels.idle}</p>
            </div>
          ) : null}
          {renderState === "loading" ? (
            <div data-testid="global-search-loading" className="space-y-2 p-1">
              {[1, 2, 3].map((i) => (
                <div key={i} className="flex items-center gap-3 px-3 py-3 animate-pulse">
                  <div className="size-8 rounded bg-muted/60 shrink-0" />
                  <div className="flex-1 space-y-2">
                    <div className="h-4 w-1/4 rounded bg-muted/60" />
                    <div className="h-3 w-1/2 rounded bg-muted/40" />
                  </div>
                </div>
              ))}
            </div>
          ) : null}
          {renderState === "error" ? (
            <div
              data-testid="global-search-error"
              className="flex flex-col items-center justify-center py-12 px-4 text-center"
            >
              <AlertCircle className="size-8 text-destructive/70 mb-3 stroke-[1.25]" />
              <p className="text-sm font-medium text-foreground">{labels.error}</p>
            </div>
          ) : null}
          {renderState === "empty" ? (
            <div
              data-testid="global-search-empty"
              className="flex flex-col items-center justify-center py-12 px-4 text-center"
            >
              <Search className="size-8 text-muted-foreground/30 mb-3 stroke-[1.25]" />
              <h3 className="text-sm font-medium text-foreground">
                {labels.noResultsFor
                  ? labels.noResultsFor.replace("{query}", query)
                  : `No results for "${query}"`}
              </h3>
              <p className="mt-1 text-xs text-muted-foreground max-w-[280px]">{labels.empty}</p>
            </div>
          ) : null}
          {groups.map((group) => (
            <CommandGroup key={group.source} heading={group.label}>
              {group.items.map((result) => {
                const source = normalizeGlobalSearchSource(result.source);
                const summary = getDisplaySummary(result);
                const isSafeUrl = isSafeGlobalSearchUrl(result.url);
                const Icon = getItemIcon(result.type, result.source);
                return (
                  <CommandItem
                    key={result.id}
                    value={`${result.title} ${result.summary ?? ""} ${result.url}`}
                    onSelect={() => handleSelect(result)}
                    disabled={!isSafeUrl}
                    data-testid="global-search-result"
                    className="group/command-item items-start gap-3 px-3 py-2.5 cursor-pointer rounded-md transition-colors duration-150 data-selected:bg-muted/70 data-selected:text-foreground mb-2 last:mb-0"
                  >
                    <Icon className="size-4.5 text-muted-foreground/60 shrink-0 mt-0.5 stroke-[1.5]" />
                    <div className="min-w-0 flex-1 space-y-1">
                      <div className="flex min-w-0 items-center gap-2 flex-wrap">
                        <span className="truncate text-sm font-medium text-foreground">
                          {result.highlightedTitle?.trim()
                            ? highlightText(result.highlightedTitle.trim(), query)
                            : highlightText(result.title, query)}
                        </span>
                        <Badge
                          variant="outline"
                          className="h-4.5 rounded-sm px-1.5 text-[10px] font-normal border-border/60 text-muted-foreground bg-muted/10"
                        >
                          {group.label}
                        </Badge>
                        <Badge
                          variant="secondary"
                          className="h-4.5 rounded-sm px-1.5 text-[10px] font-normal bg-muted/40 text-muted-foreground"
                        >
                          {result.type}
                        </Badge>
                        {result.tags?.map((tag) => (
                          <Badge
                            key={tag}
                            variant="outline"
                            className="h-4.5 rounded-sm px-1.5 text-[10px] font-normal border-border/40 text-muted-foreground/70"
                          >
                            {tag}
                          </Badge>
                        ))}
                      </div>
                      {summary ? (
                        <p className="line-clamp-2 text-xs leading-4.5 text-muted-foreground/80">
                          {result.highlightedSummary?.trim()
                            ? highlightText(result.highlightedSummary.trim(), query)
                            : highlightText(summary, query)}
                        </p>
                      ) : null}
                      <p className="truncate text-[10px] text-muted-foreground/50 font-mono">
                        {isSafeUrl ? result.url : `Unsupported ${source} link`}
                      </p>
                      {!isSafeUrl ? (
                        <span data-testid="global-search-result-disabled" className="sr-only">
                          unsupported-result
                        </span>
                      ) : null}
                    </div>
                    <CornerDownLeft className="size-3.5 text-muted-foreground/30 opacity-0 group-data-selected/command-item:opacity-100 transition-opacity self-center shrink-0 ml-auto" />
                  </CommandItem>
                );
              })}
            </CommandGroup>
          ))}
        </CommandList>
        <div className="flex items-center justify-end gap-4 border-t border-border/40 bg-muted/20 px-4 py-2.5 text-[11px] text-muted-foreground select-none">
          <div className="flex items-center gap-1.5">
            <kbd className="rounded border border-border bg-popover px-1.5 py-0.5 font-mono text-[9px] shadow-xs">
              ↑↓
            </kbd>
            <span>{labels.navigate}</span>
          </div>
          <div className="flex items-center gap-1.5">
            <kbd className="rounded border border-border bg-popover px-1.5 py-0.5 font-mono text-[9px] shadow-xs">
              ↵
            </kbd>
            <span>{labels.open}</span>
          </div>
          <div className="flex items-center gap-1.5">
            <kbd className="rounded border border-border bg-popover px-1.5 py-0.5 font-mono text-[9px] shadow-xs">
              {labels.escBadge}
            </kbd>
            <span>{labels.close}</span>
          </div>
        </div>
      </Command>
    </CommandDialog>
  );
}
