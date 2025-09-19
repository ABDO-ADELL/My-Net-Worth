using Microsoft.AspNetCore.Mvc;
using PRISM.Models;
using PRISM.Services;


namespace PRISM.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {

        private readonly IAuthService _auth;

        public AuthController(AuthService auth)
        {
            _auth = auth;
        }
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] Register model)
        {
            if (!ModelState.IsValid)
            { return BadRequest(ModelState); }


            var result = await _auth.RegisterAsync(model);

            if (!result.IsAuthenticated)
            { return BadRequest(result.Message); }


            return Ok(new { token = result.Token, ExpiresOn = result.Expiration });

            
        }
        [HttpPost("Login")]
        public async Task<IActionResult> LoginAsync([FromBody] Login model)
        {
            if (!ModelState.IsValid)
            { return BadRequest(ModelState); }


            var result = await _auth.LoginAsync(model);

            if (!result.IsAuthenticated)
            { return BadRequest(result.Message); }

            return Ok(new { token = result.Token, ExpiresOn = result.Expiration });
        }
        [HttpPost("AddRole")]
        public async Task<IActionResult> Role([FromBody] AddRole model)
        {
            if (!ModelState.IsValid)
            { return BadRequest(ModelState); }


            var result = await _auth.AddRoleAsync(model);

            if (!string.IsNullOrEmpty(result))
                return BadRequest(result);

            return Ok(model);
        }

    }
}
