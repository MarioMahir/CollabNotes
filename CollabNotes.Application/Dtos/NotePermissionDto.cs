using CollabNotes.Domain.Enums;

namespace CollabNotes.Application.Dtos;

public class NotePermissionDto
{
    public string UserId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public PermissionRole Role { get; set; }
}
