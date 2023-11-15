using ErrorOr;
using FluentValidation;
using MediatR;

namespace Torello.Application.Common.Behaviors;

/// <summary>
/// Generic ValidationBehavior that implements IPipelineBehavior for MediatR.
/// </summary>
/// <param name="validator"></param>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public class ValidationBehavior<TRequest, TResponse>(IValidator<TRequest>? validator = null)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IErrorOr
{
    /// <summary>
    /// Handle method required by IPipelineBehavior to perform validation in the pipeline.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="next"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        // If no validator provided, proceed to the next handler.
        if (validator is null)
            return await next();

        // Validate the request.
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        // If valid, proceed to the next handler.
        if (validationResult.IsValid)
            return await next();

        // Validation has failed, so lets create a list of the validation errors and return them.
        var errors = validationResult.Errors
            .ConvertAll(validationFailure => Error.Validation(
                validationFailure.PropertyName,
                validationFailure.ErrorMessage));

        return (dynamic)errors;
    }
}
