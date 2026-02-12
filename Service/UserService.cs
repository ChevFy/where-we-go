
using Microsoft.EntityFrameworkCore;
using WhereWeGo.Database;
using WhereWeGo.DTO;
using WhereWeGo.Models;

namespace WhereWeGo.Service
{
    public interface IUserService
    {
        Task<User?> GetUserByIDAsync(string id);
        Task<UserResponseDto?> CreateUserAsync(User user);
        Task<UserResponseDto?> GetUserByEmailAsync(string email);
    }
    public class UserService(AppDbContext appDbContext) : IUserService
    {
        private AppDbContext _dbContext { get; init; } = appDbContext;
        public async Task<User?> GetUserByIDAsync(string id)
        {
            return await _dbContext.Users.FindAsync(id);
        }

        public async Task<UserResponseDto?> CreateUserAsync(User user)
        {
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();
            return new UserResponseDto(user);
        }

        public async Task<UserResponseDto?> GetUserByEmailAsync(string email)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return null;
            return new UserResponseDto(user);
        }

    }
}