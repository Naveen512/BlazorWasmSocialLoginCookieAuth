using Bwasm.Cookie.Models;

namespace Bwasm.Cookie.Logics;

public interface IApiLogic
{
    Task<string> LoginAsync(LoginModel login);
    Task<(string Message, UserProfileModel? UserProfile)> UserProfileAsync();
    Task<string> LogoutAsync();
}