namespace LibraryService.Domain.Entities;

public class Book
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Category { get; set; } = null!;
    public int LibraryId { get; set; }
    public virtual Library? Library { get; set; }
}
