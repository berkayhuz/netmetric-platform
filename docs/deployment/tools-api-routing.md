# Tools API Routing

Production strategy:

- `tools-web` uses `TOOLS_API_BASE_URL=http://tools-api` for server-side calls inside the Kubernetes network.
- `api.netmetric.net` also exposes Tools API through the gateway at `/api/v1/tools/{**catch-all}`.
- The Tools API service itself stays internal-only; public traffic enters through the gateway.

This keeps browser-facing origin/CORS policy centralized at `api.netmetric.net` while allowing the web app
to use the lower-latency internal service address during server rendering and server actions.

Development strategy:

- `tools-web` defaults to `TOOLS_API_BASE_URL=http://localhost:5305`.
- Gateway development config proxies `/api/v1/tools/{**catch-all}` to the same local Tools API.

Required checks:

- `TOOLS_API_BASE_URL` must point to the internal service for Kubernetes deployments.
- `NEXT_PUBLIC_API_BASE_URL` must point to the gateway origin (`https://api.netmetric.net`) for browser-visible API references.
- Gateway production config must include both `tools-api-route` and `tools-api-cluster`.
