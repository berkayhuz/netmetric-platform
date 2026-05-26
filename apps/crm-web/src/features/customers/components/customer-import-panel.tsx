import {
  Button,
  Field,
  FieldContent,
  FieldLabel,
  Input,
  NativeSelect,
  NativeSelectOption,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
  Text,
  Textarea,
} from "@netmetric/ui";

import {
  cancelCustomerImportBatchFormAction,
  commitCustomerImportBatchFormAction,
  createCustomerImportBatchFormAction,
  previewCustomerImportBatchFormAction,
  validateCustomerImportBatchFormAction,
} from "@/features/customers/actions/customer-mutation-actions";
import type { CustomerImportBatchDto } from "@/lib/crm-api";
import { tCrm } from "@/lib/i18n/crm-i18n";

const duplicateStrategies = ["Skip", "Update existing", "Create new", "Merge"] as const;

export function CustomerImportPanel({
  batches,
  canImport,
  locale,
  showCreateForm = true,
  plainLayout = false,
}: Readonly<{
  batches: CustomerImportBatchDto[];
  canImport: boolean;
  locale?: string | null | undefined;
  showCreateForm?: boolean;
  plainLayout?: boolean;
}>) {
  if (!canImport) {
    return null;
  }

  const content = (
    <div className="space-y-6">
      {showCreateForm ? (
        <form
          action={createCustomerImportBatchFormAction}
          className="grid gap-3 grid-cols-1 max-w-lg"
        >
          <Field className="md:col-span-2">
            <FieldLabel htmlFor="customer-import-file-name">
              {tCrm("crm.customers.fields.fileName", locale)}
            </FieldLabel>
            <FieldContent>
              <Input id="customer-import-file-name" name="fileName" />
            </FieldContent>
          </Field>
          <Field className="md:col-span-2">
            <FieldLabel htmlFor="customer-import-source">
              {tCrm("crm.customers.fields.source", locale)}
            </FieldLabel>
            <FieldContent>
              <Input id="customer-import-source" name="source" />
            </FieldContent>
          </Field>
          <Field className="md:col-span-2">
            <FieldLabel htmlFor="customer-import-rows">
              {tCrm("crm.customers.fields.rowsJson", locale)}
            </FieldLabel>
            <FieldContent>
              <Textarea
                defaultValue={
                  '[{"firstName":"Ada","lastName":"Lovelace","email":"ada@example.com"}]'
                }
                id="customer-import-rows"
                name="rowsJson"
                className="h-fit min-h-32 max-h-96 font-mono text-sm"
                rows={5}
              />
            </FieldContent>
          </Field>
          <div className="md:col-span-2">
            <Button type="submit">{tCrm("crm.customers.actions.createImportBatch", locale)}</Button>
          </div>
        </form>
      ) : null}

      {batches.length === 0 ? (
        <Text className="text-sm text-muted-foreground">
          {tCrm("crm.customers.states.noImportBatches", locale)}
        </Text>
      ) : (
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>{tCrm("crm.customers.fields.fileName", locale)}</TableHead>
              <TableHead>{tCrm("crm.modules.workspace.status", locale)}</TableHead>
              <TableHead>{tCrm("crm.customers.fields.totalRows", locale)}</TableHead>
              <TableHead>{tCrm("crm.customers.fields.operations", locale)}</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {batches.map((batch) => (
              <TableRow key={batch.id}>
                <TableCell>{batch.fileName}</TableCell>
                <TableCell>{String(batch.status)}</TableCell>
                <TableCell>{batch.totalRows}</TableCell>
                <TableCell>
                  <div className="flex flex-wrap gap-2">
                    <BatchButton
                      action={previewCustomerImportBatchFormAction}
                      batchId={batch.id}
                      label={tCrm("crm.customers.actions.previewImportBatch", locale)}
                    />
                    <BatchButton
                      action={validateCustomerImportBatchFormAction}
                      batchId={batch.id}
                      label={tCrm("crm.customers.actions.validateImportBatch", locale)}
                    />
                    <form action={commitCustomerImportBatchFormAction} className="flex gap-2">
                      <input name="batchId" type="hidden" value={batch.id} />
                      <NativeSelect name="duplicateStrategy" size="sm">
                        {duplicateStrategies.map((strategy, index) => (
                          <NativeSelectOption key={strategy} value={index}>
                            {strategy}
                          </NativeSelectOption>
                        ))}
                      </NativeSelect>
                      <Button type="submit" variant="outline">
                        {tCrm("crm.customers.actions.commitImportBatch", locale)}
                      </Button>
                    </form>
                    <form action={cancelCustomerImportBatchFormAction} className="flex gap-2">
                      <input name="batchId" type="hidden" value={batch.id} />
                      <Input
                        className="max-w-36"
                        name="reason"
                        placeholder={tCrm("crm.customers.fields.reason", locale)}
                      />
                      <Button type="submit" variant="outline">
                        {tCrm("crm.customers.actions.cancelImportBatch", locale)}
                      </Button>
                    </form>
                  </div>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      )}
    </div>
  );

  if (plainLayout) {
    return content;
  }

  return <div className="p-4 sm:p-5">{content}</div>;
}

function BatchButton({
  action,
  batchId,
  label,
}: Readonly<{
  action: (formData: FormData) => Promise<void>;
  batchId: string;
  label: string;
}>) {
  return (
    <form action={action}>
      <input name="batchId" type="hidden" value={batchId} />
      <Button type="submit" variant="outline">
        {label}
      </Button>
    </form>
  );
}
