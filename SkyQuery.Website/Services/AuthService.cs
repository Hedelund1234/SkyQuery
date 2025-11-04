using SkyQuery.Website.Interfaces;
using System.Net.Http.Json;

namespace SkyQuery.Website.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        public AuthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<string> Login(string email, string password)
        {
            if (email is null || email == "")
            {
                throw new ArgumentNullException("Email is empty", nameof(email));
            }
            
            if (password is null || password == "")
            {
                throw new ArgumentNullException("Password is empty", nameof(password));
            }

            try
            {
                var response = await _httpClient.PostAsJsonAsync("auth/login", new { Email = email, Password = password });
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Bestilling fejlede: {response.StatusCode} - {error}");
                }
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
