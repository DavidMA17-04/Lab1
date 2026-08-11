namespace LibraryService.Entities.Models;

public class Book
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int LibraryId { get; set; }
    public virtual Library? Library { get; set; }
}
