import { z } from "zod";

export const postLoginDestinationSchema = z.enum(["Account", "Tools", "Crm", "Public"]);
export type PostLoginDestination = z.infer<typeof postLoginDestinationSchema>;

type DestinationOption = {
  key: PostLoginDestination;
  label: string;
  url: string;
};

const urlSchema = z.url();

function normalizeUrl(input: string | undefined, fallback: string): string {
  const value = (input?.trim() || fallback).replace(/\/+$/, "");
  return urlSchema.parse(value);
}

function buildDestinationUrls() {
  const accountUrl = normalizeUrl(process.env.NEXT_PUBLIC_ACCOUNT_URL, "http://localhost:7004");
  const toolsUrl = normalizeUrl(process.env.NEXT_PUBLIC_TOOLS_URL, "http://localhost:7005");
  const crmUrl = normalizeUrl(process.env.NEXT_PUBLIC_CRM_URL, "http://localhost:7006");
  const publicUrl = normalizeUrl(process.env.NEXT_PUBLIC_PUBLIC_URL, "http://localhost:7001");

  return {
    Account: `${new URL(accountUrl).origin}/`,
    Tools: `${new URL(toolsUrl).origin}/`,
    Crm: `${new URL(crmUrl).origin}/`,
    Public: `${new URL(publicUrl).origin}/`,
  } as const;
}

export function resolvePostLoginDestination(
  value: string | null | undefined,
): PostLoginDestination {
  const parsed = postLoginDestinationSchema.safeParse(value);
  return parsed.success ? parsed.data : "Account";
}

export function resolvePostLoginDestinationUrl(value: string | null | undefined): string {
  const destination = resolvePostLoginDestination(value);
  const urls = buildDestinationUrls();
  return urls[destination];
}

export function getPostLoginDestinationOptions(): readonly DestinationOption[] {
  const urls = buildDestinationUrls();
  return [
    { key: "Account", label: "Account", url: urls.Account },
    { key: "Tools", label: "Tools", url: urls.Tools },
    { key: "Crm", label: "CRM", url: urls.Crm },
    { key: "Public", label: "Public", url: urls.Public },
  ] as const;
}
