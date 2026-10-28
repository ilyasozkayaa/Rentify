using System.Net;
using MediatR;
using RentifyApplication.Exceptions;
using RentifyApplication.Exceptions.Enums;
using RentifyApplication.IRepositories;
using RentifyDomain.Enum;

namespace RentifyApplication.Command.RejectRentalListing;

public sealed class RejectRentalListingCommandHandler : IRequestHandler<RejectRentalListingCommand, RejectRentalListingResponse>
{
    private readonly IRentableProductRepository _repository;

    public RejectRentalListingCommandHandler(IRentableProductRepository repository) => _repository = repository;

    public async Task<RejectRentalListingResponse> Handle(RejectRentalListingCommand command, CancellationToken cancellationToken)
    {
        var listing = await _repository.GetByIdForUpdateAsync(command.RentalProductId, cancellationToken);
        if (listing is null || listing.Status != (int)RentableProductStatus.Pending)
            throw new BusinessException("The selected listing does not exist or is no longer pending.", BusinessErrorCode.RentalListingsNotPending, (int)HttpStatusCode.Conflict);

        listing.Status = (int)RentableProductStatus.Rejected;
        listing.RejectionReason = command.Reason.Trim();
        _repository.Update(listing);

        return new RejectRentalListingResponse(listing.Id, listing.Title, listing.RejectionReason);
    }
}
