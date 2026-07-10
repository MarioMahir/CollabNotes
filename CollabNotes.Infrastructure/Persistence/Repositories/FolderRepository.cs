using CollabNotes.Application.Interfaces;
using CollabNotes.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CollabNotes.Infrastructure.Persistence.Repositories;

public class FolderRepository : IFolderRepository
{
    private readonly AppDbContext _context;

    public FolderRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Folder>> GetByOwnerAsync(string ownerId)
        => await _context.Folders
            .Where(f => f.OwnerId == ownerId)
            .OrderBy(f => f.Name)
            .ToListAsync();

    public async Task AddAsync(Folder folder) => await _context.Folders.AddAsync(folder);

    public Task SaveChangesAsync() => _context.SaveChangesAsync();
}
