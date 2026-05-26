import { AlertTriangle, Ban, RefreshCcw, SearchX } from "lucide-react";

import {
  Empty,
  EmptyContent,
  EmptyDescription,
  EmptyHeader,
  EmptyMedia,
  EmptyTitle,
} from "../data-display/empty";
import { Button } from "../primitives/button";

type BaseStateProps = {
  title: string;
  description: string;
  actionLabel?: string;
  actionHref?: string;
  actionAriaLabel?: string;
};

function StateAction({ actionLabel, actionHref, actionAriaLabel }: BaseStateProps) {
  if (!actionLabel || !actionHref) {
    return null;
  }

  return (
    <Button asChild variant="outline" aria-label={actionAriaLabel ?? actionLabel}>
      <a href={actionHref}>{actionLabel}</a>
    </Button>
  );
}

export function LoadingState({
  title,
  description,
}: Pick<BaseStateProps, "title" | "description">) {
  return (
    <Empty role="status" aria-live="polite">
      <EmptyHeader>
        <EmptyMedia variant="icon">
          <RefreshCcw className="animate-spin" aria-hidden="true" />
        </EmptyMedia>
        <EmptyTitle>{title}</EmptyTitle>
        <EmptyDescription>{description}</EmptyDescription>
      </EmptyHeader>
    </Empty>
  );
}

export function EmptyState(props: BaseStateProps) {
  return (
    <Empty role="status" aria-live="polite">
      <EmptyHeader>
        <EmptyTitle>{props.title}</EmptyTitle>
        <EmptyDescription>{props.description}</EmptyDescription>
      </EmptyHeader>
      <EmptyContent>
        <StateAction {...props} />
      </EmptyContent>
    </Empty>
  );
}

export function ErrorState(props: BaseStateProps) {
  return (
    <Empty role="alert" aria-live="assertive">
      <EmptyHeader>
        <EmptyMedia variant="icon">
          <AlertTriangle aria-hidden="true" />
        </EmptyMedia>
        <EmptyTitle>{props.title}</EmptyTitle>
        <EmptyDescription>{props.description}</EmptyDescription>
      </EmptyHeader>
      <EmptyContent>
        <StateAction {...props} />
      </EmptyContent>
    </Empty>
  );
}

export function AccessDeniedState(props: BaseStateProps) {
  return (
    <Empty role="alert" aria-live="assertive">
      <EmptyHeader>
        <EmptyMedia variant="icon">
          <Ban aria-hidden="true" />
        </EmptyMedia>
        <EmptyTitle>{props.title}</EmptyTitle>
        <EmptyDescription>{props.description}</EmptyDescription>
      </EmptyHeader>
      <EmptyContent>
        <StateAction {...props} />
      </EmptyContent>
    </Empty>
  );
}

export function NotFoundState(props: BaseStateProps) {
  return (
    <Empty role="status" aria-live="polite">
      <EmptyHeader>
        <EmptyMedia variant="icon">
          <SearchX aria-hidden="true" />
        </EmptyMedia>
        <EmptyTitle>{props.title}</EmptyTitle>
        <EmptyDescription>{props.description}</EmptyDescription>
      </EmptyHeader>
      <EmptyContent>
        <StateAction {...props} />
      </EmptyContent>
    </Empty>
  );
}
