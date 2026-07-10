using CollabNotes.Application.Dtos;

namespace CollabNotes.Application.Interfaces;

public interface ICollaborationService
{
    Task<bool> CanAccessNoteAsync(Guid noteId, string userId);
    Task<IReadOnlyList<ViewerDto>> RegisterViewerAsync(Guid noteId, string connectionId, string userId, string displayName);
    ViewerLeftInfo? UnregisterViewer(string connectionId);
    Task<BlockUpdateDto> UpdateBlockAsync(Guid noteId, int blockIndex, string content, string userId);
}
