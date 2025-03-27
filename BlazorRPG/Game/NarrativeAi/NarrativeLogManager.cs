using System.Text.Json;
using System.Text.Json.Serialization;

public class NarrativeLogManager
{
    private string _baseLogDirectory;
    private string _sessionDirectory;
    private readonly JsonSerializerOptions _jsonOptions;

    public NarrativeLogManager()
    {
        // Set base directory for all logs
        _baseLogDirectory = Path.Combine("C:", "Logs");

        // Create a unique session ID for this game session
        string sessionId = $"session_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid().ToString().Substring(0, 8)}";

        // Create the dedicated session directory
        _sessionDirectory = Path.Combine(_baseLogDirectory, sessionId);

        // Configure JSON serialization options
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        // Ensure base directory exists
        EnsureDirectoryExists(_baseLogDirectory);

        // Ensure session directory exists
        EnsureDirectoryExists(_sessionDirectory);

        // Log session start for easier tracking
        LogSessionStart(sessionId);
    }

    /// <summary>
    /// Gets the full path to the session directory for the current game session
    /// </summary>
    public string GetSessionDirectory()
    {
        return _sessionDirectory;
    }

    /// <summary>
    /// Gets the full path for the next log file within the current session
    /// </summary>
    public string GetNextLogFilePath(string conversationId)
    {
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
        string fileName = conversationId != null
            ? $"narrative_log_{timestamp}_{SanitizeFileName(conversationId)}.json"
            : $"narrative_log_{timestamp}.json";

        return Path.Combine(_sessionDirectory, fileName);
    }

    /// <summary>
    /// Logs a complete API interaction including request and response
    /// </summary>
    public async Task LogApiInteractionAsync(
        string conversationId,
        List<ConversationEntry> history,
        object requestBody,
        string jsonResponse,
        string generatedContent,
        string errorMessage)
    {
        string logFilePath = GetNextLogFilePath(conversationId);

        NarrativeLogEntry logEntry = new NarrativeLogEntry
        {
            ConversationId = conversationId,
            ConversationHistory = history,
            Request = requestBody,
            RawResponse = jsonResponse,
            GeneratedContent = generatedContent,
            ErrorMessage = errorMessage,
            Timestamp = DateTime.Now
        };

        try
        {
            await File.WriteAllTextAsync(
                logFilePath,
                JsonSerializer.Serialize(logEntry, _jsonOptions)
            );
        }
        catch (Exception ex)
        {
            // If we can't log to the file, try to create an error log
            try
            {
                string errorLogPath = Path.Combine(
                    _sessionDirectory,
                    $"error_log_{DateTime.Now:yyyyMMdd_HHmmss_fff}.txt"
                );
                await File.WriteAllTextAsync(errorLogPath, $"Error writing log: {ex}");
            }
            catch
            {
                // At this point we can't do much else
            }
        }
    }

    /// <summary>
    /// Logs session start information
    /// </summary>
    private void LogSessionStart(string sessionId)
    {
        try
        {
            string sessionInfoPath = Path.Combine(_sessionDirectory, "session_info.txt");
            File.WriteAllText(sessionInfoPath,
                $"Session ID: {sessionId}\n" +
                $"Started: {DateTime.Now}\n" +
                $"Game Version: {GetGameVersion()}\n");
        }
        catch
        {
            // If we can't log session info, continue anyway
        }
    }

    /// <summary>
    /// Creates the directory if it doesn't exist
    /// </summary>
    private void EnsureDirectoryExists(string directory)
    {
        if (!Directory.Exists(directory))
        {
            try
            {
                Directory.CreateDirectory(directory);
            }
            catch (Exception ex)
            {
                // If we're on a system without permission to C:, fall back to app directory
                if (directory.StartsWith(Path.Combine("C:", "Logs")))
                {
                    string appDataPath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "Wayfarer", "Logs");

                    if (directory == _baseLogDirectory)
                    {
                        // This is the base directory, so update both paths
                        _baseLogDirectory = appDataPath;
                        _sessionDirectory = Path.Combine(appDataPath, Path.GetFileName(_sessionDirectory));
                    }
                    else if (directory == _sessionDirectory)
                    {
                        // This is the session directory
                        _sessionDirectory = Path.Combine(_baseLogDirectory, Path.GetFileName(_sessionDirectory));
                    }

                    // Retry with the new paths
                    EnsureDirectoryExists(_baseLogDirectory);
                    EnsureDirectoryExists(_sessionDirectory);
                }
                else
                {
                    // Can't create directory even in fallback location
                    throw new InvalidOperationException($"Cannot create log directory: {ex.Message}", ex);
                }
            }
        }
    }

    /// <summary>
    /// Removes invalid characters from file names
    /// </summary>
    private string SanitizeFileName(string fileName)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
        {
            fileName = fileName.Replace(c, '_');
        }
        return fileName;
    }

    /// <summary>
    /// Gets the current game version
    /// </summary>
    private string GetGameVersion()
    {
        // Replace with your actual version retrieval code
        return "1.0.0";
    }

}