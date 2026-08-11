using LibraryService.BusinessLogic.Interfaces;
using LibraryService.Entities.DTO;
using LibraryService.Entities.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryService.Api.Controllers;

public record TokenResponse(string token);

[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ITokenService _tokenService;
    private readonly JwtSettings _jwtSettings;

    public AuthController(
        IAuthenticationService authenticationService,
        ITokenService tokenService,
        JwtSettings jwtSettings)
    {
        _authenticationService = authenticationService;
        _tokenService = tokenService;
        _jwtSettings = jwtSettings;
    }

    [HttpPost("/login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(User user)
    {
        var validUser = await _authenticationService.AuthenticateAsync(user.Email, user.Password);
        if (validUser is null)
            return Unauthorized();

        var token = _tokenService.GenerateToken(validUser, _jwtSettings);
        return Ok(new TokenResponse(token));
    }
}
