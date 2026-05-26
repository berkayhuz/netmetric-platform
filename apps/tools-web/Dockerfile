# syntax=docker/dockerfile:1.7

FROM node:26-alpine AS base
WORKDIR /workspace
RUN corepack enable

FROM base AS deps
COPY pnpm-lock.yaml package.json pnpm-workspace.yaml turbo.json ./
COPY apps ./apps
COPY packages ./packages
RUN pnpm install --frozen-lockfile

FROM deps AS build
ARG NEXT_PUBLIC_APP_NAME=NetMetric\ Tools
ARG NEXT_PUBLIC_APP_ORIGIN=https://tools.netmetric.net
ARG NEXT_PUBLIC_TOOLS_URL=https://tools.netmetric.net
ARG NEXT_PUBLIC_AUTH_URL=https://auth.netmetric.net
ARG NEXT_PUBLIC_API_BASE_URL=https://api.netmetric.net
ENV NEXT_PUBLIC_APP_NAME=${NEXT_PUBLIC_APP_NAME}
ENV NEXT_PUBLIC_APP_ORIGIN=${NEXT_PUBLIC_APP_ORIGIN}
ENV NEXT_PUBLIC_TOOLS_URL=${NEXT_PUBLIC_TOOLS_URL}
ENV NEXT_PUBLIC_AUTH_URL=${NEXT_PUBLIC_AUTH_URL}
ENV NEXT_PUBLIC_API_BASE_URL=${NEXT_PUBLIC_API_BASE_URL}
RUN pnpm --filter tools-web build

FROM node:26-alpine AS runtime
WORKDIR /app
ENV NODE_ENV=production
RUN addgroup -S netmetric && adduser -S netmetric -G netmetric
COPY --from=build --chown=netmetric:netmetric /workspace/apps/tools-web/.next/standalone ./
COPY --from=build --chown=netmetric:netmetric /workspace/apps/tools-web/.next/static ./apps/tools-web/.next/static
COPY --from=build --chown=netmetric:netmetric /workspace/apps/tools-web/public ./apps/tools-web/public
EXPOSE 7005
USER netmetric
CMD ["node", "apps/tools-web/server.js"]
