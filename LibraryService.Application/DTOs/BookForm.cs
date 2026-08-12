namespace LibraryService.Application.DTOs;

public class BookForm
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Category { get; set; }
    public int LibraryId { get; set; }
}
