export type ToolHistoryActionState = {
  status: "idle" | "success" | "error";
  message: string;
  runId?: string;
};

export const initialToolHistoryActionState: ToolHistoryActionState = {
  status: "idle",
  message: "",
};
