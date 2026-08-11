using LibraryService.Entities.DTO;
using LibraryService.Entities.Settings;

namespace LibraryService.BusinessLogic.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user, JwtSettings jwtSettings);
}
