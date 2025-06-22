using Microsoft.AspNetCore.Identity;

namespace TimesynqServer.Database.Entities
{
    public class TimesynqRole : IdentityRole<Guid> 
    {
        public TimesynqRole(): base() { }

        public TimesynqRole(string rolename) : base(rolename)
        {
            
        }
    }
}
