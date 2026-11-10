using MediatR;

namespace LS.Application.Contracts.Interfaces.Common;

public interface IBackgroundRequestSender
{
    Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken);
}
