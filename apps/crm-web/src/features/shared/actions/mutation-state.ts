export type CrmMutationState = {
  status: "idle" | "success" | "error";
  message?: string;
  fieldErrors?: Record<string, string[]>;
  redirectTo?: string;
};

export const initialCrmMutationState: CrmMutationState = {
  status: "idle",
};
