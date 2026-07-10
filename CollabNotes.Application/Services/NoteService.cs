using CollabNotes.Application.Common;
using CollabNotes.Application.Dtos;
using CollabNotes.Application.Interfaces;
using CollabNotes.Application.Options;
using CollabNotes.Domain.Entities;
using CollabNotes.Domain.Enums;
using Microsoft.Extensions.Options;

namespace CollabNotes.Application.Services;

public class NoteService : INoteService
{
    private readonly INoteRepository _noteRepository;
    private readonly IUserLookupService _userLookup;
    private readonly INoteSnapshotRepository _snapshotRepository;
    private readonly NoteSnapshotOptions _snapshotOptions;

    public NoteService(
        INoteRepository noteRepository,
        IUserLookupService userLookup,
        INoteSnapshotRepository snapshotRepository,
        IOptions<NoteSnapshotOptions> snapshotOptions)
    {
        _noteRepository = noteRepository;
        _userLookup = userLookup;
        _snapshotRepository = snapshotRepository;
        _snapshotOptions = snapshotOptions.Value;
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
        var note = await EnsureEditableAsync(noteId, userId);

        await MaybeSnapshotAsync(note);

        note.Title = title;
        note.Content = content;
        note.UpdatedAtUtc = DateTime.UtcNow;

        await _noteRepository.SaveChangesAsync();
    }

    public async Task<BlockUpdateDto> UpdateBlockAsync(Guid noteId, int blockIndex, string blockContent, string userId)
    {
        var note = await EnsureEditableAsync(noteId, userId);

        await MaybeSnapshotAsync(note);

        var blocks = NoteBlockSplitter.Split(note.Content).ToList();
        while (blocks.Count <= blockIndex)
        {
            blocks.Add(string.Empty);
        }
        blocks[blockIndex] = blockContent;

        note.Content = NoteBlockSplitter.Join(blocks);
        var now = DateTime.UtcNow;
        note.UpdatedAtUtc = now;

        await _noteRepository.SaveChangesAsync();

        return new BlockUpdateDto
        {
            NoteId = noteId,
            BlockIndex = blockIndex,
            Content = blockContent,
            EditedByUserId = userId,
            UpdatedAtUtc = now
        };
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

    public async Task<IEnumerable<NoteSnapshotDto>> GetSnapshotsAsync(Guid noteId, string userId)
    {
        var permission = await _noteRepository.GetPermissionAsync(noteId, userId);
        if (permission is null)
        {
            throw new UnauthorizedAccessException("El usuario no tiene acceso a esta nota.");
        }

        var snapshots = await _snapshotRepository.GetByNoteAsync(noteId);
        return snapshots
            .OrderByDescending(s => s.CreatedAtUtc)
            .Select(ToSnapshotDto);
    }

    public async Task<NoteSnapshotDto?> GetSnapshotAsync(Guid noteId, Guid snapshotId, string userId)
    {
        var permission = await _noteRepository.GetPermissionAsync(noteId, userId);
        if (permission is null)
        {
            throw new UnauthorizedAccessException("El usuario no tiene acceso a esta nota.");
        }

        var snapshot = await _snapshotRepository.GetByIdAsync(noteId, snapshotId);
        return snapshot is null ? null : ToSnapshotDto(snapshot);
    }

    private async Task<Note> EnsureEditableAsync(Guid noteId, string userId)
    {
        var permission = await _noteRepository.GetPermissionAsync(noteId, userId);
        if (permission is null || permission.Role == PermissionRole.Reader)
        {
            throw new UnauthorizedAccessException("El usuario no puede editar esta nota.");
        }

        return await _noteRepository.GetByIdAsync(noteId)
            ?? throw new InvalidOperationException("La nota no existe.");
    }

    private async Task MaybeSnapshotAsync(Note note)
    {
        var latest = await _snapshotRepository.GetLatestAsync(note.Id);
        var dueForSnapshot = latest is null
            || DateTime.UtcNow - latest.CreatedAtUtc >= TimeSpan.FromMinutes(_snapshotOptions.SnapshotIntervalMinutes);

        if (dueForSnapshot)
        {
            await _snapshotRepository.AddAsync(new NoteSnapshot
            {
                Id = Guid.NewGuid(),
                NoteId = note.Id,
                Content = note.Content,
                CreatedAtUtc = DateTime.UtcNow
            });
        }
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

    private static NoteSnapshotDto ToSnapshotDto(NoteSnapshot snapshot) => new()
    {
        Id = snapshot.Id,
        NoteId = snapshot.NoteId,
        Content = snapshot.Content,
        CreatedAtUtc = snapshot.CreatedAtUtc
    };
}
