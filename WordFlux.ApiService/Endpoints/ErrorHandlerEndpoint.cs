using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;

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
                return Results.Problem(statusCode: 400, type: "ValidationFailure", title: "Validation error", detail: "One or more validation errors has occured", extensions: [
                    new KeyValuePair<string, object?>("errors", validationException.Errors)
                ]);
            }
            
            logger.LogError(exception, "Exception occurred: {Message}", exception.Message);
            
            return Results.Problem();
        });
        
        return app;
    }
}