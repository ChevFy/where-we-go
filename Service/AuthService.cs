// using where_we_go.Models;
// using where_we_go.DTO;

// namespace where_we_go.Service
// {
//     public interface IAuthService
//     {
//         Task<UserResponseDto?> LoginGoogleAsync(LoginGoogleDto dto);
//         Task<User?> LoginAsync(string email, string password);
//     }
//     public class AuthService(IUserService userService) : IAuthService
//     {
//         private IUserService _userService { get; init; } = userService;
//         public async Task<UserResponseDto?> LoginGoogleAsync(LoginGoogleDto dto)
//         {
//             var existUser = await _userService.GetUserByEmailAsync(dto.Email)
//                 ?? await _userService.CreateUserAsync(new User
//                 {
//                     Email = dto.Email,
//                     UserName = dto.GivenName,
//                     Firstname = dto.Name.Split(' ')[0],
//                     Lastname = dto.Name.Split(' ')[1],
//                     OAuthId = dto.ProviderKey,
//                     DateCreated = DateTime.UtcNow,
//                     DateUpdated = DateTime.UtcNow,
//                     Role = UserRoleEnum.User
//                 });
//             return existUser;
//         }

//         public Task<User?> LoginAsync(string email, string password)
//         {
//             throw new NotImplementedException();
//         }
//     }
// }