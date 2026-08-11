using LibraryService.Entities.DTO;

namespace LibraryService.BusinessLogic.Interfaces;

public interface IAuthenticationService
{
    Task<User?> AuthenticateAsync(string email, string password);
}
