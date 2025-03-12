public class NarrativeLogManager
{
    private readonly string _instanceFolder;
    private int _requestCounter = 0;

    public NarrativeLogManager()
    {
        // Create a unique folder for this game instance
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        _instanceFolder = Path.Combine("C:\\Logs", $"Game_{timestamp}");
        Directory.CreateDirectory(_instanceFolder);
    }

    public string GetNextLogFilePath()
    {
        _requestCounter++;
        return Path.Combine(_instanceFolder, $"Request_{_requestCounter}.json");
    }
}
