import { describe, expect, it } from "vitest";
import {
  calculateRentalTotal,
  formatMoney,
  parsePriceInput,
  rentalDaysBetween,
} from "./money";

describe("localized price input", () => {
  it("keeps 10,000 Turkish lira exact", () => {
    expect(parsePriceInput("10.000,00", "tr-TR")).toBe("10000.00");
  });

  it("accepts decimal input in English locale", () => {
    expect(parsePriceInput("10,000.00", "en-US")).toBe("10000.00");
  });

  it("does not accept more than two fractional digits", () => {
    expect(parsePriceInput("10.000,001", "tr-TR")).toBeNull();
  });

  it("formats prices with two currency decimals", () => {
    expect(formatMoney(10000, "TRY")).toBe("₺10.000,00");
  });
});

describe("rental totals", () => {
  it("uses checkout minus check-in days and cent based totals", () => {
    const days = rentalDaysBetween("2026-11-01", "2026-11-04");
    expect(days).toBe(3);
    expect(calculateRentalTotal(10000, days)).toBe(30000);
  });

  it("calculates decimal daily prices to cents", () => {
    expect(calculateRentalTotal(99.99, 3)).toBe(299.97);
  });
});
