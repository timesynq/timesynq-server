using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace TimesynqServer.Middleware
{
    public class LogAuthorizedUserIdMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LogAuthorizedUserIdMiddleware> _logger;

        public LogAuthorizedUserIdMiddleware(RequestDelegate next, ILogger<LogAuthorizedUserIdMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            Endpoint? endpoint = context.GetEndpoint();
            if (endpoint != null) 
            {
                IAuthorizeData? authorizeData = endpoint.Metadata.GetMetadata<IAuthorizeData>();
                if (authorizeData != null)
                {
                    string? callerId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (callerId.IsNullOrEmpty())
                    {
                        _logger.LogWarning("User with missing name identifier called endpoint {Endpoint}", endpoint.DisplayName);
                    }
                    else
                    {
                        _logger.LogInformation("User {UserId} called endpoint {Endpoint}", callerId, endpoint.DisplayName);
                    }
                }
            }

            await _next(context);
        }
    }
}
