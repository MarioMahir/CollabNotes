using CollabNotes.Application.Interfaces;
using CollabNotes.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CollabNotes.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<INoteService, NoteService>();
        services.AddScoped<IFolderService, FolderService>();
        services.AddSingleton<IPresenceTracker, PresenceTracker>();
        services.AddScoped<ICollaborationService, CollaborationService>();

        return services;
    }
}
