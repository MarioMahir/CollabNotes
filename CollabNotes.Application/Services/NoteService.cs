using CollabNotes.Application.Dtos;
using CollabNotes.Application.Interfaces;
using CollabNotes.Domain.Entities;
using CollabNotes.Domain.Enums;

namespace CollabNotes.Application.Services;

public class NoteService : INoteService
{
    private readonly INoteRepository _noteRepository;
    private readonly IUserLookupService _userLookup;

    public NoteService(INoteRepository noteRepository, IUserLookupService userLookup)
    {
        _noteRepository = noteRepository;
        _userLookup = userLookup;
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
        if (note is null)
        {
            return null;
        }

        var dto = ToDto(note);
        dto.ViewerRole = permission.Role;
        return dto;
    }

    public async Task InviteUserAsync(Guid noteId, string inviterUserId, string emailOrUsername, PermissionRole role)
    {
        var inviterPermission = await _noteRepository.GetPermissionAsync(noteId, inviterUserId);
        if (inviterPermission is null || inviterPermission.Role != PermissionRole.Owner)
        {
            throw new UnauthorizedAccessException("Solo el propietario puede invitar colaboradores.");
        }

        var inviteeUserId = await _userLookup.FindUserIdAsync(emailOrUsername)
            ?? throw new InvalidOperationException("No se encontró ningún usuario con ese email o nombre de usuario.");

        if (inviteeUserId == inviterUserId)
        {
            throw new InvalidOperationException("No puedes invitarte a ti mismo.");
        }

        var existingPermission = await _noteRepository.GetPermissionAsync(noteId, inviteeUserId);
        if (existingPermission is not null)
        {
            existingPermission.Role = role;
        }
        else
        {
            await _noteRepository.AddPermissionAsync(new NotePermission
            {
                Id = Guid.NewGuid(),
                NoteId = noteId,
                UserId = inviteeUserId,
                Role = role
            });
        }

        await _noteRepository.SaveChangesAsync();
    }

    public async Task<IEnumerable<NotePermissionDto>> GetMembersAsync(Guid noteId, string requestingUserId)
    {
        var requesterPermission = await _noteRepository.GetPermissionAsync(noteId, requestingUserId);
        if (requesterPermission is null)
        {
            throw new UnauthorizedAccessException("El usuario no tiene acceso a esta nota.");
        }

        var note = await _noteRepository.GetByIdAsync(noteId)
            ?? throw new InvalidOperationException("La nota no existe.");

        var members = new List<NotePermissionDto>();
        foreach (var permission in note.Permissions)
        {
            var displayName = await _userLookup.GetDisplayNameAsync(permission.UserId) ?? permission.UserId;
            members.Add(new NotePermissionDto
            {
                UserId = permission.UserId,
                DisplayName = displayName,
                Role = permission.Role
            });
        }

        return members;
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
