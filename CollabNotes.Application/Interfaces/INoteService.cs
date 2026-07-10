using CollabNotes.Application.Dtos;

namespace CollabNotes.Application.Interfaces;

public interface INoteService
{
    Task<IEnumerable<NoteDto>> GetNotesByFolderAsync(Guid? folderId, string userId);
    Task<NoteDto?> GetByIdAsync(Guid noteId, string userId);
    Task<NoteDto> CreateAsync(string title, Guid? folderId, string userId);
    Task UpdateAsync(Guid noteId, string content, string userId);
    Task DeleteAsync(Guid noteId, string userId);
}
