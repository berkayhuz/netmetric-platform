"use server";

import { revalidatePath } from "next/cache";

import { assertSameOriginRequest } from "@/lib/security/csrf";
import { buildAuthLoginRedirectUrl } from "@/lib/tools-auth/safe-return-url";
import { getToolsApiRequestOptions } from "@/lib/tools-api/tools-api-request-options";
import { toolsApiClient, ToolsApiError } from "@/lib/tools-api";
import { resolveActionErrorMessage } from "@netmetric/i18n";

import {
  ensureSaveFileConstraints,
  getFallbackExtension,
  normalizeArtifactFileName,
} from "./tool-history-rules";
import { getRequestLocale } from "@/lib/i18n/request-locale";
import { tTools } from "@/lib/i18n/tools-i18n";
import {
  initialToolHistoryActionState,
  type ToolHistoryActionState,
} from "./tool-history-action-state";

function readTrimmedString(formData: FormData, key: string): string {
  const value = formData.get(key);
  return typeof value === "string" ? value.trim() : "";
}

function mapSaveError(error: unknown, locale?: string | null | undefined): ToolHistoryActionState {
  const tContract = (key: string) => tTools(key, locale);

  if (error instanceof ToolsApiError) {
    if (error.kind === "unauthorized") {
      return {
        status: "error",
        message: tTools("tools.history.errors.signInRequiredSave", locale, {
          url: buildAuthLoginRedirectUrl("/history"),
        }),
      };
    }

    if (error.kind === "forbidden") {
      return { status: "error", message: tTools("tools.history.errors.saveForbidden", locale) };
    }

    if (error.kind === "payload_too_large") {
      return { status: "error", message: tTools("tools.history.errors.outputTooLarge", locale) };
    }

    if (error.kind === "unsupported_media_type") {
      return { status: "error", message: tTools("tools.history.errors.unsupportedFormat", locale) };
    }

    if (error.kind === "validation") {
      return {
        status: "error",
        message:
          error.problem?.detail ??
          resolveActionErrorMessage(
            tContract,
            "validation",
            tTools("tools.history.errors.verifyOutput", locale),
          ),
      };
    }

    if (error.kind === "rate_limited") {
      return {
        status: "error",
        message: resolveActionErrorMessage(
          tContract,
          "rate_limited",
          tTools("tools.history.errors.rateLimited", locale),
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

    return {
      status: "error",
      message: resolveActionErrorMessage(
        tContract,
        "unknown",
        tTools("tools.history.errors.saveFailed", locale),
      ),
    };
  }

  return {
    status: "error",
    message: resolveActionErrorMessage(
      tContract,
      "unknown",
      tTools("tools.history.errors.unexpectedSave", locale),
    ),
  };
}

export async function saveToHistoryAction(
  _previous: ToolHistoryActionState,
  formData: FormData,
): Promise<ToolHistoryActionState> {
  await assertSameOriginRequest();
  const locale = await getRequestLocale();

  const toolSlug = readTrimmedString(formData, "toolSlug");
  const inputSummaryJson = readTrimmedString(formData, "inputSummaryJson") || "{}";
  const outputFile = formData.get("outputFile");

  if (!(outputFile instanceof File)) {
    return { status: "error", message: tTools("tools.history.errors.outputRequired", locale) };
  }

  const constraints = ensureSaveFileConstraints({
    toolSlug,
    mimeType: outputFile.type,
    fileSize: outputFile.size,
    locale,
  });

  if (!constraints.ok) {
    return { status: "error", message: constraints.message };
  }

  const safeFileName = normalizeArtifactFileName(
    outputFile.name,
    getFallbackExtension(outputFile.type),
  );

  const requestFormData = new FormData();
  requestFormData.set("toolSlug", toolSlug);
  requestFormData.set("inputSummaryJson", inputSummaryJson);
  requestFormData.set("artifact", outputFile, safeFileName);

  try {
    const requestOptions = await getToolsApiRequestOptions();
    const response = await toolsApiClient.createHistory(requestFormData, requestOptions);

    revalidatePath("/history");
    revalidatePath(`/history/${response.runId}`);

    return {
      status: "success",
      message: tTools("tools.history.savedMessage", locale),
      runId: response.runId,
    };
  } catch (error) {
    return mapSaveError(error, locale);
  }
}

export { initialToolHistoryActionState };
