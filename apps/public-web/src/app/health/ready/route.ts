import { NextResponse } from "next/server";

const required = [
  "NEXT_PUBLIC_SITE_URL",
  "NEXT_PUBLIC_AUTH_URL",
  "NEXT_PUBLIC_ACCOUNT_URL",
  "NEXT_PUBLIC_CRM_URL",
  "NEXT_PUBLIC_TOOLS_URL",
  "NEXT_PUBLIC_API_URL",
];

export async function GET() {
  const missing = required.filter((key) => !process.env[key]?.trim());
  if (missing.length > 0) {
    return NextResponse.json({ status: "not_ready", missing }, { status: 503 });
  }

  return NextResponse.json({ status: "ready" });
}
