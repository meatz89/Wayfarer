# Wayfarer Dynamic Location & Encounter Generation Guide

This document outlines the step-by-step process for dynamically generating locations, spots, actions, and encounters within the Wayfarer game system. This process enables the AI to create consistent, balanced, and contextually appropriate game content on the fly.

## 1. Location Generation Process

### 1.1 Determine Location Type

First, identify the appropriate location type based on narrative context. Consider:

- **Environmental Context**: Is this a natural setting, settlement, or specialized building?
- **Relative Position**: How does this location relate to existing map locations?
- **Narrative Role**: What function does this location serve in the narrative?

Example types: Village, Forest, Tavern, Castle, Mine, Ruins, Shrine

### 1.2 Define Core Location Properties

Establish fundamental properties for the location:

```csharp
Location newLocation = new Location
{
    Name = "[Descriptive Name]",
    Description = "[Brief description conveying atmosphere and purpose]",
    Difficulty = [1-3 for POC, with 1 being easiest],
    PlayerKnowledge = [true if already known, false if newly discovered],
    ConnectedLocationIds = [List of locations this connects to]
};
```

Set difficulty based on location's distance from starting area (closer = easier) and inherent danger level.

### 1.3 Establish Environmental Properties

Assign environmental properties that define the location's strategic characteristics:

```csharp
// Base properties regardless of time
newLocation.EnvironmentalProperties = new List<IEnvironmentalProperty>
{
    // Select 1 from each category
    [Illumination property], // Bright, Shadowy, Dark
    [Population property],   // Crowded, Quiet, Isolated
    [Atmosphere property],   // Tense, Formal, Chaotic
    [Economic property],     // Wealthy, Commercial, Humble
    [Physical property]      // Confined, Expansive, Hazardous
};

// Time-specific variations
newLocation.TimeProperties = new Dictionary<string, List<IEnvironmentalProperty>>
{
    { "Morning", [Morning-appropriate properties] },
    { "Afternoon", [Afternoon-appropriate properties] },
    { "Evening", [Evening-appropriate properties] },
    { "Night", [Night-appropriate properties] }
};
```

Ensure properties logically correspond to the location type (e.g., markets are Crowded and Commercial, forests are typically Isolated and potentially Hazardous).

### 1.4 Create Location Spots

Generate 4-6 interaction spots based on location type:

```csharp
List<LocationSpot> spots = new List<LocationSpot>();

// For each key area in the location:
spots.Add(new LocationSpot
{
    Name = "[Area Name]",
    Description = "[Brief description of this specific area]",
    Accessibility = [Public/Communal/Private],
    InteractionType = [Rest/Commercial/Service]
});
```

Spot distribution guidelines:
- **Villages**: Include gathering places, shops, homes, and functional areas
- **Natural Areas**: Include paths, landmarks, shelters, and resource gathering areas
- **Taverns/Inns**: Include common areas, private rooms, service areas, and information centers

For the POC, aim for 4-5 spots per location that represent different interaction types.

## 2. Action Generation Process

### 2.1 Associate Actions with Location Spots

For each spot in the location, create 1-2 appropriate actions:

```csharp
ActionTemplate newAction = new ActionTemplateBuilder()
    .WithName(ActionNames.[appropriate name])
    .WithGoal("[Context-specific goal]")
    .WithComplication("[Interesting complication that creates tension]")
    .WithActionType(BasicActionTypes.[appropriate type])
    .StartsEncounter([linked encounter])
    .ExpendsCoins([cost if applicable])
    .Build();
```

Match action types to spot functions:
- **Commercial spots**: Persuade, Trade, Gather
- **Rest spots**: Rest, Discuss, Study
- **Service spots**: Analyze, Labor, Investigate

### 2.2 Create Contextual Goals and Complications

For each action, the goal and complication must:
- Relate directly to the location's narrative purpose
- Create meaningful tension
- Establish clear stakes
- Suggest multiple viable approaches

Example pattern:
- Goal: "[Achieve something valuable] in [challenging circumstances]"
- Complication: "[Specific obstacle] prevents straightforward success"

### 2.3 Set Resource Costs

Apply appropriate resource costs based on action type:
- Commercial interactions: 2-10 coins depending on value
- Services: 3-8 coins depending on exclusivity
- Social interactions: 0-3 coins for incidentals
- Rest: 4-6 coins for accommodations

## 3. Encounter Generation Process

### 3.1 Define Encounter Parameters

Create balanced encounter templates tied to actions:

