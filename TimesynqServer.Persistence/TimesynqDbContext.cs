using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TimesynqServer.Domain.Entities;

namespace TimesynqServer.Persistence
{
    public class TimesynqDbContext : IdentityDbContext<TimesynqUser, TimesynqRole, Guid>
    {
        public TimesynqDbContext(DbContextOptions<TimesynqDbContext> options) : base(options) { }

        public DbSet<Follow> Follows { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            var roles = new List<TimesynqRole>
            {
                new TimesynqRole
                {
                    Id = new Guid("fb24feb3-a13a-4170-bbc0-c5469566d184"),
                    Name = "UnconfirmedUser",
                    NormalizedName = "UNCONFIRMEDUSER",
                },
                new TimesynqRole
                {
                    Id = new Guid("a10f83cc-2836-487a-96d2-22579c443511"),
                    Name = "ConfirmedUser",
                    NormalizedName = "CONFIRMEDUSER",
                },
                new TimesynqRole
                {
                    Id = new Guid("9f743e92-c55a-4bc2-a238-014b015a01a7"),
                    Name = "Admin",
                    NormalizedName = "ADMIN",
                },
            };
            builder.Entity<TimesynqRole>().HasData(roles);

            builder.Entity<Follow>().HasKey(f => new { f.FollowerId, f.FolloweeId });
            builder.Entity<Follow>()
                .HasOne(f => f.Follower)
                .WithMany(u => u.Following)
                .HasForeignKey(f => f.FollowerId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Follow>()
                .HasOne(f => f.Followee)
                .WithMany(u => u.Followers)
                .HasForeignKey(f => f.FolloweeId)
                .OnDelete(DeleteBehavior.Restrict);

        }

    }
}
