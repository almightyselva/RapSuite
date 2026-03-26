using RapSuite.Domain.Models;

namespace RapSuite.Domain.Interfaces;

public interface IUserSessionService
{
    AuthResponse? CurrentAuth { get; }
    bool IsAuthenticated { get; }
    string? UserId { get; }
    string? Email { get; }
    string? DisplayName { get; }
    string? IdToken { get; }
    event Action? OnAuthStateChanged;
    void SetUser(AuthResponse auth);
    void ClearUser();
}
