import { NextResponse } from "next/server";

const required = ["NEXT_PUBLIC_AUTH_URL", "NEXT_PUBLIC_API_BASE_URL"];

export function GET() {
  const missing = required.filter((key) => !process.env[key]?.trim());
  if (missing.length > 0) {
    return NextResponse.json({ status: "not_ready", missing }, { status: 503 });
  }

  return NextResponse.json({ status: "ready" });
}
