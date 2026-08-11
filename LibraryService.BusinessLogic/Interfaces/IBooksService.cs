using LibraryService.Entities.Models;

namespace LibraryService.BusinessLogic.Interfaces;

public interface IBooksService
{
    Task<IEnumerable<Book>> Get(int libraryId, int[]? ids);
    Task<Book> Add(Book book);
    Task<Book> Update(Book book);
    Task<bool> Delete(Book book);
}
