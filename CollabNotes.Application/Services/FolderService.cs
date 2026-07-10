using CollabNotes.Application.Dtos;
using CollabNotes.Application.Interfaces;
using CollabNotes.Domain.Entities;

namespace CollabNotes.Application.Services;

public class FolderService : IFolderService
{
    private readonly IFolderRepository _folderRepository;

    public FolderService(IFolderRepository folderRepository)
    {
        _folderRepository = folderRepository;
    }

    public async Task<IEnumerable<FolderDto>> GetFoldersAsync(string userId)
    {
        var folders = await _folderRepository.GetByOwnerAsync(userId);
        return folders.Select(f => new FolderDto { Id = f.Id, Name = f.Name });
    }

    public async Task<FolderDto> CreateAsync(string name, string userId)
    {
        var folder = new Folder
        {
            Id = Guid.NewGuid(),
            Name = name,
            OwnerId = userId
        };

        await _folderRepository.AddAsync(folder);
        await _folderRepository.SaveChangesAsync();

        return new FolderDto { Id = folder.Id, Name = folder.Name };
    }
}
