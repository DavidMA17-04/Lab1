using Microsoft.AspNetCore.Mvc;

namespace LibraryService.Api.Features.Libraries;

public partial class LibrariesController
{
    [HttpDelete("{libraryId}")]
    public async Task<IActionResult> Delete(int libraryId)
    {
        var existingLibrary = (await _librariesService.Get(new[] { libraryId })).FirstOrDefault();
        if (existingLibrary == null)
            return NotFound();

        await _librariesService.Delete(existingLibrary);
        return NoContent();
    }
}
