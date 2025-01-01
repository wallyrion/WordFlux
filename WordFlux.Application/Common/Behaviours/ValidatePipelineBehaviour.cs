using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace WordFlux.Application.Common.Behaviours;

public class ValidationPipelineBehaviour<TRequest, TResponse>(IServiceProvider provider) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var validator = provider.GetService<IValidator<TRequest>>();
        
        if (validator != null)
        {
            await validator.ValidateAndThrowAsync(request, cancellationToken);
        }

        return await next();
    }
}