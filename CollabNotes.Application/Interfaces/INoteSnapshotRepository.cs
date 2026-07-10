using CollabNotes.Domain.Entities;

namespace CollabNotes.Application.Interfaces;

public interface INoteSnapshotRepository
{
    Task<NoteSnapshot?> GetLatestAsync(Guid noteId);
    Task AddAsync(NoteSnapshot snapshot);
    Task<IEnumerable<NoteSnapshot>> GetByNoteAsync(Guid noteId);
    Task<NoteSnapshot?> GetByIdAsync(Guid noteId, Guid snapshotId);
}
