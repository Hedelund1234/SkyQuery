using Dapr.Client;
using Microsoft.Extensions.Logging;
using SkyQuery.AppGateway.Application.Interfaces;
using SkyQuery.AppGateway.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyQuery.AppGateway.Infrastructure.Services
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

        public async Task GetNewImage(ImageRequestWithToken requestWithToken)
        {
            try
            {
                _logger.LogInformation("Requesting new image for {mgrs} - Requester: {request.UserId}", requestWithToken.Mgrs, requestWithToken.UserId);
                await _daprClient.PublishEventAsync("pubsub", "image.requested", requestWithToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error requesting new image for {mgrs}", requestWithToken);
            }
        }
    }
}
