using Microsoft.AspNetCore.Mvc;

namespace LibraryService.Api.Features.Libraries;

public partial class LibrariesController
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var libraries = await _librariesService.Get(null);
        return Ok(libraries);
    }
}
