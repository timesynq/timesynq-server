using Microsoft.AspNetCore.Identity;

namespace TimesynqServer.Database.Entities
{
    public class TimesynqUser : IdentityUser<Guid>
    {
        public string IdString => Id.ToString();
        public string DisplayName { get; set; } = string.Empty;
        public string ProfilePictureFilename { get; set; } = string.Empty;
        public DateTime CreatedOnUTC { get; set; }
        public DateTime LastUpdatedOnUTC { get; set; }
    }
}
