export type ApiResult<TData> =
  | {
      ok: true;
      data: TData;
    }
  | {
      ok: false;
      status: number;
      message: string;
      errorCode?: string;
      fieldErrors?: Record<string, string[]>;
    };

export function ok<TData>(data: TData): ApiResult<TData> {
  return {
    ok: true,
    data,
  };
}

export function fail<TData = never>(input: {
  status: number;
  message: string;
  errorCode?: string;
  fieldErrors?: Record<string, string[]>;
}): ApiResult<TData> {
  const result: {
    ok: false;
    status: number;
    message: string;
    errorCode?: string;
    fieldErrors?: Record<string, string[]>;
  } = {
    ok: false,
    status: input.status,
    message: input.message,
  };

  if (input.errorCode !== undefined) {
    result.errorCode = input.errorCode;
  }

  if (input.fieldErrors !== undefined) {
    result.fieldErrors = input.fieldErrors;
  }

  return result;
}
