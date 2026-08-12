using LibraryService.Application.DTOs;

namespace LibraryService.Application.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
}
