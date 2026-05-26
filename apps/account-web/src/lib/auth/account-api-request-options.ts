import "server-only";

import { cache } from "react";

import type { AccountApiRequestOptions } from "@/lib/account-api";

import { getAccountApiAuthContext, getRequestCorrelationId } from "./account-auth-headers";

export const getAccountApiRequestOptions = cache(async (): Promise<AccountApiRequestOptions> => {
  const authContext = await getAccountApiAuthContext();

  if (!authContext?.bearerToken) {
    throw new Error("Authenticated account context is required.");
  }

  const options: AccountApiRequestOptions = {
    authContext: {
      bearerToken: authContext.bearerToken,
    },
  };

  const correlationId = await getRequestCorrelationId();
  if (correlationId) {
    options.correlationId = correlationId;
  }

  return options;
});
