using LibraryService.Api.Common.Persistence;
using Microsoft.AspNetCore.Mvc;

namespace LibraryService.Api.Features.Books;

public partial class BooksController
{
    [HttpPut("{bookId}")]
    public async Task<IActionResult> Update(int libraryId, int bookId, BookForm form)
    {
        var library = (await _librariesService.Get(new[] { libraryId })).FirstOrDefault();
        if (library == null)
            return NotFound();

        var existing = (await _booksService.Get(libraryId, new[] { bookId })).FirstOrDefault();
        if (existing == null)
            return NotFound();

        var book = new Book
        {
            Id = bookId,
            Name = form.Name,
            Category = form.Category ?? string.Empty,
            LibraryId = libraryId
        };
        await _booksService.Update(book);
        return NoContent();
    }
}
