using LibraryService.Application.Interfaces;
using LibraryService.Domain.Entities;

namespace LibraryService.Application.Services;

public class LibrariesService : ILibrariesService
{
    private readonly ILibraryRepository _libraryRepository;

    public LibrariesService(ILibraryRepository libraryRepository)
    {
        _libraryRepository = libraryRepository;
    }

    public Task<IEnumerable<Library>> Get(int[]? ids) => _libraryRepository.Get(ids);

    public Task<Library> Add(Library library) => _libraryRepository.Add(library);

    public Task<Library> Update(Library library) => _libraryRepository.Update(library);

    public Task<bool> Delete(Library library) => _libraryRepository.Delete(library);
}
