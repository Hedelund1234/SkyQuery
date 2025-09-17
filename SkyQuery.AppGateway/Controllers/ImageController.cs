using Dapr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkyQuery.AppGateway.Application.Interfaces;
using SkyQuery.AppGateway.Domain.Entities;

namespace SkyQuery.AppGateway.Controllers
{
    [Route("images")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        private readonly IImageService _imageService;
        private readonly ILogger<ImageController> _logger;
        private readonly IImageStore _imageStore;
        public ImageController(IImageService imageService, ILogger<ImageController> logger, IImageStore imageStore)
        {
            _imageService = imageService;
            _logger = logger;
            _imageStore = imageStore;
        }

        [HttpPost("image")]
        //[Authorize(Roles = "operator")] // Can toggle auth on/off by outcommenting
        public IActionResult PostImageRequest([FromBody] ImageRequest request)
        {
            try
            {
                _imageService.GetNewImage(request);
                return Ok("Image request received.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost("available")]
        //[Topic("pubsub", "image.available")]
        public async Task<IActionResult> HandleReceivedImage(ImageAvailable imageAvailble)
        {
            _logger.LogInformation($"Received final image");
            await _imageStore.PutAsync(imageAvailble);
            return Ok();
        }
    }
}
