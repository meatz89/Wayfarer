# Wayfarer Dynamic World Progression System

## 1. Overview and Core Concepts

The Wayfarer Dynamic World Progression system enables organic expansion of the game world through narrative encounters. The system allows the game to start with a minimal set of locations and characters while dynamically growing based on player experiences.

### Core Principles:

- **Narrative-Driven Discovery**: As players engage with encounters, they naturally learn about new locations, characters, and opportunities
- **Mechanical Separation**: AI handles narrative generation while the game engine manages all mechanical elements
- **Progressive Expansion**: World grows outward from the player's experiences
- **Elegant Simplicity**: Focus on clean, understandable systems that enhance player immersion

### System Overview:

1. **Encounter Narrative Generation**: AI creates encounter text based on location, present characters, and player history
2. **World Discovery**: AI identifies new locations, characters, and opportunities mentioned in encounters
3. **Entity Development**: When players interact with discovered entities, AI develops their details
4. **State Modification**: AI suggests mechanical changes (relationship values, resources, etc.) for the game to implement

## 2. Game State Elements

### Player State

```csharp
public class PlayerState
{
    // Core identity
    public string Name { get; set; }
    public string Background { get; set; }
    
    // Progression systems
    public int Level { get; set; }
    public int ExperiencePoints { get; set; }
    public Dictionary<string, int> Skills { get; set; } = new Dictionary<string, int>();
    
    // Resources
    public int Money { get; set; }
    public int Food { get; set; }
    public List<Item> Inventory { get; set; } = new List<Item>();
    
    // Encounter resources (reset at start of encounters)
    public int MaxHealth { get; set; }
    public int MaxConcentration { get; set; }
    public int MaxConfidence { get; set; }
    
    // Relationships with characters
    public Dictionary<string, Relationship> Relationships { get; set; } = new Dictionary<string, Relationship>();
    
    // Card collection (player skills)
    public List<Card> UnlockedCards { get; set; } = new List<Card>();
    
    // Location knowledge
    public List<string> DiscoveredLocationIds { get; set; } = new List<string>();
    public string CurrentLocationId { get; set; }
    
    // Travel capabilities
    public List<string> UnlockedTravelMethods { get; set; } = new List<string>();
}

public class Relationship
{
    public string CharacterId { get; set; }
    public int Value { get; set; }  // Numeric relationship value
    public string Status { get; set; }  // Predefined relationship status
    public List<string> SharedHistory { get; set; } = new List<string>();
}
```

### World State

```csharp
public class WorldState
{
    // Core data collections
    public Dictionary<string, Location> Locations { get; set; } = new Dictionary<string, Location>();
    public Dictionary<string, Character> Characters { get; set; } = new Dictionary<string, Character>();
    public Dictionary<string, Opportunity> Opportunities { get; set; } = new Dictionary<string, Opportunity>();
    
    // Game time
    public int CurrentTimeMinutes { get; set; }  // Minutes since game start
    
    // World history
    public List<string> WorldEvents { get; set; } = new List<string>();
}
```

### Location

```csharp
public class Location
{
    // Identity
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    
    // Physical connections
    public List<string> ConnectedLocationIds { get; set; } = new List<string>();
    
    // Interaction spots within the location
    public List<LocationSpot> Spots { get; set; } = new List<LocationSpot>();
    
    // Mechanical properties
    public List<string> EnvironmentalProperties { get; set; } = new List<string>();
    public Dictionary<string, List<string>> TimeProperties { get; set; } = new Dictionary<string, List<string>>();
    public int Difficulty { get; set; }
    
    // Narrative elements (directly from AI)
    public string DetailedDescription { get; set; }
    public string History { get; set; }
    public string PointsOfInterest { get; set; }
    
    // Strategic gameplay elements
    public List<StrategicTag> StrategicTags { get; set; } = new List<StrategicTag>();
    public List<NarrativeTag> NarrativeTags { get; set; } = new List<NarrativeTag>();
    
    // Travel information
    public int TravelTimeMinutes { get; set; }  // Time to reach from connected locations
    public string TravelDescription { get; set; }
}

public class LocationSpot
{
    // Identity
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    
    // Connections
    public string LocationId { get; set; }
    public List<string> ResidentCharacterIds { get; set; } = new List<string>();
    public List<string> AssociatedOpportunityIds { get; set; } = new List<string>();
    
    // Interaction
    public string InteractionType { get; set; }  // "Character", "Quest", "Shop", "Feature", etc.
    public string InteractionDescription { get; set; }
    
    // Visual/positioning data (for map display)
    public string IconType { get; set; }
    public string Position { get; set; }  // "North", "Center", "Southeast", etc.
}
```

