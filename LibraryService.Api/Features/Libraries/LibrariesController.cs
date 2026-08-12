using LibraryService.Api.Features.Libraries.Shared;
using Microsoft.AspNetCore.Mvc;

namespace LibraryService.Api.Features.Libraries;

[ApiController]
[Route("api/[controller]")]
public partial class LibrariesController : ControllerBase
{
    private readonly ILibrariesService _librariesService;

    public LibrariesController(ILibrariesService librariesService)
    {
        _librariesService = librariesService;
    }
}
