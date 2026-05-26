import "server-only";

import { getToolsApiConfig, joinToolsApiPath } from "./tools-api-config";
import { toolsApiEndpoints } from "./tools-api-endpoints";
import { ToolsApiError, statusToToolsApiErrorKind } from "./tools-api-errors";
import { applyCorrelationId, getCorrelationIdFromHeaders } from "./correlation";
import { normalizeProblemDetails } from "./problem-details";
import type {
  CreateToolRunResponse,
  ToolHistoryPageResponse,
  ToolHistoryQuery,
  ToolRunDetailResponse,
  ToolsApiDownloadResponse,
  ToolsApiRequestOptions,
} from "./tools-api-types";

type RequestOptions = ToolsApiRequestOptions & {
  method: string;
  route: string;
  body?: unknown;
  query?: Record<string, string | number | boolean | undefined>;
  contentType?: string;
};

function withQuery(
  route: string,
  query: Record<string, string | number | boolean | undefined>,
): string {
  const params = new URLSearchParams();

  for (const [key, value] of Object.entries(query)) {
    if (value === undefined) {
      continue;
    }

    params.set(key, String(value));
  }

  const suffix = params.toString();
  return suffix ? `${route}?${suffix}` : route;
}

function readBodyAsJson(body: unknown, contentType?: string): BodyInit | undefined {
  if (body === undefined) {
    return undefined;
  }

  if (body instanceof FormData) {
    return body;
  }

  if (contentType && contentType !== "application/json") {
    return body as BodyInit;
  }

  return JSON.stringify(body);
}

async function parseResponsePayload(response: Response): Promise<unknown> {
  const text = await response.text();
  if (!text) {
    return null;
  }

  try {
    return JSON.parse(text) as unknown;
  } catch {
    return text;
  }
}

function buildHeaders(
  authContext: ToolsApiRequestOptions["authContext"],
  correlationId: string | undefined,
): Headers {
  const headers = new Headers();
  headers.set("accept", "application/json");

  if (authContext?.bearerToken) {
    headers.set("authorization", `Bearer ${authContext.bearerToken}`);
  }

  applyCorrelationId(headers, correlationId);
  return headers;
}

function createTimeoutSignal(timeoutMs: number, parentSignal?: AbortSignal): AbortSignal {
  const timeoutSignal = AbortSignal.timeout(timeoutMs);

  if (!parentSignal) {
    return timeoutSignal;
  }

  if (parentSignal.aborted) {
    return parentSignal;
  }

  const controller = new AbortController();
  const abort = () => controller.abort();
  parentSignal.addEventListener("abort", abort, { once: true });
  timeoutSignal.addEventListener("abort", abort, { once: true });

  return controller.signal;
}

async function request<TResponse>(options: RequestOptions): Promise<TResponse> {
  const route = options.query ? withQuery(options.route, options.query) : options.route;
  const requestUrl = joinToolsApiPath(route);
  const headers = buildHeaders(options.authContext, options.correlationId);
  const body = readBodyAsJson(options.body, options.contentType);

  if (body && !(body instanceof FormData) && !options.contentType) {
    headers.set("content-type", "application/json");
  }

  if (options.contentType) {
    headers.set("content-type", options.contentType);
  }

  const signal = createTimeoutSignal(
    options.timeoutMs ?? getToolsApiConfig().defaultTimeoutMs,
    options.signal,
  );

  const requestInit: RequestInit = {
    method: options.method,
    headers,
    cache: "no-store",
    signal,
    redirect: "manual",
  };

  if (body !== undefined) {
    requestInit.body = body;
  }

  let response: Response;
  try {
    response = await fetch(requestUrl, requestInit);
  } catch {
    const errorInput: ConstructorParameters<typeof ToolsApiError>[0] = {
      message: "Tools API is unavailable.",
      status: 503,
      kind: "upstream_unavailable",
    };
    if (options.correlationId) {
      errorInput.correlationId = options.correlationId;
    }
    throw new ToolsApiError(errorInput);
  }

  const payload = await parseResponsePayload(response);
  const responseCorrelationId =
    getCorrelationIdFromHeaders(response.headers) ?? options.correlationId;

  if (!response.ok) {
    const problem = normalizeProblemDetails(payload);
    const errorInput: ConstructorParameters<typeof ToolsApiError>[0] = {
      message: problem?.detail ?? problem?.title ?? "Tools API request failed.",
      status: response.status,
      kind: statusToToolsApiErrorKind(response.status),
    };
    if (problem) {
      errorInput.problem = problem;
    }
    if (responseCorrelationId) {
      errorInput.correlationId = responseCorrelationId;
    }
    throw new ToolsApiError(errorInput);
  }

  if (response.status === 204 || payload === null) {
    return undefined as TResponse;
  }

  return payload as TResponse;
}

