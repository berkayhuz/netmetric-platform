export type HttpMethod = "GET" | "POST" | "DELETE";

export type ToolsApiAuthContext = {
  bearerToken?: string;
};

export type ToolsApiRequestOptions = {
  authContext?: ToolsApiAuthContext;
  correlationId?: string;
  timeoutMs?: number;
  signal?: AbortSignal;
};

export type ToolHistoryQuery = {
  page?: number;
  pageSize?: number;
  toolSlug?: string;
};

export type ToolRunSummaryResponse = {
  runId: string;
  toolSlug: string;
  createdAtUtc: string;
  artifactCount: number;
};

export type ToolArtifactResponse = {
  artifactId: string;
  mimeType: string;
  sizeBytes: number;
  fileName: string;
  checksumSha256: string;
  createdAtUtc: string;
  expiresAtUtc?: string | null;
};

export type ToolRunDetailResponse = {
  runId: string;
  toolSlug: string;
  inputSummaryJson: string;
  createdAtUtc: string;
  artifacts: ToolArtifactResponse[];
};

export type ToolHistoryPageResponse = {
  page: number;
  pageSize: number;
  totalCount: number;
  items: ToolRunSummaryResponse[];
};

export type CreateToolRunResponse = {
  runId: string;
  artifactId: string;
  createdAtUtc: string;
};

export type ToolsApiDownloadResponse = {
  contentType: string;
  contentDisposition: string | null;
  body: ReadableStream<Uint8Array>;
};
