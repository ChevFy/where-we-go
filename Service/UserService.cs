
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using where_we_go.Database;
using where_we_go.DTO;
using where_we_go.Models;

namespace where_we_go.Service
{
    public interface IUserService
    {
        Task<IdentityResult> UpdateUserAsync(UpdateUserDto model);
    }
    public class UserService(UserManager<User> userManager) : IUserService
    {
        private UserManager<User> _userManager { get; init; } = userManager;
        
        public async Task<IdentityResult> UpdateUserAsync(UpdateUserDto model)
        {
            
            var user = await _userManager.FindByIdAsync(model.Id);
            if(user is null)
                return IdentityResult.Failed(new IdentityError { Description = "ไม่พบผู้ใช้งานนี้"});


            return await _userManager.UpdateAsync(user);
        }
        

    }
}