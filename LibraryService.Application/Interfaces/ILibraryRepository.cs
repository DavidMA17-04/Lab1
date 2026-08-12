using LibraryService.Domain.Entities;

namespace LibraryService.Application.Interfaces;

public interface ILibraryRepository
{
    Task<IEnumerable<Library>> Get(int[]? ids);
    Task<Library> Add(Library library);
    Task<IEnumerable<Library>> AddRange(IEnumerable<Library> libraries);
    Task<Library> Update(Library library);
    Task<bool> Delete(Library library);
    Task<bool> Exists(int id);
}
