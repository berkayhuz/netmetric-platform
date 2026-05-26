import "server-only";

export const toolsApiEndpoints = {
  historyList: {
    method: "GET",
    route: "/api/v1/tools/history",
  },
  historyCreate: {
    method: "POST",
    route: "/api/v1/tools/history",
  },
  historyDetail: (id: string) => ({
    method: "GET",
    route: `/api/v1/tools/history/${id}`,
  }),
  historyDelete: (id: string) => ({
    method: "DELETE",
    route: `/api/v1/tools/history/${id}`,
  }),
  historyDownload: (id: string) => ({
    method: "GET",
    route: `/api/v1/tools/history/${id}/download`,
  }),
} as const;
