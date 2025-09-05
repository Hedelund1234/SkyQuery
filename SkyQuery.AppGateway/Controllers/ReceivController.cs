using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SkyQuery.AppGateway.Controllers
{
    [Route("receiver")]
    [ApiController]
    public class ReceivController : ControllerBase
    {
        private readonly ILogger<ReceivController> _logger;
        private readonly DaprClient _daprClient;

        public ReceivController(DaprClient daprClient, ILogger<ReceivController> logger)
        {
            _logger = logger;
            _daprClient = daprClient;
        }

        [Topic("pubsub", "image.available")]
        public async Task<IActionResult> HandleReceivedImage(object request)
        {
            _logger.LogInformation($"Received final image");
            return Ok();
        }
    }
}
