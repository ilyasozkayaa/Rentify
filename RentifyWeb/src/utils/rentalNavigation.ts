export type RentalSuccessAction = "complete" | "my-rentals";

export function rentalSuccessDestination(action: RentalSuccessAction): string {
  return action === "complete" ? "/" : "/my-rentals";
}
