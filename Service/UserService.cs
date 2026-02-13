
using Microsoft.EntityFrameworkCore;
using where_we_go.Database;
using where_we_go.DTO;
using where_we_go.Models;

namespace where_we_go.Service
{
    public interface IUserService
    {
    }
    public class UserService(AppDbContext appDbContext) : IUserService
    {
        private AppDbContext _dbContext { get; init; } = appDbContext;

    }
}