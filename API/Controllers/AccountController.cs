using Application.Contracts;
using Application.DTOs.Request.Account;
using Application.DTOs.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController(IAccount account) : ControllerBase
    {
        [HttpPost("identity/create")]
        public async Task<ActionResult<GeneralResponse>> CreateAdmin(CreateAccountDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new GeneralResponse(false, "Invalid Model"));

            return Ok(await account.CreateAccountAsync(model));
        }

        [HttpPost("identity/login")]
        [SwaggerRequestExample(typeof(LoginDTO), typeof(LoginRequestExample))]
        public async Task<ActionResult<GeneralResponse>> Login(LoginDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new GeneralResponse(false, "Invalid Model"));

            return Ok(await account.LoginAccountAsync(model));
        }

        [HttpPost("identity/refresh")]
        public async Task<ActionResult<GeneralResponse>> RefreshToken(RefreshTokenDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new GeneralResponse(false, "Invalid Model"));

            return Ok(await account.RefreshTokenAsync(model));
        }

        [HttpPost("identity/role/create")]
        public async Task<ActionResult<GeneralResponse>> CreateRole(CreateRoleDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new GeneralResponse(false, "Invalid Model"));

            return Ok(await account.CreateRoleAsync(model));
        }

        [HttpGet("identity/role/get")]
        public async Task<ActionResult<GeneralResponse>> GetRole() => Ok(await account.GetRoleAsync());

        [HttpPost("/setting")]
        public async Task<ActionResult<GeneralResponse>> CreateAdmin()
        {
            await account.CreateAdmin();
            return Ok(new GeneralResponse(true, "Admin Created"));
        }

        [HttpGet("identity/users-with-roles")]
        public async Task<ActionResult<GeneralResponse>> GetUsersWithRoles() => Ok(await account.GetUsersWithRolesAsync());

        [HttpPost("identity/change-role")]
        public async Task<ActionResult<GeneralResponse>> ChangeUserRole(ChangeUserRoleRequestDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new GeneralResponse(false, "Invalid Model"));
            return Ok(await account.ChangeUserRoleAsync(model));

        }

    }
}