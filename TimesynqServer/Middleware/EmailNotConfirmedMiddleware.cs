using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text.Json;
using TimesynqServer.Database.Entities;
using TimesynqServer.Models.DTO;

namespace TimesynqServer.Middleware
{
    public class EmailNotConfirmedMiddleware
    {

        private readonly RequestDelegate _next;

        public EmailNotConfirmedMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, UserManager<TimesynqUser> userManager)
        {

            ClaimsPrincipal user = context.User;

            if(user?.Identity?.IsAuthenticated == false)
            {
                await _next(context);
                return;
            }

            //we can use the null-forgiving operator as long as LogAuthorizedUserIdMiddleware is before EmailNotConfirmedMiddleware in the pipeline
            string callerId = context.User.FindFirst(ClaimTypes.NameIdentifier)!.Value!;

            TimesynqUser? timesynqUser = await userManager.FindByIdAsync(callerId);
            if(timesynqUser?.EmailConfirmed == true)
            {
                await _next(context);
                return;
            }

            Endpoint? endpoint = context.GetEndpoint();
            IReadOnlyList<AuthorizeAttribute>? authorizeAttributes = endpoint?.Metadata.GetOrderedMetadata<AuthorizeAttribute>();

            if(authorizeAttributes == null)
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

            var response = new ResponseDTO<object>
            {
                StatusCode = StatusCodes.Status403Forbidden,
                Errors = ["Confirm your email to access this."],
                Result = null,
            };

            string serializedResponse = JsonSerializer.Serialize(response);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status403Forbidden;

            await context.Response.WriteAsync(serializedResponse);
        }

    }
}
