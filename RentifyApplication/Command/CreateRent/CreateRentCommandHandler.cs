using MediatR;
using RentifyApplication.Exceptions;
using RentifyApplication.Exceptions.Enums;
using RentifyApplication.Constants;
using RentifyApplication.IRepositories;
using RentifyApplication.IServices;
using RentifyDomain.Entities;
using RentifyDomain.Enum;
using System.Net;

namespace RentifyApplication.Command.CreateRent;

public sealed class CreateRentCommandHandler : IRequestHandler<CreateRentCommand, CreateRentResponse>
{
    private readonly IRentableProductRepository _rentableProductRepository;
    private readonly IRentRepository _rentRepository;
    private readonly IRedisCacheService _redisCacheService;

    public CreateRentCommandHandler(IRentableProductRepository rentableProductRepository, IRentRepository rentRepository, IRedisCacheService redisCacheService)
    {
        _rentableProductRepository = rentableProductRepository;
        _rentRepository = rentRepository;
        _redisCacheService = redisCacheService;
    }

    public async Task<CreateRentResponse> Handle(CreateRentCommand command, CancellationToken cancellationToken)
    {
        var cacheKey = $"{RedisCacheKeyPrefixes.RentRequest}:{command.RentableProductId}:{command.StartDate:yyyyMMdd}:{command.EndDate:yyyyMMdd}";

        if (await _redisCacheService.ExistsAsync(cacheKey, cancellationToken))
        {
            throw new BusinessException("This rental request is already being processed. Please try again.", BusinessErrorCode.RentalBookingInProgress, (int)HttpStatusCode.Conflict);
        }

        await _redisCacheService.SetAsync(cacheKey, "processing", cancellationToken);

        try
        {
            return await CreateRentAsync(command, cancellationToken);
        }
        finally
        {
            await _redisCacheService.RemoveAsync(cacheKey, CancellationToken.None);
        }
    }

    private async Task<CreateRentResponse> CreateRentAsync(CreateRentCommand command, CancellationToken cancellationToken)
    {
        var rentableProduct = await _rentableProductRepository.GetByIdForUpdateAsync(command.RentableProductId, cancellationToken);
        if (rentableProduct is null || rentableProduct.Status != (int)RentableProductStatus.Active)
        {
            throw new BusinessException("The selected rental listing is unavailable.", BusinessErrorCode.RentableProductUnavailable, (int)HttpStatusCode.Conflict);
        }

        var hasOverlap = await _rentRepository.HasConfirmedOverlapAsync(rentableProduct.Id, command.StartDate, command.EndDate, cancellationToken);

        if (hasOverlap)
        {
            throw new BusinessException("The selected rental dates are unavailable.", BusinessErrorCode.RentalDatesUnavailable, (int)HttpStatusCode.Conflict);
        }

        var rentalDays = command.EndDate.DayNumber - command.StartDate.DayNumber;
        var totalPrice = rentableProduct.Price * rentalDays;

        var rent = new Rent
        {
            RenterUserId = command.RenterUserId,
            RentableProductId = rentableProduct.Id,
            StartDate = command.StartDate,
            EndDate = command.EndDate,
            TotalPrice = totalPrice,
            Status = (int)RentStatus.Confirmed
        };

        await _rentRepository.AddAsync(rent, cancellationToken);

        return new CreateRentResponse(
            rent.RentableProductId,
            rent.StartDate,
            rent.EndDate,
            rent.TotalPrice,
            rentableProduct.Currency,
            RentStatus.Confirmed);
    }
}