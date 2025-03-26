using CocosTradingAPI.Application.DTOs;
using CocosTradingAPI.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CocosTradingAPI.Controllers
{
    [Route("api/portfolio")]
    [ApiController]
    public class PortfolioController : ControllerBase
    {
        private readonly IPortfolioService _portfolioService;

        public PortfolioController(IPortfolioService portfolioService)
        {
            _portfolioService = portfolioService;
        }

        
        [HttpGet("{userId:int}")]
        public async Task<ActionResult<PortfolioDto>> GetPortfolio(int userId)
        {
            if (userId <= 0)
                return BadRequest("Invalid userId.");

            var result = await _portfolioService.GetPortfolioAsync(userId);

            if (result == null || (result.Positions.Count == 0 && result.AvailableCash == 0))
                return NotFound($"No portfolio data found for user {userId}.");

            return Ok(result);
        }
    }
}
