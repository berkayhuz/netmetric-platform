import { translate } from "@netmetric/i18n";

import { updateProfileAction } from "@/features/account/actions/profile-actions";
import { ProfileEditForm } from "@/features/account/components/profile-edit-form";
import {
  getAccountOptionsForPage,
  getProfileForPage,
} from "@/features/account/data/account-read-data";
import { mapAccountLanguageToLocale } from "@/lib/account-locale";
import { handleAccountApiPageError } from "@/lib/auth/handle-account-api-page-error";
import { requireAccountSession } from "@/lib/auth/require-account-session";

export default async function ProfilePage() {
  await requireAccountSession("/profile");

  let profile;
  let options;
  let locale = "en";
  try {
    [profile, options] = await Promise.all([getProfileForPage(), getAccountOptionsForPage()]);
    locale = mapAccountLanguageToLocale(profile.culture);
  } catch (error) {
    handleAccountApiPageError(error);
  }

  return (
    <ProfileEditForm
      profile={profile}
      options={options}
      action={updateProfileAction}
      copy={{
        pageTitle: translate("account.profile.title", { locale }),
        pageDescription: translate("account.profile.description", { locale }),
        updatedTitle: translate("account.profile.updated.title", { locale }),
        updateFailedTitle: translate("account.profile.updated.errorTitle", { locale }),
        editCardTitle: translate("account.profile.edit.title", { locale }),
        editCardDescription: translate("account.profile.edit.description", { locale }),
        fields: {
          displayName: translate("account.profile.fields.displayName", { locale }),
          firstName: translate("account.profile.fields.firstName", { locale }),
          lastName: translate("account.profile.fields.lastName", { locale }),
          phoneCountry: translate("account.profile.fields.phoneCountry", { locale }),
          noPhone: translate("account.profile.phone.noPhone", { locale }),
          phoneNationalNumber: translate("account.profile.fields.phoneNationalNumber", { locale }),
          jobTitle: translate("account.profile.fields.jobTitle", { locale }),
          department: translate("account.profile.fields.department", { locale }),
          timeZone: translate("account.profile.fields.timeZone", { locale }),
          language: translate("account.profile.fields.language", { locale }),
        },
        help: {
          displayNameManaged: translate("account.profile.help.displayNameManaged", { locale }),
          phoneNationalNumber: translate("account.profile.help.phoneNationalNumber", { locale }),
        },
        actions: {
          save: translate("account.profile.actions.save", { locale }),
          saving: translate("account.profile.actions.saving", { locale }),
          reset: translate("account.profile.actions.reset", { locale }),
        },
      }}
    />
  );
}
