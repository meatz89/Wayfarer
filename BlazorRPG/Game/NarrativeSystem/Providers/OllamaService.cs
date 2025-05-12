using System.Text.Json;

public class OllamaService
{
    private readonly NpcCharacterGenerator _characterGenerator;
    private readonly OllamaProvider ollamaProvider;

    public OllamaService(
        string gameInstanceId,
        string ollamaBaseUrl,
        NarrativeLogManager logManager)
    {
        // Create the AI provider and client
        ollamaProvider = new OllamaProvider(ollamaBaseUrl);
        AIClient aiClient = new AIClient(ollamaProvider, gameInstanceId, logManager);

        // Initialize character generator
        _characterGenerator = new NpcCharacterGenerator(aiClient);
    }

    public async Task<NpcCharacter> GenerateCharacterAsync(
        string archetype = "commoner",
        string region = "a small rural village",
        string gender = "",
        int minAge = 18,
        int maxAge = 60,
        string additionalTraits = "",
        IResponseStreamWatcher watcher = null)
    {
        CharacterGenerationRequest request = new CharacterGenerationRequest
        {
            Archetype = archetype,
            Region = region,
            Gender = gender,
            MinAge = minAge,
            MaxAge = maxAge,
            AdditionalTraits = additionalTraits
        };

        // Pass the watcher to the character generator
        NpcCharacter character = await _characterGenerator.GenerateCharacterAsync(request, watcher);
        return character;
    }

    public async Task SaveCharacterToFileAsync(NpcCharacter character, string filePath)
    {
        string json = JsonSerializer.Serialize(character, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        string directory = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllTextAsync(filePath, json);
    }
}