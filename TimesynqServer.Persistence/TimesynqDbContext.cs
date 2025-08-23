using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TimesynqServer.Domain.Entities;
using TimesynqServer.Domain.Entities.Follows;
using TimesynqServer.Domain.Entities.Users;

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

            //i'm putting this here because ef keeps renaming CreatedOnUTC and ProfilePicture instead of adding new columns
            builder.Entity<TimesynqUser>()
                .Property(u => u.SavedUserName)
                .HasColumnName("SavedUserName");
            builder.Entity<TimesynqUser>()
                .Property(u => u.NormalizedSavedUserName)
                .HasColumnName("NormalizedSavedUserName");
            builder.Entity<TimesynqUser>()
                .Property(u => u.ProfilePicture)
                .HasColumnName("ProfilePicture");
            builder.Entity<TimesynqUser>()
                .Property(u => u.CreatedOnUTC)
                .HasColumnName("CreatedOnUTC");
            builder.Entity<TimesynqUser>()
                .Property(u => u.LastUpdatedOnUTC)
                .HasColumnName("LastUpdatedOnUTC");
            builder.Entity<TimesynqUser>()
                .Property(u => u.LastUpdatedUserNameUTC)
                .HasColumnName("LastUpdatedUserNameUTC");
            builder.Entity<TimesynqUser>()
                .Property(u => u.DeletedOnUTC)
                .HasColumnName("DeletedOnUTC");

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

            builder.Entity<TimesynqUser>()
                .HasQueryFilter(b => b.DeletedOnUTC == null);
            builder.Entity<Follow>()
                .HasQueryFilter(b => b.Follower!.DeletedOnUTC == null && b.Followee!.DeletedOnUTC == null);
        }

    }
}
