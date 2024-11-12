using Coinqueror.TradeShifter.Data;
using Coinqueror.TradeShifter.Models;

namespace Coinqueror.TradeShifter.Services
{
    public class UserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public async Task CreateUserAsync(UserModel user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }
    }
}