### Character

```csharp
public class Character
{
    // Identity
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    
    // Narrative elements
    public string Role { get; set; }  // Not an enum - flexible text
    public string Personality { get; set; }
    public string Background { get; set; }
    public string Appearance { get; set; }
    
    // Location
    public string HomeLocationId { get; set; }
    public List<string> KnownLocationIds { get; set; } = new List<string>();
    
    // Relationships
    public Dictionary<string, string> RelationshipsWithOthers { get; set; } = new Dictionary<string, string>();
    
    // Encounter preferences
    public Dictionary<string, int> ApproachPreferences { get; set; } = new Dictionary<string, int>();
    
    // Player interaction history
    public List<string> InteractionHistory { get; set; } = new List<string>();
}
```

### Opportunity

```csharp
public class Opportunity
{
    // Identity
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    
    // Classification
    public string Type { get; set; }  // "Quest", "Mystery", "Job" - flexible text
    public string Status { get; set; } = "Available";
    
    // Connections
    public string LocationId { get; set; }
    public string LocationSpotId { get; set; }  // Specific spot where this opportunity is found
    public List<string> RelatedCharacterIds { get; set; } = new List<string>();
    
    // Narrative details
    public string DetailedDescription { get; set; }
    public string Challenges { get; set; }
    
    // Rewards (suggested values for engine to implement)
    public Dictionary<string, int> ResourceRewards { get; set; } = new Dictionary<string, int>();
    public List<string> ItemRewards { get; set; } = new List<string>();
    public Dictionary<string, int> RelationshipChanges { get; set; } = new Dictionary<string, int>();
    public List<string> SkillExperience { get; set; } = new List<string>();
}
```

## 3. AI Integration System

The AI integration system handles the communication between the game engine and AI service, focusing on narrative generation and world discovery.

### AI Service Interface

```csharp
public interface IAIService
{
    // Core AI functions
    Task<string> GenerateEncounterNarrative(EncounterContext context);
    Task<DiscoveredEntities> ExtractWorldDiscoveries(string encounterNarrative, WorldContext worldContext);
    Task<EntityDetails> DevelopEntityDetails(string entityType, string entityId, EntityContext entityContext);
    Task<StateChangeRecommendations> GenerateStateChanges(string encounterOutcome, EncounterContext context);
}
```

### Context Objects

```csharp
// Information provided to AI about the current encounter
public class EncounterContext
{
    public Location Location { get; set; }
    public List<Character> PresentCharacters { get; set; }
    public List<Opportunity> AvailableOpportunities { get; set; }
    public string TimeOfDay { get; set; }
    public List<string> CurrentEnvironmentalProperties { get; set; }
    public PlayerSummary Player { get; set; }
    public List<string> PreviousInteractions { get; set; }
}

// Summarized player information for AI context
public class PlayerSummary
{
    public string Name { get; set; }
    public string Background { get; set; }
    public int Level { get; set; }
    public List<string> TopSkills { get; set; }
    public List<Card> TopCards { get; set; }
    public Dictionary<string, string> KeyRelationships { get; set; }
}

// General world information for discovery context
public class WorldContext
{
    public List<string> KnownLocationNames { get; set; }
    public List<string> KnownCharacterNames { get; set; }
    public List<string> RecentWorldEvents { get; set; }
}

// Context for developing specific entities
public class EntityContext
{
    public string Name { get; set; }
    public string CurrentDescription { get; set; }
    public Dictionary<string, string> RelatedEntities { get; set; }
    public List<string> InteractionHistory { get; set; }
}
```

### Response Objects

```csharp
// New entities discovered in encounter narrative
public class DiscoveredEntities
{
    public List<DiscoveredLocation> Locations { get; set; }
    public List<DiscoveredCharacter> Characters { get; set; }
    public List<DiscoveredOpportunity> Opportunities { get; set; }
}

// Minimal location information from discovery
public class DiscoveredLocation
{
    public string Name { get; set; }
    public string Description { get; set; }
    public List<string> ConnectedLocationNames { get; set; }
}

// Detailed entity information from development
public class EntityDetails
{
    public string DetailedDescription { get; set; }
    public string Background { get; set; }
    public Dictionary<string, string> AdditionalProperties { get; set; }
    public List<string> EnvironmentalProperties { get; set; }
    public Dictionary<string, List<string>> TimeBasedProperties { get; set; }
}

// AI recommendations for state changes after encounters
public class StateChangeRecommendations
{
    public Dictionary<string, int> ResourceChanges { get; set; }
    public Dictionary<string, int> RelationshipChanges { get; set; }
    public List<string> SkillExperienceGained { get; set; }
    public List<string> SuggestedWorldEvents { get; set; }
}
```

