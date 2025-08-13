using Microsoft.EntityFrameworkCore;
using TimesynqServer.Persistence;

namespace TimesynqServer.Extensions
{
    public static class MigrationExtensions
    {
        /// <summary>
        /// Applies any pending Entity Framework Core database migrations to ensure the database schema is up to date.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> instance used to access application services.</param>
        public static void ApplyMigrations(this IApplicationBuilder app)
        {
            using IServiceScope scope = app.ApplicationServices.CreateScope();
            using TimesynqDbContext dbContext = scope.ServiceProvider.GetRequiredService<TimesynqDbContext>();
            dbContext.Database.Migrate();
        }
    }
}
