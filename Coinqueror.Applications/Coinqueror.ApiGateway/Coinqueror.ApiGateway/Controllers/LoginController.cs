using Coinqueror.Shared.Models.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Coinqueror.ApiGateway.Controllers
{
    public class LoginController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public LoginController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserModelDTO loginUserModelDTO)
        {
            // Forward the login request to the TradeShifter project
            var response = await _httpClient.PostAsJsonAsync("http://<TradeShifter_URL>/api/login", loginUserModelDTO);

            if (response.IsSuccessStatusCode)
            {
                var client = new HttpClient();
                var tradeShifterUrl = _configuration["ApiSettings:TradeShifter:DockerBaseUrl"];
                var result = await client.GetAsync($"{tradeShifterUrl}/api/endpoint");

                //var result = await response.Content.ReadFromJsonAsync<object>(); // You can create a specific model for the response
                return Ok(response);
            }

            return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
        }
    }
}
