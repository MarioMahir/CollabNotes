using CollabNotes.Domain.Enums;

namespace CollabNotes.Application.Dtos;

public class NoteDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public Guid? FolderId { get; set; }
    public string OwnerId { get; set; } = string.Empty;
    public DateTime UpdatedAtUtc { get; set; }
    public PermissionRole ViewerRole { get; set; }
}
