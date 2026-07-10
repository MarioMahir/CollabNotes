using CollabNotes.Application.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace CollabNotes.Infrastructure.Identity;

public class UserLookupService : IUserLookupService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserLookupService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<string?> FindUserIdAsync(string emailOrUsername)
    {
        var user = await _userManager.FindByEmailAsync(emailOrUsername)
            ?? await _userManager.FindByNameAsync(emailOrUsername);

        return user?.Id;
    }

    public async Task<string?> GetDisplayNameAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user?.UserName;
    }
}
