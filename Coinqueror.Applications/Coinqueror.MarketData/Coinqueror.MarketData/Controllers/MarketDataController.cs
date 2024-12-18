using Microsoft.AspNetCore.Mvc;

namespace Coinqueror.MarketData.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MarketDataController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly string name = "MarketDataController";
        private readonly IConfiguration _configuration;

        public MarketDataController(ILogger<MarketDataController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        // Simple endpoint for API Gateway to hit
        [HttpGet("GetMarketData")]
        public IActionResult GetMarketData()
        {
            _logger.LogDebug("MarketDataController: GetMarketData() called");
            // This can return a simple string response for now
            return Ok("This is the Market Data service response");
        }
    }
}
