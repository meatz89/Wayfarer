# Practical Implementation: World Evolution System

## Core Prompts

### 1. World Evolution Prompt

```
Using the encounter narrative below, determine how the world should evolve in response to the player's choices and interests.

ENCOUNTER NARRATIVE:
{{encounterNarrative}}

CHARACTER BACKGROUND:
{{characterBackground}}

CURRENT WORLD STATE:
- Current location: {{currentLocation}}
- Known locations: {{knownLocations}}
- Known characters: {{knownCharacters}}
- Active opportunities: {{activeOpportunities}}

INSTRUCTIONS:
1. Identify what interested the player during this encounter
2. Apply "Purpose or Perish" - only include elements that advance the plot OR reinforce tone
3. Create 0-2 new location spots at the current location
4. Create 0-3 new actions at existing spots
5. Create 0-2 new characters with clear purpose
6. Create 0-1 new locations connected to current ones
7. Create 0-2 new opportunities from player actions

FOR EACH NEW ELEMENT:
- Make it require player interaction
- Connect it to the player's choices or background
- Keep descriptions brief but evocative
- Ensure it has a clear purpose

RESPONSE FORMAT:
Respond with ONLY a valid JSON object following this exact structure:

{
  "newLocationSpots": [
    {
      "name": "Spot name",
      "description": "Brief description",
      "interactionType": "Character/Shop/Feature/Service",
      "actions": [
        {
          "name": "Action name",
          "description": "What this action involves"
        }
      ]
    }
  ],
  "newActions": [
    {
      "spotName": "Existing spot name",
      "name": "Action name",
      "description": "What this action involves"
    }
  ],
  "newCharacters": [
    {
      "name": "Character name",
      "role": "Character's occupation",
      "description": "Brief physical and personality description",
      "location": "Where they can be found"
    }
  ],
  "newLocations": [
    {
      "name": "Location name",
      "description": "Brief description",
      "connectedTo": "Name of location it connects to",
      "environmentalProperties": ["Property1", "Property2"],
      "spots": [
        {
          "name": "Spot name",
          "description": "Brief description",
          "interactionType": "Character/Shop/Feature/Service",
          "actions": [
            {
              "name": "Action name",
              "description": "What this action involves"
            }
          ]
        }
      ]
    }
  ],
  "newOpportunities": [
    {
      "name": "Opportunity name",
      "type": "Quest/Job/Mystery",
      "description": "Brief description",
      "location": "Where it takes place",
      "relatedCharacter": "Character name"
    }
  ]
}
```

### 2. Memory Consolidation Prompt

```
Create a concise memory record of this encounter:

ENCOUNTER NARRATIVE:
{{encounterNarrative}}

INSTRUCTIONS:
Generate a single paragraph (2-3 sentences) that captures:
- Key player decisions
- Critical information discovered
- Significant character interactions
- Major world developments

This memory will be referenced in future encounters.

RESPONSE FORMAT:
Respond with ONLY the memory paragraph, no additional text.
```

## Data Models

### Input Models

```csharp
// Simple input model for World Evolution
public class WorldEvolutionInput
{
    public string EncounterNarrative { get; set; }
    public string CharacterBackground { get; set; }
    public string CurrentLocation { get; set; }
    public string KnownLocations { get; set; }
    public string KnownCharacters { get; set; }
    public string ActiveOpportunities { get; set; }
}

// Simple input model for Memory Consolidation
public class MemoryConsolidationInput
{
    public string EncounterNarrative { get; set; }
}
```

### Output Models

```csharp
// Flat models for World Evolution response
public class WorldEvolutionResponse
{
    public List<LocationSpot> NewLocationSpots { get; set; } = new List<LocationSpot>();
    public List<NewAction> NewActions { get; set; } = new List<NewAction>();
    public List<Character> NewCharacters { get; set; } = new List<Character>();
    public List<Location> NewLocations { get; set; } = new List<Location>();
    public List<Opportunity> NewOpportunities { get; set; } = new List<Opportunity>();
}

public class LocationSpot
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string InteractionType { get; set; }
    public List<Action> Actions { get; set; } = new List<Action>();
}

public class NewAction
{
    public string SpotName { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}

public class Action
{
    public string Name { get; set; }
    public string Description { get; set; }
}

public class Character
{
    public string Name { get; set; }
    public string Role { get; set; }
    public string Description { get; set; }
    public string Location { get; set; }
}

public class Location
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string ConnectedTo { get; set; }
    public List<string> EnvironmentalProperties { get; set; } = new List<string>();
    public List<LocationSpot> Spots { get; set; } = new List<LocationSpot>();
}

public class Opportunity
{
    public string Name { get; set; }
    public string Type { get; set; }
    public string Description { get; set; }
    public string Location { get; set; }
    public string RelatedCharacter { get; set; }
}
```

## Integration Implementation

