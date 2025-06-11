using Application.DTOs.Request.Account;
using Application.DTOs.Response.Account;
using Application.Services;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Headers;

namespace Application.Extensions
{
    public class CustomHttpHandler(LocalStorageService localStorageService,
        NavigationManager navigationManager,
        IAccountService accountService) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            { 
                var loginUrl = request.RequestUri!.AbsoluteUri.Contains(Constant.LoginRoute);
                var registerUrl = request.RequestUri!.AbsoluteUri.Contains(Constant.RegisterRoute);
                var refreshTokenUrl = request.RequestUri!.AbsoluteUri.Contains(Constant.RefreshTokenRoute);
                var adminCreateUrll = request.RequestUri!.AbsoluteUri.Contains(Constant.CreateAdminRoute);
                if (loginUrl || registerUrl || refreshTokenUrl || adminCreateUrll)
                {
                    return await base.SendAsync(request, cancellationToken);
                }
                var result = await base.SendAsync(request, cancellationToken);
                if (result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    var tokenModel = await localStorageService.GetModelFromToken();
                    if (string.IsNullOrEmpty(tokenModel.Token))
                    {
                        return result;
                    }
                    var newJwtToken = await GetRefreshToken(tokenModel.Refresh!);
                    if (string.IsNullOrEmpty(newJwtToken))
                    {
                        return result;
                    }
                    request.Headers.Authorization = new AuthenticationHeaderValue(Constant.HttpClientHeaderSchema, newJwtToken);
                    return await base.SendAsync(request, cancellationToken);
                }
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CustomHttpHandler] Error: {ex}");
                // Opcional: puedes devolver una respuesta vacía con código de error
                return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent("Error interno en CustomHttpHandler")
                };
            }
        }

        private async Task<string> GetRefreshToken(string refreshToken)
        {
            try
            {
                var response = await accountService.RefreshTokenAsync(new RefreshTokenDTO() { Token = refreshToken });
                if (response is null || response.Token is null)
                {
                    await ClearBrowserStorage();
                    NavigateToLogin();
                    return null!;
                }
                await localStorageService.RemoveTokenFromBrowserLocalStorage();
                await localStorageService.SetBrowserLocalStorage(new LocalStorageDTO()
                {
                    Token = response.Token,
                    Refresh = response.RefreshToken
                });
                return response.Token;
            }
            catch { return null!; }
        }

        private void NavigateToLogin() =>
            navigationManager.NavigateTo(navigationManager.BaseUri, true, true);

        private async Task ClearBrowserStorage() => 
            await localStorageService.RemoveTokenFromBrowserLocalStorage();
    }
}
