using AutoMapper;
using RentifyApi.Models;
using RentifyApplication.Command.CreateRentalListing;

namespace RentifyApi.Mappers;

public sealed class CreateRentalListingMappingProfile : Profile
{
    public CreateRentalListingMappingProfile()
    {
        CreateMap<CreateRentalListingRequest, CreateRentalListingCommand>()
            .ForMember(command => command.OwnerUserId, options => options.Ignore());
    }
}
