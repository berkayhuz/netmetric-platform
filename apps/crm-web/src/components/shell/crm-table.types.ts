import type { ReactNode } from "react";

export type CrmEntityTableColumn<TItem> = {
  key: string;
  header: string;
  render: (item: TItem) => ReactNode;
};
