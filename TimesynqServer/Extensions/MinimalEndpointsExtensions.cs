using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using TimesynqServer.Domain.Entities.Users;

namespace TimesynqServer.Extensions
{
    public static class MinimalEndpointsExtensions
    {
        public static IEndpointRouteBuilder MapMinimalApiEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("refresh-cookie", async (HttpContext httpContext, SignInManager<TimesynqUser> signInManager, UserManager<TimesynqUser> userManager, [FromQuery] bool useSessionCookies = true) =>
            {
                var user = await userManager.GetUserAsync(httpContext.User);
                if (user == null)
                {
                    return Results.Unauthorized();
                }

                await signInManager.SignInAsync(user, isPersistent: !useSessionCookies);

                return Results.NoContent();
            }).RequireAuthorization();

            app.MapPost("logout", async (SignInManager<TimesynqUser> signInManager) =>
            {
                await signInManager.SignOutAsync().ConfigureAwait(false);
            });

            app.MapGet("ping", (ILogger<Program> logger) =>
            {
                logger.LogInformation("pong");
                return "pong";
            });

            app.MapPost("redis", async (IConnectionMultiplexer redis) =>
            {
                IDatabase db = redis.GetDatabase();
                await db.StringSetAsync("testkey", "testvalue");
                return "success";
            });

            return app;
        }
    }
}