## 4. World Discovery and Development Process

The world discovery process extracts new entities from encounter narratives and develops them when needed.

### Discovery Workflow

1. After each encounter, the narrative is sent to the AI
2. AI extracts mentions of new locations, characters, and opportunities
3. These entities are added to the world state with minimal information
4. When the player encounters or inquires about these entities, they are fully developed

### Implementation

#### Initial Discovery

```csharp
public async Task ProcessEncounterForDiscoveries(string encounterNarrative)
{
    // Prepare current world context for the AI
    var worldContext = new WorldContext
    {
        KnownLocationNames = worldState.Locations.Values.Select(l => l.Name).ToList(),
        KnownCharacterNames = worldState.Characters.Values.Select(c => c.Name).ToList(),
        RecentWorldEvents = worldState.WorldEvents.TakeLast(5).ToList()
    };
    
    // Extract discoveries from the narrative
    var discoveries = await aiService.ExtractWorldDiscoveries(encounterNarrative, worldContext);
    
    // Process discovered entities
    foreach (var location in discoveries.Locations)
    {
        if (!worldState.Locations.Values.Any(l => l.Name == location.Name))
        {
            // Create minimal location entry
            var newLocation = new Location
            {
                Id = GenerateId("loc"),
                Name = location.Name,
                Description = location.Description,
                ConnectedLocationIds = ResolveLocationNamesTolIds(location.ConnectedLocationNames),
                Spots = new List<LocationSpot>() // Empty spots list to be filled during development
            };
            
            // Add bidirectional connections
            UpdateLocationConnections(newLocation);
            
            // Add to world state
            worldState.Locations.Add(newLocation.Id, newLocation);
            
            // Add to player's discovered locations if directly mentioned in encounter
            if (encounterNarrative.Contains(location.Name))
            {
                playerState.DiscoveredLocationIds.Add(newLocation.Id);
            }
        }
    }
    
    // Process discovered characters
    foreach (var character in discoveries.Characters)
    {
        if (!worldState.Characters.Values.Any(c => c.Name == character.Name))
        {
            // Resolve home location
            string homeLocationId = ResolveLocationNameToId(character.HomeLocationName);
            
            // Create minimal character entry
            var newCharacter = new Character
            {
                Id = GenerateId("char"),
                Name = character.Name,
                Description = character.Description,
                HomeLocationId = homeLocationId,
                Role = character.Role ?? "Unknown"
            };
            
            // Add to world state
            worldState.Characters.Add(newCharacter.Id, newCharacter);
            
            // Add character to a spot at their home location
            AddCharacterToLocationSpot(newCharacter.Id, homeLocationId);
        }
    }
    
    // Process discovered opportunities
    foreach (var opportunity in discoveries.Opportunities)
    {
        if (!worldState.Opportunities.Values.Any(o => o.Name == opportunity.Name))
        {
            // Resolve location and character
            string locationId = ResolveLocationNameToId(opportunity.LocationName);
            List<string> characterIds = ResolveCharacterNamesToIds(opportunity.CharacterNames);
            
            // Create minimal opportunity entry
            var newOpportunity = new Opportunity
            {
                Id = GenerateId("opp"),
                Name = opportunity.Name,
                Description = opportunity.Description,
                Type = opportunity.Type ?? "Quest",
                LocationId = locationId,
                RelatedCharacterIds = characterIds,
                Status = "Available"
            };
            
            // Add to world state
            worldState.Opportunities.Add(newOpportunity.Id, newOpportunity);
            
            // Add opportunity to appropriate location spot
            AddOpportunityToLocationSpot(newOpportunity.Id, locationId, characterIds);
        }
    }
}

// Helper method to add character to a location spot
private void AddCharacterToLocationSpot(string characterId, string locationId)
{
    var location = worldState.Locations[locationId];
    var character = worldState.Characters[characterId];
    
    // Find appropriate existing spot or create new one
    LocationSpot spot = location.Spots.FirstOrDefault(s => 
        s.InteractionType == "Character" && 
        s.Name.Contains(character.Role));
    
    if (spot == null)
    {
        // Create new spot for character
        spot = new LocationSpot
        {
            Id = GenerateId("spot"),
            Name = $"{character.Role} Area",
            Description = $"Where {character.Name} can be found",
            LocationId = locationId,
            InteractionType = "Character",
            InteractionDescription = $"Speak with {character.Name}",
            IconType = GetIconForRole(character.Role),
            Position = AssignRandomPosition()
        };
        location.Spots.Add(spot);
    }
    
    // Add character to spot
    spot.ResidentCharacterIds.Add(characterId);
}

// Helper method to add opportunity to location spot
private void AddOpportunityToLocationSpot(string opportunityId, string locationId, List<string> characterIds)
{
    var location = worldState.Locations[locationId];
    var opportunity = worldState.Opportunities[opportunityId];
    
    LocationSpot spot = null;
    
    // If opportunity is related to a character, use their spot
    if (characterIds.Any())
    {
        string primaryCharacterId = characterIds.First();
        spot = location.Spots.FirstOrDefault(s => 
            s.ResidentCharacterIds.Contains(primaryCharacterId));
    }
    
    // If no character spot found, find or create appropriate spot based on opportunity type
    if (spot == null)
    {
        spot = location.Spots.FirstOrDefault(s => 
            s.InteractionType == opportunity.Type);
            
        if (spot == null)
        {
            // Create new spot for opportunity
            spot = new LocationSpot
            {
                Id = GenerateId("spot"),
                Name = GetSpotNameForOpportunityType(opportunity.Type),
                Description = $"Where {opportunity.Type.ToLower()} opportunities can be found",
                LocationId = locationId,
                InteractionType = opportunity.Type,
                InteractionDescription = $"Explore {opportunity.Type.ToLower()} opportunities",
                IconType = GetIconForOpportunityType(opportunity.Type),
                Position = AssignRandomPosition()
            };
            location.Spots.Add(spot);
        }
    }
    
    // Add opportunity to spot
    spot.AssociatedOpportunityIds.Add(opportunityId);
}
```

