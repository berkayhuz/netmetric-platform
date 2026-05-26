export type MutationState = {
  status: "idle" | "success" | "error";
  code?: "conflict";
  message?: string;
  fieldErrors?: Record<string, string[]>;
  data?: {
    setup?: {
      sharedKey: string;
      authenticatorUri: string;
    };
    recoveryCodes?: string[];
    mfaEnabled?: boolean;
  };
};

export const initialMutationState: MutationState = {
  status: "idle",
};
