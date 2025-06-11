using Application.Services;

namespace Application.Extensions
{
    public class HttpClientService(IHttpClientFactory httpClientFactory, LocalStorageService localStorageService)
    {
        private HttpClient CreateClient() => httpClientFactory.CreateClient(Constant.HttpClientName);
        public HttpClient GetPublicClient() => CreateClient();
        public async Task<HttpClient> GetPrivateClient()
        {
            try
            {
                var client = CreateClient();
                var localStorageDTO = await localStorageService.GetModelFromToken();
                if (string.IsNullOrEmpty(localStorageDTO.Token))
                {
                    return client;
                }
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(Constant.HttpClientHeaderSchema, localStorageDTO.Token);
                return client;
            }
            catch
            {
                return new HttpClient();
            }
        }
    }
}
