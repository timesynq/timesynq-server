using Amazon.SimpleEmail;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.OpenTelemetry;
using TimesynqServer.Domain.Entities;
using TimesynqServer.Infrastructure.Email;
using TimesynqServer.Infrastructure.Extensions;
using TimesynqServer.Infrastructure.Logging;
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

            services.AddAuthorization();
            services.AddAuthentication().AddCookie(IdentityConstants.ApplicationScheme);

            string? dbConnectionString = configuration.GetConnectionString("SqlServerDatabase");

            services.AddDbContext<TimesynqDbContext>(options => options.UseSqlServer(dbConnectionString));

            services.AddIdentityCore<TimesynqUser>(options =>
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

            services.AddTransient<IEmailSender<TimesynqUser>, EmailSender<TimesynqUser>>();

            services.AddDefaultAWSOptions(configuration.GetAWSOptions());
            services.AddAWSService<IAmazonSimpleEmailService>();
            services.Configure<EmailSenderOptions>(configuration.GetSection(EmailSenderOptions.ConfigurationSection));

            services.AddSignalR();

            return services;
        }

        public static void AddIdentityEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapTimesynqIdentityApi<TimesynqUser>();
        }

    }
}
