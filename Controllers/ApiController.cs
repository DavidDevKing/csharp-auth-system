using System.Dynamic;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
namespace csharp_auth_system.Controllers;

public abstract class ApiController : ControllerBase
{
    protected IActionResult HandleResult<T>(ServiceResult<T> result)
    {
        if (result == null) return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Service returned an empty result" });
        if (!result.IsSuccess) return ProcessError(result.StatusCode, result.ErrorMessage);
        if (result.Warnings != null && result.Warnings.Any())
        {
            string? jsonString = JsonSerializer.Serialize(result.Data);
            ExpandoObject? flatObject = JsonSerializer.Deserialize<ExpandoObject>(jsonString);
            IDictionary<string, object>? dict = flatObject;
            dict["warnings"] = result.Warnings;
            return StatusCode(result.StatusCode, new { data = dict });
        }
        return StatusCode(result.StatusCode, result.Data);
    }


    protected IActionResult HandleResult(ServiceResult result)
    {
        if (result == null) return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Service returned an empty result" });
        if (!result.IsSuccess) return ProcessError(result.StatusCode, result.ErrorMessage);
        if (result.Warnings != null) return StatusCode(result.StatusCode, new { warnings = result.Warnings });
        return NoContent();
    }


    private IActionResult ProcessError(int statusCode, string? errorMessage)
    {
        var errorResponse = new
        {
            error = errorMessage ?? "An unexpected error occured while processing your request",
            statusCode = statusCode,
        };
        return StatusCode(statusCode, errorResponse);
    }
}