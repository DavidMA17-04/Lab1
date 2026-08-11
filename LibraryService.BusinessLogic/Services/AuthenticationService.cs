using LibraryService.BusinessLogic.Interfaces;
using LibraryService.Entities.DTO;

namespace LibraryService.BusinessLogic.Services;

public class AuthenticationService : IAuthenticationService
{
    public Task<User?> AuthenticateAsync(string email, string password)
    {
        if (email == "admin" && password == "1234")
        {
            return Task.FromResult<User?>(new User
            {
                Id = 1,
                Email = email,
                Password = password,
                Role = "admin"
            });
        }

        return Task.FromResult<User?>(null);
    }
}
