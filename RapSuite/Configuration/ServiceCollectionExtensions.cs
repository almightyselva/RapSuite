using RapSuite.Infrastructure.AI;
using RapSuite.Infrastructure.Firebase;
using RapSuite.Infrastructure.Session;

namespace RapSuite.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRapSuiteServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Firebase configuration
        services.Configure<FirebaseConfig>(configuration.GetSection(FirebaseConfig.SectionName));

        // Firebase Auth
        services.AddHttpClient<IFirebaseAuthService, FirebaseAuthService>();

        // Firestore
        services.AddHttpClient<IFirestoreService, FirestoreService>();

        // NVIDIA AI
        services.AddHttpClient<ILyricsAiService, NvidiaLyricsAiService>();

        // User session (scoped per Blazor Server circuit)
        services.AddScoped<UserSessionService>();

        return services;
    }
}
