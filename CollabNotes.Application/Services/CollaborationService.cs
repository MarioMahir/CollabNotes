using CollabNotes.Application.Dtos;
using CollabNotes.Application.Interfaces;

namespace CollabNotes.Application.Services;

public class CollaborationService : ICollaborationService
{
    private readonly INoteService _noteService;
    private readonly IPresenceTracker _presenceTracker;

    public CollaborationService(INoteService noteService, IPresenceTracker presenceTracker)
    {
        _noteService = noteService;
        _presenceTracker = presenceTracker;
    }

    public async Task<bool> CanAccessNoteAsync(Guid noteId, string userId)
        => await _noteService.GetByIdAsync(noteId, userId) is not null;

    public Task<IReadOnlyList<ViewerDto>> RegisterViewerAsync(Guid noteId, string connectionId, string userId, string displayName)
        => Task.FromResult(_presenceTracker.AddViewer(noteId, connectionId, userId, displayName));

    public ViewerLeftInfo? UnregisterViewer(string connectionId)
        => _presenceTracker.RemoveViewer(connectionId);

    public Task<BlockUpdateDto> UpdateBlockAsync(Guid noteId, int blockIndex, string content, string userId)
        => _noteService.UpdateBlockAsync(noteId, blockIndex, content, userId);
}
