using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using SkyQuery.ImageService.Domain.Entities;

namespace SkyQuery.ImageService.Controllers
{
    [ApiController]
    [Route("image")]
    public class ImageController : ControllerBase
    {
        private readonly ILogger<ImageController> _logger;
        private readonly DaprClient _daprClient;

        public ImageController(DaprClient daprClient, ILogger<ImageController> logger)
        {
            _logger = logger;
            _daprClient = daprClient;
        }

        [Topic("pubsub", "image.requested")]
        public async Task<IActionResult> HandleImageRequest([FromBody] ImageRequest request)
        {
            _logger.LogInformation($"Received image request for UserId: {request.UserId}, MGRS: {request.Mgrs}");
            try
            {
                await _daprClient.PublishEventAsync("pubsub", "image.available", "Dette er et satelitbillede");
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
