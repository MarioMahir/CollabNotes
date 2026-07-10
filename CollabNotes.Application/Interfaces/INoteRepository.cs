using CollabNotes.Domain.Entities;

namespace CollabNotes.Application.Interfaces;

public interface INoteRepository
{
    Task<IEnumerable<Note>> GetByUserAsync(string userId, Guid? folderId);
    Task<Note?> GetByIdAsync(Guid noteId);
    Task<NotePermission?> GetPermissionAsync(Guid noteId, string userId);
    Task AddAsync(Note note);
    void Remove(Note note);
    Task SaveChangesAsync();
}
