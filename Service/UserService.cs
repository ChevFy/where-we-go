using Microsoft.EntityFrameworkCore;
using where_we_go.Database;
using where_we_go.DTO;
using where_we_go.Models;

namespace where_we_go.Service
{
    public interface IUserService
    {
        public Task<PaginatedResponseDto<UserResponseDto>> GetUsersAsync(PaginatedQueryDto query);
    }

    public class UserService(AppDbContext appDbContext) : BaseService, IUserService
    {
        private AppDbContext _dbContext { get; init; } = appDbContext;

        public async Task<PaginatedResponseDto<UserResponseDto>> GetUsersAsync(PaginatedQueryDto query)
        {
            var usersQuery = _dbContext.Users
                .AsNoTracking()
                .OrderBy(u => u.UserName);

            return await ToPaginatedResponseAsync(
                usersQuery,
                query,
                user => new UserResponseDto(user, [])
            );
        }
    }
}