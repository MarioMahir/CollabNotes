using CollabNotes.Application.Dtos;

namespace CollabNotes.Application.Interfaces;

public class ViewerLeftInfo
{
    public Guid NoteId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}

public interface IPresenceTracker
{
    IReadOnlyList<ViewerDto> AddViewer(Guid noteId, string connectionId, string userId, string displayName);
    ViewerLeftInfo? RemoveViewer(string connectionId);
    IReadOnlyList<ViewerDto> GetViewers(Guid noteId);
}
