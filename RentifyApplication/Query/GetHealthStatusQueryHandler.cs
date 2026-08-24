using MediatR;

namespace RentifyApplication.Query;

public sealed class GetHealthStatusQueryHandler : IRequestHandler<GetHealthStatusQuery, string>
{
    public async Task<string> Handle(GetHealthStatusQuery request, CancellationToken cancellationToken)
    {
        return "Rentify API is running.";
    }
}
