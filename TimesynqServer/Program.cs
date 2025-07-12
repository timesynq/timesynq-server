using Amazon.SimpleEmail;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TimesynqServer.Database;
using TimesynqServer.Database.Entities;
using TimesynqServer.Extensions;
using TimesynqServer.Hubs.TrackerHub;
using TimesynqServer.Services.Email;
using TimesynqServer.Services.Repository.FollowRepository;
using TimesynqServer.Services.Repository.UserRepository;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthorization();
builder.Services.AddAuthentication().AddCookie(IdentityConstants.ApplicationScheme);

string? dbConnectionString = builder.Configuration.GetConnectionString("SqlServerDatabase");

builder.Services.AddDbContext<TimesynqDbContext>(options => options.UseSqlServer(dbConnectionString));

builder.Services.AddIdentityCore<TimesynqUser>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 12;

    options.SignIn.RequireConfirmedAccount = false;
})
    .AddRoles<TimesynqRole>()
    .AddEntityFrameworkStores<TimesynqDbContext>()
    .AddUserStore<UserStore<TimesynqUser, TimesynqRole, TimesynqDbContext, Guid>>()
    .AddErrorDescriber<CustomIdentityErrorDescriber>()
    .AddApiEndpoints();

builder.Services.AddTransient<IEmailSender<TimesynqUser>, EmailSender<TimesynqUser>>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IFollowRepository, FollowRepository>();

builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
builder.Services.AddAWSService<IAmazonSimpleEmailService>();
builder.Services.Configure<EmailSenderOptions>(builder.Configuration.GetSection(EmailSenderOptions.ConfigurationSection));

builder.Services.AddSignalR();

builder.AddRedisClient("Redis");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.ApplyMigrations();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapTimesynqIdentityApi<TimesynqUser>();

app.MapHub<TrackerHub>("hub");

app.MapGet("ping", () =>
{
    return "pong";
});

app.Run();
