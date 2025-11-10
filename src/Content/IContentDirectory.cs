/// <summary>
/// Configuration interface for content directory path.
/// Allows injecting the content path without hardcoding.
/// </summary>
public interface IContentDirectory
{
/// <summary>
/// Path to the content directory containing game JSON files.
/// </summary>
string Path { get; }
}

/// <summary>
/// Default implementation of IContentDirectory.
/// </summary>
public class ContentDirectory : IContentDirectory
{
public string Path { get; set; }

public ContentDirectory()
{
    Path = "Content";
}
}