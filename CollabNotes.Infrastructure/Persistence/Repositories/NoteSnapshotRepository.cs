using CollabNotes.Application.Interfaces;
using CollabNotes.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CollabNotes.Infrastructure.Persistence.Repositories;

public class NoteSnapshotRepository : INoteSnapshotRepository
{
    private readonly AppDbContext _context;

    public NoteSnapshotRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<NoteSnapshot?> GetLatestAsync(Guid noteId)
        => _context.NoteSnapshots
            .Where(s => s.NoteId == noteId)
            .OrderByDescending(s => s.CreatedAtUtc)
            .FirstOrDefaultAsync();

    public async Task AddAsync(NoteSnapshot snapshot) => await _context.NoteSnapshots.AddAsync(snapshot);

    public async Task<IEnumerable<NoteSnapshot>> GetByNoteAsync(Guid noteId)
        => await _context.NoteSnapshots.Where(s => s.NoteId == noteId).ToListAsync();

    public Task<NoteSnapshot?> GetByIdAsync(Guid noteId, Guid snapshotId)
        => _context.NoteSnapshots.FirstOrDefaultAsync(s => s.NoteId == noteId && s.Id == snapshotId);
}
