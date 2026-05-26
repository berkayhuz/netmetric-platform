import "server-only";

import { headers } from "next/headers";

import { appEnv } from "@/lib/app-env";

export async function assertSameOriginRequest(): Promise<void> {
  const requestHeaders = await headers();
  const origin = requestHeaders.get("origin");
  const host = requestHeaders.get("host");

  if (!origin || !host) {
    return;
  }

  const expectedOrigin = new URL(appEnv.accountUrl).origin;

  if (origin !== expectedOrigin) {
    throw new Error("Invalid request origin.");
  }
}
