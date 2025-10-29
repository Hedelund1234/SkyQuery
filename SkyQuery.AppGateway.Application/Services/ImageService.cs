using Dapr.Client;
using Microsoft.Extensions.Logging;
using SkyQuery.AppGateway.Application.Interfaces;
using SkyQuery.AppGateway.Domain.Entities;

namespace SkyQuery.AppGateway.Application.Services
{
    public class ImageService : IImageService
    {
        private readonly DaprClient _daprClient;
        private readonly ILogger<ImageService> _logger;
        public ImageService(DaprClient daprClient, ILogger<ImageService> logger)
        {
            _daprClient = daprClient;
            _logger = logger;
        }

        public async Task GetNewImage(ImageRequest request)
        {
            try
            {
                _logger.LogInformation("Requesting new image for {mgrs} - Requester: {request.UserId}", request.Mgrs, request.UserId);
                await _daprClient.PublishEventAsync("pubsub", "image.requested", request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error requesting new image for {mgrs}", request);
            }
        }
    }
}
