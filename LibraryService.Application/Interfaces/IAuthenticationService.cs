using LibraryService.Application.DTOs;

namespace LibraryService.Application.Interfaces;

public interface IAuthenticationService
{
    Task<User?> AuthenticateAsync(string email, string password);
}
