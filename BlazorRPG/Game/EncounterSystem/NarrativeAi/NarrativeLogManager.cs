using System.Text.Json;
using System.Text.Json.Serialization;

public class NarrativeLogManager
{
    private string _baseLogDirectory;
    private readonly string _gameInstanceId;
    private readonly JsonSerializerOptions _jsonOptions;

    public NarrativeLogManager()
    {
        string gameInstanceId = string.Empty;

        // Set base directory to C:\Logs
        _baseLogDirectory = Path.Combine("C:", "Logs");

        // Generate a unique game instance ID if not provided
        _gameInstanceId = gameInstanceId ?? $"game_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid().ToString().Substring(0, 8)}";

        // Configure JSON serialization options
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        // Ensure base directory exists
        EnsureDirectoryExists(_baseLogDirectory);

        // Ensure game instance directory exists
        EnsureDirectoryExists(GetGameInstanceDirectory());
    }

    /// <summary>
    /// Gets the full path to the game instance directory
    /// </summary>
    public string GetGameInstanceDirectory()
    {
        return Path.Combine(_baseLogDirectory, _gameInstanceId);
    }

    /// <summary>
    /// Gets the full path for the next log file
    /// </summary>
    public string GetNextLogFilePath(string conversationId)
    {
        string gameInstanceDir = GetGameInstanceDirectory();
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
        string fileName = conversationId != null
            ? $"narrative_log_{timestamp}_{SanitizeFileName(conversationId)}.json"
            : $"narrative_log_{timestamp}.json";

        return Path.Combine(gameInstanceDir, fileName);
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
            ErrorMessage = errorMessage
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
                    GetGameInstanceDirectory(),
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
                if (_baseLogDirectory.StartsWith("C:"))
                {
                    _baseLogDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
                    EnsureDirectoryExists(_baseLogDirectory);
                    EnsureDirectoryExists(GetGameInstanceDirectory());
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
}