#### Entity Development

```csharp
public async Task DevelopLocation(string locationId)
{
    var location = worldState.Locations[locationId];
    
    // Skip if already developed
    if (!string.IsNullOrEmpty(location.DetailedDescription))
        return;
    
    // Create development context
    var entityContext = new EntityContext
    {
        Name = location.Name,
        CurrentDescription = location.Description,
        RelatedEntities = GetRelatedEntities(locationId),
        InteractionHistory = GetLocationInteractionHistory(locationId)
    };
    
    // Get detailed information from AI
    var details = await aiService.DevelopEntityDetails("location", locationId, entityContext);
    
    // Update location with details
    location.DetailedDescription = details.DetailedDescription;
    location.History = details.AdditionalProperties.GetValueOrDefault("History", string.Empty);
    location.PointsOfInterest = details.AdditionalProperties.GetValueOrDefault("PointsOfInterest", string.Empty);
    location.EnvironmentalProperties = details.EnvironmentalProperties;
    location.TimeProperties = details.TimeBasedProperties;
    
    // Generate strategic tags based on environmental properties
    location.StrategicTags = GenerateStrategicTags(details.EnvironmentalProperties);
    
    // Generate location spots based on points of interest
    GenerateLocationSpots(location, details);
    
    // Update world state
    worldState.Locations[locationId] = location;
}

private void GenerateLocationSpots(Location location, EntityDetails details)
{
    // Create spots for existing characters at this location
    var charactersAtLocation = worldState.Characters.Values
        .Where(c => c.HomeLocationId == location.Id)
        .ToList();
        
    foreach (var character in charactersAtLocation)
    {
        AddCharacterToLocationSpot(character.Id, location.Id);
    }
    
    // Create spots for existing opportunities at this location
    var opportunitiesAtLocation = worldState.Opportunities.Values
        .Where(o => o.LocationId == location.Id)
        .ToList();
        
    foreach (var opportunity in opportunitiesAtLocation)
    {
        AddOpportunityToLocationSpot(opportunity.Id, location.Id, opportunity.RelatedCharacterIds);
    }
    
    // Create spots based on points of interest from AI
    if (!string.IsNullOrEmpty(details.AdditionalProperties.GetValueOrDefault("PointsOfInterest", string.Empty)))
    {
        var pointsOfInterest = ParsePointsOfInterest(details.AdditionalProperties["PointsOfInterest"]);
        
        foreach (var poi in pointsOfInterest)
        {
            // Skip if this appears to be a duplicate of an existing spot
            if (location.Spots.Any(s => s.Name.Equals(poi.Name, StringComparison.OrdinalIgnoreCase)))
                continue;
                
            var newSpot = new LocationSpot
            {
                Id = GenerateId("spot"),
                Name = poi.Name,
                Description = poi.Description,
                LocationId = location.Id,
                InteractionType = poi.Type,
                InteractionDescription = $"Examine {poi.Name}",
                IconType = GetIconForPointOfInterestType(poi.Type),
                Position = AssignRandomPosition(),
                ResidentCharacterIds = new List<string>(),
                AssociatedOpportunityIds = new List<string>()
            };
            
            location.Spots.Add(newSpot);
        }
    }
    
    // Ensure we have at least basic spots even if AI didn't provide enough information
    EnsureBasicLocationSpots(location);
}

private List<PointOfInterest> ParsePointsOfInterest(string poiText)
{
    var result = new List<PointOfInterest>();
    
    // Try to parse structured data first
    try
    {
        if (poiText.Contains("{") && poiText.Contains("}"))
        {
            // Attempt to parse JSON
            return JsonConvert.DeserializeObject<List<PointOfInterest>>(poiText);
        }
    }
    catch { /* Continue with text parsing */ }
    
    // Basic text parsing as fallback
    var lines = poiText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
    PointOfInterest currentPoi = null;
    
    foreach (var line in lines)
    {
        string trimmedLine = line.Trim();
        
        if (trimmedLine.EndsWith(":") || (trimmedLine.Length > 0 && char.IsUpper(trimmedLine[0]) && !trimmedLine.StartsWith("-")))
        {
            // This looks like a title, so start a new POI
            if (currentPoi != null)
                result.Add(currentPoi);
                
            currentPoi = new PointOfInterest
            {
                Name = trimmedLine.TrimEnd(':'),
                Type = InferPoiType(trimmedLine),
                Description = ""
            };
        }
        else if (currentPoi != null)
        {
            // Add to the description of the current POI
            currentPoi.Description += trimmedLine + " ";
        }
    }
    
    if (currentPoi != null)
        result.Add(currentPoi);
        
    return result;
}

private void EnsureBasicLocationSpots(Location location)
{
    // Make sure we have at least some basic spots in each location
    string locationType = InferLocationType(location.Name, location.Description);
    
    // Define standard spots based on location type
    var standardSpots = GetStandardSpotsForLocationType(locationType);
    
    foreach (var spot in standardSpots)
    {
        // Skip if we already have a similar spot
        if (location.Spots.Any(s => s.Name.Equals(spot.Name, StringComparison.OrdinalIgnoreCase)))
            continue;
            
        location.Spots.Add(spot);
    }
}

private class PointOfInterest
{
    public string Name { get; set; }
    public string Type { get; set; }
    public string Description { get; set; }
}
```

