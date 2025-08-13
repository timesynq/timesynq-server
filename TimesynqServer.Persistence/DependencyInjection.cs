using Microsoft.Extensions.DependencyInjection;
using TimesynqServer.Persistence.Repository.FollowRepository;
using TimesynqServer.Persistence.Repository.UserRepository;

namespace TimesynqServer.Persistence
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPersistenceServices(this IServiceCollection services)
        {
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IFollowRepository, FollowRepository>();
            return services;
        }
    }
}
