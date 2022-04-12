using System.Security.Claims;
using Bwasm.Cookie.Models;
using Microsoft.AspNetCore.Components.Authorization;

namespace Bwasm.Cookie.Providers;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return new AuthenticationState(claimsPrincipal);
    }

    public void SetAuthInfo(UserProfileModel userProfile)
    {
        var identity = new ClaimsIdentity(new[]{
            new Claim(ClaimTypes.Email, userProfile.Email),
            new Claim(ClaimTypes.Name, $"{userProfile.FirstName} {userProfile.LastName}"),
            new Claim("UserId", userProfile.ToString())
        }, "AuthCookie");

        claimsPrincipal = new ClaimsPrincipal(identity);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public void ClearAuthInfo()
    {
        claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}