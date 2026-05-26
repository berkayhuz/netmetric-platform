import { confirmEmailChangeAction } from "@/features/account/actions/security-credential-actions";
import { EmailChangeConfirmPanel } from "@/features/account/components/email-change-confirm-panel";
import { requireAccountSession } from "@/lib/auth/require-account-session";

type EmailConfirmPageProps = {
  searchParams?: Promise<Record<string, string | string[] | undefined>>;
};

function readToken(
  params: Record<string, string | string[] | undefined> | undefined,
): string | undefined {
  if (!params) {
    return undefined;
  }

  const value = params.token;
  if (typeof value === "string") {
    return value.trim() || undefined;
  }

  if (Array.isArray(value) && value.length > 0 && typeof value[0] === "string") {
    return value[0].trim() || undefined;
  }

  return undefined;
}

export default async function SecurityEmailConfirmPage({ searchParams }: EmailConfirmPageProps) {
  await requireAccountSession("/security/email/confirm");
  const resolvedSearchParams = searchParams ? await searchParams : undefined;
  const token = readToken(resolvedSearchParams);

  return <EmailChangeConfirmPanel action={confirmEmailChangeAction} tokenFromQuery={token} />;
}
