using MediatR;

namespace RentifyApplication.Command;

public interface ICommand<out TResponse> : IRequest<TResponse>
{
}

public interface ITransactionalCommand
{
}
