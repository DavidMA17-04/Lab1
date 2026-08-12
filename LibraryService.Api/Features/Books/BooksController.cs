using LibraryService.Api.Features.Books.Shared;
using LibraryService.Api.Features.Libraries.Shared;
using Microsoft.AspNetCore.Mvc;

namespace LibraryService.Api.Features.Books;

[ApiController]
[Route("api/libraries/{libraryId}/[controller]")]
public partial class BooksController : ControllerBase
{
    private readonly ILibrariesService _librariesService;
    private readonly IBooksService _booksService;

    public BooksController(IBooksService booksService, ILibrariesService librariesService)
    {
        _librariesService = librariesService;
        _booksService = booksService;
    }
}
