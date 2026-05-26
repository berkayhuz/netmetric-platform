"use client";

import { useActionState, useEffect, useId, useRef } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import {
  Button,
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
  Field,
  FieldContent,
  FieldError,
  FieldLabel,
  Input,
  Textarea,
} from "@netmetric/ui";

import { CrmMutationResult } from "@/components/forms/crm-mutation-result";
import {
  createClassificationSchemeOperationalAction,
  createContractOperationalAction,
  createFinanceOrderOperationalAction,
  createSmartLabelRuleOperationalAction,
  createTagGroupOperationalAction,
  createTagOperationalAction,
} from "@/features/modules/actions/operational-module-mutation-actions";
import {
  initialCrmMutationState,
  type CrmMutationState,
} from "@/features/shared/actions/mutation-state";
import { tCrm } from "@/lib/i18n/crm-i18n";

type OperationalModuleQuickCreateProps = {
  locale?: string | null | undefined;
  modulePath: string;
};

type OperationalFormAction = (
  state: CrmMutationState,
  formData: FormData,
) => Promise<CrmMutationState>;

type QuickField = {
  name: string;
  labelKey: string;
  type?: "text" | "color" | "textarea";
  required?: boolean;
  maxLength?: number;
  defaultValue?: string;
};

export function OperationalModuleQuickCreate({
  locale,
  modulePath,
}: Readonly<OperationalModuleQuickCreateProps>) {
  if (modulePath === "/contracts") {
    return (
      <SingleCreateWorkspace
        locale={locale}
        titleKey="crm.modules.workspace.actions.contractTitle"
        descriptionKey="crm.modules.workspace.actions.contractDescription"
        action={createContractOperationalAction}
        submitKey="crm.modules.workspace.actions.createContract"
      />
    );
  }

  if (modulePath === "/finance") {
    return (
      <SingleCreateWorkspace
        locale={locale}
        titleKey="crm.modules.workspace.actions.orderTitle"
        descriptionKey="crm.modules.workspace.actions.orderDescription"
        action={createFinanceOrderOperationalAction}
        submitKey="crm.modules.workspace.actions.createOrder"
      />
    );
  }

  if (modulePath === "/tags") {
    return <TagCreateWorkspace locale={locale} />;
  }

  if (modulePath === "/work-management" || modulePath === "/activities") {
    return <WorkManagementShortcutWorkspace locale={locale} />;
  }

  return null;
}

function WorkManagementShortcutWorkspace({
  locale,
}: Readonly<{ locale?: string | null | undefined }>) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>{tCrm("crm.modules.workspace.actions.workManagementTitle", locale)}</CardTitle>
        <CardDescription>
          {tCrm("crm.modules.workspace.actions.workManagementDescription", locale)}
        </CardDescription>
      </CardHeader>
      <CardContent className="flex flex-col gap-3 sm:flex-row">
        <Button asChild>
          <Link href="/tasks/new">{tCrm("crm.modules.workspace.actions.createTask", locale)}</Link>
        </Button>
        <Button asChild variant="secondary">
          <Link href="/tasks/meetings/new">
            {tCrm("crm.modules.workspace.actions.scheduleMeeting", locale)}
          </Link>
        </Button>
      </CardContent>
    </Card>
  );
}

function SingleCreateWorkspace({
  locale,
  titleKey,
  descriptionKey,
  action,
  submitKey,
}: Readonly<{
  locale?: string | null | undefined;
  titleKey: string;
  descriptionKey: string;
  action: OperationalFormAction;
  submitKey: string;
}>) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>{tCrm(titleKey, locale)}</CardTitle>
        <CardDescription>{tCrm(descriptionKey, locale)}</CardDescription>
      </CardHeader>
      <CardContent>
        <InlineCreateForm
          action={action}
          fields={[
            {
              name: "code",
              labelKey: "crm.modules.workspace.actions.fields.code",
              required: true,
              maxLength: 64,
            },
            {
              name: "name",
              labelKey: "crm.modules.workspace.actions.fields.name",
              required: true,
              maxLength: 200,
            },
            {
              name: "description",
              labelKey: "crm.modules.workspace.actions.fields.description",
              type: "textarea",
              maxLength: 2000,
            },
          ]}
          locale={locale}
          submitKey={submitKey}
        />
      </CardContent>
    </Card>
  );
}

