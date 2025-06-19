using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TimesynqServer.Database.Entities;

namespace TimesynqServer.Database
{
    public class TimesynqDbContext : IdentityDbContext<TimesynqUser, TimesynqRole, Guid>
    {
        public TimesynqDbContext(DbContextOptions<TimesynqDbContext> options) : base(options) { }
    }
}
