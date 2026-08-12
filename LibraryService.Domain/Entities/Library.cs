namespace LibraryService.Domain.Entities;

public class Library
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Location { get; set; } = null!;
}
