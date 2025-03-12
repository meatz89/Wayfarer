
// This class will come from your existing codebase
public class NarrativeLogManager
{
    public string GetNextLogFilePath()
    {
        string logsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Logs", "NarrativeAI");

        if (!Directory.Exists(logsDirectory))
        {
            Directory.CreateDirectory(logsDirectory);
        }

        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
        return Path.Combine(logsDirectory, $"narrative_log_{timestamp}.txt");
    }
}