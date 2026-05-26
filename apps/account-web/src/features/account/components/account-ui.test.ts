import { createElement } from "react";
import { afterEach, vi } from "vitest";
import { describe, expect, it } from "vitest";
import { cleanup, render, screen, waitFor } from "@testing-library/react";
import { fireEvent } from "@testing-library/react";
import { toast } from "@netmetric/ui/client";

vi.mock("@netmetric/ui/client", async () => {
  const actual =
    await vi.importActual<typeof import("@netmetric/ui/client")>("@netmetric/ui/client");

  return {
    ...actual,
    toast: {
      success: vi.fn(),
      error: vi.fn(),
    },
  };
});

import {
  formatNationalPhoneNumber,
  getMaxNationalDigitsForCountry,
  getSupportedCountryCode,
  limitNationalDigitsForCountry,
  normalizePhoneNationalNumber,
  ProfileEditForm,
} from "./profile-edit-form";
import { PreferencesEditForm } from "./preferences-edit-form";
import { UserAvatar } from "./user-avatar";
import { SessionsManagementPanel } from "./sessions-management-panel";
import type { MutationState } from "../actions/mutation-state";
import type {
  AccountOptionsResponse,
  MyProfileResponse,
  OrganizationMembershipSummaryResponse,
  TrustedDevicesResponse,
  UserPreferenceResponse,
  UserSessionsResponse,
} from "../../../lib/account-api";

afterEach(() => {
  cleanup();
  vi.clearAllMocks();
});

const options: AccountOptionsResponse = {
  languages: [
    { value: "en-US", label: "English" },
    { value: "tr-TR", label: "Turkish" },
  ],
  timeZones: [{ value: "UTC", label: "UTC" }],
  themes: [
    { value: "System", label: "System" },
    { value: "Dark", label: "Dark" },
    { value: "Light", label: "Light" },
  ],
  dateFormats: [{ value: "yyyy-MM-dd", label: "2026-05-15" }],
  phoneCountries: [
    { iso2: "TR", name: "Turkey", dialCode: "+90" },
    { iso2: "US", name: "United States", dialCode: "+1" },
    { iso2: "GB", name: "United Kingdom", dialCode: "+44" },
    { iso2: "AF", name: "Afghanistan", dialCode: "+93" },
    { iso2: "AL", name: "Albania", dialCode: "+355" },
    { iso2: "DE", name: "Germany", dialCode: "+49" },
    { iso2: "AZ", name: "Azerbaijan", dialCode: "+994" },
    { iso2: "AE", name: "United Arab Emirates", dialCode: "+971" },
    { iso2: "SA", name: "Saudi Arabia", dialCode: "+966" },
  ],
};

const profileCopy = {
  pageTitle: "Profile",
  pageDescription: "Update profile information and manage your avatar.",
  updatedTitle: "Profile updated",
  updateFailedTitle: "Update failed",
  editCardTitle: "Edit profile",
  editCardDescription: "Changes are saved to your account profile.",
  fields: {
    displayName: "Display name",
    firstName: "First name",
    lastName: "Last name",
    phoneCountry: "Phone country",
    noPhone: "No phone",
    phoneNationalNumber: "Phone national number",
    jobTitle: "Job title",
    department: "Department",
    timeZone: "Time zone",
    language: "Language",
  },
  help: {
    displayNameManaged: "Display name is managed by backend profile rules.",
    phoneNationalNumber: "Enter without country code.",
  },
  actions: {
    save: "Save profile",
    saving: "Saving...",
    reset: "Reset",
  },
} as const;

const profile: MyProfileResponse = {
  id: "p1",
  tenantId: "t1",
  userId: "u1",
  firstName: "Ada",
  lastName: "Lovelace",
  displayName: "Ada Lovelace",
  phoneNumber: null,
  phoneCountryIso2: null,
  phoneCountryCallingCode: null,
  phoneNationalNumber: null,
  avatarUrl: null,
  jobTitle: null,
  department: null,
  timeZone: "UTC",
  culture: "en-US",
  version: "v1",
};

const preferences: UserPreferenceResponse = {
  id: "pref1",
  theme: "System",
  language: "en-US",
  timeZone: "UTC",
  dateFormat: "yyyy-MM-dd",
  postLoginDestination: "Account",
  defaultOrganizationId: null,
  faviconUrl: null,
  version: "v1",
};

const organizations: OrganizationMembershipSummaryResponse[] = [
  {
    organizationId: "org1",
    tenantId: "tenant1",
    organizationName: "Main Org",
    organizationSlug: "main-org",
    status: "active",
    isDefault: true,
    joinedAt: "2026-05-15T00:00:00Z",
    lastPermissionRefreshAt: null,
    roles: ["owner"],
  },
];