### 1. Service for AI Calls

```csharp
public class WorldEvolutionService
{
    private readonly HttpClient httpClient;
    private readonly string apiEndpoint;
    private readonly string apiKey;

    public WorldEvolutionService(string apiEndpoint, string apiKey)
    {
        this.httpClient = new HttpClient();
        this.apiEndpoint = apiEndpoint;
        this.apiKey = apiKey;
    }

    public async Task<WorldEvolutionResponse> ProcessWorldEvolution(WorldEvolutionInput input)
    {
        // Format your prompt with the input data
        string prompt = FormatWorldEvolutionPrompt(input);
        
        // Call the AI API
        string response = await CallAiApi(prompt);
        
        // Parse and validate the response
        return ParseWorldEvolutionResponse(response);
    }

    public async Task<string> ConsolidateMemory(MemoryConsolidationInput input)
    {
        // Format your memory prompt
        string prompt = FormatMemoryPrompt(input);
        
        // Call the AI API
        string response = await CallAiApi(prompt);
        
        // Return the trimmed response
        return response.Trim();
    }

    private string FormatWorldEvolutionPrompt(WorldEvolutionInput input)
    {
        // Your prompt template with replaced placeholders
        return "Using the encounter narrative below, determine how the world should evolve in response to the player's choices and interests.\n\n" +
               "ENCOUNTER NARRATIVE:\n" + input.EncounterNarrative + "\n\n" +
               "CHARACTER BACKGROUND:\n" + input.CharacterBackground + "\n\n" +
               "CURRENT WORLD STATE:\n" +
               "- Current location: " + input.CurrentLocation + "\n" +
               "- Known locations: " + input.KnownLocations + "\n" +
               "- Known characters: " + input.KnownCharacters + "\n" +
               "- Active opportunities: " + input.ActiveOpportunities + "\n\n" +
               // Include the rest of the prompt...
               "RESPONSE FORMAT:\n" +
               "Respond with ONLY a valid JSON object following this exact structure:\n\n" +
               "{\n  \"newLocationSpots\": [...],\n  \"newActions\": [...],\n  \"newCharacters\": [...],\n  \"newLocations\": [...],\n  \"newOpportunities\": [...]\n}";
    }

    private string FormatMemoryPrompt(MemoryConsolidationInput input)
    {
        return "Create a concise memory record of this encounter:\n\n" +
               "ENCOUNTER NARRATIVE:\n" + input.EncounterNarrative + "\n\n" +
               "INSTRUCTIONS:\n" +
               "Generate a single paragraph (2-3 sentences) that captures:\n" +
               "- Key player decisions\n" +
               "- Critical information discovered\n" +
               "- Significant character interactions\n" +
               "- Major world developments\n\n" +
               "This memory will be referenced in future encounters.\n\n" +
               "RESPONSE FORMAT:\n" +
               "Respond with ONLY the memory paragraph, no additional text.";
    }

    private async Task<string> CallAiApi(string prompt)
    {
        // Set up the request to your AI provider
        var request = new HttpRequestMessage(HttpMethod.Post, apiEndpoint)
        {
            Content = new StringContent(JsonSerializer.Serialize(new
            {
                prompt = prompt,
                temperature = 0.7,
                max_tokens = 1000
            }), Encoding.UTF8, "application/json")
        };

        request.Headers.Add("Authorization", $"Bearer {apiKey}");

        // Send the request
        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        // Parse the response
        string responseBody = await response.Content.ReadAsStringAsync();
        
        // Extract the actual text response from the API's JSON response
        // This will vary based on your AI provider
        var responseObject = JsonSerializer.Deserialize<JsonElement>(responseBody);
        return responseObject.GetProperty("choices")[0].GetProperty("text").GetString();
    }

    private WorldEvolutionResponse ParseWorldEvolutionResponse(string response)
    {
        try
        {
            // Try to parse the JSON response
            return JsonSerializer.Deserialize<WorldEvolutionResponse>(response);
        }
        catch (Exception ex)
        {
            // Handle parsing errors with fallback
            Console.WriteLine($"Error parsing AI response: {ex.Message}");
            return new WorldEvolutionResponse();
        }
    }
}
```

### 2. World State Updates

