namespace RapSuite.Domain.Auth;

public class FirebaseSignUpRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool ReturnSecureToken { get; set; } = true;
}

public class FirebaseSignInRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool ReturnSecureToken { get; set; } = true;
}

public class FirebaseAuthResponse
{
    public string IdToken { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string ExpiresIn { get; set; } = string.Empty;
    public string LocalId { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
}

public class FirebaseAuthError
{
    public FirebaseErrorDetail? Error { get; set; }
}

public class FirebaseErrorDetail
{
    public int Code { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class FirebaseUpdateProfileRequest
{
    public string IdToken { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public bool ReturnSecureToken { get; set; } = true;
}
