using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coinqueror.ApiGateway.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Gateway_MarketDataController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public Gateway_MarketDataController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet("Welcome")]
        public IActionResult GetWelcomeMessage()
        {
            return Ok("Welcome to the Coinqueror API Gateway!");
        }

        // Protected endpoint with authentication (JWT required)
        [Authorize] // This ensures the user is authenticated before calling the service
        [HttpGet("GetProtectedData")]
        public IActionResult GetProtectedData()
        {
            // Access user data from the JWT token (you can check claims, etc.)
            var username = User?.Identity?.Name; // Username or any other claim you included in the token
            return Ok($"Protected Data! You are logged in as {username}");
        }

        [Authorize] // This ensures the user is authenticated before calling the service
        [HttpGet("DirectMarketDataProj")]
        public IActionResult DirectToMarketDataProj()
        {
            // Access user data from the JWT token (you can check claims, etc.)
            var username = User?.Identity?.Name; // Username or any other claim you included in the token
            return Ok($"Protected Data! You are logged in as {username}");
        }

        [Authorize]
        [HttpGet("GetMarketData")]
        public async Task<IActionResult> GetMarketData()
        {
            try
            {
                //Create an instance of a named client "MarketData" getting its properties from _httpClientFactory registered in Program.cs
                //HttpClientFactory disposes the HttpClient instance automatically
                //So "using" is not required
                var client = _httpClientFactory.CreateClient("MarketData");

                var response = await client.GetAsync("/api/MarketData/GetMarketData");

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    return Ok(data);
                }
                else
                {
                    return StatusCode((int)response.StatusCode, "Failed to retrieve market data");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }

        }
    }
}
