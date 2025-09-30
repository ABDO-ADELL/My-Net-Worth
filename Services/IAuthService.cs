using PRISM.Models.Authmodels;
using System.Collections.Generic;

namespace PRISM.Services

{
    public interface IAuthService
    {
        Task<AuthModel> RegisterAsync(RegisterModel model);
        Task<AuthModel> LoginAsync(LoginModel model);
        Task<string> AddRoleAsync(AddRole model);
        Task<AuthModel> RefreshTokenAsync(string token);
        Task<bool> RevokeTokenAsync(string token);



        //Task<IEnumerable<string>> GetAllRolesAsync();
        //Task<IEnumerable<AppUser>> GetAllUsersAsync();
    }
}
