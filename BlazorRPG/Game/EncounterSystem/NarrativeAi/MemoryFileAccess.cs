using System.Text.Json;
using System.Text.Json.Serialization;

public class MemoryFileAccess
{
    const string fileName = $"memory.json";

    public static async Task<string> ReadFromMemoryFile(ChoiceOutcome outcome, EncounterStatus newStatus)
    {
        string memoryContent = string.Empty;
        try
        {
            string _baseLogDirectory = Path.Combine("C:", "Logs");
            string filePath = Path.Combine(_baseLogDirectory, fileName);

            // Configure JSON serialization options
            var _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            memoryContent = await File.ReadAllTextAsync(filePath);
        }
        catch(Exception e)
        {

        }
        return memoryContent;
    }

    public static async Task WriteToMemoryFile(ChoiceOutcome outcome, EncounterStatus newStatus, string memoryContent)
    {
        string _baseLogDirectory = Path.Combine("C:", "Logs");
        string filePath = Path.Combine(_baseLogDirectory, fileName);

        // Configure JSON serialization options
        var _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        await File.WriteAllTextAsync(
            _baseLogDirectory,
            JsonSerializer.Serialize(memoryContent, _jsonOptions)
        );
    }
}