using System.Net;
using System.Text.Json;
using TimesynqServer.Models.DTO;

namespace TimesynqServer.Middleware
{
    public class ExceptionsMiddleware
    {

        private readonly RequestDelegate _next;

        public ExceptionsMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex) 
            {
                //todo: log exception

                var response = new ResponseDTO<object>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Errors = ["An unexpected error occurred."],
                    Result = null,
                };

                string serializedResponse = JsonSerializer.Serialize(response);

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                await context.Response.WriteAsync(serializedResponse);
            }
        }

    }
}
