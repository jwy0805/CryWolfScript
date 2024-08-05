public class UserService : IUserService
{
    public string UserAccount { get; set; }
    public string AccessToken => ServiceLocator.GetService<ITokenService>().GetAccessToken();
    public string RefreshToken => ServiceLocator.GetService<ITokenService>().GetRefreshToken();
}
