using LibraryService.Application.Interfaces;
using LibraryService.Domain.Entities;

namespace LibraryService.Application.Services;

public class BooksService : IBooksService
{
    private readonly IBookRepository _bookRepository;

    public BooksService(IBookRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }

    public Task<IEnumerable<Book>> Get(int libraryId, int[]? ids) => _bookRepository.Get(libraryId, ids);

    public Task<Book> Add(Book book) => _bookRepository.Add(book);

    public Task<Book> Update(Book book) => _bookRepository.Update(book);

    public Task<bool> Delete(Book book) => _bookRepository.Delete(book);
}
