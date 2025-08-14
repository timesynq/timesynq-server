using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace TimesynqServer.Controllers
{
    [ApiController]
    public class AuthorizedController : ControllerBase
    {
        public Guid CallerId => Guid.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException("Caller ID is missing.")
        );
    }
}
