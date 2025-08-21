using Microsoft.Extensions.DependencyInjection;
using TimesynqServer.Domain.Entities.Follows;
using TimesynqServer.Domain.Entities.Users;
using TimesynqServer.Persistence.Repository;
using TimesynqServer.Persistence.UnitOfWork;

namespace TimesynqServer.Persistence
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPersistenceServices(this IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork, EfUnitOfWork>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IFollowRepository, FollowRepository>();
            return services;
        }
    }
}
