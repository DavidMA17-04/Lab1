using Microsoft.AspNetCore.Mvc;

namespace LibraryService.Api.Features.Books;

public partial class BooksController
{
    [HttpDelete("{bookId}")]
    public async Task<IActionResult> Delete(int libraryId, int bookId)
    {
        var library = (await _librariesService.Get(new[] { libraryId })).FirstOrDefault();
        if (library == null)
            return NotFound();

        var existing = (await _booksService.Get(libraryId, new[] { bookId })).FirstOrDefault();
        if (existing == null)
            return NotFound();

        await _booksService.Delete(existing);
        return NoContent();
    }
}
