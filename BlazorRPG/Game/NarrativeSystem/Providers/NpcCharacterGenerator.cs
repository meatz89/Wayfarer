using System.Text.Json;

public class NpcCharacterGenerator
{
    private readonly AIClient _aiClient;

    public NpcCharacterGenerator(AIClient aiClient)
    {
        _aiClient = aiClient ?? throw new ArgumentNullException(nameof(aiClient));
    }

    public async Task<NpcCharacter> GenerateCharacterAsync(CharacterGenerationRequest request)
    {
        List<ConversationEntry> messages = new List<ConversationEntry>();

        // System message with instructions
        messages.Add(new ConversationEntry
        {
            Role = "system",
            Content = @"You are a medieval character generator for the text-based RPG 'Wayfarer'. 
Generate a detailed medieval character following the narrative style principles:
- Write with measured elegance, focusing on ordinary moments and intimate details
- Create characters that feel flesh and blood real with private hopes and quiet sorrows
- Include background that shapes who they are, revealed through subtle details
- Focus on intimate conflicts: relationships, unfulfilled dreams, daily bread, personal honor
- Be historically authentic for medieval life without fantasy elements

Respond ONLY with a JSON object matching this exact structure:
{
  ""name"": ""[Character's full name]"",
  ""age"": [age as integer],
  ""gender"": ""[male or female]"",
  ""occupation"": ""[Primary occupation]"",
  ""appearance"": ""[Brief physical description, 2-3 sentences]"",
  ""background"": ""[Life history and key events, 3-5 sentences]"",
  ""personality"": ""[Core traits and behaviors, 2-3 sentences]"",
  ""motivation"": ""[What drives this character, 1-2 sentences]"",
  ""quirk"": ""[A distinctive habit or trait, 1 sentence]"",
  ""secret"": ""[Something this person doesn't want others to know, 1-2 sentences]"",
  ""possessions"": [Array of 3-5 notable items they own, as strings],
  ""skills"": [Array of 2-4 things they're good at, as strings],
  ""relationships"": [Array of 2-3 important connections to other people, as strings]
}

Your response must be ONLY this JSON with no other text, headers, or explanations."
        });

        // User message with specific request
        messages.Add(new ConversationEntry
        {
            Role = "user",
            Content = $"Generate a {request.Archetype} character from {request.Region} with the following specifications:\n" +
                      $"Gender: {(string.IsNullOrEmpty(request.Gender) ? "any" : request.Gender)}\n" +
                      $"Age range: {request.MinAge}-{request.MaxAge}\n" +
                      $"Additional traits: {request.AdditionalTraits}"
        });

        // The model should already be pulled and available
        string response = await _aiClient.GetCompletionAsync(messages, null, null);
        NpcCharacter character = OllamaResponseParser.ParseCharacterJson(response);
        return character;
    }
}