```csharp
EncounterTemplate newEncounter = new EncounterTemplate()
{
    Duration = [4-6 turns],
    MaxPressure = [8-13 based on difficulty],
    PartialThreshold = [8-12 based on difficulty],
    StandardThreshold = [12-16 based on difficulty],
    ExceptionalThreshold = [16-20 based on difficulty],
    
    Hostility = [Friendly/Neutral/Hostile based on context],
    
    MomentumBoostApproaches = [2 approaches that work well here],
    DangerousApproaches = [1-2 approaches that are risky here],
    
    PressureReducingFocuses = [2 focuses that help manage pressure],
    MomentumReducingFocuses = [1-2 focuses that are counterproductive],
};
```

Scale parameters based on:
- Location difficulty (higher difficulty = higher thresholds)
- Encounter type (social/intellectual/physical)
- Narrative stakes (higher stakes = higher rewards and risks)

For the POC, aim for encounter durations of 3-6 turns.

### 3.2 Select Strategic Tags

Choose 4 strategic tags that match the location's environmental properties:

```csharp
newEncounter.encounterStrategicTags =
[
    new StrategicTag("[Thematic name for property]", [illumination property]),
    new StrategicTag("[Thematic name for property]", [population property]),
    new StrategicTag("[Thematic name for property]", [atmosphere/economic property]),
    new StrategicTag("[Thematic name for property]", [physical property])
];
```

Strategic tag guidelines:
- Names should thematically reflect both property and location
- Combination should create a coherent strategic landscape
- Should suggest multiple valid approaches to the encounter

### 3.3 Select Narrative Tags

Choose 2-3 narrative tags that create interesting constraints:

```csharp
newEncounter.encounterNarrativeTags =
[
    NarrativeTagRepository.[Tag matching primary approach],
    NarrativeTagRepository.[Tag creating interesting limitation],
    NarrativeTagRepository.[Tag that interacts with environment]
];
```

For narrative tags:
- Include at least one tag that activates with a favored approach in this location
- Include tags that create meaningful strategic choices
- Prefer tags that narratively fit the location's atmosphere

## 4. Integration Considerations

### 4.1 Approach Balance

For the POC with reduced approaches (Dominance, Rapport, Analysis):
- Each location should favor at least one approach
- No single approach should be optimal everywhere
- Ensure disadvantaged approaches still have some utility

### 4.2 Contextual Naming

Create thematically consistent names that convey function:
- Location names should suggest their purpose
- Spot names should indicate their function
- Action names should suggest the interaction type
- Strategic tag names should match location theme

### 4.3 Difficulty Progression

Structure difficulty to create natural progression:
- Starting location (Village): Difficulty 1
- Connected locations (Tavern): Difficulty 1
- Secondary locations (Forest): Difficulty 2
- Distant locations: Difficulty 3

### 4.4 Narrative Consistency

Maintain consistency across generated elements:
- Location description should align with environmental properties
- Spot descriptions should match their accessibility and function
- Action complications should relate to location difficulties
- Encounter parameters should reflect narrative stakes

## 5. Technical Implementation Notes

### 5.1 Enum Updates

When creating new action names or location names, they must be added to the appropriate enums:

```csharp
public enum LocationNames
{
    None = 0,
    Village,
    Forest,
    Tavern,
    [New Location Name]
}

public enum ActionNames
{
    // Existing actions...
    [New Action Name]
}
```

### 5.2 Property Selection

When selecting environmental properties, use the predefined static instances:

```csharp
// Illumination options
Illumination.Bright
Illumination.Shadowy
Illumination.Dark

// Population options
Population.Crowded
Population.Quiet
Population.Isolated

// Atmosphere options
Atmosphere.Tense
Atmosphere.Formal
Atmosphere.Chaotic

// Economic options
Economic.Wealthy
Economic.Commercial
Economic.Humble

// Physical options
Physical.Confined
Physical.Expansive
Physical.Hazardous
```

### 5.3 Approach and Focus Tag Usage

For the POC, use these reduced approaches:

```csharp
// Approaches
ApproachTags.Dominance
ApproachTags.Rapport
ApproachTags.Analysis

// Focuses (all five)
FocusTags.Relationship
FocusTags.Information
FocusTags.Physical
FocusTags.Environment
FocusTags.Resource
```

## 6. Example Generation Process

For reference, here's a complete example of the generation process:

1. Create Forest location
2. Add Forest Path spot with ForestTravel action
3. Define BanditEncounter linked to that action
4. Set appropriate environmental properties (Shadowy, Isolated, etc.)
5. Select narrative tags relevant to a bandit encounter
6. Define balanced encounter parameters

The result is a coherent location with appropriate spots, actions and encounters that provides meaningful gameplay choices while maintaining narrative consistency.
