using CollabNotes.Application.Interfaces;
using CollabNotes.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CollabNotes.Infrastructure.Persistence.Repositories;

public class NoteRepository : INoteRepository
{
    private readonly AppDbContext _context;

    public NoteRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Note>> GetByUserAsync(string userId, Guid? folderId)
    {
        var query = _context.Notes
            .Where(n => n.Permissions.Any(p => p.UserId == userId));

        if (folderId.HasValue)
        {
            query = query.Where(n => n.FolderId == folderId.Value);
        }

        return await query.OrderByDescending(n => n.UpdatedAtUtc).ToListAsync();
    }

    public Task<Note?> GetByIdAsync(Guid noteId)
        => _context.Notes.Include(n => n.Permissions).FirstOrDefaultAsync(n => n.Id == noteId);

    public Task<NotePermission?> GetPermissionAsync(Guid noteId, string userId)
        => _context.NotePermissions.FirstOrDefaultAsync(p => p.NoteId == noteId && p.UserId == userId);

    public async Task AddAsync(Note note) => await _context.Notes.AddAsync(note);

    public void Remove(Note note) => _context.Notes.Remove(note);

    public Task SaveChangesAsync() => _context.SaveChangesAsync();
}
