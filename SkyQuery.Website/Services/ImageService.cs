using SkyQuery.Website.Interfaces;
using System.Net.Http.Json;

namespace SkyQuery.Website.Services
{
    public class ImageService : IImageService
    {
        private readonly HttpClient _httpClient;
        public ImageService(HttpClient httpClient)
        {
            _httpClient = httpClient;
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
