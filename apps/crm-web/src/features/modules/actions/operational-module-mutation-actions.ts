"use server";

import { revalidatePath } from "next/cache";

import { mapCrmMutationErrorToState } from "@/features/shared/actions/mutation-error-map";
import type { CrmMutationState } from "@/features/shared/actions/mutation-state";
import { emptyToNull } from "@/features/shared/forms/schema-primitives";
import { CrmApiError, crmApiClient } from "@/lib/crm-api";
import { getCrmApiRequestOptions } from "@/lib/crm-auth/crm-api-request-options";
import type { CrmCapability } from "@/lib/crm-auth/crm-capabilities";
import { requireCrmActionCapability } from "@/lib/crm-auth/require-crm-action-capability";
import { tCrm } from "@/lib/i18n/crm-i18n";
import { getRequestLocale } from "@/lib/i18n/request-locale";
import { assertSameOriginRequest } from "@/lib/security/csrf";

type TextFieldRule = {
  field: string;
  labelKey: string;
  required?: boolean;
  maxLength?: number;
};

const colorPattern = /^#[0-9a-fA-F]{6}$/;

function readText(formData: FormData, field: string): string {
  const value = formData.get(field);
  return typeof value === "string" ? value.trim() : "";
}

function readFields(
  formData: FormData,
  rules: readonly TextFieldRule[],
  locale: string,
): { values: Record<string, string>; fieldErrors?: Record<string, string[]> } {
  const values: Record<string, string> = {};
  const fieldErrors: Record<string, string[]> = {};

  for (const rule of rules) {
    const value = readText(formData, rule.field);
    values[rule.field] = value;

    if (rule.required && value.length === 0) {
      fieldErrors[rule.field] = [
        tCrm("crm.modules.workspace.actions.validation.required", locale).replace(
          "{field}",
          tCrm(rule.labelKey, locale),
        ),
      ];
      continue;
    }

    if (rule.maxLength && value.length > rule.maxLength) {
      fieldErrors[rule.field] = [
        tCrm("crm.modules.workspace.actions.validation.maxLength", locale)
          .replace("{field}", tCrm(rule.labelKey, locale))
          .replace("{max}", String(rule.maxLength)),
      ];
    }
  }

  return {
    values,
    ...(Object.keys(fieldErrors).length > 0 ? { fieldErrors } : {}),
  };
}

function normalizeColor(value: string): string {
  return colorPattern.test(value) ? value : "#2563eb";
}

function normalizeConditionJson(
  value: string,
  locale: string,
): { value?: string; fieldErrors?: Record<string, string[]> } {
  if (value.length === 0) {
    return {
      fieldErrors: {
        conditionJson: [tCrm("crm.modules.workspace.actions.validation.conditionRequired", locale)],
      },
    };
  }

  try {
    const parsed = JSON.parse(value) as unknown;
    const normalized = JSON.stringify(parsed);
    if (normalized.length > 200) {
      return {
        fieldErrors: {
          conditionJson: [
            tCrm("crm.modules.workspace.actions.validation.maxLength", locale)
              .replace(
                "{field}",
                tCrm("crm.modules.workspace.actions.fields.conditionJson", locale),
              )
              .replace("{max}", "200"),
          ],
        },
      };
    }

    return { value: normalized };
  } catch {
    return {
      fieldErrors: {
        conditionJson: [tCrm("crm.modules.workspace.actions.validation.conditionJson", locale)],
      },
    };
  }
}

function mapOperationalMutationError(
  error: unknown,
  returnPath: string,
  locale: string,
): CrmMutationState {
  if (error instanceof CrmApiError) {
    if (error.kind === "forbidden") {
      return {
        status: "error",
        message: tCrm("crm.modules.workspace.actions.permissionRequired", locale),
      };
    }

    if (error.kind === "server_error" || error.kind === "upstream_unavailable") {
      return {
        status: "error",
        message: tCrm("crm.modules.workspace.actions.unavailable", locale),
      };
    }
  }

  return mapCrmMutationErrorToState(error, returnPath);
}

