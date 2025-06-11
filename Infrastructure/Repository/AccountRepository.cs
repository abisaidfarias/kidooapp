
using Application.Contracts;
using Application.DTOs.Request.Account;
using Application.DTOs.Response;
using Application.DTOs.Response.Account;
using Application.Extensions;
using Domain.Entity.Authentication;
using Infrastructure.Data;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Repository
{
    public class AccountRepository(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager,
        IConfiguration config, SignInManager<ApplicationUser> signInManager, AppDbContext context) : IAccount
    {

        private async Task<ApplicationUser> FindUserByEmailAsync(string email)
            => await userManager.FindByEmailAsync(email);

        private async Task<ApplicationUser> FindRoleByNameAsync(string roleName)
            => await userManager.FindByNameAsync(roleName);

        private static string GenerateRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        private async Task<string> GenerateToken(ApplicationUser user)
        {
            try
            {
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
                var userClaims = new[]
                {
                    new Claim(ClaimTypes.Name, user.FirstName!),
                    new Claim(ClaimTypes.Email, user.Email !),
                    new Claim(ClaimTypes.Role, (await userManager.GetRolesAsync(user)).FirstOrDefault()!),
                    new Claim("Fullname", $"{user.FirstName ?? ""} {user.LastName ?? ""}".Trim())
                };
                var token = new JwtSecurityToken(
                    issuer: config["Jwt:Issuer"],
                    audience: config["Jwt:Audience"],
                    claims: userClaims,
                    expires: DateTime.UtcNow.AddDays(7),
                    signingCredentials: credentials
                );

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch { return null!; }

        }

        private async Task<GeneralResponse> AssignUserToRole(ApplicationUser user, IdentityRole role)
        {
            if (user is null || role is null) return new GeneralResponse(false, "User or Role not found");

            if (await FindRoleByNameAsync(role.Name!) == null)
                await CreateRoleAsync(role.Adapt(new CreateRoleDTO()));

            var result = await userManager.AddToRoleAsync(user, role.Name!);
            var error = CheckResponse(result);
            if (!string.IsNullOrEmpty(error))
            {
                return new GeneralResponse(false, error);
            }
            else
            {
                return new GeneralResponse(true, $"{user.UserName} assigned to {role.Name} role");
            }
        }
        private static string CheckResponse(IdentityResult result)
        {
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return string.Join(Environment.NewLine, errors);
            }
            return null!;
        }

        public async Task<GeneralResponse> ChangeUserRoleAsync(ChangeUserRoleRequestDTO model)
        {
            if (await FindRoleByNameAsync(model.RoleName) is null) return new GeneralResponse(false, "Role not found");
            if (await FindUserByEmailAsync(model.Email) is null) return new GeneralResponse(false, "User not found");

            var user = await FindUserByEmailAsync(model.Email);
            var previousRole = (await userManager.GetRolesAsync(user)).FirstOrDefault();
            var removeOldRole = await userManager.RemoveFromRoleAsync(user, previousRole!);
            var error = CheckResponse(removeOldRole);
            if (!string.IsNullOrEmpty(error))
                return new GeneralResponse(false, error);

            var result = await userManager.AddToRoleAsync(user, model.RoleName);
            var response = CheckResponse(result);
            if (!string.IsNullOrEmpty(error))
                return new GeneralResponse(false, response);
            else
                return new GeneralResponse(true, "Role Changed");
        }

        public async Task<GeneralResponse> CreateAccountAsync(CreateAccountDTO model)
        {
            try
            {
                if (await FindUserByEmailAsync(model.Email) != null)
                {
                    return new GeneralResponse(false, "User already exists");
                }
                var user = new ApplicationUser()
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    UserName = model.Email,
                    ClassroomAssigned = model.ClassroomAssigned,
                    DateOfBirth = model.DateOfBirth,
                    CreatedAt = DateTime.Now,
                    SchoolName = model.SchoolName,
                    StartDate = model.StartDate,
                    Position = model.Position
                };
                var result = await userManager.CreateAsync(user);
                var error = CheckResponse(result)!;
                if (!string.IsNullOrEmpty(error))
                {
                    return new GeneralResponse(false, error);
                }
                var (flag, message) = await AssignUserToRole(user, new IdentityRole(model.Role));
                return new GeneralResponse(flag, message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task CreateAdmin()
        {
            try
            {
                if (await FindRoleByNameAsync(Constant.Role.Admin) != null) return;
                var admin = new CreateAccountDTO()
                {
                    FirstName = "Admin",
                    LastName = "Admin",
                    Email = "abisaidfarias@gmail.com",
                    Role = Constant.Role.Admin
                };
                await CreateAccountAsync(admin);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<GeneralResponse> CreateRoleAsync(CreateRoleDTO model)
        {
            try
            {
                if(await FindRoleByNameAsync(model.Name!) == null)
                {
                    var response = await roleManager.CreateAsync(new IdentityRole(model.Name!));
                    var error = CheckResponse(response);
                    if (!string.IsNullOrEmpty(error))
                    {
                        return new GeneralResponse(false, error);
                    }
                    else
                    {
                        return new GeneralResponse(true, $"{model.Name} role created");
                    }
                }
                return new GeneralResponse(false, "Role already exists");
            }
            catch (Exception ex)
            {
                return new GeneralResponse(false, ex.Message);
            }
        }
        public async Task<IEnumerable<GetRoleDTO>> GetRoleAsync() 
            => (await roleManager.Roles.ToListAsync()).Adapt<IEnumerable<GetRoleDTO>>();

        public async Task<IEnumerable<GetUsersWithRolesResponseDTO>> GetUsersWithRolesAsync()
        {
            var allUsers = await userManager.Users.ToListAsync();
            if (allUsers is null || !allUsers.Any()) return [];

            var allRoles = await roleManager.Roles.ToListAsync();
            var roleDictionary = allRoles
                .Where(r => r.Name != null)
                .ToDictionary(r => r.Name!.ToLower(), r => new { r.Id, r.Name });

            var userRoles = await Task.WhenAll(allUsers.Select(async user => new
            {
                user,
                Roles = await userManager.GetRolesAsync(user)
            }));

            return [.. userRoles
                .SelectMany(ur => ur.Roles, (ur, role) => new { ur.user, role })
                .Where(ur => roleDictionary.ContainsKey(ur.role.ToLower()))
                .Select(ur => new GetUsersWithRolesResponseDTO
                {
                    FirstName = ur.user.FirstName,
                    LastName = ur.user.LastName,
                    Email = ur.user.Email,
                    RoleId = roleDictionary[ur.role.ToLower()].Id,
                    RoleName = roleDictionary[ur.role.ToLower()].Name
                })];
        }

        public async Task<LoginResponse> LoginAccountAsync(LoginDTO model)
        {
            try
            {
                var user = await FindUserByEmailAsync(model.Email);
                if (user is null) return new LoginResponse(false, "User not found");

                var result = signInManager.CheckPasswordSignInAsync(user, model.Password, false);
                if (!result.Result.Succeeded) return new LoginResponse(false, "Invalid password");

                var jwtToken = await GenerateToken(user);
                var refreshToken = GenerateRefreshToken();
                if (string.IsNullOrEmpty(jwtToken) || string.IsNullOrEmpty(refreshToken))
                {
                    return new LoginResponse(false, "An error has occurred while trying to log in. Please contact the administrator");
                }
                else
                {
                    var saveToken = await SaveRefreshToken(user.Id, refreshToken);
                    if (saveToken.Flag)
                    {
                        return new LoginResponse(true, $"{user.FirstName} successfully logged in", jwtToken, refreshToken);
                    }
                    else
                    {
                        return new LoginResponse(false, saveToken.Message);
                    }
                }
            }
            catch (Exception)
            {
                return new LoginResponse(false, "An error has occurred while trying to log in. Please contact the administrator");
            }
        }

        public async Task<LoginResponse> RefreshTokenAsync(RefreshTokenDTO model)
        {
            var token = await context.RefreshTokens.FirstOrDefaultAsync(x => x.Token == model.Token);
            if (token is null)
            {
                return new LoginResponse(false, "Invalid token");
            }
            var user = await userManager.FindByIdAsync(token.UserId!);
            if (user == null)
            {
                return new LoginResponse(false, "User not found");
            }
            string newToken = await GenerateToken(user);
            string newRefreshToken = GenerateRefreshToken();
            var saveToken = await SaveRefreshToken(user.Id, newRefreshToken);
            if (saveToken.Flag)
            {
                return new LoginResponse(true, "Token refreshed successfully", newToken, newRefreshToken);
            }
            else
            {
                return new LoginResponse(false, saveToken.Message);
            }
        }
        private async Task<GeneralResponse> SaveRefreshToken(string userId, string token)
        {
            try
            {
                var user = await context.RefreshTokens.FirstOrDefaultAsync(x => x.UserId == userId);
                if(user is null)
                {
                    context.RefreshTokens.Add(new RefreshToken() { UserId = userId, Token = token });
                }
                else
                {
                    user.Token = token;
                }
                await context.SaveChangesAsync();
                return new GeneralResponse(true,null!);
            }
            catch (Exception ex)
            {
                return new GeneralResponse(false, ex.Message);
            }
        }
    }
}
