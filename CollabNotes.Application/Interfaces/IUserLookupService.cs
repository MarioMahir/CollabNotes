namespace CollabNotes.Application.Interfaces;

public interface IUserLookupService
{
    Task<string?> FindUserIdAsync(string emailOrUsername);
    Task<string?> GetDisplayNameAsync(string userId);
}