## 5. Encounter Generation System

The encounter generation system creates narrative content for player interactions with locations and characters.

### Encounter Workflow

1. When player enters a location, the engine prepares context information
2. AI generates encounter narrative based on location, time, present characters, and history
3. Player makes choices (cards) during the encounter
4. AI generates narrative outcomes from player choices
5. AI suggests state changes based on encounter outcome
6. Engine implements state changes and discovers new entities

### Implementation

#### Generating Initial Encounter

```csharp
public async Task<string> GenerateLocationEncounter(string locationId)
{
    var location = worldState.Locations[locationId];
    
    // Develop location if not already done
    if (string.IsNullOrEmpty(location.DetailedDescription))
    {
        await DevelopLocation(locationId);
    }
    
    // Get time of day
    string timeOfDay = GetTimeOfDay(worldState.CurrentTimeMinutes);
    
    // Get environment properties for current time
    List<string> currentProperties = location.TimeProperties.ContainsKey(timeOfDay)
        ? location.TimeProperties[timeOfDay]
        : location.EnvironmentalProperties;
    
    // Find characters at this location
    var presentCharacters = worldState.Characters.Values
        .Where(c => c.HomeLocationId == locationId)
        .ToList();
    
    // Find available opportunities
    var opportunities = worldState.Opportunities.Values
        .Where(o => o.LocationId == locationId && o.Status == "Available")
        .ToList();
    
    // Get player's history with this location
    var previousInteractions = GetLocationInteractionHistory(locationId);
    
    // Create encounter context
    var context = new EncounterContext
    {
        Location = location,
        PresentCharacters = presentCharacters,
        AvailableOpportunities = opportunities,
        TimeOfDay = timeOfDay,
        CurrentEnvironmentalProperties = currentProperties,
        Player = CreatePlayerSummary(),
        PreviousInteractions = previousInteractions
    };
    
    // Generate encounter narrative
    return await aiService.GenerateEncounterNarrative(context);
}
```

