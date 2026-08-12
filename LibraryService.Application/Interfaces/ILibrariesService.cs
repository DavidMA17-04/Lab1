using LibraryService.Domain.Entities;

namespace LibraryService.Application.Interfaces;

public interface ILibrariesService
{
    Task<IEnumerable<Library>> Get(int[]? ids);
    Task<Library> Add(Library library);
    Task<Library> Update(Library library);
    Task<bool> Delete(Library library);
}
