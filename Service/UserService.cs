
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using where_we_go.Database;
using where_we_go.DTO;
using where_we_go.Models;

namespace where_we_go.Service
{
    public interface IUserService
    {
        public Task<PaginatedResponseDto<UserResponseDto>> GetUsersAsync(UserQueryDto query);
        Task<IdentityResult> UpdateUserAsync(AdminUpdateUserDto model);
    }

    public class UserService(UserManager<User> userManager, AppDbContext appContext) : BaseService, IUserService
    {
        private UserManager<User> _userManager { get; init; } = userManager;
        private AppDbContext _dbContext { get; init; } = appContext;

        /* For Admin */

        public async Task<IdentityResult> UpdateUserAsync(AdminUpdateUserDto model)
        {

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user is null)
                return IdentityResult.Failed(new IdentityError { Description = "User Not FOund" });


            return await _userManager.UpdateAsync(user);
        }

        public async Task<PaginatedResponseDto<UserResponseDto>> GetUsersAsync(UserQueryDto query)
        {
            var usersQuery = _dbContext.Users.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(query.NameFilter))
            {
                var keyword = query.NameFilter.Trim();
                usersQuery = usersQuery.Where(u =>
                    u.Name.Contains(keyword));
            }

            usersQuery = (query.SortBy ?? "").ToLower() switch
            {
                "name" => usersQuery.OrderBy(u => u.Name),
                "name_desc" => usersQuery.OrderByDescending(u => u.Name),
                _ => usersQuery.OrderBy(u => u.Id)
            };

            return await ToPaginatedResponseAsync(
                usersQuery, query, u => new UserResponseDto(u, [], u.ProfileImageKey ?? string.Empty)
            );
        }

    }
}