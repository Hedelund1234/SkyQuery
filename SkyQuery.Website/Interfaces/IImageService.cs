namespace SkyQuery.Website.Interfaces
{
    public interface IImageService
    {
        Task OrderImage(string userId, string mgrs);
        Task<byte[]> GetImageAsync(string userId, string mrgs);
    }
}
