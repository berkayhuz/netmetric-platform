"use client";

import { useMemo, useState } from "react";
import { useActionState } from "react";
import { useFormStatus } from "react-dom";
import {
  AsYouType,
  getCountryCallingCode,
  Metadata,
  isSupportedCountry,
  isValidPhoneNumber,
  type CountryCode,
} from "libphonenumber-js/min";
import {
  Alert,
  AlertDescription,
  AlertTitle,
  Button,
  Field,
  FieldContent,
  FieldError,
  FieldLabel,
  FieldSet,
  Input,
  TextTitle,
  cn,
} from "@netmetric/ui";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@netmetric/ui/client";

import type {
  AccountOptionsResponse,
  CountryCallingCodeOption,
  MyProfileResponse,
} from "@/lib/account-api";
import { resolveLanguageSelectState } from "@/lib/language-select";

import { initialMutationState, type MutationState } from "../actions/mutation-state";
import { AvatarManagementPanel } from "./avatar-management-panel";
import { AccountPagePanel } from "./account-page-panel";
import { tAccountClient } from "@/lib/i18n/account-i18n";

type ProfileEditFormProps = {
  profile: MyProfileResponse;
  options: AccountOptionsResponse;
  copy: ProfileEditCopy;
  action: (state: MutationState, formData: FormData) => Promise<MutationState>;
};

export type ProfileEditCopy = {
  pageTitle: string;
  pageDescription: string;
  updatedTitle: string;
  updateFailedTitle: string;
  editCardTitle: string;
  editCardDescription: string;
  fields: {
    displayName: string;
    firstName: string;
    lastName: string;
    phoneCountry: string;
    noPhone: string;
    phoneNationalNumber: string;
    jobTitle: string;
    department: string;
    timeZone: string;
    language: string;
  };
  help: {
    displayNameManaged: string;
    phoneNationalNumber: string;
  };
  actions: {
    save: string;
    saving: string;
    reset: string;
  };
};

function SubmitButton({ copy }: { copy: ProfileEditCopy }) {
  const { pending } = useFormStatus();

  return (
    <Button size="xs" type="submit" disabled={pending}>
      {pending ? copy.actions.saving : copy.actions.save}
    </Button>
  );
}

