import { describe, expect, it } from "vitest";
import { rentalSuccessDestination } from "./rentalNavigation";

describe("rental success navigation", () => {
  it("sends Complete to the home page", () => {
    expect(rentalSuccessDestination("complete")).toBe("/");
  });

  it("sends My Rentals to the rentals page, separately from home", () => {
    expect(rentalSuccessDestination("my-rentals")).toBe("/my-rentals");
    expect(rentalSuccessDestination("my-rentals")).not.toBe(
      rentalSuccessDestination("complete"),
    );
  });
});
