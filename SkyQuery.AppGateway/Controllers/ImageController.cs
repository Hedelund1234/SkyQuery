using Microsoft.AspNetCore.Mvc;
using SkyQuery.AppGateway.Application.Interfaces;
using SkyQuery.AppGateway.Domain.Entities;
using Microsoft.AspNetCore.Authorization;

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
        [Authorize(Roles = "operator")] // Can toggle auth on/off by outcommenting
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

        [HttpPost("available")] // Used for receiving images from ImageService
        //[Topic("pubsub", "image.available")]
        public async Task<IActionResult> HandleReceivedImage(ImageAvailable imageAvailble)
        {
            _logger.LogInformation("Received final image {imageAvailable.Mgrs} requested from {imageAvailable.UserId}", imageAvailble.Mgrs ,imageAvailble.UserId);
            await _imageStore.PutAsync(imageAvailble);
            return Ok();
        }

        [HttpGet("ready/{userId}")]
        [Authorize(Roles = "operator")] // Can toggle auth on/off by outcommenting
        public List<string> CheckForImages(Guid userId)
        {
            List<string> result;
            try
            {
                result = _imageStore.CheckReadiness(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError("Something went wront while checking images for {userId}", userId);
                throw new Exception(ex.Message);
            }

            _logger.LogInformation("Checked if any images avaialble for {userId}", userId);
            return result;
        }

        [HttpPost("get")]
        [Authorize(Roles = "operator")] // Can toggle auth on/off by outcommenting
        public async Task<IActionResult> GetImage([FromBody] ImageRequest request)
        {
            if (request.UserId == Guid.Empty)
            {
                return BadRequest("Invalid userId");
            }
                
            if (string.IsNullOrWhiteSpace(request.Mgrs))
            {
                return BadRequest("Mgrs is required");
            }

            
            try
            {
                var result = await _imageStore.GetAsync(request.UserId, request.Mgrs);
                var contentType = result.ContentType?.Trim().ToLowerInvariant() switch
                {
                    "png" => "image/png",
                    _ => "application/octet-stream"
                };
                _logger.LogInformation("Picture was received by {request.userId}", request.UserId);
                return File(result.Image, contentType);
            }

            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (FileNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get image for {UserId} / {Mgrs}", request.UserId, request.Mgrs);
                return StatusCode(500, "Unexpected error");
            }
        }
    }
}
