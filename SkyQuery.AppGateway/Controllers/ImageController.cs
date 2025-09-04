using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SkyQuery.AppGateway.Entities;
using SkyQuery.AppGateway.Interfaces;

namespace SkyQuery.AppGateway.Controllers
{
    [Route("entry")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        private readonly IImageService _imageService;
        public ImageController(IImageService imageService)
        {
            _imageService = imageService;
        }

        [HttpPost("image")]
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
    }
}
