using LibraryService.Application.DTOs;
using LibraryService.Application.Interfaces;
using LibraryService.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace LibraryService.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LibrariesController : ControllerBase
{
    private readonly ILibrariesService _librariesService;

    public LibrariesController(ILibrariesService librariesService)
    {
        _librariesService = librariesService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var libraries = await _librariesService.Get(null);
        return Ok(libraries);
    }

    [HttpGet("{libraryId}")]
    public async Task<IActionResult> Get(int libraryId)
    {
        var library = (await _librariesService.Get(new[] { libraryId })).FirstOrDefault();
        if (library == null)
            return NotFound();
        return Ok(library);
    }

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
