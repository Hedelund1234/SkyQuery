using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using SkyQuery.ImageService.Application.Interfaces;
using SkyQuery.ImageService.Domain.Entities;

namespace SkyQuery.ImageService.Controllers
{
    [ApiController]
    [Route("image")]
    public class ImageController : ControllerBase
    {
        private readonly ILogger<ImageController> _logger;
        private readonly DaprClient _daprClient;
        private readonly IDataforsyningService _dataforsyningService;

        public ImageController(DaprClient daprClient, ILogger<ImageController> logger, IDataforsyningService dataforsyningService)
        {
            _logger = logger;
            _daprClient = daprClient;
            _dataforsyningService = dataforsyningService;
        }

        [Topic("pubsub", "image.requested")]
        public async Task<IActionResult> HandleImageRequest([FromBody] ImageRequest request)
        {
            _logger.LogInformation($"Received image request for UserId: {request.UserId}, MGRS: {request.Mgrs}");
            try
            {
                ImageAvailable result = await _dataforsyningService.GetMapFromDFAsync(request);

                await _daprClient.PublishEventAsync("pubsub", "image.available", result);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing image available event");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
