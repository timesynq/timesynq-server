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
            try
            {
                await _next(context);
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

                string serializedResponse = JsonSerializer.Serialize(response);

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                await context.Response.WriteAsync(serializedResponse);
            }
        }

    }
}
