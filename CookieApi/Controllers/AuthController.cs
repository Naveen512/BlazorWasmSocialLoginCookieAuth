using System.Security.Claims;
using CookieApi.Data;
using CookieApi.Data.Entities;
using CookieApi.Dtos;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authentication.Twitter;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CookieApi.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly CookieAuthContext _cookieAuthContext;
    public AuthController(CookieAuthContext cookieAuthContext)
    {
        _cookieAuthContext = cookieAuthContext;
    }

    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> LoginAsync(LoginDto login)
    {
        var user = await _cookieAuthContext
        .User.Where(_ => _.Email.ToLower() == login.Email.ToLower() &&
        _.Password == login.Password && _.ExternalLoginName == null)
        .FirstOrDefaultAsync();
        if (user == null)
        {
            return BadRequest("Invalid Credentials");
        }

        var claims = new List<Claim>
        {
            new Claim("userid", user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email)
        };

        var claimsIdentity = new ClaimsIdentity(
            claims, CookieAuthenticationDefaults.AuthenticationScheme);

        var authProperties = new AuthenticationProperties();

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);
        return Ok("Success");
    }

    [HttpPost]
    [Route("logout")]
    public async Task<IActionResult> LogoutAsync()
    {
        await HttpContext.SignOutAsync();
        return Ok("success");
    }

    [Authorize]
    [HttpGet]
    [Route("user-profile")]
    public async Task<IActionResult> UserProfileAsync()
    {
        int userId = HttpContext.User.Claims
        .Where(_ => _.Type == "userid")
        .Select(_ => Convert.ToInt32(_.Value))
        .First();

        var userProfile = await _cookieAuthContext
        .User
        .Where(_ => _.Id == userId)
        .Select(_ => new UserProfileDto
        {
            UserId = _.Id,
            Email = _.Email,
            FirstName = _.FirstName,
            LastName = _.LastName
        }).FirstOrDefaultAsync();

        return Ok(userProfile);
    }

    [HttpGet]
    [Route("facebook-login")]
    public IActionResult FacebookLogin(string returnURL)
    {
        return Challenge(
            new AuthenticationProperties()
            {
                RedirectUri = Url.Action(nameof(FacebookLoginCallback), new { returnURL })
            },
            FacebookDefaults.AuthenticationScheme
        );
    }

    [HttpGet]
    [Route("facebook-login-callback")]
    public async Task<IActionResult> FacebookLoginCallback(string returnURL)
    {
        var authenticationResult = await HttpContext
        .AuthenticateAsync(FacebookDefaults.AuthenticationScheme);
        if (authenticationResult.Succeeded)
        {
            string email = HttpContext
            .User.Claims.Where(_ => _.Type == ClaimTypes.Email)
            .Select(_ => _.Value)
            .FirstOrDefault();

            string firstName = HttpContext
            .User.Claims.Where(_ => _.Type == ClaimTypes.Name)
            .Select(_ => _.Value)
            .FirstOrDefault();

            string lastName = HttpContext
            .User.Claims.Where(_ => _.Type == ClaimTypes.Surname)
            .Select(_ => _.Value)
            .FirstOrDefault();

            var user = await ManageExternalLoginUser(
                email,
                firstName,
                lastName,
                "Facebook"
            );

            await RefreshExternalSignIn(user);
            return Redirect($"{returnURL}?externalauth=true");
        }
        return Redirect($"{returnURL}?externalauth=false");
    }

    private async Task<User> ManageExternalLoginUser(string email,
    string firstName,
    string lastName,
    string externalLoginName)
    {
        var user = await _cookieAuthContext
        .User.Where(_ => _.Email.ToLower() == email.ToLower()
        && _.ExternalLoginName.ToLower() == externalLoginName.ToLower())
        .FirstOrDefaultAsync();

        if (user != null)
        {
            return user;
        }

        var newUser = new User
        {
            Email = email,
            ExternalLoginName = externalLoginName,
            FirstName = firstName,
            LastName = lastName
        };
        _cookieAuthContext.User.Add(newUser);
        await _cookieAuthContext.SaveChangesAsync();
        return newUser;
    }

    private async Task RefreshExternalSignIn(User user)
    {
        var claims = new List<Claim>
        {
            new Claim("userid", user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email)
        };

        var claimsIdentity = new ClaimsIdentity(
            claims, CookieAuthenticationDefaults.AuthenticationScheme);

        var authProperties = new AuthenticationProperties();

        HttpContext.User.AddIdentity(claimsIdentity);

        await HttpContext.SignOutAsync();

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);
    }

    [HttpGet]
    [Route("twitter-login")]
    public IActionResult TwitterLogin(string returnURL)
    {
        return Challenge(
            new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(TwitterLoginCallback), new { returnURL })
            },
            TwitterDefaults.AuthenticationScheme
        );
    }

    [HttpGet]
    [Route("twitter-login-callback")]
    public async Task<IActionResult> TwitterLoginCallback(string returnURL)
    {
        var authenticationResult = await HttpContext
        .AuthenticateAsync(TwitterDefaults.AuthenticationScheme);
        if (authenticationResult.Succeeded)
        {
            string email = HttpContext
            .User.Claims.Where(_ => _.Type == ClaimTypes.Email)
            .Select(_ => _.Value)
            .FirstOrDefault();

            string firstName = HttpContext
            .User.Claims.Where(_ => _.Type == ClaimTypes.Name)
            .Select(_ => _.Value)
            .FirstOrDefault();

            var user = await ManageExternalLoginUser(
                email,
                firstName,
                string.Empty,
                "Twitter"
            );

            await RefreshExternalSignIn(user);
            return Redirect($"{returnURL}?externalauth=true");
        }
        return Redirect($"{returnURL}?externalauth=false");
    }

    [HttpGet]
    [Route("microsoft-account-login")]
    public IActionResult MicrosoftAccountLogin(string returnURL)
    {
        return Challenge(
            new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(MicrosoftAccountLoginCallBack), new { returnURL })
            },
            MicrosoftAccountDefaults.AuthenticationScheme
        );
    }

    [HttpGet]
    [Route("microsoft-account-login-callback")]
    public async Task<IActionResult> MicrosoftAccountLoginCallBack(string returnURL)
    {
        var authenticationResult = await HttpContext
        .AuthenticateAsync(MicrosoftAccountDefaults.AuthenticationScheme);
        if (authenticationResult.Succeeded)
        {
            string email = HttpContext
            .User.Claims.Where(_ => _.Type == ClaimTypes.Email)
            .Select(_ => _.Value)
            .FirstOrDefault();

            string firstName = HttpContext
            .User.Claims.Where(_ => _.Type == ClaimTypes.GivenName)
            .Select(_ => _.Value)
            .FirstOrDefault();

            string lastName = HttpContext
            .User.Claims.Where(_ => _.Type == ClaimTypes.Surname)
            .Select(_ => _.Value)
            .FirstOrDefault();

            var user = await ManageExternalLoginUser(
                email,
                firstName,
                lastName,
                "Microsoft Account"
            );

            await RefreshExternalSignIn(user);
            return Redirect($"{returnURL}?externalauth=true");
        }
        return Redirect($"{returnURL}?externalauth=false");
    }

    [HttpGet]
    [Route("google-login")]
    public IActionResult GoogleLogin(string returnURL)
    {
        return Challenge(
            new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(GoogleLoginCallBack), new { returnURL })
            },
            GoogleDefaults.AuthenticationScheme
        );
    }

    [HttpGet]
    [Route("google-login-callback")]
    public async Task<IActionResult> GoogleLoginCallBack(string returnURL)
    {
        var authenticationResult = await HttpContext
        .AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
        if (authenticationResult.Succeeded)
        {
            string email = HttpContext
            .User.Claims.Where(_ => _.Type == ClaimTypes.Email)
            .Select(_ => _.Value)
            .FirstOrDefault();

            string firstName = HttpContext
            .User.Claims.Where(_ => _.Type == ClaimTypes.GivenName)
            .Select(_ => _.Value)
            .FirstOrDefault();

            string lastName = HttpContext
            .User.Claims.Where(_ => _.Type == ClaimTypes.Surname)
            .Select(_ => _.Value)
            .FirstOrDefault();

            var user = await ManageExternalLoginUser(
                email,
                firstName,
                lastName,
                "Google"
            );

            await RefreshExternalSignIn(user);
            return Redirect($"{returnURL}?externalauth=true");
        }
        return Redirect($"{returnURL}?externalauth=false");
    }
}