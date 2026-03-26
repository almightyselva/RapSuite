using Microsoft.Extensions.Options;
using RapSuite.Domain.Entities;
using RapSuite.Domain.Interfaces;
using RapSuite.Infrastructure.Configuration;

namespace RapSuite.Infrastructure.Firebase;

public class FirestoreUserRepository : FirestoreServiceBase, IUserRepository
{
    public FirestoreUserRepository(HttpClient httpClient, IOptions<FirebaseConfig> config)
        : base(httpClient, config) { }

    public async Task SaveProfileAsync(string userId, AppUser user)
    {
        var body = new Dictionary<string, string>
        {
            ["displayName"] = user.DisplayName,
            ["email"] = user.Email,
            ["createdAt"] = user.CreatedAt.ToString("o")
        };

        await PatchAsync(body, "users", userId);
    }
}
