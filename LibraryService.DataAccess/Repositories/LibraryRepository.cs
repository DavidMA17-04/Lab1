using LibraryService.DataAccess.Data;
using LibraryService.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryService.DataAccess.Repositories;

public class LibraryRepository : ILibraryRepository
{
    private readonly LibraryContext _context;

    public LibraryRepository(LibraryContext context)
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

    public async Task<IEnumerable<Library>> AddRange(IEnumerable<Library> libraries)
    {
        await _context.Libraries.AddRangeAsync(libraries);
        await _context.SaveChangesAsync();
        return libraries;
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

    public async Task<bool> Exists(int id)
    {
        return await _context.Libraries.AnyAsync(x => x.Id == id);
    }
}