const sessions: UserSessionsResponse = {
  items: [
    {
      id: "s1",
      deviceName: "Current Device",
      ipAddress: "127.0.0.1",
      userAgent: "UA",
      approximateLocation: "Local",
      createdAt: "2026-05-15T10:00:00Z",
      lastSeenAt: "2026-05-15T10:00:00Z",
      expiresAt: "2026-05-16T10:00:00Z",
      isCurrent: true,
      isActive: true,
    },
    {
      id: "s2",
      deviceName: "Other Device",
      ipAddress: "127.0.0.2",
      userAgent: "UA2",
      approximateLocation: "Local",
      createdAt: "2026-05-15T08:00:00Z",
      lastSeenAt: "2026-05-15T09:00:00Z",
      expiresAt: "2026-05-16T08:00:00Z",
      isCurrent: false,
      isActive: true,
    },
  ],
};

const devices: TrustedDevicesResponse = {
  items: [
    {
      id: "d1",
      name: "Laptop",
      userAgent: "UA",
      ipAddress: "127.0.0.1",
      trustedAt: "2026-05-15T08:00:00Z",
      expiresAt: "2026-06-15T09:00:00Z",
      isCurrent: false,
      isActive: true,
    },
  ],
};

describe("Account forms and panels", () => {
  const idleMutation = async (
    previous: MutationState,
    formData: FormData,
  ): Promise<MutationState> => {
    void previous;
    void formData;
    return { status: "idle" };
  };

  it("renders profile selects from options", () => {
    render(
      createElement(ProfileEditForm, { profile, options, copy: profileCopy, action: idleMutation }),
    );
    expect(screen.getByText("Phone country")).toBeTruthy();
    expect(screen.getByText("Time zone")).toBeTruthy();
    expect(screen.getByText("Language")).toBeTruthy();
  });

  it("formats phone input while submitting clean national digits", () => {
    const trProfile: MyProfileResponse = {
      ...profile,
      phoneCountryIso2: "TR",
      phoneCountryCallingCode: "+90",
    };

    const { container } = render(
      createElement(ProfileEditForm, {
        profile: trProfile,
        options,
        copy: profileCopy,
        action: idleMutation,
      }),
    );

    const phoneInput = screen.getByLabelText("Phone national number") as HTMLInputElement;
    fireEvent.change(phoneInput, { target: { value: "5551234567" } });

    const submittedPhoneInput = container.querySelector<HTMLInputElement>(
      'input[name="phoneNationalNumber"]',
    );
    const submittedCountryInput = container.querySelector<HTMLInputElement>(
      'input[name="phoneCountryIso2"]',
    );
    const form = container
      .querySelector<HTMLInputElement>('input[name="version"]')
      ?.closest("form");
    const formData = new FormData(form ?? undefined);

    expect(phoneInput.hasAttribute("name")).toBe(false);
    expect(phoneInput.value).toBe("555 123 45 67");
    expect(submittedPhoneInput?.value).toBe("5551234567");
    expect(submittedCountryInput?.value).toBe("TR");
    expect(formData.get("phoneNationalNumber")).toBe("5551234567");
    expect(formData.get("phoneCountryIso2")).toBe("TR");
  });

  it("normalizes pasted phone punctuation and supports metadata formatting by country", () => {
    expect(normalizePhoneNationalNumber("(555) 123-45-67")).toBe("5551234567");
    expect(formatNationalPhoneNumber("5551234567", "TR")).toBe("555 123 45 67");
    expect(formatNationalPhoneNumber("5551234567", "US")).toBe("(555) 123-4567");
    expect(formatNationalPhoneNumber("02079460056", "GB")).toBe("020 7946 0056");
    expect(formatNationalPhoneNumber("701234567", "AF")).toBe("701234567");
    expect(formatNationalPhoneNumber("672123456", "AL")).toBe("672123456");
    expect(formatNationalPhoneNumber("5551234567", undefined)).toBe("5551234567");
  });

  it("keeps digits when country changes and reapplies national formatting", () => {
    const typedDigits = normalizePhoneNationalNumber("555 123 45 67");
    const trLimited = limitNationalDigitsForCountry(typedDigits, "TR", "+90");
    const usLimited = limitNationalDigitsForCountry(typedDigits, "US", "+1");

    expect(trLimited).toBe("5551234567");
    expect(usLimited).toBe("5551234567");
    expect(formatNationalPhoneNumber(trLimited, "TR")).toBe("555 123 45 67");
    expect(formatNationalPhoneNumber(usLimited, "US")).toBe("(555) 123-4567");
  });

  it("derives max national digits from metadata and enforces limits for AF and AL", () => {
    const afMaxNationalDigits = getMaxNationalDigitsForCountry("AF", "+93");
    const alMaxNationalDigits = getMaxNationalDigitsForCountry("AL", "+355");

    expect(afMaxNationalDigits).toBeGreaterThan(0);
    expect(alMaxNationalDigits).toBeGreaterThan(0);
    expect(afMaxNationalDigits).toBeLessThanOrEqual(12);
    expect(alMaxNationalDigits).toBeLessThanOrEqual(12);

    const longInput = "12345678901234567890";
    const afLimited = limitNationalDigitsForCountry(longInput, "AF", "+93");
    const alLimited = limitNationalDigitsForCountry(longInput, "AL", "+355");

    expect(afLimited.length).toBe(afMaxNationalDigits);
    expect(alLimited.length).toBe(alMaxNationalDigits);
  });

  it("returns a metadata-based max national digit limit for every supported country in options", () => {
    for (const country of options.phoneCountries) {
      const supportedCountry = getSupportedCountryCode(country.iso2, "__none__");
      const maxNationalDigits = getMaxNationalDigitsForCountry(supportedCountry, country.dialCode);
      expect(Number.isInteger(maxNationalDigits)).toBe(true);
      expect(maxNationalDigits).toBeGreaterThan(0);
      expect(maxNationalDigits).toBeLessThanOrEqual(15);
    }
  });

  it("normalizes no-country phone input without formatting or runtime errors", () => {
    const { container } = render(
      createElement(ProfileEditForm, { profile, options, copy: profileCopy, action: idleMutation }),
    );

    const phoneInput = screen.getByLabelText("Phone national number") as HTMLInputElement;
    fireEvent.change(phoneInput, { target: { value: "(555) 123-45-67" } });

    const form = container
      .querySelector<HTMLInputElement>('input[name="version"]')
      ?.closest("form");
    const formData = new FormData(form ?? undefined);

    expect(phoneInput.value).toBe("5551234567");
    expect(formData.get("phoneNationalNumber")).toBe("5551234567");
    expect(formData.get("phoneCountryIso2")).toBe("");
  });

  it("caps Turkish national digits to metadata max while preserving digit-only submit value", () => {
    const trProfile: MyProfileResponse = {
      ...profile,
      phoneCountryIso2: "TR",
      phoneCountryCallingCode: "+90",
    };

    const { container } = render(
      createElement(ProfileEditForm, {
        profile: trProfile,
        options,
        copy: profileCopy,
        action: idleMutation,
      }),
    );

    const phoneInput = screen.getByLabelText("Phone national number") as HTMLInputElement;
    const trMaxNationalDigits = getMaxNationalDigitsForCountry("TR", "+90");

    fireEvent.change(phoneInput, { target: { value: "5551234567890123" } });

    const submittedPhoneInput = container.querySelector<HTMLInputElement>(
      'input[name="phoneNationalNumber"]',
    );
    expect(submittedPhoneInput?.value.length).toBe(trMaxNationalDigits);
    expect(submittedPhoneInput?.value).toBe("5551234567890123".slice(0, trMaxNationalDigits));
  });

  it("renders preferences selects including default organization", () => {
    render(
      createElement(PreferencesEditForm, {
        preferences,
        options,
        organizations,
        action: idleMutation,
      }),
    );
    expect(screen.getByText("Language")).toBeTruthy();
    expect(screen.getByText("Time zone")).toBeTruthy();
    expect(screen.getByText("Theme")).toBeTruthy();
    expect(screen.getByText(/Date format/i)).toBeTruthy();
    expect(screen.getByText(/After sign-in, open/i)).toBeTruthy();
    expect(screen.getByText(/Default organization/i)).toBeTruthy();
  });

  it("shows a reload action when preferences update hits a concurrency conflict", async () => {
    const conflictAction = async (
      previous: MutationState,
      formData: FormData,
    ): Promise<MutationState> => {
      void previous;
      void formData;
      return {
        status: "error",
        code: "conflict",
        message: "Your preferences changed in another tab.",
      };
    };

    render(
      createElement(PreferencesEditForm, {
        preferences,
        options,
        organizations,
        action: conflictAction,
      }),
    );

    fireEvent.click(screen.getByRole("button", { name: /save preferences/i }));

    await waitFor(() => {
      expect(toast.error).toHaveBeenCalledWith(
        expect.any(String),
        expect.objectContaining({
          action: expect.objectContaining({
            label: expect.stringMatching(/Reload latest preferences/i),
          }),
          description: "Your preferences changed in another tab.",
        }),
      );
    });
  });

  it("renders default avatar when avatar url is missing", () => {
    render(createElement(UserAvatar, { displayName: "Ada Lovelace", avatarUrl: null }));
    expect(screen.getByLabelText("Ada Lovelace default avatar")).toBeTruthy();
    expect(screen.getByText("AL")).toBeTruthy();
  });

  it("exposes revoke-one and revoke-all-other actions", () => {
    render(
      createElement(SessionsManagementPanel, {
        sessions,
        trustedDevices: devices,
        dateSettings: { locale: "en-US", timeZone: "UTC", dateFormat: "yyyy-MM-dd" },
        revokeSessionAction: idleMutation,
        revokeOtherSessionsAction: idleMutation,
        revokeTrustedDeviceAction: idleMutation,
        revokeOtherTrustedDevicesAction: idleMutation,
      }),
    );

    expect(screen.getByRole("button", { name: /Other Device actions/i })).toBeTruthy();
    expect(screen.getByRole("button", { name: /Laptop actions/i })).toBeTruthy();
    expect(screen.getAllByRole("button", { name: /Options/i }).length).toBeGreaterThan(0);
  });
});
