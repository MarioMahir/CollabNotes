namespace CollabNotes.Application.Interfaces;

public interface ICollaborationService
{
    Task<bool> CanAccessNoteAsync(Guid noteId, string userId);
}
