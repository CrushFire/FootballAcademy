using Core.Results;
using Microsoft.AspNetCore.Mvc;

namespace FootballAcademy.Extensions
{
    public static class ResultExtension
    {
            public static IActionResult ToActionResult<T>(this Result<T> result)
            {
                return result.IsSuccess
                    ? new OkObjectResult(new { success = true, data = result.Data })
                    : new ObjectResult(new { success = false, error = result.ErrorMessage })
                    { StatusCode = result.StatusCode };
            }
        }
    }
