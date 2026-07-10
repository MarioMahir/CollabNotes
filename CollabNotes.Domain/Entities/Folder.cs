namespace CollabNotes.Domain.Entities;

public class Folder
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string OwnerId { get; set; } = string.Empty;

    public ICollection<Note> Notes { get; set; } = new List<Note>();
}
