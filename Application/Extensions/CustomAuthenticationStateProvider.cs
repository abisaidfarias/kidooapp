using Application.DTOs.Request.Account;
using Application.DTOs.Response.Account;
using Application.Services;
using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Application.Extensions
{
    public class CustomAuthenticationStateProvider(LocalStorageService localStorageService) : AuthenticationStateProvider
    {
        private readonly ClaimsPrincipal anonymous = new(new ClaimsIdentity());
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var tokenModel = await localStorageService.GetModelFromToken();
            if (string.IsNullOrEmpty(tokenModel.Token) || string.IsNullOrWhiteSpace(tokenModel.Token))
            {
                return new AuthenticationState(anonymous);
            }
            var getUserClaims = DecryptToken(tokenModel.Token!);
            if(getUserClaims == null)
            {
                return await Task.FromResult(new AuthenticationState(anonymous));
            }
            var claimsPrincipal = SetClaimPrincipal(getUserClaims);
            return await Task.FromResult(new AuthenticationState(claimsPrincipal));
        }
        public async Task UpdateAuthenticationState(LocalStorageDTO localStorageDTO)
        {
            var claimPrincipal = new ClaimsPrincipal();
            if(localStorageDTO.Token is not null || localStorageDTO.Refresh is not null)
            {
                await localStorageService.SetBrowserLocalStorage(localStorageDTO);
                var getUserClaims = DecryptToken(localStorageDTO.Token!);
                claimPrincipal = SetClaimPrincipal(getUserClaims);
            }
            else
            {
                await localStorageService.RemoveTokenFromBrowserLocalStorage();
            }
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimPrincipal)));
        }

        private static ClaimsPrincipal SetClaimPrincipal(UserClaimsDTO claims)
        {
            if (claims.Email is null)
            {
                return new ClaimsPrincipal();
            }
            return new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Name, claims.UserName),
                new Claim(ClaimTypes.Email, claims.Email),
                new Claim(ClaimTypes.Role, claims.Role),
                new Claim("Fullname", claims.Fullname)
            ], Constant.AuthenticationType));
        }

        private static UserClaimsDTO DecryptToken(string token)
        {
            try
            {
                if(string.IsNullOrEmpty(token))
                {
                    return new UserClaimsDTO();
                }
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                var name = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
                var email = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                var role = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                var fullname = jwtToken.Claims.FirstOrDefault(c => c.Type == "Fullname")?.Value;
                return new UserClaimsDTO(fullname!, name!, email!, role!);
            }
            catch (Exception)
            {
                return null!;
            }

        }

    }
}
