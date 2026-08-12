using LibraryService.Application.DTOs;
using LibraryService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryService.WebAPI.Controllers;

public record TokenResponse(string token);

[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ITokenService _tokenService;

    public AuthController(
        IAuthenticationService authenticationService,
        ITokenService tokenService)
    {
        _authenticationService = authenticationService;
        _tokenService = tokenService;
    }

    [HttpPost("/login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(User user)
    {
        var validUser = await _authenticationService.AuthenticateAsync(user.Email, user.Password);
        if (validUser is null)
            return Unauthorized();

        var token = _tokenService.GenerateToken(validUser);
        return Ok(new TokenResponse(token));
    }
}
