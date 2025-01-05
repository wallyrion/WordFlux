using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using WordFlux.Domain.Exceptions;

namespace WordFlux.ApiService.Endpoints;

public static class ErrorHandlerEndpoint
{
    public static WebApplication MapGlobalErrorHandling(this WebApplication app)
    {
        app.UseExceptionHandler("/error");
        
        app.Map("/error", (HttpContext context, ILogger<Program> logger) => {
            Console.WriteLine("Error");
    
            var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

            if (exception is null)
            {
                return Results.Problem();
            }

            if (exception is ValidationException validationException)
            {
                var errors = validationException.Errors.Select(x => new KeyValuePair<string, string[]> (x.PropertyName, [x.ErrorMessage]));

                return Results.ValidationProblem(errors: errors);
            }
            
            if (exception is DomainValidationException domainValidationException)
            {
                return Results.ValidationProblem(errors: new List<KeyValuePair<string, string[]>>
                {
                    new(domainValidationException.PropertyName ?? "", [domainValidationException.Message])
                });
            }
            
            logger.LogError(exception, "Exception occurred: {Message}", exception.Message);
            
            return Results.Problem();
        });
        
        return app;
    }
}