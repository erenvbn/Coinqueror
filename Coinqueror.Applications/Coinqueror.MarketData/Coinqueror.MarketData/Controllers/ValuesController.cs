using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Coinqueror.MarketData.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly string name = "ValuesController";
        private readonly IConfiguration _configuration;

        public ValuesController()
        {

        }

        // Endpoint to get all users
        [HttpGet("ValuesController", Name = "ValuesController")]
        public async Task<IActionResult> GetAllValues()
        {
            return Ok();
        }
    }
}
