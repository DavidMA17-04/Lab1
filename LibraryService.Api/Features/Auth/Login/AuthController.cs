using LibraryService.Api.Common.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryService.Api.Features.Auth.Login;

[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly JwtSettings _jwtSettings;

    public AuthController(IAuthenticationService authenticationService, JwtSettings jwtSettings)
    {
        _authenticationService = authenticationService;
        _jwtSettings = jwtSettings;
    }

    [HttpPost("/login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(User user)
    {
        var validUser = await _authenticationService.AuthenticateAsync(user.Email, user.Password);
        if (validUser is null)
            return Unauthorized();

        var token = TokenGenerator.GenerateToken(validUser, _jwtSettings);
        return Ok(new TokenResponse(token));
    }
}
