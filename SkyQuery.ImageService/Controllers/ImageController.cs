using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using SkyQuery.ImageService.Application.Interfaces;
using SkyQuery.ImageService.Domain.Entities;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace SkyQuery.ImageService.Controllers
{
    [ApiController]
    [Route("imageservice")]
    public class ImageController : ControllerBase
    {
        private readonly ILogger<ImageController> _logger;
        private readonly DaprClient _daprClient;
        private readonly IDataforsyningService _dataforsyningService;
        private readonly IActorTokenValidator _actorTokenValidator;

        public ImageController(DaprClient daprClient, ILogger<ImageController> logger, IDataforsyningService dataforsyningService, IActorTokenValidator actorTokenValidator)
        {
            _logger = logger;
            _daprClient = daprClient;
            _dataforsyningService = dataforsyningService;
            _actorTokenValidator = actorTokenValidator;
        }

        [Topic("pubsub", "image.requested")]
        public async Task<IActionResult> HandleImageRequest([FromBody] ImageRequestWithToken requestWithToken)
        {
            string roleNeeded = "operator";
            _logger.LogInformation($"Received image request for UserId: {requestWithToken.UserId}, MGRS: {requestWithToken.Mgrs}");
            try
            {
                //Validation of token
                var principal = _actorTokenValidator.Validate(requestWithToken.JWT);
                if (principal is null)
                {
                    _logger.LogInformation($"[INCORRECT JWT] Unauthorized access blocked for user: {requestWithToken.UserId} -- Send to DLQ");
                    await _daprClient.PublishEventAsync("pubsub", "image.requested.dlq", requestWithToken);
                    return Ok();
                }

                var isOperator = principal.Claims.Any(c =>
                    (c.Type == ClaimTypes.Role || c.Type == "role" || c.Type == "roles") &&
                    c.Value.Equals(roleNeeded, StringComparison.OrdinalIgnoreCase));

                if (!isOperator)
                {
                    _logger.LogInformation($"[INCORRECT ROLE] Unauthorized access blocked for user: {requestWithToken.UserId} -- Send to DLQ");
                    await _daprClient.PublishEventAsync("pubsub", "image.requested.dlq", requestWithToken);
                    return Ok();
                }

                var requestWithoutToken = new ImageRequest { UserId = requestWithToken.UserId, Mgrs = requestWithToken.Mgrs };
                ImageAvailable result = await _dataforsyningService.GetMapFromDFAsync(requestWithoutToken);

                // 1) Create request body without body
                var request = _daprClient.CreateInvokeMethodRequest(
                    HttpMethod.Post,
                    "skyquery-appgateway-dapr",
                    "images/available");

                // 2) Add body as json
                request.Content = JsonContent.Create(result); // Content-Type: application/json

                // 3) Adds Authorization: Beaerer <token>
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", requestWithToken.JWT); // app.UseHttpsRedirection(); on recieving end (AuthService) has to be disabled in development environment

                var response = await _daprClient.InvokeMethodWithResponseAsync(request);

                if (response.StatusCode is HttpStatusCode.Forbidden)
                {
                    _logger.LogInformation($"URGENT(Potentiel security risk) Image requested for user: {requestWithToken.UserId} for picture MGRS: {requestWithToken.Mgrs} might be comprimised ---- Request moved to DLQ");
                    await _daprClient.PublishEventAsync("pubsub", "image.requested.dlq", requestWithToken);
                    return Ok();
                }

                // Sends with service invocation
                //await _daprClient.InvokeMethodAsync<ImageAvailable>(
                //   HttpMethod.Post,
                //   "skyquery-appgateway-dapr",
                //   "images/available",
                //   result);

                //await _daprClient.PublishEventAsync("pubsub", "image.available", result);
                _logger.LogInformation($"Send image for UserId: {requestWithToken.UserId}, MGRS: {requestWithToken.Mgrs}");
                return Ok();
            }
            catch (InvalidDataException ex)
            {
                await _daprClient.PublishEventAsync("pubsub", "image.requested.dlq", requestWithToken);
                _logger.LogError(ex, "Error in request (moved to DLQ)");
                return Ok();
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error publishing image available event");
                _logger.LogError(ex, "Error using service invocation image available event");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
