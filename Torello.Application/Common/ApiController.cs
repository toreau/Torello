using ErrorOr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Torello.Domain.Common.Errors;

namespace Torello.Application.Common;

[ApiController]
[Route("")]
[Authorize]
public class ApiController : ControllerBase
{
    protected IActionResult Problem(List<Error> errors)
    {
        if (errors.Count is 0)
            return Problem();

        if (errors.All(error => error.Type == ErrorType.Validation))
            return ValidationProblem(errors);

        HttpContext.Items["errors"] = errors;

        return Problem(errors[0]);
    }

    private ObjectResult Problem(Error error)
    {
        var statusCode = error switch
        {
            { NumericType: CustomErrorTypes.Unauthorized } => StatusCodes.Status401Unauthorized,

            { Type: ErrorType.Validation } => StatusCodes.Status400BadRequest,
            { Type: ErrorType.NotFound } => StatusCodes.Status404NotFound,
            { Type: ErrorType.Conflict } => StatusCodes.Status409Conflict,

            _ => StatusCodes.Status500InternalServerError
        };

        return Problem(statusCode: statusCode, detail: error.Description);
    }

    private IActionResult ValidationProblem(List<Error> errors)
    {
        var modelStateDictionary = new ModelStateDictionary();

        foreach (var error in errors)
            modelStateDictionary.AddModelError(error.Code, error.Description);

        return ValidationProblem(modelStateDictionary);
    }
}