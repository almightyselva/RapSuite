using RapSuite.Domain.Common;
using RapSuite.Domain.Models;

namespace RapSuite.Domain.Interfaces;

public interface IAuthService
{
    Task<Result<AuthResponse>> SignUpAsync(string email, string password);
    Task<Result<AuthResponse>> SignInAsync(string email, string password);
    Task<bool> UpdateProfileAsync(string idToken, string displayName);
}
