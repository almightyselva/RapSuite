using RapSuite.Domain.Interfaces;
using RapSuite.Domain.Models;

namespace RapSuite.Infrastructure.Session;

public class UserSessionService : IUserSessionService
{
    public AuthResponse? CurrentAuth { get; private set; }
    public bool IsAuthenticated => CurrentAuth != null && !string.IsNullOrEmpty(CurrentAuth.IdToken);
    public string? UserId => CurrentAuth?.LocalId;
    public string? Email => CurrentAuth?.Email;
    public string? DisplayName => CurrentAuth?.DisplayName;
    public string? IdToken => CurrentAuth?.IdToken;

    public event Action? OnAuthStateChanged;

    public void SetUser(AuthResponse auth)
    {
        CurrentAuth = auth;
        OnAuthStateChanged?.Invoke();
    }

    public void ClearUser()
    {
        CurrentAuth = null;
        OnAuthStateChanged?.Invoke();
    }
}
