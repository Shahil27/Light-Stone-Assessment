using Microsoft.AspNetCore.Mvc;

namespace Light_Stone_Assessment.Controllers
{
    [ApiController]
    [Route("/health")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() => Ok(new { status = "Healthy" });
    }
}