async function prepareOperationalAction(
  path: string,
  capability: CrmCapability,
): Promise<{
  locale: string;
  options: Awaited<ReturnType<typeof getCrmApiRequestOptions>>;
}> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(path, capability);

  const [locale, options] = await Promise.all([getRequestLocale(), getCrmApiRequestOptions()]);
  return { locale, options };
}

export async function createContractOperationalAction(
  _previous: CrmMutationState,
  formData: FormData,
): Promise<CrmMutationState> {
  const { locale, options } = await prepareOperationalAction("/contracts", "contracts.manage");
  const parsed = readFields(
    formData,
    [
      {
        field: "code",
        labelKey: "crm.modules.workspace.actions.fields.code",
        required: true,
        maxLength: 64,
      },
      {
        field: "name",
        labelKey: "crm.modules.workspace.actions.fields.name",
        required: true,
        maxLength: 200,
      },
      {
        field: "description",
        labelKey: "crm.modules.workspace.actions.fields.description",
        maxLength: 2000,
      },
    ],
    locale,
  );

  if (parsed.fieldErrors) {
    return {
      status: "error",
      message: tCrm("crm.forms.errors.reviewTitle", locale),
      fieldErrors: parsed.fieldErrors,
    };
  }

  try {
    await crmApiClient.createContract(
      {
        code: parsed.values.code ?? "",
        name: parsed.values.name ?? "",
        description: emptyToNull(parsed.values.description),
      },
      options,
    );
    revalidatePath("/contracts");
    return {
      status: "success",
      message: tCrm("crm.modules.workspace.actions.contractCreated", locale),
    };
  } catch (error) {
    return mapOperationalMutationError(error, "/contracts", locale);
  }
}

export async function createFinanceOrderOperationalAction(
  _previous: CrmMutationState,
  formData: FormData,
): Promise<CrmMutationState> {
  const { locale, options } = await prepareOperationalAction("/finance", "finance.manage");
  const parsed = readFields(
    formData,
    [
      {
        field: "code",
        labelKey: "crm.modules.workspace.actions.fields.code",
        required: true,
        maxLength: 64,
      },
      {
        field: "name",
        labelKey: "crm.modules.workspace.actions.fields.name",
        required: true,
        maxLength: 200,
      },
      {
        field: "description",
        labelKey: "crm.modules.workspace.actions.fields.description",
        maxLength: 2000,
      },
    ],
    locale,
  );

  if (parsed.fieldErrors) {
    return {
      status: "error",
      message: tCrm("crm.forms.errors.reviewTitle", locale),
      fieldErrors: parsed.fieldErrors,
    };
  }

  try {
    await crmApiClient.createOrder(
      {
        code: parsed.values.code ?? "",
        name: parsed.values.name ?? "",
        description: emptyToNull(parsed.values.description),
      },
      options,
    );
    revalidatePath("/finance");
    return {
      status: "success",
      message: tCrm("crm.modules.workspace.actions.orderCreated", locale),
    };
  } catch (error) {
    return mapOperationalMutationError(error, "/finance", locale);
  }
}

export async function createTagOperationalAction(
  _previous: CrmMutationState,
  formData: FormData,
): Promise<CrmMutationState> {
  const { locale, options } = await prepareOperationalAction("/tags", "tags.manage");
  const parsed = readFields(
    formData,
    [
      {
        field: "name",
        labelKey: "crm.modules.workspace.actions.fields.name",
        required: true,
        maxLength: 200,
      },
      {
        field: "color",
        labelKey: "crm.modules.workspace.actions.fields.color",
        maxLength: 200,
      },
    ],
    locale,
  );

  if (parsed.fieldErrors) {
    return {
      status: "error",
      message: tCrm("crm.forms.errors.reviewTitle", locale),
      fieldErrors: parsed.fieldErrors,
    };
  }

  try {
    await crmApiClient.createTag(
      {
        name: parsed.values.name ?? "",
        color: normalizeColor(parsed.values.color ?? ""),
      },
      options,
    );
    revalidatePath("/tags");
    return {
      status: "success",
      message: tCrm("crm.modules.workspace.actions.tagCreated", locale),
    };
  } catch (error) {
    return mapOperationalMutationError(error, "/tags", locale);
  }
}

