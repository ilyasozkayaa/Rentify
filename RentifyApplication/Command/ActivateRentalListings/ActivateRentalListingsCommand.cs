using RentifyApplication.Command;

namespace RentifyApplication.Command.ActivateRentalListings;

public sealed record ActivateRentalListingsCommand(IReadOnlyCollection<int> RentalProductIds) : ICommand<ActivateRentalListingsResponse>;
