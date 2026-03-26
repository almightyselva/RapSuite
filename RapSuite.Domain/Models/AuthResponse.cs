namespace RapSuite.Domain.Models;

public class AuthResponse
{
    public string IdToken { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string ExpiresIn { get; set; } = string.Empty;
    public string LocalId { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
}
