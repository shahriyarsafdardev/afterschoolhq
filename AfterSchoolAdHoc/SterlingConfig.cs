using AfterSchoolLibrary.Integrations;

namespace AfterSchoolAdHoc;

public class SterlingConfig
{
    public SterlingCredentials? Sterling { get; init; }
}
public class SterlingCredentials
{
    public string Endpoint { get; init; }
    public string ClientId { get; init; }
    public string ClientSecret { get; init; }
    public string PackageId { get; init; }
    public string AccountId { get; init; }
}