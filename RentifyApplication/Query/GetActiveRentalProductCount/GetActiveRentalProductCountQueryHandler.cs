using MediatR;
using RentifyApplication.IRepositories;

namespace RentifyApplication.Query.GetActiveRentalProductCount;

public sealed class GetActiveRentalProductCountQueryHandler : IRequestHandler<GetActiveRentalProductCountQuery, int>
{
    private readonly IRentableProductRepository _repository;

    public GetActiveRentalProductCountQueryHandler(IRentableProductRepository repository)
    {
        _repository = repository;
    }

    public Task<int> Handle(GetActiveRentalProductCountQuery request, CancellationToken cancellationToken)
    {
        return _repository.CountActiveAsync(cancellationToken);
    }
}
