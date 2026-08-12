using LibraryService.Api.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LibraryService.Api.Features.Books.Shared;

public interface IBooksService
{
    Task<IEnumerable<Book>> Get(int libraryId, int[]? ids);
    Task<Book> Add(Book book);
    Task<Book> Update(Book book);
    Task<bool> Delete(Book book);
}

public class BooksService : IBooksService
{
    private readonly LibraryContext _context;

    public BooksService(LibraryContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Book>> Get(int libraryId, int[]? ids)
    {
        var query = _context.Books.AsQueryable().Where(b => b.LibraryId == libraryId);
        if (ids != null && ids.Any())
            query = query.Where(b => ids.Contains(b.Id));
        return await query.ToListAsync();
    }

    public async Task<Book> Add(Book book)
    {
        await _context.Books.AddAsync(book);
        await _context.SaveChangesAsync();
        return book;
    }

    public async Task<Book> Update(Book book)
    {
        var existing = await _context.Books.SingleAsync(x => x.Id == book.Id);
        existing.Name = book.Name;
        existing.Category = book.Category;
        existing.LibraryId = book.LibraryId;
        _context.Books.Update(existing);
        await _context.SaveChangesAsync();
        return book;
    }

    public async Task<bool> Delete(Book book)
    {
        var existing = await _context.Books.SingleOrDefaultAsync(x => x.Id == book.Id);
        if (existing == null)
            return false;

        _context.Books.Remove(existing);
        await _context.SaveChangesAsync();
        return true;
    }
}
