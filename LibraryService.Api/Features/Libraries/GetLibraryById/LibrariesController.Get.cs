using Microsoft.AspNetCore.Mvc;

namespace LibraryService.Api.Features.Libraries;

public partial class LibrariesController
{
    [HttpGet("{libraryId}")]
    public async Task<IActionResult> Get(int libraryId)
    {
        var library = (await _librariesService.Get(new[] { libraryId })).FirstOrDefault();
        if (library == null)
            return NotFound();
        return Ok(library);
    }
}
