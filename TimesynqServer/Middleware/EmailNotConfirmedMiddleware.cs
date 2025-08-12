using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Security.Claims;
using TimesynqServer.Application.Service.UserService;

namespace TimesynqServer.Middleware
{
    public class EmailNotConfirmedMiddleware
    {

        private readonly RequestDelegate _next;

        public EmailNotConfirmedMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IUserService userService, ProblemDetailsFactory problemDetailsFactory)
        {

            ClaimsPrincipal user = context.User;

            if (user?.Identity?.IsAuthenticated == false)
            {
                await _next(context);
                return;
            }

            //we can use the null-forgiving operator as long as LogAuthorizedUserIdMiddleware is before EmailNotConfirmedMiddleware in the pipeline
            string callerId = context.User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;

            bool isEmailConfirmed = await userService.IsUserConfirmed(Guid.Parse(callerId));
            if(isEmailConfirmed)
            {
                await _next(context);
                return;
            }

            Endpoint? endpoint = context.GetEndpoint();
            IReadOnlyList<AuthorizeAttribute>? authorizeAttributes = endpoint?.Metadata.GetOrderedMetadata<AuthorizeAttribute>();

            if (authorizeAttributes == null)
            {
                await _next(context);
                return;
            }

            bool requiresConfirmedRole = authorizeAttributes
                .Where(a => !string.IsNullOrEmpty(a.Roles))
                .SelectMany(a => a.Roles!.Split(','))
                .Select(r => r.Trim())
                .Contains("ConfirmedUser", StringComparer.OrdinalIgnoreCase);

            if (!requiresConfirmedRole)
            {
                await _next(context);
                return;
            }

            var problemDetails = problemDetailsFactory.CreateProblemDetails(
                    httpContext: context,
                    statusCode: StatusCodes.Status403Forbidden,
                    title: "Forbidden.",
                    type: "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                    detail: "Confirm your email to access this."
                );

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = StatusCodes.Status403Forbidden;

            await context.Response.WriteAsJsonAsync(problemDetails);
        }

    }
}
