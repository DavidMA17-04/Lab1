using LibraryService.Api.Common.Persistence;
using LibraryService.Api.Features.Libraries.CreateLibrary;
using Microsoft.AspNetCore.Mvc;

namespace LibraryService.Api.Features.Libraries;

public partial class LibrariesController
{
    [HttpPost]
    public async Task<IActionResult> Add(LibraryForm form)
    {
        var library = new Library
        {
            Id = form.Id,
            Name = form.Name,
            Location = form.Location
        };
        await _librariesService.Add(library);
        return Ok(library);
    }
}
