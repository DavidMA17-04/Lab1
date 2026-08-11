using LibraryService.Entities.Models;

namespace LibraryService.DataAccess.Repositories;

public interface IBookRepository
{
    Task<IEnumerable<Book>> Get(int libraryId, int[]? ids);
    Task<Book> Add(Book book);
    Task<Book> Update(Book book);
    Task<bool> Delete(Book book);
}
