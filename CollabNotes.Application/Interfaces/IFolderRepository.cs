using CollabNotes.Domain.Entities;

namespace CollabNotes.Application.Interfaces;

public interface IFolderRepository
{
    Task<IEnumerable<Folder>> GetByOwnerAsync(string ownerId);
    Task AddAsync(Folder folder);
    Task SaveChangesAsync();
}
