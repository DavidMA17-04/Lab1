using LibraryService.Api.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LibraryService.Api.Features.Libraries.Shared;

public interface ILibrariesService
{
    Task<IEnumerable<Library>> Get(int[]? ids);
    Task<Library> Add(Library library);
    Task<Library> Update(Library library);
    Task<bool> Delete(Library library);
}

public class LibrariesService : ILibrariesService
{
    private readonly LibraryContext _context;

    public LibrariesService(LibraryContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Library>> Get(int[]? ids)
    {
        var query = _context.Libraries.AsQueryable();
        if (ids != null && ids.Any())
            query = query.Where(x => ids.Contains(x.Id));
        return await query.ToListAsync();
    }

    public async Task<Library> Add(Library library)
    {
        await _context.Libraries.AddAsync(library);
        await _context.SaveChangesAsync();
        return library;
    }

    public async Task<Library> Update(Library library)
    {
        var existing = await _context.Libraries.SingleAsync(x => x.Id == library.Id);
        existing.Name = library.Name;
        existing.Location = library.Location;
        _context.Libraries.Update(existing);
        await _context.SaveChangesAsync();
        return library;
    }

    public async Task<bool> Delete(Library library)
    {
        var existing = await _context.Libraries.SingleOrDefaultAsync(x => x.Id == library.Id);
        if (existing == null)
            return false;

        _context.Libraries.Remove(existing);
        await _context.SaveChangesAsync();
        return true;
    }
}
