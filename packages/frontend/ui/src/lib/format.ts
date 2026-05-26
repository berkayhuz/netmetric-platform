export function formatCurrency(value: number, currency = "TRY", locale = "tr-TR") {
  return new Intl.NumberFormat(locale, {
    style: "currency",
    currency,
  }).format(value);
}

export function formatDate(value: Date | string | number, locale = "tr-TR") {
  return new Intl.DateTimeFormat(locale, {
    dateStyle: "medium",
  }).format(new Date(value));
}
