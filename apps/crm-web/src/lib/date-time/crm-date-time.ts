export type CrmDateSettings = {
  locale: string;
  timeZone: string;
  dateFormat: string;
};

function getDateOrder(dateFormat: string): Intl.DateTimeFormatOptions {
  switch (dateFormat) {
    case "yyyy-MM-dd":
      return { year: "numeric", month: "2-digit", day: "2-digit" };
    case "dd/MM/yyyy":
      return { day: "2-digit", month: "2-digit", year: "numeric" };
    case "MM/dd/yyyy":
      return { month: "2-digit", day: "2-digit", year: "numeric" };
    case "dd.MM.yyyy":
      return { day: "2-digit", month: "2-digit", year: "numeric" };
    case "d MMM yyyy":
      return { day: "numeric", month: "short", year: "numeric" };
    default:
      return { year: "numeric", month: "2-digit", day: "2-digit" };
  }
}

function toDate(value: string | null | undefined): Date | null {
  if (!value) {
    return null;
  }

  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? null : date;
}

function fallbackDateString(date: Date): string {
  return date.toISOString();
}

export function formatCrmDate(value: string | null | undefined, settings: CrmDateSettings): string {
  const date = toDate(value);
  if (!date) {
    return "-";
  }

  try {
    return new Intl.DateTimeFormat(settings.locale, {
      ...getDateOrder(settings.dateFormat),
      timeZone: settings.timeZone,
    }).format(date);
  } catch {
    return fallbackDateString(date);
  }
}

export function formatCrmDateTime(
  value: string | null | undefined,
  settings: CrmDateSettings,
): string {
  const date = toDate(value);
  if (!date) {
    return "-";
  }

  try {
    const datePart = new Intl.DateTimeFormat(settings.locale, {
      ...getDateOrder(settings.dateFormat),
      timeZone: settings.timeZone,
    }).format(date);
    const timePart = new Intl.DateTimeFormat(settings.locale, {
      hour: "2-digit",
      minute: "2-digit",
      second: "2-digit",
      hour12: false,
      timeZone: settings.timeZone,
    }).format(date);
    return `${datePart} ${timePart}`;
  } catch {
    return fallbackDateString(date);
  }
}
