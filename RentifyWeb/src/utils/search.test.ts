import { describe, expect, it } from "vitest";
import { validateSearchQuery } from "./search";

describe("validateSearchQuery", () => {
  it("rejects an empty query", () =>
    expect(validateSearchQuery("  ")).toBe(
      "Tell us what you are looking for.",
    ));
  it("requires enough words and detail", () =>
    expect(validateSearchQuery("car in")).toContain("at least three words"));
  it("accepts a useful natural language query", () =>
    expect(
      validateSearchQuery("A sunny villa in Antalya for two weeks"),
    ).toBeNull());
  it("limits queries to 500 characters", () =>
    expect(
      validateSearchQuery(`villa in Antalya ${"x".repeat(500)}`),
    ).toContain("500 characters"));
});
