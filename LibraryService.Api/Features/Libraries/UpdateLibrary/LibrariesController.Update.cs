using LibraryService.Api.Common.Persistence;
using LibraryService.Api.Features.Libraries.CreateLibrary;
using Microsoft.AspNetCore.Mvc;

namespace LibraryService.Api.Features.Libraries;

public partial class LibrariesController
{
    [HttpPut("{libraryId}")]
    public async Task<IActionResult> Update(int libraryId, LibraryForm form)
    {
        var existingLibrary = (await _librariesService.Get(new[] { libraryId })).FirstOrDefault();
        if (existingLibrary == null)
            return NotFound();

        var library = new Library
        {
            Id = libraryId,
            Name = form.Name,
            Location = form.Location
        };
        await _librariesService.Update(library);
        return NoContent();
    }
}
