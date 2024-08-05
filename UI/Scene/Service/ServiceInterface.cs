using System;
using System.Threading.Tasks;

public interface IUserService
{
    string UserAccount { get; set; }
    string AccessToken { get; }
    string RefreshToken { get; }
}

public interface IWebService
{
    Task<T> SendWebRequestAsync<T>(string url, string method, object obj);
    Task SendWebRequest<T>(string url, string method, object obj, Action<T> responseAction);
}

public interface ITokenService
{
    void SaveAccessToken(string accessToken);
    void SaveRefreshToken(string refreshToken);
    string GetAccessToken();
    string GetRefreshToken();
}