export function ProfileEditForm({ profile, options, copy, action }: ProfileEditFormProps) {
  const [state, formAction] = useActionState(action, initialMutationState);
  const languageSelect = resolveLanguageSelectState(profile.culture, options.languages);
  const noPhoneCountry = "__none__";
  const [phoneCountryIso2, setPhoneCountryIso2] = useState(
    profile.phoneCountryIso2 ?? noPhoneCountry,
  );
  const [timeZone, setTimeZone] = useState(profile.timeZone);
  const [culture, setCulture] = useState(languageSelect.selectedValue);

  return (
    <AccountPagePanel title={copy.pageTitle} description={copy.pageDescription}>
      <div className="mr-auto w-full min-w-0 space-y-6 lg:w-96">
        <AvatarManagementPanel profile={profile} />
        <form action={formAction} className="space-y-4" noValidate>
          <input type="hidden" name="version" value={profile.version} />
          <FieldSet>
            <FormField
              id="firstName"
              name="firstName"
              label={copy.fields.firstName}
              defaultValue={profile.firstName}
              error={state.fieldErrors?.firstName?.[0]}
              helpText={undefined}
              readOnly={undefined}
            />
            <FormField
              id="lastName"
              name="lastName"
              label={copy.fields.lastName}
              defaultValue={profile.lastName}
              error={state.fieldErrors?.lastName?.[0]}
              helpText={undefined}
              readOnly={undefined}
            />
            <PhoneNumberField
              className="sm:col-span-2"
              countries={options.phoneCountries}
              countryError={state.fieldErrors?.phoneCountryIso2?.[0]}
              countryValue={phoneCountryIso2}
              copy={copy}
              defaultNationalNumber={profile.phoneNationalNumber ?? ""}
              noPhoneCountry={noPhoneCountry}
              onCountryChange={setPhoneCountryIso2}
              phoneError={state.fieldErrors?.phoneNationalNumber?.[0]}
            />
            <FormField
              id="jobTitle"
              name="jobTitle"
              label={copy.fields.jobTitle}
              defaultValue={profile.jobTitle ?? ""}
              error={state.fieldErrors?.jobTitle?.[0]}
              helpText={undefined}
              readOnly={undefined}
            />
            <FormField
              id="department"
              name="department"
              label={copy.fields.department}
              defaultValue={profile.department ?? ""}
              error={state.fieldErrors?.department?.[0]}
              helpText={undefined}
              readOnly={undefined}
            />
            <Field>
              <FieldLabel htmlFor="timeZone">{copy.fields.timeZone}</FieldLabel>
              <FieldContent>
                <input type="hidden" id="timeZone" name="timeZone" value={timeZone} />
                <Select
                  value={timeZone}
                  onValueChange={(nextValue) => setTimeZone(nextValue ?? "")}
                >
                  <SelectTrigger aria-invalid={Boolean(state.fieldErrors?.timeZone?.[0])}>
                    <SelectValue placeholder={tAccountClient("account.profile.selectTimeZone")} />
                  </SelectTrigger>
                  <SelectContent className="!w-full">
                    {options.timeZones.map((item) => (
                      <SelectItem key={item.value} value={item.value}>
                        {item.label}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                <FieldError id="timeZone-error">{state.fieldErrors?.timeZone?.[0]}</FieldError>
              </FieldContent>
            </Field>
            <Field>
              <FieldLabel htmlFor="culture">{copy.fields.language}</FieldLabel>
              <FieldContent>
                <input type="hidden" id="culture" name="culture" value={culture} />
                <Select value={culture} onValueChange={(nextValue) => setCulture(nextValue ?? "")}>
                  <SelectTrigger aria-invalid={Boolean(state.fieldErrors?.culture?.[0])}>
                    <SelectValue placeholder={tAccountClient("account.profile.selectLanguage")} />
                  </SelectTrigger>
                  <SelectContent>
                    {languageSelect.options.map((item) => (
                      <SelectItem key={item.value} value={item.value}>
                        {item.label}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                <FieldError id="culture-error">{state.fieldErrors?.culture?.[0]}</FieldError>
              </FieldContent>
            </Field>
          </FieldSet>
          <div className="flex flex-wrap items-center gap-2">
            <SubmitButton copy={copy} />
            <Button size="xs" type="reset" variant="outline">
              {copy.actions.reset}
            </Button>
          </div>
        </form>
      </div>

      {state.status === "success" ? (
        <Alert>
          <AlertTitle>{copy.updatedTitle}</AlertTitle>
          <AlertDescription>{state.message}</AlertDescription>
        </Alert>
      ) : null}

      {state.status === "error" && state.message ? (
        <Alert variant="destructive">
          <AlertTitle>{copy.updateFailedTitle}</AlertTitle>
          <AlertDescription>{state.message}</AlertDescription>
        </Alert>
      ) : null}
    </AccountPagePanel>
  );
}

type PhoneNumberFieldProps = {
  className?: string;
  countries: CountryCallingCodeOption[];
  countryError: string | undefined;
  countryValue: string;
  copy: ProfileEditCopy;
  defaultNationalNumber: string;
  noPhoneCountry: string;
  onCountryChange: (value: string) => void;
  phoneError: string | undefined;
};

function PhoneNumberField({
  className,
  countries,
  countryError,
  countryValue,
  copy,
  defaultNationalNumber,
  noPhoneCountry,
  onCountryChange,
  phoneError,
}: PhoneNumberFieldProps) {
  const selectedCountry = countries.find((country) => country.iso2 === countryValue);
  const selectedCountryCode = getSupportedCountryCode(countryValue, noPhoneCountry);
  const [nationalDigits, setNationalDigits] = useState(() =>
    limitNationalDigitsForCountry(
      normalizePhoneNationalNumber(defaultNationalNumber),
      selectedCountryCode,
      selectedCountry?.dialCode,
    ),
  );
  const submittedNationalDigits = useMemo(
    () =>
      limitNationalDigitsForCountry(
        normalizePhoneNationalNumber(nationalDigits),
        selectedCountryCode,
        selectedCountry?.dialCode,
      ),
    [nationalDigits, selectedCountryCode, selectedCountry?.dialCode],
  );
  const formattedNationalNumber = useMemo(
    () => formatNationalPhoneNumber(submittedNationalDigits, selectedCountryCode),
    [submittedNationalDigits, selectedCountryCode],
  );
  const hasError = Boolean(countryError || phoneError);
  const describedBy = [
    copy.help.phoneNationalNumber ? "phoneNationalNumber-help" : undefined,
    countryError ? "phoneCountryIso2-error" : undefined,
    phoneError ? "phoneNationalNumber-error" : undefined,
  ]
    .filter(Boolean)
    .join(" ");

  return (
    <Field className={className} data-invalid={hasError || undefined}>
      <div className="flex items-end justify-between gap-3">
        <FieldLabel htmlFor="phoneNationalNumber">{copy.fields.phoneNationalNumber}</FieldLabel>
        <span className="text-xs font-medium text-muted-foreground">
          {copy.fields.phoneCountry}
        </span>
      </div>
      <FieldContent>
        <input
          type="hidden"
          id="phoneCountryIso2"
          name="phoneCountryIso2"
          value={countryValue === noPhoneCountry ? "" : countryValue}
        />
        <input type="hidden" name="phoneNationalNumber" value={submittedNationalDigits} />
        <div
          className={cn(
            "group/phone-field grid min-h-8 w-full grid-cols-[4.25rem_minmax(0,1fr)] overflow-hidden rounded-md border border-input bg-background shadow-xs transition-[border-color,box-shadow,background-color] duration-200",
            "focus-within:border-ring focus-within:ring-3 focus-within:ring-ring/35",
            "hover:border-ring/60",
            "has-[[data-slot][aria-invalid=true]]:border-destructive has-[[data-slot][aria-invalid=true]]:ring-3 has-[[data-slot][aria-invalid=true]]:ring-destructive/20",
            "dark:bg-input/30 dark:hover:bg-input/50 dark:has-[[data-slot][aria-invalid=true]]:ring-destructive/40",
          )}
        >
          <Select
            value={countryValue}
            onValueChange={(nextValue) => onCountryChange(nextValue ?? noPhoneCountry)}
          >
            <SelectTrigger
              aria-invalid={Boolean(countryError)}
              aria-label={copy.fields.phoneCountry}
              aria-describedby={describedBy || undefined}
              className={cn(
                "h-full min-h-8 w-full self-stretch rounded-none border-0 border-r bg-muted/35 px-3 shadow-none ring-0 focus-visible:ring-0",
                "hover:bg-muted/60 data-placeholder:text-muted-foreground dark:bg-transparent dark:hover:bg-input/50",
              )}
            >
              <SelectValue placeholder={copy.fields.noPhone} />
            </SelectTrigger>
            <SelectContent align="start" className="min-w-72">
              <SelectItem value={noPhoneCountry}>{copy.fields.noPhone}</SelectItem>
              {countries.map((country) => (
                <SelectItem key={country.iso2} value={country.iso2}>
                  <span className="font-medium">{country.name}</span>
                  <span className="text-muted-foreground">
                    {country.iso2} {country.dialCode}
                  </span>
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
          <div className="flex min-h-8 min-w-0 items-center bg-transparent">
            {selectedCountry ? (
              <span className="ml-3 shrink-0 rounded-sm bg-muted px-2 py-1 text-sm font-medium text-muted-foreground">
                {selectedCountry.dialCode}
              </span>
            ) : null}
            <Input
              id="phoneNationalNumber"
              type="tel"
              inputMode="tel"
              autoComplete="tel-national"
              pattern="[0-9]*"
              value={formattedNationalNumber}
              placeholder="5551234567"
              aria-invalid={Boolean(phoneError)}
              aria-describedby={describedBy || undefined}
              onChange={(event) => {
                setNationalDigits(
                  limitNationalDigitsForCountry(
                    normalizePhoneNationalNumber(event.currentTarget.value),
                    selectedCountryCode,
                    selectedCountry?.dialCode,
                  ),
                );
              }}
              className="h-8 min-w-0 flex-1 rounded-none border-0 bg-transparent px-3 shadow-none ring-0 focus-visible:ring-0 aria-invalid:ring-0 dark:bg-transparent"
            />
          </div>
        </div>
        {copy.help.phoneNationalNumber ? (
          <TextTitle
            id="phoneNationalNumber-help"
            className="mt-1 ml-1 text-xs text-muted-foreground"
          >
            {copy.help.phoneNationalNumber}
          </TextTitle>
        ) : null}
        <FieldError id="phoneCountryIso2-error">{countryError}</FieldError>
        <FieldError id="phoneNationalNumber-error">{phoneError}</FieldError>
      </FieldContent>
    </Field>
  );
}

const e164MaxDigits = 15;
const maxNationalDigitsCache = new Map<CountryCode, number>();

export function onlyDigits(value: string): string {
  return value.replace(/\D/g, "");
}

export function normalizePhoneNationalNumber(value: string): string {
  return onlyDigits(value);
}

export function getSupportedCountryCode(
  iso2: string,
  noPhoneCountry: string,
): CountryCode | undefined {
  if (iso2 === noPhoneCountry) {
    return undefined;
  }

  const normalizedCountry = iso2.toUpperCase();
  return isSupportedCountry(normalizedCountry) ? normalizedCountry : undefined;
}

export function formatNationalPhoneNumber(digits: string, iso2: CountryCode | undefined): string {
  if (!digits || !iso2 || !isSupportedCountry(iso2)) {
    return digits;
  }

  return new AsYouType(iso2).input(digits);
}

export function isValidNationalPhoneNumber(digits: string, iso2: CountryCode | undefined): boolean {
  if (!digits || !iso2 || !isSupportedCountry(iso2)) {
    return false;
  }

  return isValidPhoneNumber(digits, iso2);
}

export function getMaxNationalDigitsForCountry(
  iso2: CountryCode | undefined,
  dialCode: string | undefined,
): number {
  const e164NationalDigitLimit = getE164NationalDigitLimit(iso2, dialCode);
  if (!iso2) {
    return e164NationalDigitLimit;
  }

  const cachedLimit = maxNationalDigitsCache.get(iso2);
  if (cachedLimit) {
    return Math.min(cachedLimit, e164NationalDigitLimit);
  }

  try {
    const metadata = new Metadata();
    metadata.selectNumberingPlan(iso2);
    const possibleLengths = metadata.numberingPlan
      ?.possibleLengths()
      .filter((length) => Number.isInteger(length) && length > 0);

    if (possibleLengths && possibleLengths.length > 0) {
      const maxPossibleNationalDigits = Math.max(...possibleLengths);
      maxNationalDigitsCache.set(iso2, maxPossibleNationalDigits);
      return Math.min(maxPossibleNationalDigits, e164NationalDigitLimit);
    }
  } catch {
    // Fallback to E.164-safe limit below.
  }

  return e164NationalDigitLimit;
}

function getE164NationalDigitLimit(
  iso2: CountryCode | undefined,
  dialCode: string | undefined,
): number {
  const countryCallingCodeDigitCount = iso2
    ? getCountryCallingCode(iso2).length
    : onlyDigits(dialCode ?? "").length;

  if (countryCallingCodeDigitCount <= 0) {
    return e164MaxDigits;
  }

  return Math.max(0, e164MaxDigits - countryCallingCodeDigitCount);
}

export function limitNationalDigitsForCountry(
  digits: string,
  iso2: CountryCode | undefined,
  dialCode: string | undefined,
): string {
  const maxNationalDigits = getMaxNationalDigitsForCountry(iso2, dialCode);
  if (maxNationalDigits <= 0) {
    return "";
  }

  return digits.slice(0, maxNationalDigits);
}

type FormFieldProps = {
  id: string;
  name: string;
  label: string;
  defaultValue: string;
  error: string | undefined;
  helpText: string | undefined;
  readOnly: boolean | undefined;
};

function FormField({ id, name, label, defaultValue, error, helpText, readOnly }: FormFieldProps) {
  const describedBy = error ? `${id}-error` : helpText ? `${id}-help` : undefined;

  return (
    <Field>
      <FieldLabel htmlFor={id}>{label}</FieldLabel>
      <FieldContent>
        <Input
          id={id}
          name={name}
          defaultValue={defaultValue}
          readOnly={readOnly}
          aria-invalid={Boolean(error)}
          aria-describedby={describedBy}
        />
        {helpText ? (
          <TextTitle id={`${id}-help`} className="text-xs mt-0.5 ml-1 text-muted-foreground">
            {helpText}
          </TextTitle>
        ) : null}
        <FieldError id={`${id}-error`}>{error}</FieldError>
      </FieldContent>
    </Field>
  );
}
