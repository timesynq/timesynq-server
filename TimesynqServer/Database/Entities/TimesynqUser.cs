using Microsoft.AspNetCore.Identity;

namespace TimesynqServer.Database.Entities
{
    public class TimesynqUser : IdentityUser<Guid>
    {
        public string IdString => Id.ToString();
        public string ProfilePicture { get; set; } = string.Empty;
        public DateTime CreatedOnUTC { get; set; }
        public DateTime LastUpdatedOnUTC { get; set; }

        //Navigation Properties
        public ICollection<Follow> Followers { get; set; } = [];
        public ICollection<Follow> Following { get; set; } = [];


    }
}
