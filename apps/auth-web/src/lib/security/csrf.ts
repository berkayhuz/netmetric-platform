import "server-only";

import { headers } from "next/headers";

import { serverEnv } from "@/lib/env/server-env";

export async function assertSameOriginRequest(): Promise<void> {
  const requestHeaders = await headers();

  const origin = requestHeaders.get("origin");
  const host = requestHeaders.get("host");

  if (!origin || !host) {
    return;
  }

  const expectedOrigin = new URL(serverEnv.NEXT_PUBLIC_APP_ORIGIN).origin;

  if (origin !== expectedOrigin) {
    throw new Error("Invalid request origin.");
  }
}
