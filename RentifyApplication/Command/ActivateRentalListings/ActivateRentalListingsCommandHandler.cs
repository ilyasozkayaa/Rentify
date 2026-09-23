using MediatR;
using System.Net;
using RentifyApplication.Exceptions;
using RentifyApplication.Exceptions.Enums;
using RentifyApplication.IRepositories;
using RentifyDomain.Enum;

namespace RentifyApplication.Command.ActivateRentalListings;

public sealed class ActivateRentalListingsCommandHandler : IRequestHandler<ActivateRentalListingsCommand, ActivateRentalListingsResponse>
{
    private readonly IRentableProductRepository _rentableProductRepository;

    public ActivateRentalListingsCommandHandler(IRentableProductRepository rentableProductRepository)
    {
        _rentableProductRepository = rentableProductRepository;
    }

    public async Task<ActivateRentalListingsResponse> Handle(ActivateRentalListingsCommand command, CancellationToken cancellationToken)
    {
        var pendingListings = await _rentableProductRepository.GetPendingByIdsAsync(command.RentalProductIds, cancellationToken);
        var pendingIds = pendingListings.Select(listing => listing.Id).ToHashSet();

        if (pendingIds.Count != command.RentalProductIds.Count || command.RentalProductIds.Any(id => !pendingIds.Contains(id)))
        {
            throw new BusinessException("One or more selected listings do not exist or are not pending.", BusinessErrorCode.RentalListingsNotPending, (int)HttpStatusCode.Conflict);
        }

        foreach (var listing in pendingListings)
            listing.Status = (int)RentableProductStatus.Active;

        var activatedIds = command.RentalProductIds.ToArray();
        return new ActivateRentalListingsResponse(activatedIds, activatedIds.Length);
    }
}
