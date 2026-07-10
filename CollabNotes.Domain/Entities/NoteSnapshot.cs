namespace CollabNotes.Domain.Entities;

public class NoteSnapshot
{
    public Guid Id { get; set; }
    public Guid NoteId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }

    public Note? Note { get; set; }
}