async function requestDownload(
  route: string,
  options: ToolsApiRequestOptions,
): Promise<ToolsApiDownloadResponse> {
  const requestUrl = joinToolsApiPath(route);
  const headers = buildHeaders(options.authContext, options.correlationId);

  let response: Response;
  try {
    response = await fetch(requestUrl, {
      method: "GET",
      headers,
      cache: "no-store",
      redirect: "manual",
      signal: createTimeoutSignal(
        options.timeoutMs ?? getToolsApiConfig().defaultTimeoutMs,
        options.signal,
      ),
    });
  } catch {
    const errorInput: ConstructorParameters<typeof ToolsApiError>[0] = {
      message: "Tools API is unavailable.",
      status: 503,
      kind: "upstream_unavailable",
    };
    if (options.correlationId) {
      errorInput.correlationId = options.correlationId;
    }
    throw new ToolsApiError(errorInput);
  }

  const responseCorrelationId =
    getCorrelationIdFromHeaders(response.headers) ?? options.correlationId;

  if (!response.ok) {
    const payload = await parseResponsePayload(response);
    const problem = normalizeProblemDetails(payload);
    const errorInput: ConstructorParameters<typeof ToolsApiError>[0] = {
      message: problem?.detail ?? problem?.title ?? "Tools API request failed.",
      status: response.status,
      kind: statusToToolsApiErrorKind(response.status),
    };
    if (problem) {
      errorInput.problem = problem;
    }
    if (responseCorrelationId) {
      errorInput.correlationId = responseCorrelationId;
    }
    throw new ToolsApiError(errorInput);
  }

  if (!response.body) {
    const errorInput: ConstructorParameters<typeof ToolsApiError>[0] = {
      message: "Download response body was empty.",
      status: 502,
      kind: "server_error",
    };
    if (responseCorrelationId) {
      errorInput.correlationId = responseCorrelationId;
    }
    throw new ToolsApiError(errorInput);
  }

  return {
    body: response.body,
    contentType: response.headers.get("content-type") ?? "application/octet-stream",
    contentDisposition: response.headers.get("content-disposition"),
  };
}

export const toolsApiClient = {
  getHistory(query: ToolHistoryQuery, options: ToolsApiRequestOptions = {}) {
    return request<ToolHistoryPageResponse>({
      method: toolsApiEndpoints.historyList.method,
      route: toolsApiEndpoints.historyList.route,
      query,
      ...options,
    });
  },

  getHistoryDetail(runId: string, options: ToolsApiRequestOptions = {}) {
    const endpoint = toolsApiEndpoints.historyDetail(runId);
    return request<ToolRunDetailResponse>({
      method: endpoint.method,
      route: endpoint.route,
      ...options,
    });
  },

  createHistory(formData: FormData, options: ToolsApiRequestOptions = {}) {
    return request<CreateToolRunResponse>({
      method: toolsApiEndpoints.historyCreate.method,
      route: toolsApiEndpoints.historyCreate.route,
      body: formData,
      ...options,
    });
  },

  deleteHistory(runId: string, options: ToolsApiRequestOptions = {}) {
    const endpoint = toolsApiEndpoints.historyDelete(runId);
    return request<void>({
      method: endpoint.method,
      route: endpoint.route,
      ...options,
    });
  },

  downloadHistory(runId: string, options: ToolsApiRequestOptions = {}) {
    const endpoint = toolsApiEndpoints.historyDownload(runId);
    return requestDownload(endpoint.route, options);
  },
};
