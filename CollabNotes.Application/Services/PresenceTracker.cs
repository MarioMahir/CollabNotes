using System.Collections.Concurrent;
using CollabNotes.Application.Dtos;
using CollabNotes.Application.Interfaces;

namespace CollabNotes.Application.Services;

public class PresenceTracker : IPresenceTracker
{
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<string, ViewerDto>> _viewersByNote = new();
    private readonly ConcurrentDictionary<string, Guid> _noteByConnection = new();

    public IReadOnlyList<ViewerDto> AddViewer(Guid noteId, string connectionId, string userId, string displayName)
    {
        var viewers = _viewersByNote.GetOrAdd(noteId, _ => new ConcurrentDictionary<string, ViewerDto>());
        viewers[connectionId] = new ViewerDto
        {
            UserId = userId,
            DisplayName = displayName,
            ConnectionId = connectionId
        };
        _noteByConnection[connectionId] = noteId;

        return GetViewers(noteId);
    }

    public ViewerLeftInfo? RemoveViewer(string connectionId)
    {
        if (!_noteByConnection.TryRemove(connectionId, out var noteId))
        {
            return null;
        }

        if (!_viewersByNote.TryGetValue(noteId, out var viewers)
            || !viewers.TryRemove(connectionId, out var viewer))
        {
            return null;
        }

        return new ViewerLeftInfo
        {
            NoteId = noteId,
            UserId = viewer.UserId,
            DisplayName = viewer.DisplayName
        };
    }

    public IReadOnlyList<ViewerDto> GetViewers(Guid noteId)
        => _viewersByNote.TryGetValue(noteId, out var viewers)
            ? viewers.Values.ToList()
            : [];
}
