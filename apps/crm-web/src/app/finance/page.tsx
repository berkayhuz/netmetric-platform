import { renderCrmModuleShell } from "@/features/modules/render-module-shell";

export default async function Page() {
  return renderCrmModuleShell("/finance");
}
