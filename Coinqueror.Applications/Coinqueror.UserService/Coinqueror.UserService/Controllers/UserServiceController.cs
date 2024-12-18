using Coinqueror.Shared.Models.DTOs;
using Coinqueror.UserService.Data;
using Coinqueror.UserService.Models;
using Coinqueror.UserService.Services.HelperServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Coinqueror.UserService.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class UserServiceController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger _logger;
        private readonly string name = "UserServiceController";
        private readonly IConfiguration _configuration;
        private readonly JwtSettings _jwtSettings;

        public UserServiceController(AppDbContext context, ILogger<UserServiceController> logger, IConfiguration configuration)
        {
            _context = context;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration;
        }

        // Endpoint to get all users
        [HttpGet("GetAllUsers", Name = "GetAllUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users.ToListAsync(); // Fetch all users
            return Ok(users);
        }

        // Endpoint to get a specific user by ID
        [HttpGet("GetUser/{id}", Name = "GetUser")]
        public IActionResult GetUser(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }
            else
            {
                return Ok(user);
            }
        }

        // Endpoint to create a new user
        [HttpPost("CreateUser/", Name = "CreateUser")]
        public IActionResult CreateUser([FromBody] UserModelDTO userModelDTO)
        {
            var isEmailExists = _context.Users.Any(u => u.Email == userModelDTO.Email);

            if (isEmailExists)
            {
                return BadRequest("Email already exists.");
            }
            else
            {
                var passwordSalt = BCrypt.Net.BCrypt.GenerateSalt();
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(userModelDTO.Password, passwordSalt);

                // Hash and salt the Binance API key
                var binanceApiKeySalt = BCrypt.Net.BCrypt.GenerateSalt();
                var hashedBinanceApiKey = BCrypt.Net.BCrypt.HashPassword(userModelDTO.BinanceApiKey, binanceApiKeySalt);

                // Hash and salt the Binance API secret
                var binanceApiSecretSalt = BCrypt.Net.BCrypt.GenerateSalt();
                var hashedBinanceApiSecret = BCrypt.Net.BCrypt.HashPassword(userModelDTO.BinanceApiSecret, binanceApiSecretSalt);

                var newUser = new UserModel
                {
                    Name = userModelDTO.Name,
                    Email = userModelDTO.Email,
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    Username = userModelDTO.Username,
                    BinanceApiKeyHash = hashedBinanceApiKey,
                    BinanceApiKeySalt = binanceApiKeySalt,
                    BinanceApiSecretHash = hashedBinanceApiSecret,
                    BinanceApiSecretSalt = binanceApiSecretSalt,
                    Token = null // Gerekirse token generate edilebilir
                };

                // Add new user to the database
                _context.Users.Add(newUser);
                _context.SaveChanges();
                _logger.LogInformation(name, $"New user created: ID:{newUser.Id}, Name:{newUser.Name}");
                return Ok("Data sent successfully");
            }
        }

        //Endpoint to update an existing user by ID
        //DELETE /api/TradeShifter/id
        [HttpPut("UpdateUser/{id}", Name = "UpdateUser")]
        public IActionResult UpdateUser(int id, [FromBody] UserModelDTO userModelDTO)
        {
            var updatingUser = _context.Users.Find(id);

            if (updatingUser != null)
            {
                updatingUser.Name = userModelDTO.Name;
                updatingUser.Email = userModelDTO.Email;
                updatingUser.Username = userModelDTO.Username;
            }
            else
            {
                return StatusCode(StatusCodes.Status404NotFound);
            }

            _context.Users.Update(updatingUser);

            return Ok();
        }

        // Endpoint to delete a user by ID
        [HttpDelete("DeleteUser/{id}", Name = "DeleteUser")]
        public IActionResult DeleteUser(int id)
        {
            return Ok();
        }

        [HttpPost("Login/", Name = "LoginUser")]
        public IActionResult LoginUser([FromBody] LoginUserModelDTO loginUserModelDTO)
        {
            if (loginUserModelDTO.Password == null || loginUserModelDTO.Email == null)
            {
                return BadRequest("Login credentials are required.");
            }

            var matchingUser = _context.Users.FirstOrDefault(u => u.Email == loginUserModelDTO.Email);

            if (matchingUser == null)
            {
                return Unauthorized("Invalid email or password."); // Generic error for security
            }

            if (!BCrypt.Net.BCrypt.Verify(loginUserModelDTO.Password, matchingUser.PasswordHash))
            {
                return Unauthorized("Invalid email or password.");
            }

            // Create JWT token
            var expiryDays = 2;
            var jwtKeyString = _configuration["JwtSettings:SecretKey"];
            var issuer = _configuration["JwtSettings:Issuer"];
            var audience = _configuration["JwtSettings:Audience"];

            var token = StaticHelperServices.GenerateJwtToken(matchingUser, expiryDays, jwtKeyString, issuer, audience);

            // Option : Storing the generated token in an HTTP-only cookie (recommended for security)
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Ensure it's HTTPS only
                Expires = DateTime.UtcNow.AddDays(expiryDays)
            };

            Response.Cookies.Append("authToken", token, cookieOptions);

            //matchingUser.Token = token; // Optional: store the token in the user record
            matchingUser.LastLoginDate = DateTime.UtcNow; // Update last login time
            matchingUser.LastLoginExpiryDate = DateTime.UtcNow.AddDays(expiryDays); // Set token expiration

            _context.Users.Update(matchingUser); // Save changes to the user
            _context.SaveChanges(); // Commit the changes

            return Ok(new { Message = $"Login successful Token: {token}" });
        }

        [HttpPost("Logout/{userEmail}", Name = "LogoutUser")]
        public IActionResult LogoutUser(string userEmail)
        {
            // Find the user based on the provided email
            var loggingOutUser = _context.Users.First(u => u.Email == userEmail);

            // Check if the user exists
            if (loggingOutUser != null)
            {
                // Invalidate the token
                loggingOutUser.Token = null;

                // Save changes to the database
                _context.Users.Update(loggingOutUser);
                _context.SaveChanges(); // Ensure changes are saved

                return Ok("Logout successful");
            }
            else
            {
                return NotFound("User not found.");
            }
        }
    }
}