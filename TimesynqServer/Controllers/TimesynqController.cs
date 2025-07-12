using Microsoft.AspNetCore.Mvc;
using TimesynqServer.Models.DTO;

namespace TimesynqServer.Controllers
{
    public class TimesynqController : ControllerBase
    {
        [NonAction]
        public IActionResult OkResponse<T>(int statusCode, T? result, string? resourceUri = null)
        {
            var responseDTO = new ResponseDTO<T>
            {
                StatusCode = statusCode,
                Errors = null,
                Result = result,
            };

            return statusCode == 201 ?
                Created(resourceUri, responseDTO) :
                Ok(responseDTO);
        }

        [NonAction]
        public IActionResult ErrorResponse(int statusCode, string errorMessage)
        {

            var responseDTO = new ResponseDTO<object>
            {
                StatusCode = statusCode,
                Errors = [errorMessage],
                Result = null,
            };

            return StatusCode(statusCode, responseDTO);
        }

        [NonAction]
        public IActionResult ErrorResponse(int statusCode, ICollection<string> errors)
        {
            var responseDTO = new ResponseDTO<object>
            {
                StatusCode = statusCode,
                Errors = errors,
                Result = null,
            };

            return StatusCode(statusCode, responseDTO);
        }
    }
}