```csharp
public class WorldStateManager
{
    private List<Location> locations = new List<Location>();
    private List<Character> characters = new List<Character>();
    private List<Opportunity> opportunities = new List<Opportunity>();
    private List<string> memoryEntries = new List<string>();

    public Location CurrentLocation { get; private set; }

    public void SetCurrentLocation(string locationName)
    {
        CurrentLocation = locations.FirstOrDefault(l => l.Name == locationName);
    }

    public async Task ProcessEncounterOutcome(string encounterNarrative)
    {
        // Create the service
        var evolutionService = new WorldEvolutionService("your-api-endpoint", "your-api-key");

        // Prepare the input
        var input = PrepareWorldEvolutionInput(encounterNarrative);

        // Process world evolution
        var evolutionResponse = await evolutionService.ProcessWorldEvolution(input);

        // Update world state
        IntegrateWorldEvolution(evolutionResponse);

        // Create memory entry
        var memoryInput = new MemoryConsolidationInput { EncounterNarrative = encounterNarrative };
        string memoryEntry = await evolutionService.ConsolidateMemory(memoryInput);
        memoryEntries.Add(memoryEntry);
    }

    private WorldEvolutionInput PrepareWorldEvolutionInput(string encounterNarrative)
    {
        return new WorldEvolutionInput
        {
            EncounterNarrative = encounterNarrative,
            CharacterBackground = "The player is a former soldier turned merchant seeking new opportunities.",  // Get from player state
            CurrentLocation = CurrentLocation?.Name ?? "Unknown",
            KnownLocations = string.Join(", ", locations.Select(l => l.Name)),
            KnownCharacters = string.Join(", ", characters.Select(c => c.Name)),
            ActiveOpportunities = string.Join(", ", opportunities.Select(o => o.Name))
        };
    }

    private void IntegrateWorldEvolution(WorldEvolutionResponse evolution)
    {
        // Add new location spots to current location
        if (CurrentLocation != null)
        {
            foreach (var spot in evolution.NewLocationSpots)
            {
                if (CurrentLocation.Spots == null)
                    CurrentLocation.Spots = new List<LocationSpot>();
                
                CurrentLocation.Spots.Add(spot);
            }
        }

        // Add new actions to existing spots
        foreach (var newAction in evolution.NewActions)
        {
            if (CurrentLocation != null && CurrentLocation.Spots != null)
            {
                var spotToUpdate = CurrentLocation.Spots.FirstOrDefault(s => s.Name == newAction.SpotName);
                if (spotToUpdate != null)
                {
                    if (spotToUpdate.Actions == null)
                        spotToUpdate.Actions = new List<Action>();
                    
                    spotToUpdate.Actions.Add(new Action
                    {
                        Name = newAction.Name,
                        Description = newAction.Description
                    });
                }
            }
        }

        // Add new characters
        characters.AddRange(evolution.NewCharacters);

        // Add new locations
        locations.AddRange(evolution.NewLocations);

        // Add new opportunities
        opportunities.AddRange(evolution.NewOpportunities);
    }

    // Additional helper methods for managing world state...
}
```

### 3. Integration with Encounter System

```csharp
public class EncounterSystem
{
    private readonly WorldStateManager worldState;
    private readonly WorldEvolutionService evolutionService;

    public EncounterSystem(WorldStateManager worldState, WorldEvolutionService evolutionService)
    {
        this.worldState = worldState;
        this.evolutionService = evolutionService;
    }

    public async Task<string> GenerateEncounter(string locationName, string spotName, string actionName)
    {
        // Your existing encounter generation logic...
        
        // Return the generated encounter narrative
        return "Generated encounter narrative...";
    }

    public async Task ProcessEncounterOutcome(string encounterNarrative)
    {
        // Process world evolution
        await worldState.ProcessEncounterOutcome(encounterNarrative);
        
        // Notify player of changes
        NotifyPlayerOfWorldChanges();
    }

    private void NotifyPlayerOfWorldChanges()
    {
        // Simple notification system
        Console.WriteLine("The world has evolved based on your actions!");
        
        // In a real implementation, you'd send these to your UI system
    }
}
```

## Implementation Steps

1. **Create the Base Classes**: Start with implementing the data models and service classes.

2. **Set Up AI Integration**: Configure your API connection and test the basic prompt handling.

3. **Implement Post-Encounter Processing**:
   ```csharp
   // Example usage
   var encounterSystem = new EncounterSystem(worldStateManager, evolutionService);
   
   // After encounter completes
   string encounterNarrative = "The full narrative of the encounter...";
   await encounterSystem.ProcessEncounterOutcome(encounterNarrative);
   ```

4. **Test World Evolution**: Run a test encounter and verify that:
   - The AI generates reasonable world evolutions
   - New elements are properly integrated into the world state
   - Memory entries are created and stored

5. **Add UI Notifications**: Implement simple UI notifications for new discoveries.

## Practical Tips

1. **Keep Prompt Templates in External Files**: For easier management and updates.

2. **Implement Response Validation**: Add checks to ensure AI responses meet your requirements.

3. **Create Fallback Mechanisms**: Have default responses ready if AI parsing fails.

4. **Cache Common Queries**: Reduce AI calls for frequently requested information.

5. **Start Simple**: Begin with just location spots and actions before adding more complex elements.

This implementation gives you a functional world evolution system that integrates with your existing encounter framework while keeping complexity to a minimum. The flat data structures avoid nested complexity, and the focused AI prompts handle the cognitive work of determining what should evolve based on player actions.