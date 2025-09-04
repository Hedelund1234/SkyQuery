using SkyQuery.AppGateway.Entities;

namespace SkyQuery.AppGateway.Interfaces
{
    public interface IImageService
    {
        Task GetNewImage(ImageRequest request);
    }
}
