using CollabNotes.Domain.Enums;

namespace CollabNotes.Domain.Entities;

public class NotePermission
{
    public Guid Id { get; set; }
    public Guid NoteId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public PermissionRole Role { get; set; }

    public Note? Note { get; set; }
}
