using Microsoft.Extensions.DependencyInjection;
using TimesynqServer.Application.Service.FollowService;
using TimesynqServer.Application.Service.UserService;

namespace TimesynqServer.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IFollowService, FollowService>();
            return services;
        }
    }
}
