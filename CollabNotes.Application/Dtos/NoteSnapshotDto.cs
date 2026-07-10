namespace CollabNotes.Application.Dtos;

public class NoteSnapshotDto
{
    public Guid Id { get; set; }
    public Guid NoteId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}
