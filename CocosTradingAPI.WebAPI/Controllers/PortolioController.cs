using Microsoft.AspNetCore.Mvc;

namespace CocosTradingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PortfolioController : ControllerBase
    {
        // GET: api/portfolio
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Hello World");
        }
    }
}
