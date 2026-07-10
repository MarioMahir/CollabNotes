using CollabNotes.Application.Dtos;

namespace CollabNotes.Application.Interfaces;

public interface IFolderService
{
    Task<IEnumerable<FolderDto>> GetFoldersAsync(string userId);
    Task<FolderDto> CreateAsync(string name, string userId);
}
