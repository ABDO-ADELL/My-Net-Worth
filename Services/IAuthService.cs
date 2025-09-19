using PRISM.Models;
using System.Collections.Generic;

namespace PRISM.Services

{
    public interface IAuthService
    {
        Task<AuthModel> RegisterAsync(Register model);
        Task<AuthModel> LoginAsync(Login model);
        Task<string> AddRoleAsync(AddRole model);

        //Task<IEnumerable<string>> GetAllRolesAsync();
        //Task<IEnumerable<AppUser>> GetAllUsersAsync();
    }
}
