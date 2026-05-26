"use server";

import { revalidatePath } from "next/cache";

import { assertSameOriginRequest } from "@/lib/security/csrf";
import { getRequestLocale } from "@/lib/i18n/request-locale";
import { tTools } from "@/lib/i18n/tools-i18n";
import { buildAuthLoginRedirectUrl } from "@/lib/tools-auth/safe-return-url";
import { getToolsApiRequestOptions } from "@/lib/tools-api/tools-api-request-options";
import { toolsApiClient, ToolsApiError } from "@/lib/tools-api";
import { resolveActionErrorMessage } from "@netmetric/i18n";

import {
  initialToolHistoryActionState,
  type ToolHistoryActionState,
} from "./tool-history-action-state";

function readTrimmedString(formData: FormData, key: string): string {
  const value = formData.get(key);
  return typeof value === "string" ? value.trim() : "";
}

function mapDeleteError(
  error: unknown,
  locale?: string | null | undefined,
): ToolHistoryActionState {
  const tContract = (key: string) => tTools(key, locale);

  if (error instanceof ToolsApiError) {
    if (error.kind === "unauthorized") {
      return {
        status: "error",
        message: tTools("tools.history.errors.signInRequired", locale, {
          url: buildAuthLoginRedirectUrl("/history"),
        }),
      };
    }

    if (error.kind === "forbidden") {
      return { status: "error", message: tTools("tools.history.errors.deleteForbidden", locale) };
    }

    if (error.kind === "not_found") {
      return {
        status: "error",
        message: resolveActionErrorMessage(
          tContract,
          "not_found",
          tTools("tools.history.errors.notFound", locale),
        ),
      };
    }

    if (error.kind === "rate_limited") {
      return {
        status: "error",
        message: resolveActionErrorMessage(
          tContract,
          "rate_limited",
          tTools("tools.history.errors.rateLimitedRetry", locale),
        ),
      };
    }

    if (error.kind === "server_error" || error.kind === "upstream_unavailable") {
      return {
        status: "error",
        message: resolveActionErrorMessage(
          tContract,
          "server_error",
          tTools("tools.history.errors.serviceUnavailable", locale),
        ),
      };
    }
  }

  return {
    status: "error",
    message: resolveActionErrorMessage(
      tContract,
      "unknown",
      tTools("tools.history.errors.unexpectedDelete", locale),
    ),
  };
}

export async function deleteToolRunAction(
  _previous: ToolHistoryActionState,
  formData: FormData,
): Promise<ToolHistoryActionState> {
  await assertSameOriginRequest();
  const locale = await getRequestLocale();

  const runId = readTrimmedString(formData, "runId");
  const confirm = readTrimmedString(formData, "confirm");

  if (!runId) {
    return { status: "error", message: tTools("tools.history.errors.runIdRequired", locale) };
  }

  if (confirm !== "delete-tool-run") {
    return {
      status: "error",
      message: tTools("tools.history.errors.confirmRequired", locale),
    };
  }

  try {
    const requestOptions = await getToolsApiRequestOptions();
    await toolsApiClient.deleteHistory(runId, requestOptions);

    revalidatePath("/history");
    revalidatePath(`/history/${runId}`);

    return {
      status: "success",
      message: tTools("tools.history.deletedMessage", locale),
    };
  } catch (error) {
    return mapDeleteError(error, locale);
  }
}

export { initialToolHistoryActionState };
