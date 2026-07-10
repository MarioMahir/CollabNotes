using CollabNotes.Application.Interfaces;
using CollabNotes.Infrastructure.Identity;
using CollabNotes.Infrastructure.Persistence;
using CollabNotes.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CollabNotes.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
            .AddEntityFrameworkStores<AppDbContext>();

        services.AddScoped<INoteRepository, NoteRepository>();
        services.AddScoped<IFolderRepository, FolderRepository>();
        services.AddScoped<IUserLookupService, UserLookupService>();

        return services;
    }
}
