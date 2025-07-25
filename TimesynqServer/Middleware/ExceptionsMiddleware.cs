using Azure;
using System.Net;
using System.Text.Json;
using TimesynqServer.Models.DTO;

namespace TimesynqServer.Middleware
{
    public class ExceptionsMiddleware
    {

        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionsMiddleware> _logger;

        public ExceptionsMiddleware(RequestDelegate next, ILogger<ExceptionsMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {

            async void Respond<T>(ResponseDTO<T> responseDTO)
            {

                string serializedResponse = JsonSerializer.Serialize(responseDTO);

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = responseDTO.StatusCode;

                await context.Response.WriteAsync(serializedResponse);
            }
            ;

            try
            {
                await _next(context);
            }
            catch (BadHttpRequestException badHttpRequestException)
            {
                //this block is for requests thrown by minimal apis, such as the ones used in the identity
                //it's somewhat of a hacky fix because AFAIK throwing BadHttpRequestException is how minimal apis handle malformed request bodies

                _logger.LogWarning("BadHttpRequestException caught: {Exception}", badHttpRequestException.ToString());

                var response = new ResponseDTO<object>
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Errors = ["Request body is incorrectly formed."],
                    Result = null,
                };

                Respond(response);

            }
            catch(InvalidOperationException invalidOperationException)
            {
                //this exception is thrown in the /refresh identity endpoint
                //catching it here lets me return 401 instead of 500

                _logger.LogWarning("InvalidOperationException caught: {Exception}", invalidOperationException.ToString());

                var response = new ResponseDTO<object>
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Errors = ["Invalid operation."],
                    Result = null,
                };

                Respond(response);

            }
            catch (Exception ex) 
            {
                _logger.LogError("Exception caught: {Exception}", ex.ToString());

                var response = new ResponseDTO<object>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Errors = ["An unexpected error occurred."],
                    Result = null,
                };

                Respond(response);
            }
        }

    }
}
