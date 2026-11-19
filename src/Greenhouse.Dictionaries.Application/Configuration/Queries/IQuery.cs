using MediatR;

namespace Greenhouse.Dictionaries.Application.Configuration.Queries;

public interface IQuery<out TResult> : IRequest<TResult>
{
}