function TagCreateWorkspace({ locale }: Readonly<{ locale?: string | null | undefined }>) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>{tCrm("crm.modules.workspace.actions.tagsTitle", locale)}</CardTitle>
        <CardDescription>
          {tCrm("crm.modules.workspace.actions.tagsDescription", locale)}
        </CardDescription>
      </CardHeader>
      <CardContent className="grid gap-4 xl:grid-cols-2">
        <ActionPanel
          locale={locale}
          titleKey="crm.modules.workspace.actions.tagTitle"
          descriptionKey="crm.modules.workspace.actions.tagDescription"
          action={createTagOperationalAction}
          submitKey="crm.modules.workspace.actions.createTag"
          fields={[
            {
              name: "name",
              labelKey: "crm.modules.workspace.actions.fields.name",
              required: true,
              maxLength: 200,
            },
            {
              name: "color",
              labelKey: "crm.modules.workspace.actions.fields.color",
              type: "color",
              defaultValue: "#2563eb",
              maxLength: 200,
            },
          ]}
        />
        <ActionPanel
          locale={locale}
          titleKey="crm.modules.workspace.actions.tagGroupTitle"
          descriptionKey="crm.modules.workspace.actions.tagGroupDescription"
          action={createTagGroupOperationalAction}
          submitKey="crm.modules.workspace.actions.createTagGroup"
          fields={[
            {
              name: "name",
              labelKey: "crm.modules.workspace.actions.fields.name",
              required: true,
              maxLength: 200,
            },
            {
              name: "color",
              labelKey: "crm.modules.workspace.actions.fields.color",
              type: "color",
              defaultValue: "#16a34a",
              maxLength: 200,
            },
          ]}
        />
        <ActionPanel
          locale={locale}
          titleKey="crm.modules.workspace.actions.smartLabelTitle"
          descriptionKey="crm.modules.workspace.actions.smartLabelDescription"
          action={createSmartLabelRuleOperationalAction}
          submitKey="crm.modules.workspace.actions.createSmartLabel"
          fields={[
            {
              name: "name",
              labelKey: "crm.modules.workspace.actions.fields.name",
              required: true,
              maxLength: 200,
            },
            {
              name: "entityType",
              labelKey: "crm.modules.workspace.actions.fields.entityType",
              required: true,
              maxLength: 200,
              defaultValue: "Customer",
            },
            {
              name: "conditionJson",
              labelKey: "crm.modules.workspace.actions.fields.conditionJson",
              type: "textarea",
              required: true,
              maxLength: 200,
              defaultValue: '{"field":"status","operator":"equals","value":"active"}',
            },
          ]}
        />
        <ActionPanel
          locale={locale}
          titleKey="crm.modules.workspace.actions.classificationTitle"
          descriptionKey="crm.modules.workspace.actions.classificationDescription"
          action={createClassificationSchemeOperationalAction}
          submitKey="crm.modules.workspace.actions.createClassification"
          fields={[
            {
              name: "name",
              labelKey: "crm.modules.workspace.actions.fields.name",
              required: true,
              maxLength: 200,
            },
            {
              name: "entityType",
              labelKey: "crm.modules.workspace.actions.fields.entityType",
              required: true,
              maxLength: 200,
              defaultValue: "Customer",
            },
          ]}
        />
      </CardContent>
    </Card>
  );
}

function ActionPanel({
  locale,
  titleKey,
  descriptionKey,
  action,
  fields,
  submitKey,
}: Readonly<{
  locale?: string | null | undefined;
  titleKey: string;
  descriptionKey: string;
  action: OperationalFormAction;
  fields: QuickField[];
  submitKey: string;
}>) {
  return (
    <section className="rounded-md border bg-muted/20 p-4">
      <div className="mb-4 space-y-1">
        <h3 className="text-sm font-semibold">{tCrm(titleKey, locale)}</h3>
        <p className="text-sm text-muted-foreground">{tCrm(descriptionKey, locale)}</p>
      </div>
      <InlineCreateForm action={action} fields={fields} locale={locale} submitKey={submitKey} />
    </section>
  );
}

function InlineCreateForm({
  action,
  fields,
  locale,
  submitKey,
}: Readonly<{
  action: OperationalFormAction;
  fields: QuickField[];
  locale?: string | null | undefined;
  submitKey: string;
}>) {
  const router = useRouter();
  const formRef = useRef<HTMLFormElement>(null);
  const formId = useId();
  const [state, formAction, isPending] = useActionState(action, initialCrmMutationState);

  useEffect(() => {
    if (state.status === "success") {
      formRef.current?.reset();
      router.refresh();
    }
  }, [router, state.status]);

  return (
    <form action={formAction} className="space-y-4" ref={formRef}>
      <CrmMutationResult locale={locale} state={state} />
      <div className="grid gap-4 sm:grid-cols-2">
        {fields.map((field) => (
          <QuickInput
            field={field}
            formId={formId}
            key={field.name}
            locale={locale}
            state={state}
          />
        ))}
      </div>
      <div className="flex justify-end">
        <Button type="submit" disabled={isPending} aria-busy={isPending}>
          {isPending ? tCrm("crm.forms.actions.saving", locale) : tCrm(submitKey, locale)}
        </Button>
      </div>
    </form>
  );
}

function QuickInput({
  field,
  formId,
  locale,
  state,
}: Readonly<{
  field: QuickField;
  formId: string;
  locale?: string | null | undefined;
  state: CrmMutationState;
}>) {
  const inputId = `${formId}-${field.name}`;
  const error = state.fieldErrors?.[field.name]?.[0];
  const commonProps = {
    id: inputId,
    name: field.name,
    required: field.required,
    maxLength: field.maxLength,
    defaultValue: field.defaultValue,
    "aria-invalid": Boolean(error),
  };

  return (
    <Field className={field.type === "textarea" ? "sm:col-span-2" : undefined}>
      <FieldLabel htmlFor={inputId}>{tCrm(field.labelKey, locale)}</FieldLabel>
      <FieldContent>
        {field.type === "textarea" ? (
          <Textarea {...commonProps} rows={3} />
        ) : (
          <Input {...commonProps} type={field.type ?? "text"} />
        )}
        <FieldError>{error}</FieldError>
      </FieldContent>
    </Field>
  );
}
