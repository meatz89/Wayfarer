/// <summary>
/// Result of attempting to perform work at a location
/// </summary>
public class WorkResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int CoinsEarned { get; set; }
}