using LibraryService.Entities.Models;

namespace LibraryService.BusinessLogic.Interfaces;

public interface ILibrariesService
{
    Task<IEnumerable<Library>> Get(int[]? ids);
    Task<Library> Add(Library library);
    Task<Library> Update(Library library);
    Task<bool> Delete(Library library);
}
