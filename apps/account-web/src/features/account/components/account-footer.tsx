import { Muted } from "@netmetric/ui";
import { tAccountClient } from "@/lib/i18n/account-i18n";

export function AccountFooter() {
  return (
    <footer className="border-t border-border/80">
      <div className="mx-auto w-full max-w-6xl px-4 py-6 sm:px-6 lg:px-8">
        <Muted>{tAccountClient("account.footer.scaffold")}</Muted>
      </div>
    </footer>
  );
}
