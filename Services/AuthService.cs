using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PRISM.Helpers;
using PRISM.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
namespace PRISM.Services
{
    public class AuthService : IAuthService
    {

        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly JWT _jwt;

        public AuthService(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager,IOptions<JWT> jwt)
        {
            _userManager = userManager;
            _jwt = jwt.Value;
            _roleManager = roleManager;
        }
        public async Task<AuthModel> RegisterAsync(Register model)
        {
            if (await _userManager.FindByEmailAsync(model.Email) is not null)
            { return new AuthModel { Message = "Email is already registered!" }; }
            if (await _userManager.FindByNameAsync(model.Email) is not null)
            { return new AuthModel { Message = "Username is already registered!" }; }

            var user = new AppUser
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                UserName = model.Email,
                SecurityStamp = Guid.NewGuid().ToString()
            };


            var result = await _userManager.CreateAsync(user, model.Password);
            return result.Succeeded ? new AuthModel { Message = "User created successfully!" } : new AuthModel
            {
                Message = "User did not create successfully! Please try again."
                    + string.Join(", ", result.Errors.Select(e => e.Description))
            };

            var jwtSecurityToken = await CreateJwtToken(user);
            return new AuthModel
            {
                Email = user.Email,
                IsAuthenticated = true,
                Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                Expiration = jwtSecurityToken.ValidTo,
                UserName = user.UserName,
                Roles = (await _userManager.GetRolesAsync(user)).ToList()
            };

        }

        public async Task<AuthModel> LoginAsync(Login model)
        {
            var authmodel = new AuthModel();
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user is null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                authmodel.Message = "Email or Password is Incorrect!";
            }
            var jwtSecurityToken = await CreateJwtToken(user);
            var roles = await _userManager.GetRolesAsync(user);
            var userClaims = await _userManager.GetClaimsAsync(user);
            authmodel.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

            authmodel.IsAuthenticated = true;
            authmodel.Email = user.Email;
            authmodel.Expiration = jwtSecurityToken.ValidTo;
            authmodel.UserName = user.UserName;
            authmodel.Roles = roles.ToList();
            authmodel.Message = "Login Successful!";

            return authmodel;
        }


        public async Task<string>AddRoleAsync(AddRole model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user is null || !await _roleManager.RoleExistsAsync(model.RoleName))
                return "User or Role does not exist!";
            if (await _userManager.IsInRoleAsync(user, model.RoleName))
                return "User already assigned to this role!";

            var result = await _userManager.AddToRoleAsync(user, model.RoleName);

            return result.Succeeded ? "User added to role successfully!"
                :"Error: User could not be added to role.";
        }

        private async Task<JwtSecurityToken> CreateJwtToken(AppUser user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            var roleClaims = new List<Claim>();

            foreach (var role in roles)
                roleClaims.Add(new Claim("roles", role));

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("uid", user.Id)
            }
            .Union(userClaims)
            .Union(roleClaims);

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTime.Now.AddDays(_jwt.DurationInDays),
                signingCredentials: signingCredentials);

            return jwtSecurityToken;
        }
    }
}
