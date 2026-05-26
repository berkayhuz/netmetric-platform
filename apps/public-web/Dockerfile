# syntax=docker/dockerfile:1.7

FROM node:22-alpine AS base
WORKDIR /workspace
RUN corepack enable

FROM base AS deps
COPY pnpm-lock.yaml package.json pnpm-workspace.yaml turbo.json ./
COPY apps ./apps
COPY packages ./packages
RUN pnpm install --frozen-lockfile

FROM deps AS build
ARG NEXT_PUBLIC_SITE_URL
ARG NEXT_PUBLIC_AUTH_URL
ARG NEXT_PUBLIC_ACCOUNT_URL
ARG NEXT_PUBLIC_CRM_URL
ARG NEXT_PUBLIC_TOOLS_URL
ARG NEXT_PUBLIC_API_URL
ENV NEXT_PUBLIC_SITE_URL=${NEXT_PUBLIC_SITE_URL}
ENV NEXT_PUBLIC_AUTH_URL=${NEXT_PUBLIC_AUTH_URL}
ENV NEXT_PUBLIC_ACCOUNT_URL=${NEXT_PUBLIC_ACCOUNT_URL}
ENV NEXT_PUBLIC_CRM_URL=${NEXT_PUBLIC_CRM_URL}
ENV NEXT_PUBLIC_TOOLS_URL=${NEXT_PUBLIC_TOOLS_URL}
ENV NEXT_PUBLIC_API_URL=${NEXT_PUBLIC_API_URL}
RUN pnpm --filter public-web build

FROM node:22-alpine AS runtime
WORKDIR /app
ENV NODE_ENV=production
RUN addgroup -S netmetric && adduser -S netmetric -G netmetric
COPY --from=build --chown=netmetric:netmetric /workspace/apps/public-web/.next/standalone ./
COPY --from=build --chown=netmetric:netmetric /workspace/apps/public-web/.next/static ./apps/public-web/.next/static
COPY --from=build --chown=netmetric:netmetric /workspace/apps/public-web/public ./apps/public-web/public
EXPOSE 7003
USER netmetric
CMD ["node", "apps/public-web/server.js"]
