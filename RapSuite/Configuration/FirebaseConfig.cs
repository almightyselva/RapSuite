namespace RapSuite.Configuration;

public class FirebaseConfig
{
    public const string SectionName = "Firebase";
    public string ApiKey { get; set; } = string.Empty;
    public string ProjectId { get; set; } = string.Empty;

    public string AuthBaseUrl => "https://identitytoolkit.googleapis.com/v1";
    public string FirestoreBaseUrl => $"https://firestore.googleapis.com/v1/projects/{ProjectId}/databases/(default)/documents";
}
