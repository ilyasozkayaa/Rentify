using AutoMapper;
using RentifyApi.Models;
using RentifyApplication.Command.CreateRentalListing;
using RentifyApplication.Command.CreateRent;

namespace RentifyApi.Mappers;

public sealed class CreateRentalListingMappingProfile : Profile
{
    public CreateRentalListingMappingProfile()
    {
        CreateMap<CreateRentalListingRequest, CreateRentalListingCommand>()
            .ForCtorParam(nameof(CreateRentalListingCommand.OwnerUserId), options => options.MapFrom(_ => 0));
        CreateMap<CreateRentRequest, CreateRentCommand>()
            .ForCtorParam(nameof(CreateRentCommand.RenterUserId), options => options.MapFrom(_ => 0));
    }
}
