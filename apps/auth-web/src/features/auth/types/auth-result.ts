export type AuthActionState<TData = unknown> =
  | {
      status: "idle";
      message?: string;
      data?: never;
      fieldErrors?: never;
    }
  | {
      status: "success";
      message?: string;
      data?: TData;
      fieldErrors?: never;
    }
  | {
      status: "error";
      message: string;
      data?: never;
      fieldErrors?: Record<string, string[]>;
    };

export const initialAuthActionState: AuthActionState = {
  status: "idle",
};

export function authActionSuccess<TData>(data?: TData, message?: string): AuthActionState<TData> {
  const result: {
    status: "success";
    message?: string;
    data?: TData;
    fieldErrors?: never;
  } = {
    status: "success",
  };

  if (message !== undefined) {
    result.message = message;
  }

  if (data !== undefined) {
    result.data = data;
  }

  return result;
}

export function authActionError(
  message: string,
  fieldErrors?: Record<string, string[]>,
): AuthActionState {
  const result: {
    status: "error";
    message: string;
    data?: never;
    fieldErrors?: Record<string, string[]>;
  } = {
    status: "error",
    message,
  };

  if (fieldErrors !== undefined) {
    result.fieldErrors = fieldErrors;
  }

  return result;
}
