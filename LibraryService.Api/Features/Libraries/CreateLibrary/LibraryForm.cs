namespace LibraryService.Api.Features.Libraries.CreateLibrary;

public class LibraryForm
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Location { get; set; } = null!;
}
