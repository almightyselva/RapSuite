using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RapSuite.Domain.Interfaces;
using RapSuite.Infrastructure.AI;
using RapSuite.Infrastructure.Configuration;
using RapSuite.Infrastructure.Firebase;
using RapSuite.Infrastructure.Session;

namespace RapSuite.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Firebase configuration
        services.Configure<FirebaseConfig>(configuration.GetSection(FirebaseConfig.SectionName));

        // Firebase Auth
        services.AddHttpClient<IAuthService, FirebaseAuthService>();

        // Repositories
        services.AddHttpClient<IAlbumRepository, FirestoreAlbumRepository>();
        services.AddHttpClient<ISongRepository, FirestoreSongRepository>();
        services.AddHttpClient<IUserRepository, FirestoreUserRepository>();

        // NVIDIA AI
        services.AddSingleton<ILyricsAiService, NvidiaLyricsAiService>();

        // User session (scoped per Blazor Server circuit)
        services.AddScoped<IUserSessionService, UserSessionService>();

        return services;
    }
}
