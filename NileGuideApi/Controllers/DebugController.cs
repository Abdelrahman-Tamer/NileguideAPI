using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NileGuideApi.Controllers
{
    // TEMPORARY: remove this controller after verifying forwarded client IP handling in Azure.
    [Route("api/debug")]
    [ApiController]
    public class DebugController : ControllerBase
    {
        [AllowAnonymous]
        [HttpGet("ip")]
        public IActionResult GetIpDebugInfo()
        {
            return Ok(new
            {
                remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString(),
                xForwardedFor = HttpContext.Request.Headers["X-Forwarded-For"].ToString(),
                xForwardedProto = HttpContext.Request.Headers["X-Forwarded-Proto"].ToString(),
                host = HttpContext.Request.Host.ToString()
            });
        }
    }
}
