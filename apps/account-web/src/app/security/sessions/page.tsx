import {
  revokeOtherSessionsAction,
  revokeOtherTrustedDevicesAction,
  revokeSessionAction,
  revokeTrustedDeviceAction,
} from "@/features/account/actions/session-management-actions";
import { SessionsManagementPanel } from "@/features/account/components/sessions-management-panel";
import { getAccountDateSettingsForPage } from "@/features/account/data/account-read-data";
import { getSessionsAndDevicesForPage } from "@/features/account/data/security-read-data";
import { handleAccountApiPageError } from "@/lib/auth/handle-account-api-page-error";
import { requireAccountSession } from "@/lib/auth/require-account-session";

export default async function SecuritySessionsPage() {
  await requireAccountSession("/security/sessions");

  let data;
  let dateSettings;
  try {
    [data, dateSettings] = await Promise.all([
      getSessionsAndDevicesForPage(),
      getAccountDateSettingsForPage(),
    ]);
  } catch (error) {
    handleAccountApiPageError(error);
  }

  return (
    <SessionsManagementPanel
      sessions={data.sessions}
      trustedDevices={data.trustedDevices}
      dateSettings={dateSettings}
      revokeSessionAction={revokeSessionAction}
      revokeOtherSessionsAction={revokeOtherSessionsAction}
      revokeTrustedDeviceAction={revokeTrustedDeviceAction}
      revokeOtherTrustedDevicesAction={revokeOtherTrustedDevicesAction}
    />
  );
}
