using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using StackExchange.Redis;
using TimesynqServer.Domain.Entities.Users;
using TimesynqServer.Extensions;
using TimesynqServer.Infrastructure;
using TimesynqServer.Middleware;
using TimesynqServer.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();

builder.AddServiceDefaults();

builder.Services.AddPersistenceServices();
builder.AddInfrastructure();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var problemDetailsFactory = context.HttpContext.RequestServices
            .GetRequiredService<ProblemDetailsFactory>();

        var problemDetails = problemDetailsFactory.CreateValidationProblemDetails(
            context.HttpContext,
            context.ModelState,
            statusCode: StatusCodes.Status400BadRequest,
            title: "One or more validation errors occurred.");

        return new BadRequestObjectResult(problemDetails)
        {
            ContentTypes = { "application/problem+json" }
        };
    };
});

//todo: add ProdCorsPolicy
string DevCorsPolicy = "_DevCorsPolicy";
string? clientUrl = builder.Configuration["Client:Url"] ?? throw new NullReferenceException("Client URL not specified.");
builder.Services.AddCors(p => p.AddPolicy(DevCorsPolicy, builder =>
{
    builder.WithOrigins(clientUrl).AllowAnyMethod().AllowAnyHeader().AllowCredentials();
}));

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseCors(DevCorsPolicy);

    app.ApplyMigrations();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseMiddleware<ExceptionsMiddleware>();
app.UseMiddleware<LogAuthorizedUserIdMiddleware>();
app.UseMiddleware<EmailNotConfirmedMiddleware>();

app.UseAuthorization();

app.MapControllers();
app.AddIdentityEndpoints();

app.AddHubs();

app.MapPost("refresh-cookie", async (HttpContext httpContext, SignInManager<TimesynqUser> signInManager, UserManager<TimesynqUser> userManager, [FromQuery] bool useSessionCookies = true) =>
{
    var user = await userManager.GetUserAsync(httpContext.User);
    if(user == null)
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

app.Run();
