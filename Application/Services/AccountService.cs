using Application.DTOs.Request.Account;
using Application.DTOs.Response;
using Application.DTOs.Response.Account;
using Application.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class AccountService(HttpClientService httpClientService) : IAccountService
    {
        public async Task<LoginResponse> LoginAccountAsync(LoginDTO model)
        {
            try
            {
                var publicClient = httpClientService.GetPublicClient();
                var test = Constant.LoginRoute;
                var response = await publicClient.PostAsJsonAsync(test, model);
                string error = CheckResponseStatus(response);
                if(!string.IsNullOrEmpty(error))
                {
                    return new LoginResponse(false, error);
                }
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                return result!;
            }
            catch (Exception ex)
            {
                return new LoginResponse(false, ex.Message);
            }
        }

        private string CheckResponseStatus(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                return response.StatusCode switch
                {
                    System.Net.HttpStatusCode.Unauthorized => "Unauthorized",
                    System.Net.HttpStatusCode.Forbidden => "Forbidden",
                    System.Net.HttpStatusCode.NotFound => "Not Found",
                    System.Net.HttpStatusCode.InternalServerError => "Internal Server Error",
                    _ => "Unknown Error"
                };
            }
            return string.Empty;
        }

        public Task<GeneralResponse> ChangeUserRoleAsync(ChangeUserRoleRequestDTO model)
        {
            throw new NotImplementedException();
        }
        public Task<GeneralResponse> CreateAccountAsync(CreateAccountDTO model)
        {
            throw new NotImplementedException();
        }
        public Task CreateAdmin()
        {
            throw new NotImplementedException();
        }
        public Task<GeneralResponse> CreateRoleAsync(CreateRoleDTO model)
        {
            throw new NotImplementedException();
        }
        public Task<IEnumerable<GetRoleDTO>> GetRoleAsync()
        {
            throw new NotImplementedException();
        }
        public Task<IEnumerable<GetUsersWithRolesResponseDTO>> GetUsersWithRolesAsync()
        {
            throw new NotImplementedException();
        }
        public Task<LoginResponse> RefreshTokenAsync(RefreshTokenDTO model)
        {
            throw new NotImplementedException();
        }
    }
}
