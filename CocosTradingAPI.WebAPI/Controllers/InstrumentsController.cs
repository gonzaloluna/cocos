using CocosTradingAPI.Application.Interfaces;
using CocosTradingAPI.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace CocosTradingAPI.WebAPI.Controllers
{
    [ApiController]
    [Route("api/instruments")]
    public class InstrumentsController : ControllerBase
    {
        private readonly IInstrumentRepository _instrumentRepo;

        public InstrumentsController(IInstrumentRepository instrumentRepo)
        {
            _instrumentRepo = instrumentRepo;
        }

        [HttpGet]
        public async Task<ActionResult<List<InstrumentDto>>> SearchInstruments([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("You must provide a search query by ticker or name.");

            var result = await _instrumentRepo.SearchByTickerOrNameAsync(query);
            return Ok(result);
        }
    }
}
