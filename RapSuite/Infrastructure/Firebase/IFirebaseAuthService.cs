using RapSuite.Domain.Auth;

namespace RapSuite.Infrastructure.Firebase;

public interface IFirebaseAuthService
{
    Task<(FirebaseAuthResponse? Response, string? Error)> SignUpAsync(string email, string password);
    Task<(FirebaseAuthResponse? Response, string? Error)> SignInAsync(string email, string password);
    Task<bool> UpdateProfileAsync(string idToken, string displayName);
}
