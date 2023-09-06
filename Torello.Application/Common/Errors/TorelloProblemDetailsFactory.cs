using System.Diagnostics;
using ErrorOr;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;

namespace Torello.Application.Common.Errors;

public class TorelloProblemDetailsFactory : ProblemDetailsFactory
{
    private readonly ApiBehaviorOptions _apiBehaviorOptions;

    public TorelloProblemDetailsFactory(IOptions<ApiBehaviorOptions> apiBehaviorOptions)
    {
        _apiBehaviorOptions = apiBehaviorOptions?.Value
                              ?? throw new ArgumentNullException(nameof(apiBehaviorOptions));
    }

    public override ProblemDetails CreateProblemDetails(
        HttpContext httpContext,
        int? statusCode = null,
        string? title = null,
        string? type = null,
        string? detail = null,
        string? instance = null
    )
    {
        statusCode ??= 500;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Type = type,
            Detail = detail,
            Instance = instance
        };

        ApplyProblemDetailsDefault(httpContext, problemDetails, statusCode.Value);

        return problemDetails;
    }

    public override ValidationProblemDetails CreateValidationProblemDetails(
        HttpContext httpContext,
        ModelStateDictionary modelStateDictionary,
        int? statusCode = null,
        string? title = null,
        string? type = null,
        string? detail = null,
        string? instance = null
    )
    {
        if (modelStateDictionary is null)
            throw new ArgumentNullException(nameof(modelStateDictionary));

        statusCode ??= 400;

        var problemDetails = new ValidationProblemDetails(modelStateDictionary)
        {
            Status = statusCode,
            Type = type,
            Detail = detail,
            Instance = instance
        };

        problemDetails.Title ??= title;

        ApplyProblemDetailsDefault(httpContext, problemDetails, statusCode.Value);

        return problemDetails;
    }

    private void ApplyProblemDetailsDefault(
        HttpContext httpContext,
        ProblemDetails problemDetails,
        int statusCode
    )
    {
        problemDetails.Status ??= statusCode;

        if (_apiBehaviorOptions.ClientErrorMapping.TryGetValue(statusCode, out var clientErrorData))
        {
            problemDetails.Title ??= clientErrorData.Title;
            problemDetails.Type ??= clientErrorData.Link;
        }

        var traceId = Activity.Current?.Id ?? httpContext?.TraceIdentifier;

        if (traceId is not null)
            problemDetails.Extensions["traceId"] = traceId;

        if (httpContext?.Items["errors"] is List<Error> errors)
            problemDetails.Extensions.Add("errorCodes", errors.Select(e => e.Code));
    }
}