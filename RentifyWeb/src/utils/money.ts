export function parsePriceInput(
  value: string,
  locale = "tr-TR",
): string | null {
  const input = value.trim().replace(/[\s\u00a0\u202f]/g, "");
  if (!input || input.startsWith("-") || !/^[+\d.,]+$/.test(input)) return null;

  const parts = new Intl.NumberFormat(locale).formatToParts(1234.5);
  const decimalSeparator =
    parts.find((part) => part.type === "decimal")?.value ?? ",";
  const groupSeparator =
    parts.find((part) => part.type === "group")?.value ?? ".";
  let decimalAt = -1;
  const commaAt = input.lastIndexOf(",");
  const dotAt = input.lastIndexOf(".");

  if (commaAt >= 0 && dotAt >= 0) decimalAt = Math.max(commaAt, dotAt);
  else {
    const separator = commaAt >= 0 ? "," : dotAt >= 0 ? "." : "";
    if (separator === decimalSeparator)
      decimalAt = input.lastIndexOf(separator);
    else if (separator && separator !== groupSeparator)
      decimalAt = input.lastIndexOf(separator);
    else if (separator) {
      const trailingDigits = input.length - input.lastIndexOf(separator) - 1;
      if (trailingDigits > 0 && trailingDigits < 3)
        decimalAt = input.lastIndexOf(separator);
    }
  }

  const integerPart = decimalAt >= 0 ? input.slice(0, decimalAt) : input;
  const fractionPart = decimalAt >= 0 ? input.slice(decimalAt + 1) : "";
  if (
    !integerPart ||
    !/^\d+(?:[.,]\d{3})*$/.test(integerPart) ||
    fractionPart.length > 2 ||
    (decimalAt >= 0 && !/^\d{1,2}$/.test(fractionPart))
  )
    return null;

  const integerDigits = integerPart.replace(/[.,]/g, "");
  const cents = BigInt(`${integerDigits}${fractionPart.padEnd(2, "0")}`);
  if (integerDigits.length > 16 || cents <= 0n) return null;
  return `${integerDigits}.${fractionPart.padEnd(2, "0")}`;
}

export function rentalDaysBetween(startDate: string, endDate: string): number {
  const start = Date.parse(`${startDate}T00:00:00Z`);
  const end = Date.parse(`${endDate}T00:00:00Z`);
  return Number.isFinite(start) && Number.isFinite(end) && end > start
    ? (end - start) / 86_400_000
    : 0;
}

export function calculateRentalTotal(dailyPrice: number, days: number): number {
  if (!Number.isFinite(dailyPrice) || !Number.isInteger(days) || days <= 0)
    return 0;
  const dailyCents = Math.round(dailyPrice * 100);
  return (dailyCents * days) / 100;
}

export function formatMoney(
  amount: number,
  currency: string,
  locale = "tr-TR",
): string {
  try {
    return new Intl.NumberFormat(locale, {
      style: "currency",
      currency,
      minimumFractionDigits: 2,
      maximumFractionDigits: 2,
    }).format(amount);
  } catch {
    return `${new Intl.NumberFormat(locale, { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(amount)} ${currency}`;
  }
}
