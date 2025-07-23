# Narrative System Usage Example

This document shows how to use the refactored narrative system with JSON-based narratives, effect system, and journal tracking.

## 1. Loading Narratives from JSON

```csharp
// In your initialization code (e.g., GameWorldInitializer)
var narrativeLoader = serviceProvider.GetService<NarrativeLoader>();
var narrativeManager = serviceProvider.GetService<NarrativeManager>();

// Load all narratives from JSON
await narrativeManager.LoadNarrativesFromJsonAsync();

// Or load specific narrative
var tutorialNarrative = await narrativeLoader.LoadNarrativeAsync("wayfarer_tutorial");
```

## 2. Starting a Narrative

```csharp
// Start the tutorial narrative
narrativeManager.StartNarrative("wayfarer_tutorial");

// This will:
// - Apply starting conditions (coins, stamina, location)
// - Show introduction message
// - Record in journal
// - Set appropriate flags
```

## 3. Tracking Progress with Journal

```csharp
var journal = narrativeManager.GetJournal();

// Check if player has done something before
if (journal.HasMadeChoice("wayfarer_tutorial", "help_martha"))
{
    // Player helped Martha in the past
}

// Get statistics
var stats = journal.GetStatistics();
Console.WriteLine($"Narratives completed: {stats.TotalNarrativesCompleted}");
Console.WriteLine($"Completion rate: {stats.CompletionRate:P}");

// Query history
var npcHistory = journal.GetNPCHistory("martha_docks");
var recentEvents = journal.GetRecentHistory(7); // Last 7 days
```

## 4. Creating New Narratives

Create a new JSON file in `Content/Templates/narratives/` folder:

```json
{
  "id": "smuggler_introduction",
  "name": "The Shadow Path",
  "description": "Introduction to the underground economy",
  "introductionMessage": "A hooded figure beckons from the alley...",
  "steps": [
    {
      "id": "meet_contact",
      "name": "Meet the Contact",
      "requiredAction": "Converse",
      "requiredNPC": "shadow_dealer",
      "effects": [
        {
          "type": "SetFlag",
          "parameters": {
            "flag": "shadow_network_unlocked",
            "value": true
          }
        },
        {
          "type": "GrantToken",
          "parameters": {
            "npcId": "shadow_dealer",
            "tokenType": "Shadow",
            "amount": 1
          }
        }
      ]
    }
  ]
}
```

## 5. Adding Custom Effects

```csharp
// Create a custom effect
public class RevealSecretLocationEffect : INarrativeEffect
{
    public string EffectType => "RevealSecretLocation";
    
    public Task<NarrativeEffectResult> Apply(GameWorld world, Dictionary<string, object> parameters)
    {
        var locationId = parameters["locationId"].ToString();
        var spotId = parameters["spotId"].ToString();
        
        // Add the secret spot to the location
        var location = world.GetLocation(locationId);
        // ... add spot logic ...
        
        return Task.FromResult(NarrativeEffectResult.Succeeded($"Revealed {spotId}"));
    }
    
    public bool ValidateParameters(Dictionary<string, object> parameters)
    {
        return parameters.ContainsKey("locationId") && parameters.ContainsKey("spotId");
    }
}

// Register it
effectRegistry.RegisterEffect<RevealSecretLocationEffect>("RevealSecretLocation");
```

## 6. Using Effects in Narratives

```json
{
  "id": "discover_secret",
  "steps": [
    {
      "id": "find_entrance",
      "effects": [
        {
          "type": "RevealSecretLocation",
          "parameters": {
            "locationId": "lower_ward",
            "spotId": "hidden_tavern"
          }
        },
        {
          "type": "ModifyCoins",
          "parameters": {
            "amount": -5
          }
        },
        {
          "type": "CreateObligation",
          "parameters": {
            "id": "tavern_keeper_debt",
            "name": "Tavern Tab",
            "description": "You owe the keeper for information",
            "sourceNpcId": "secret_tavern_keeper",
            "relatedTokenType": "Shadow",
            "constraintEffects": ["NoShadowRefusal"]
          }
        }
      ]
    }
  ]
}
```

## 7. Converting Hard-Coded Narratives

The tutorial narrative has been converted from the hard-coded `NarrativeDefinitions.CreateTutorial()` to `narratives.json`. The benefits:

- **Content creators can modify without code changes**
- **Hot-reload during development** (when implemented)
- **Version control friendly** - see narrative changes in diffs
- **Reusable effects** - combine effects in different ways
- **Data-driven narrative flow** - no code compilation needed

## Migration Path

1. Keep existing `NarrativeBuilder` for programmatic creation
2. Load JSON narratives alongside programmatic ones
3. Gradually convert hard-coded narratives to JSON
4. Use effects system for all narrative consequences
5. Query journal for adaptive narratives based on history