#### Processing Encounter Outcome

```csharp
public async Task ProcessEncounterOutcome(string encounterNarrative, string encounterOutcome, EncounterState finalState)
{
    // First, discover new entities
    await ProcessEncounterForDiscoveries(encounterNarrative + "\n" + encounterOutcome);
    
    // Create encounter context
    var context = new EncounterContext { /* populate from encounter state */ };
    
    // Get state change recommendations
    var recommendations = await aiService.GenerateStateChanges(encounterOutcome, context);
    
    // Apply resource changes
    foreach (var resource in recommendations.ResourceChanges)
    {
        ApplyResourceChange(resource.Key, resource.Value);
    }
    
    // Apply relationship changes
    foreach (var relationship in recommendations.RelationshipChanges)
    {
        ApplyRelationshipChange(relationship.Key, relationship.Value);
    }
    
    // Apply skill experience
    foreach (var skill in recommendations.SkillExperienceGained)
    {
        ApplySkillExperience(skill);
    }
    
    // Record world events
    foreach (var worldEvent in recommendations.SuggestedWorldEvents)
    {
        worldState.WorldEvents.Add(worldEvent);
    }
    
    // Record interaction with location and characters
    RecordLocationInteraction(context.Location.Id, encounterOutcome);
    foreach (var character in context.PresentCharacters)
    {
        RecordCharacterInteraction(character.Id, encounterOutcome);
    }
}
```

## 6. Map and Travel System

The map system allows players to navigate between discovered locations with various travel methods.

### Travel Methods

- **Walking**: Default travel method, available to all players
- **Horseback**: Faster travel, unlocked through encounters or purchases
- **Carriage/Wagon**: Fast travel on major roads, unlocked through opportunities
- **Special Methods**: Quest-specific or magical travel methods

### Travel Time Calculation

```csharp
public int CalculateTravelTime(string startLocationId, string endLocationId, string travelMethod)
{
    // Get base travel time between locations
    int baseTravelMinutes = worldState.Locations[endLocationId].TravelTimeMinutes;
    
    // Apply travel method modifier
    double modifier = GetTravelMethodSpeedModifier(travelMethod);
    
    // Calculate final travel time
    int travelMinutes = (int)(baseTravelMinutes / modifier);
    
    return travelMinutes;
}

public double GetTravelMethodSpeedModifier(string travelMethod)
{
    switch (travelMethod)
    {
        case "Walking": return 1.0;
        case "Horseback": return 2.0;
        case "Carriage": return 1.8;
        case "Royal Messenger": return 2.5;
        default: return 1.0;
    }
}
```

### Travel Implementation

```csharp
public async Task TravelToLocation(string destinationLocationId, string travelMethod)
{
    string startLocationId = playerState.CurrentLocationId;
    
    // Calculate travel time
    int travelMinutes = CalculateTravelTime(startLocationId, destinationLocationId, travelMethod);
    
    // Advance game time
    worldState.CurrentTimeMinutes += travelMinutes;
    
    // Consume resources
    ConsumeTravelResources(travelMinutes, travelMethod);
    
    // Determine if travel encounter occurs
    if (ShouldGenerateTravelEncounter(startLocationId, destinationLocationId, travelMethod))
    {
        await GenerateTravelEncounter(startLocationId, destinationLocationId, travelMethod);
    }
    
    // Update player location
    playerState.CurrentLocationId = destinationLocationId;
    
    // Generate arrival encounter
    await GenerateLocationEncounter(destinationLocationId);
}
```

## 7. Opportunity System

The opportunity system handles quests, mysteries, and jobs that emerge from encounters.

### Opportunity Types

- **Quests**: Multi-step adventures with narrative resolution
- **Mysteries**: Investigative opportunities that reveal world lore
- **Jobs**: Repeatable activities for resource gain
- **Events**: Time-limited special opportunities

### Implementation

