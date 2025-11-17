using System.Net.Http.Headers;
using System.Net.Http.Json;
using JoinRpg.XGameApi.Contract;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JoinRpg.Client
{
    public class JoinUserInfoClient(HttpClient httpClient, IOptions<JoinConnectOptions> options, ILogger<JoinUserInfoClient> logger)
    {
        private readonly string host = options.Value.Host.TrimEnd('/');
        private readonly string username = options.Value.UserName;
        private readonly string password = options.Value.Password;

        private string? accessToken = null;

        public async Task<PlayerInfo?> GetUserInfo(int userId)
        {
            accessToken ??= await AuthenticateAsync();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; RatingbastiliaRu/1.0)");
            var response = await httpClient.GetAsync(new Uri($"{host}/x-api/users/{userId}"));
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<PlayerInfo>();
        }

        private async Task<string?> AuthenticateAsync()
        {
            try
            {
                var formData = new Dictionary<string, string>
                {
                    { "username", username },
                    { "password", password },
                    { "grant_type", "password" }
                };

                var content = new FormUrlEncodedContent(formData);

                var response = await httpClient.PostAsync($"{host}/x-api/token", content);

                if (response.IsSuccessStatusCode)
                {
                    var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();

                    if (tokenResponse?.access_token != null)
                    {
                        return tokenResponse.access_token;
                    }
                }
                logger.LogWarning($"Ошибка авторизации: {response.StatusCode}");
                var errorResponse = await response.Content.ReadAsStringAsync();
                logger.LogWarning($"Ответ сервера: {errorResponse}");
                return null;
            }
            catch (Exception ex)
            {
                logger.LogWarning($"Ошибка при авторизации: {ex.Message}");
                return null;
            }
        }
        private record class TokenResponse(string access_token, string token_type, int expires_in);
    }
}
