namespace CollabNotes.Domain.Entities;

public class Note
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string OwnerId { get; set; } = string.Empty;
    public Guid? FolderId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    public Folder? Folder { get; set; }
    public ICollection<NotePermission> Permissions { get; set; } = new List<NotePermission>();
}
