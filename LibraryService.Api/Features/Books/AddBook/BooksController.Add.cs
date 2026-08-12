using LibraryService.Api.Common.Persistence;
using Microsoft.AspNetCore.Mvc;

namespace LibraryService.Api.Features.Books;

public partial class BooksController
{
    [HttpPost]
    public async Task<IActionResult> Add(int libraryId, BookForm form)
    {
        var library = (await _librariesService.Get(new[] { libraryId })).FirstOrDefault();
        if (library == null)
            return NotFound();

        var book = new Book
        {
            Id = form.Id,
            Name = form.Name,
            Category = form.Category ?? string.Empty,
            LibraryId = libraryId
        };
        await _booksService.Add(book);
        return StatusCode(StatusCodes.Status201Created, book);
    }
}
