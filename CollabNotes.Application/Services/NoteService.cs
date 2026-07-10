using CollabNotes.Application.Dtos;
using CollabNotes.Application.Interfaces;
using CollabNotes.Domain.Entities;
using CollabNotes.Domain.Enums;

namespace CollabNotes.Application.Services;

public class NoteService : INoteService
{
    private readonly INoteRepository _noteRepository;

    public NoteService(INoteRepository noteRepository)
    {
        _noteRepository = noteRepository;
    }

    public async Task<IEnumerable<NoteDto>> GetNotesByFolderAsync(Guid? folderId, string userId)
    {
        var notes = await _noteRepository.GetByUserAsync(userId, folderId);
        return notes.Select(ToDto);
    }

    public async Task<NoteDto?> GetByIdAsync(Guid noteId, string userId)
    {
        var permission = await _noteRepository.GetPermissionAsync(noteId, userId);
        if (permission is null)
        {
            return null;
        }

        var note = await _noteRepository.GetByIdAsync(noteId);
        return note is null ? null : ToDto(note);
    }

    public async Task<NoteDto> CreateAsync(string title, Guid? folderId, string userId)
    {
        var now = DateTime.UtcNow;
        var note = new Note
        {
            Id = Guid.NewGuid(),
            Title = title,
            Content = string.Empty,
            OwnerId = userId,
            FolderId = folderId,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };
        note.Permissions.Add(new NotePermission
        {
            Id = Guid.NewGuid(),
            NoteId = note.Id,
            UserId = userId,
            Role = PermissionRole.Owner
        });

        await _noteRepository.AddAsync(note);
        await _noteRepository.SaveChangesAsync();

        return ToDto(note);
    }

    public async Task UpdateAsync(Guid noteId, string title, string content, string userId)
    {
        var permission = await _noteRepository.GetPermissionAsync(noteId, userId);
        if (permission is null || permission.Role == PermissionRole.Reader)
        {
            throw new UnauthorizedAccessException("El usuario no puede editar esta nota.");
        }

        var note = await _noteRepository.GetByIdAsync(noteId)
            ?? throw new InvalidOperationException("La nota no existe.");

        note.Title = title;
        note.Content = content;
        note.UpdatedAtUtc = DateTime.UtcNow;

        await _noteRepository.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid noteId, string userId)
    {
        var permission = await _noteRepository.GetPermissionAsync(noteId, userId);
        if (permission is null || permission.Role != PermissionRole.Owner)
        {
            throw new UnauthorizedAccessException("Solo el propietario puede eliminar esta nota.");
        }

        var note = await _noteRepository.GetByIdAsync(noteId)
            ?? throw new InvalidOperationException("La nota no existe.");

        _noteRepository.Remove(note);
        await _noteRepository.SaveChangesAsync();
    }

    private static NoteDto ToDto(Note note) => new()
    {
        Id = note.Id,
        Title = note.Title,
        Content = note.Content,
        FolderId = note.FolderId,
        OwnerId = note.OwnerId,
        UpdatedAtUtc = note.UpdatedAtUtc
    };
}
