namespace RapSuite.Configuration;

public class FirebaseConfig
{
    public const string SectionName = "Firebase";
    public string ApiKey { get; set; } = string.Empty;
    public string ProjectId { get; set; } = string.Empty;
    public string DatabaseUrl { get; set; } = string.Empty;
}
