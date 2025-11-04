using Blazored.LocalStorage;
using SkyQuery.Website.Interfaces;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace SkyQuery.Website.Services
{
    public class ImageService : IImageService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        public ImageService(HttpClient httpClient, ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
        }
        
        public async Task OrderImage(string userId, string mgrs)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("UserId is required", nameof(userId));
            }
                
            if (string.IsNullOrWhiteSpace(mgrs))
            {
                throw new ArgumentException("MGRS is required", nameof(mgrs));
            }

            var token = await _localStorage.GetItemAsStringAsync("authToken");

            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            

            var request = new
            {
                UserId = userId,
                Mgrs = mgrs
            };

            try
            {
                var response = await _httpClient.PostAsJsonAsync("images/image", request);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Bestilling fejlede: {response.StatusCode} - {error}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<byte[]> GetImageAsync(string userId, string mrgs)
        {
            var token = await _localStorage.GetItemAsStringAsync("authToken");

            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var request = new
            {
                UserId = userId,
                Mgrs = mrgs
            };

            var response = await _httpClient.PostAsJsonAsync("images/get", request);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Fejl: {response.StatusCode}");
            }
                
            return await response.Content.ReadAsByteArrayAsync();
        }
    }
}
