using Microsoft.AspNetCore.Mvc;

namespace LibraryService.Api.Features.Books;

public partial class BooksController
{
    [HttpGet]
    public async Task<IActionResult> GetAll(int libraryId)
    {
        var library = (await _librariesService.Get(new[] { libraryId })).FirstOrDefault();
        if (library == null)
            return NotFound();

        var books = await _booksService.Get(libraryId, null);
        return Ok(books);
    }
}
