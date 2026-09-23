using System.Text.Json;
using MediatR;
using RentifyApplication.IRepositories;
using RentifyDomain.Entities;
using RentifyDomain.Enum;

namespace RentifyApplication.Command.CreateRentalListing;

public sealed class CreateRentalListingCommandHandler : IRequestHandler<CreateRentalListingCommand, CreateRentalListingResponse>
{
    private readonly IRentableProductRepository _rentableProductRepository;

    public CreateRentalListingCommandHandler(IRentableProductRepository rentableProductRepository)
    {
        _rentableProductRepository = rentableProductRepository;
    }

    public async Task<CreateRentalListingResponse> Handle(CreateRentalListingCommand command, CancellationToken cancellationToken)
    {
        var rentableProduct = new RentableProduct
        {
            OwnerUserId = command.OwnerUserId,
            RentalType = command.RentalType,
            CityCode = command.CityCode,
            District = string.IsNullOrWhiteSpace(command.District) ? null : command.District.Trim(),
            Title = command.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(command.Description) ? null : command.Description.Trim(),
            Price = command.Price,
            Currency = Enum.Parse<Currency>(command.Currency, true).ToString(),
            Attributes = JsonDocument.Parse(command.Attributes.GetRawText()),
            Status = (int)RentableProductStatus.Pending
        };

        await _rentableProductRepository.AddAsync(rentableProduct, cancellationToken);

        return new CreateRentalListingResponse(rentableProduct.Title, (RentableProductStatus)rentableProduct.Status, rentableProduct.CreatedAt);
    }
}