```csharp
public async Task AdvanceOpportunity(string opportunityId, string progress)
{
    var opportunity = worldState.Opportunities[opportunityId];
    
    // Create context for AI
    var context = new OpportunityContext
    {
        Opportunity = opportunity,
        Progress = progress,
        Player = CreatePlayerSummary(),
        RelatedEntities = GetOpportunityRelatedEntities(opportunityId)
    };
    
    // Get next steps from AI
    var nextStep = await aiService.GenerateOpportunityProgression(context);
    
    // Update opportunity with next step
    opportunity.DetailedDescription = nextStep.UpdatedDescription;
    opportunity.Status = nextStep.NewStatus;
    
    // If completed, apply rewards
    if (nextStep.NewStatus == "Completed")
    {
        ApplyOpportunityRewards(opportunity);
    }
    
    // If failed, apply consequences
    if (nextStep.NewStatus == "Failed")
    {
        ApplyOpportunityConsequences(opportunity);
    }
    
    // Update world state
    worldState.Opportunities[opportunityId] = opportunity;
}
```

## 8. Resource and Relationship System

The resource and relationship systems track player's material possessions and connections with NPCs.

### Resources

- **Health**: Physical wellbeing (encounter resource)
- **Concentration**: Mental focus (encounter resource)
- **Confidence**: Social poise (encounter resource)
- **Money**: Currency for purchases
- **Food**: Sustenance required for travel and survival
- **Items**: Equipment, quest items, and collectibles

### Relationships

- **Value**: Numeric rating from -100 to 100
- **Status**: Text description of relationship ("Stranger", "Friend", "Ally", "Enemy", etc.)
- **History**: Key interactions that affected the relationship

### Implementation

```csharp
public void ApplyRelationshipChange(string characterId, int change)
{
    // Ensure relationship exists
    if (!playerState.Relationships.ContainsKey(characterId))
    {
        playerState.Relationships[characterId] = new Relationship
        {
            CharacterId = characterId,
            Value = 0,
            Status = "Stranger",
            SharedHistory = new List<string>()
        };
    }
    
    // Apply change
    var relationship = playerState.Relationships[characterId];
    relationship.Value = Math.Clamp(relationship.Value + change, -100, 100);
    
    // Update status based on value
    relationship.Status = GetRelationshipStatus(relationship.Value);
}

private string GetRelationshipStatus(int value)
{
    if (value <= -75) return "Nemesis";
    if (value <= -50) return "Enemy";
    if (value <= -25) return "Hostile";
    if (value <= -10) return "Distrusting";
    if (value < 10) return "Neutral";
    if (value < 25) return "Acquaintance";
    if (value < 50) return "Friend";
    if (value < 75) return "Trusted Friend";
    return "Ally";
}
```

## 9. Game Time System

The game time system tracks the passage of time and its effects on the game world.

### Time Tracking

- Time measured in minutes since game start
- Converted to hours/days for player display
- Affects environmental properties of locations

### Implementation

```csharp
public string GetTimeOfDay(int totalMinutes)
{
    // Calculate hours (24-hour clock)
    int totalHours = (totalMinutes / 60) % 24;
    
    if (totalHours >= 5 && totalHours < 12) return "Morning";
    if (totalHours >= 12 && totalHours < 17) return "Afternoon";
    if (totalHours >= 17 && totalHours < 21) return "Evening";
    return "Night";
}

public List<string> GetCurrentEnvironmentalProperties(string locationId)
{
    var location = worldState.Locations[locationId];
    string timeOfDay = GetTimeOfDay(worldState.CurrentTimeMinutes);
    
    // Get time-specific properties if available
    if (location.TimeProperties.ContainsKey(timeOfDay))
    {
        return location.TimeProperties[timeOfDay];
    }
    
    // Fall back to general properties
    return location.EnvironmentalProperties;
}
```

## 10. Integration with Card System

The dynamic world system integrates with the Wayfarer card system by:

1. Providing environmental properties that activate strategic tags
2. Creating context-sensitive encounter choices based on location attributes
3. Interpreting card effectiveness based on character approach preferences

### Card-Environment Interaction

