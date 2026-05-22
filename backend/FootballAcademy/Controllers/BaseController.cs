using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FootballAcademy.Controllers
{
    [Route("api/[controller]")]
    public class BaseController : ControllerBase
    {
        protected long? UserId => long.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : null;
    }
}
