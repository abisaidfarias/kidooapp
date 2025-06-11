using Application.DTOs.Request.Account;
using NetcodeHub.Packages.Extensions.LocalStorage;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Application.Extensions
{
    public class LocalStorageService(ILocalStorageService localStorageService) 
    {
        public async Task<string> GetBrowserLocalStorage()
        {
           var tokenModel = await localStorageService.GetEncryptedItemAsStringAsync(Constant.BrowserStorageKey);
            return tokenModel;
        }
        public async Task<LocalStorageDTO> GetModelFromToken()
        {
            try
            {
                var token = await GetBrowserLocalStorage();
                if (string.IsNullOrEmpty(token) || string.IsNullOrWhiteSpace(token))
                {
                    return new LocalStorageDTO();
                }
                return DeserializeJsonString<LocalStorageDTO>(token);
            }
            catch
            {
                return new LocalStorageDTO();
            }
        }
        public async Task SetBrowserLocalStorage(LocalStorageDTO localStorage)
        {
            try
            {
                var token = SerializeObj(localStorage);
                await localStorageService.SaveAsEncryptedStringAsync(Constant.BrowserStorageKey, token);
            }
            catch { }
        }
        public async Task RemoveTokenFromBrowserLocalStorage() 
            => await localStorageService.DeleteItemAsync(Constant.BrowserStorageKey);
        private static T DeserializeJsonString<T>(string jsonString)
            => JsonSerializer.Deserialize<T>(jsonString, JsonOptions())!;
        private static string SerializeObj<T>(T modelObject) =>
            JsonSerializer.Serialize(modelObject, JsonOptions());
        private static JsonSerializerOptions JsonOptions()
        {
            return new JsonSerializerOptions
            {
                AllowTrailingCommas = true,
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip,
            };
        }
    }
}
