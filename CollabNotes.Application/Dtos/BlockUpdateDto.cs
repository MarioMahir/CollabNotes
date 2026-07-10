namespace CollabNotes.Application.Dtos;

public class BlockUpdateDto
{
    public Guid NoteId { get; set; }
    public int BlockIndex { get; set; }
    public string Content { get; set; } = string.Empty;
    public string EditedByUserId { get; set; } = string.Empty;
    public DateTime UpdatedAtUtc { get; set; }
}
