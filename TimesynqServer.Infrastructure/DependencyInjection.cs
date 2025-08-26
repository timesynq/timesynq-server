using Amazon.SimpleEmail;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.OpenTelemetry;
using TimesynqServer.Application.Service;
using TimesynqServer.Common;
using TimesynqServer.Domain.Entities;
using TimesynqServer.Domain.Entities.Users;
using TimesynqServer.Infrastructure.Email;
using TimesynqServer.Infrastructure.Extensions;
using TimesynqServer.Infrastructure.Logging;
using TimesynqServer.Infrastructure.Service.FollowService;
using TimesynqServer.Infrastructure.Service.UserService;
using TimesynqServer.Persistence;

namespace TimesynqServer.Infrastructure
{
    public static class DependencyInjection
    {
        public static IHostApplicationBuilder AddInfrastructure(this IHostApplicationBuilder builder)
        {
            builder.Services.AddInfrastructureServices(builder.Configuration);
            builder.AddRedisClient("Redis");
            return builder;
        }

        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDatabase(configuration);
            services.AddAuth();

            services.AddTransient<IEmailSender<TimesynqUser>, EmailSender<TimesynqUser>>();

            services.AddDefaultAWSOptions(configuration.GetAWSOptions());
            services.AddAWSService<IAmazonSimpleEmailService>();
            services.Configure<EmailSenderOptions>(configuration.GetSection(EmailSenderOptions.ConfigurationSection));

            services.AddSignalR();

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IFollowService, FollowService>();

            return services;
        }

        public static IServiceCollection AddLogging(this IServiceCollection services, IConfiguration configuration)
        {
            SerilogOptions serilogOptions = configuration.GetSection(SerilogOptions.ConfigurationSection).Get<SerilogOptions>()!;
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.OpenTelemetry(x =>
                {
                    x.Endpoint = serilogOptions.Endpoint;
                    x.Protocol = OtlpProtocol.HttpProtobuf;
                    x.Headers = new Dictionary<string, string>
                    {
                        ["X-Seq-ApiKey"] = serilogOptions.ApiKey,
                    };
                    x.ResourceAttributes = new Dictionary<string, object>
                    {
                        ["service.name"] = serilogOptions.ServiceName,
                    };
                })
                .CreateLogger();

            services.AddSerilog();
            return services;
        }

        public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            string? dbConnectionString = configuration.GetConnectionString("timesynq-db");
            services.AddDbContext<TimesynqDbContext>(options => options.UseSqlServer(dbConnectionString, b => b.MigrationsAssembly("TimesynqServer.Persistence")));
            return services;
        }

        public static IServiceCollection AddAuth(this IServiceCollection services)
        {
            services.AddAuthorization();
            services.AddAuthentication().AddCookie(IdentityConstants.ApplicationScheme);

            services.AddIdentityCore<TimesynqUser>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = UserConstants.MinPasswordLength;

                options.SignIn.RequireConfirmedAccount = false;
            })
                .AddRoles<TimesynqRole>()
                .AddEntityFrameworkStores<TimesynqDbContext>()
                .AddUserStore<UserStore<TimesynqUser, TimesynqRole, TimesynqDbContext, Guid>>()
                .AddErrorDescriber<CustomIdentityErrorDescriber>()
                .AddApiEndpoints();

            return services;
        }

        public static void AddIdentityEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapTimesynqIdentityApi<TimesynqUser>();
        }

    }
}
