import { redirect } from "next/navigation";

function resolveCrmTrashUrl(): string {
  const crmBaseUrl = process.env.NEXT_PUBLIC_CRM_URL?.trim();
  if (!crmBaseUrl) {
    return "/login";
  }

  try {
    return new URL("/trash", crmBaseUrl).toString();
  } catch {
    return "/login";
  }
}

export default function AuthTrashRedirectPage() {
  redirect(resolveCrmTrashUrl());
}