```csharp
public List<StrategicTag> GetActiveStrategicTags(string locationId)
{
    var location = worldState.Locations[locationId];
    var properties = GetCurrentEnvironmentalProperties(locationId);
    
    // Base tags from location
    var activeTags = new List<StrategicTag>(location.StrategicTags);
    
    // Add property-based tags
    foreach (var property in properties)
    {
        var propertyTags = GetTagsForProperty(property);
        activeTags.AddRange(propertyTags);
    }
    
    return activeTags;
}

private List<StrategicTag> GetTagsForProperty(string property)
{
    // Map properties to strategic tags
    switch (property)
    {
        case "Bright":
            return new List<StrategicTag> {
                new StrategicTag { Approach = "Precision", Effect = StrategicEffect.IncreaseMomentum },
                new StrategicTag { Approach = "Concealment", Effect = StrategicEffect.DecreaseMomentum }
            };
        case "Dark":
            return new List<StrategicTag> {
                new StrategicTag { Approach = "Concealment", Effect = StrategicEffect.IncreaseMomentum },
                new StrategicTag { Approach = "Precision", Effect = StrategicEffect.DecreaseMomentum }
            };
        // Additional properties...
        default:
            return new List<StrategicTag>();
    }
}
```

## 11. Location Spot System

The Location Spot system provides specific interaction points within each location where characters, opportunities, and features can be found.

### Core Concepts

- **Fixed Character Placement**: Characters always belong to a specific location and spot
- **Interaction Points**: Players interact with the world through these discrete spots
- **Dynamic Generation**: New locations are automatically populated with appropriate spots
- **Opportunity Association**: Quests and jobs are tied to specific spots within locations

### Spot Types

- **Character Spots**: Where NPCs can be found and interacted with
- **Feature Spots**: Physical features of the location (wells, altars, buildings)
- **Opportunity Spots**: Where quests, jobs, and mysteries can be initiated
- **Shop Spots**: Where trading and commerce take place
- **Service Spots**: Where specific services can be used (inns, taverns, etc.)

### Implementation

```csharp
public List<InteractionOption> GetSpotInteractions(string locationId, string spotId)
{
    var location = worldState.Locations[locationId];
    var spot = location.Spots.FirstOrDefault(s => s.Id == spotId);
    
    if (spot == null)
        return new List<InteractionOption>();
        
    var interactions = new List<InteractionOption>();
    
    // Add character interactions
    foreach (var characterId in spot.ResidentCharacterIds)
    {
        var character = worldState.Characters[characterId];
        interactions.Add(new InteractionOption
        {
            Type = "Character",
            Name = $"Speak with {character.Name}",
            Description = $"Initiate a conversation with {character.Name}, {character.Role}",
            TargetId = characterId
        });
    }
    
    // Add opportunity interactions
    foreach (var opportunityId in spot.AssociatedOpportunityIds)
    {
        var opportunity = worldState.Opportunities[opportunityId];
        
        if (opportunity.Status == "Available" || opportunity.Status == "In Progress")
        {
            interactions.Add(new InteractionOption
            {
                Type = opportunity.Type,
                Name = opportunity.Name,
                Description = opportunity.Description,
                TargetId = opportunityId
            });
        }
    }
    
    // Add spot-specific interactions
    if (spot.InteractionType == "Feature")
    {
        interactions.Add(new InteractionOption
        {
            Type = "Examine",
            Name = $"Examine {spot.Name}",
            Description = spot.InteractionDescription,
            TargetId = spotId
        });
    }
    
    return interactions;
}

public class InteractionOption
{
    public string Type { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string TargetId { get; set; }
}
```

### Spot Assignment Logic

When adding characters or opportunities to a location:

1. **Character Assignment**:
   - Find a spot matching the character's role
   - If none exists, create a new spot appropriate to their role
   - Add the character to that spot's resident list

2. **Opportunity Assignment**:
   - If related to a character, assign to that character's spot
   - Otherwise, find a spot matching the opportunity type
   - If no matching spot exists, create a new spot
   - Add the opportunity to that spot's associated list

## 12. Implementation Notes

### AI Prompt Design

- Keep prompts focused on specific, discrete tasks
- Include only necessary context information
- Use consistent format for similar request types
- Design prompts for predictable, structured responses
- Include fallback parsing for unexpected formats

### Error Handling

- Implement JSON parsing with fallback regex parsing
- Validate all AI-generated content before storing in game state
- Maintain default values for when AI produces invalid data
- Log all parsing failures for later analysis

### Data Persistence

- Save entire world state after significant changes
- Include versioning for future compatibility
- Separate player state from world state for efficient saving

---

This document provides a comprehensive framework for implementing the Wayfarer Dynamic World Progression system while respecting the clear separation between AI-driven narrative and engine-managed game mechanics. The system is designed for flexibility and extensibility as the game evolves.