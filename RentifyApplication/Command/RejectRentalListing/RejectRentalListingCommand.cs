using RentifyApplication.Command;

namespace RentifyApplication.Command.RejectRentalListing;

public sealed record RejectRentalListingCommand(int RentalProductId, string Reason) : ICommand<RejectRentalListingResponse>;
public sealed record RejectRentalListingResponse(int RentalProductId, string Title, string Reason);
