namespace SkyQuery.Website.Interfaces
{
    public interface IAuthService
    {
        Task<string> Login(string email, string password);
    }
}
