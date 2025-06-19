using Microsoft.EntityFrameworkCore;
using TimesynqServer.Database;

namespace TimesynqServer.Extensions
{
    public static class MigrationExtensions
    {
        public static void ApplyMigrations(this IApplicationBuilder app)
        {
            using IServiceScope scope = app.ApplicationServices.CreateScope();
            using TimesynqDbContext dbContext = scope.ServiceProvider.GetRequiredService<TimesynqDbContext>();
            dbContext.Database.Migrate();

        }
    }
}
