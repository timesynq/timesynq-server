using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

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

        public async Task InvokeAsync(HttpContext context, ProblemDetailsFactory problemDetailsFactory)
        {

            async void Respond(ProblemDetails problemDetails)
            {
                context.Response.ContentType = "application/problem+json";
                context.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;

                await context.Response.WriteAsJsonAsync(problemDetails);
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

                var problemDetails = problemDetailsFactory.CreateProblemDetails(
                    httpContext: context,
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Bad Request",
                    type: "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    detail: "The request body is incorrectly formed"
                );

                Respond(problemDetails);

            }
            catch (InvalidOperationException invalidOperationException)
            {
                //this exception is thrown in the /refresh identity endpoint
                //catching it here lets me return 401 instead of 500

                _logger.LogWarning("InvalidOperationException caught: {Exception}", invalidOperationException.ToString());

                var problemDetails = problemDetailsFactory.CreateProblemDetails(
                    httpContext: context,
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Invalid Operation",
                    type: "https://datatracker.ietf.org/doc/html/rfc7235#section-3.1",
                    detail: "Invalid operation."
                );

                Respond(problemDetails);

            }
            catch (Exception ex)
            {
                _logger.LogError("Exception caught: {Exception}", ex.ToString());

                var problemDetails = problemDetailsFactory.CreateProblemDetails(
                    httpContext: context,
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Internal Server Error",
                    type: "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                    detail: "An unexpected error occurred."
                );

                Respond(problemDetails);
            }
        }

    }
}
