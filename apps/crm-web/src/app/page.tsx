import { redirect } from "next/navigation";

import { requireCrmSession } from "@/lib/crm-auth/require-crm-session";

export default async function HomePage() {
  await requireCrmSession("/");
  redirect("/dashboard");
}
