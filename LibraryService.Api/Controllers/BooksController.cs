using LibraryService.BusinessLogic.Interfaces;
using LibraryService.Entities.DTO;
using LibraryService.Entities.Models;
using Microsoft.AspNetCore.Mvc;

namespace LibraryService.Api.Controllers;

[ApiController]
[Route("api/libraries/{libraryId}/[controller]")]
public class BooksController : ControllerBase
{
    private readonly ILibrariesService _librariesService;
    private readonly IBooksService _booksService;

    public BooksController(IBooksService booksService, ILibrariesService librariesService)
    {
        _librariesService = librariesService;
        _booksService = booksService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(int libraryId)
    {
        var library = (await _librariesService.Get(new[] { libraryId })).FirstOrDefault();
        if (library == null)
            return NotFound();

        var books = await _booksService.Get(libraryId, null);
        return Ok(books);
    }

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
