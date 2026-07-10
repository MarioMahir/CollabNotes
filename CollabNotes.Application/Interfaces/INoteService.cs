using CollabNotes.Application.Dtos;
using CollabNotes.Domain.Enums;

namespace CollabNotes.Application.Interfaces;

public interface INoteService
{
    Task<IEnumerable<NoteDto>> GetNotesByFolderAsync(Guid? folderId, string userId);
    Task<NoteDto?> GetByIdAsync(Guid noteId, string userId);
    Task<NoteDto> CreateAsync(string title, Guid? folderId, string userId);
    Task UpdateAsync(Guid noteId, string title, string content, string userId);
    Task DeleteAsync(Guid noteId, string userId);
    Task InviteUserAsync(Guid noteId, string inviterUserId, string emailOrUsername, PermissionRole role);
    Task<IEnumerable<NotePermissionDto>> GetMembersAsync(Guid noteId, string requestingUserId);
    Task<BlockUpdateDto> UpdateBlockAsync(Guid noteId, int blockIndex, string blockContent, string userId);
    Task<IEnumerable<NoteSnapshotDto>> GetSnapshotsAsync(Guid noteId, string userId);
    Task<NoteSnapshotDto?> GetSnapshotAsync(Guid noteId, Guid snapshotId, string userId);
}