export async function createTagGroupOperationalAction(
  _previous: CrmMutationState,
  formData: FormData,
): Promise<CrmMutationState> {
  const { locale, options } = await prepareOperationalAction("/tags", "tags.manage");
  const parsed = readFields(
    formData,
    [
      {
        field: "name",
        labelKey: "crm.modules.workspace.actions.fields.name",
        required: true,
        maxLength: 200,
      },
      {
        field: "color",
        labelKey: "crm.modules.workspace.actions.fields.color",
        maxLength: 200,
      },
    ],
    locale,
  );

  if (parsed.fieldErrors) {
    return {
      status: "error",
      message: tCrm("crm.forms.errors.reviewTitle", locale),
      fieldErrors: parsed.fieldErrors,
    };
  }

  try {
    await crmApiClient.createTagGroup(
      {
        name: parsed.values.name ?? "",
        color: emptyToNull(parsed.values.color),
      },
      options,
    );
    revalidatePath("/tags");
    return {
      status: "success",
      message: tCrm("crm.modules.workspace.actions.tagGroupCreated", locale),
    };
  } catch (error) {
    return mapOperationalMutationError(error, "/tags", locale);
  }
}

export async function createSmartLabelRuleOperationalAction(
  _previous: CrmMutationState,
  formData: FormData,
): Promise<CrmMutationState> {
  const { locale, options } = await prepareOperationalAction("/tags", "tags.manage");
  const parsed = readFields(
    formData,
    [
      {
        field: "name",
        labelKey: "crm.modules.workspace.actions.fields.name",
        required: true,
        maxLength: 200,
      },
      {
        field: "entityType",
        labelKey: "crm.modules.workspace.actions.fields.entityType",
        required: true,
        maxLength: 200,
      },
    ],
    locale,
  );
  const condition = normalizeConditionJson(readText(formData, "conditionJson"), locale);

  if (parsed.fieldErrors || condition.fieldErrors) {
    return {
      status: "error",
      message: tCrm("crm.forms.errors.reviewTitle", locale),
      fieldErrors: { ...(parsed.fieldErrors ?? {}), ...(condition.fieldErrors ?? {}) },
    };
  }

  try {
    await crmApiClient.createSmartLabelRule(
      {
        name: parsed.values.name ?? "",
        entityType: parsed.values.entityType ?? "",
        conditionJson: condition.value ?? "{}",
      },
      options,
    );
    revalidatePath("/tags");
    return {
      status: "success",
      message: tCrm("crm.modules.workspace.actions.smartLabelCreated", locale),
    };
  } catch (error) {
    return mapOperationalMutationError(error, "/tags", locale);
  }
}

export async function createClassificationSchemeOperationalAction(
  _previous: CrmMutationState,
  formData: FormData,
): Promise<CrmMutationState> {
  const { locale, options } = await prepareOperationalAction("/tags", "tags.manage");
  const parsed = readFields(
    formData,
    [
      {
        field: "name",
        labelKey: "crm.modules.workspace.actions.fields.name",
        required: true,
        maxLength: 200,
      },
      {
        field: "entityType",
        labelKey: "crm.modules.workspace.actions.fields.entityType",
        required: true,
        maxLength: 200,
      },
    ],
    locale,
  );

  if (parsed.fieldErrors) {
    return {
      status: "error",
      message: tCrm("crm.forms.errors.reviewTitle", locale),
      fieldErrors: parsed.fieldErrors,
    };
  }

  try {
    await crmApiClient.createClassificationScheme(
      {
        name: parsed.values.name ?? "",
        entityType: parsed.values.entityType ?? "",
      },
      options,
    );
    revalidatePath("/tags");
    return {
      status: "success",
      message: tCrm("crm.modules.workspace.actions.classificationCreated", locale),
    };
  } catch (error) {
    return mapOperationalMutationError(error, "/tags", locale);
  }
}
