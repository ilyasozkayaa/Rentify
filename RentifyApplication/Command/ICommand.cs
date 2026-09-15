using MediatR;

namespace RentifyApplication.Command;

public interface ICommand<out TResponse> : IRequest<TResponse>
{
}