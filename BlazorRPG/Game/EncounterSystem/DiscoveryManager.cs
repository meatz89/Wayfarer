using Newtonsoft.Json;

public class DiscoveryManager
{
    public WorldState worldState => GameState.WorldState;
    public PlayerState playerState => GameState.PlayerState;

    public GameState GameState { get; }
    public NarrativeService NarrativeService { get; }

    public DiscoveryManager(GameState gameState, NarrativeService aiService)
    {
        GameState = gameState;
        this.NarrativeService = aiService;
    }

    public async Task ProcessEncounterForDiscoveries(string encounterNarrative)
    {
        // Prepare current world context for the AI
        WorldContext worldContext = new WorldContext
        {
            KnownLocations = worldState.GetLocations().Select(l => l.Name).ToList(),
            KnownCharacters = worldState.GetCharacters().Select(c => c.Name).ToList(),
            RecentWorldEvents = worldState.WorldEvents.TakeLast(5).ToList()
        };

        // Extract discoveries from the narrative
        DiscoveredEntities discoveries = await NarrativeService.ExtractWorldDiscoveries(encounterNarrative, worldContext);

        // Process discovered entities
        foreach (DiscoveredLocation location in discoveries.Locations)
        {
            if (!worldState.GetLocations().Any(l => l.Name == location.Name))
            {
                // Create minimal location entry
                Location newLocation = LocationFactory.Create
                (
                    location.Name,
                    location.Description,
                    location.ConnectedLocations,
                    new List<LocationSpot>() // Empty spots list to be filled during development
                );

                // Add bidirectional connections
                UpdateLocationConnections(newLocation);

                // Add to world state
                worldState.AddLocation(newLocation.Name, newLocation);

                // Add to player's discovered locations if directly mentioned in encounter
                if (encounterNarrative.Contains(location.Name))
                {
                    playerState.DiscoveredLocationIds.Add(newLocation.Name);
                }
            }
        }

        // Process discovered characters
        foreach (DiscoveredCharacter character in discoveries.Characters)
        {
            if (!worldState.GetCharacters().Any(c => c.Name == character.Name))
            {
                // Resolve home location
                string homeLocationId = character.HomeLocation;

                // Create minimal character entry
                Character newCharacter = new Character
                {
                    Name = character.Name,
                    Description = character.Description,
                    HomeLocationId = homeLocationId,
                    Role = character.Role ?? "Unknown"
                };

                // Add to world state
                worldState.AddCharacter(newCharacter.Name, newCharacter);

                // Add character to a spot at their home location
                AddCharacterToLocationSpot(newCharacter.Name, homeLocationId);
            }
        }

        // Process discovered opportunities
        foreach (DiscoveredOpportunity opportunity in discoveries.Opportunities)
        {
            if (!worldState.GetOpportunities().Any(o => o.Name == opportunity.Name))
            {
                // Resolve location and character
                string locationId = opportunity.Location;
                List<string> characterIds = opportunity.Characters;

                // Create minimal opportunity entry
                Opportunity newOpportunity = new Opportunity
                {
                    Name = opportunity.Name,
                    Description = opportunity.Description,
                    Type = opportunity.Type ?? "Quest",
                    LocationId = locationId,
                    RelatedCharacterIds = characterIds,
                    Status = "Available"
                };

                // Add to world state
                worldState.AddOpportunity(newOpportunity.Name, newOpportunity);

                // Add opportunity to appropriate location spot
                AddOpportunityToLocationSpot(newOpportunity.Name, locationId, characterIds);
            }
        }
    }


    // Helper method to add character to a location spot
    private void AddCharacterToLocationSpot(string characterId, string locationId)
    {
        Location location = worldState.GetLocation(locationId);
        Character character = worldState.GetCharacter(characterId);

        // Find appropriate existing spot or create new one
        LocationSpot spot = location.Spots.FirstOrDefault(s =>
            s.InteractionType == "Character" &&
            s.Name.Contains(character.Role));

        if (spot == null)
        {
            // Create new spot for character
            spot = LocationSpotFactory.Create
            (
                $"{character.Role} Area",
                $"Where {character.Name} can be found",
                locationId,
                "Character",
                $"Speak with {character.Name}",
                AssignRandomPosition()
            );
            location.Spots.Add(spot);
        }

        // Add character to spot
        spot.ResidentCharacterIds.Add(characterId);
    }

    // Helper method to add opportunity to location spot
    private void AddOpportunityToLocationSpot(string opportunityId, string locationId, List<string> characterIds)
    {
        Location location = worldState.GetLocation(locationId);
        Opportunity opportunity = worldState.GetOpportunity(opportunityId);

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
                spot = LocationSpotFactory.Create
                (
                    opportunity.Type,
                    $"Where {opportunity.Type.ToLower()} opportunities can be found",
                    locationId,
                    opportunity.Type,
                    $"Explore {opportunity.Type.ToLower()} opportunities",
                    AssignRandomPosition()
                );

                location.Spots.Add(spot);
            }
        }

        // Add opportunity to spot
        spot.AssociatedOpportunityIds.Add(opportunityId);
    }


    public async Task DevelopLocation(string locationId)
    {
        if (false)
        {

            Location location = worldState.GetLocation(locationId);

            // Skip if already developed
            if (!string.IsNullOrEmpty(location.DetailedDescription))
                return;

            // Create development context
            EntityContext entityContext = new EntityContext
            {
                Name = location.Name,
                CurrentDescription = location.Description,
                RelatedEntities = GetRelatedEntities(locationId),
                InteractionHistory = GetLocationInteractionHistory(locationId)
            };

            // Get detailed information from AI
            EntityDetails details = await NarrativeService.DevelopEntityDetails("location", locationId, entityContext);

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
    }

    private void GenerateLocationSpots(Location location, EntityDetails details)
    {
        // Create spots for existing characters at this location
        List<Character> charactersAtLocation = worldState.GetCharacters()
            .Where(c => c.HomeLocationId == location.Name)
            .ToList();

        foreach (Character? character in charactersAtLocation)
        {
            AddCharacterToLocationSpot(character.Name, location.Name);
        }

        // Create spots for existing opportunities at this location
        var opportunitiesAtLocation = worldState.GetOpportunities()
            .Where(o => o.LocationId == location.Name)
            .ToList();

        foreach (var opportunity in opportunitiesAtLocation)
        {
            AddOpportunityToLocationSpot(opportunity.Name, location.Name, opportunity.RelatedCharacterIds);
        }

        // Create spots based on points of interest from AI
        if (!string.IsNullOrEmpty(details.AdditionalProperties.GetValueOrDefault("PointsOfInterest", string.Empty)))
        {
            List<PointOfInterest> pointsOfInterest = ParsePointsOfInterest(details.AdditionalProperties["PointsOfInterest"]);

            foreach (PointOfInterest poi in pointsOfInterest)
            {
                // Skip if this appears to be a duplicate of an existing spot
                if (location.Spots.Any(s => s.Name.Equals(poi.Name, StringComparison.OrdinalIgnoreCase)))
                    continue;

                LocationSpot newSpot = LocationSpotFactory.Create
                (
                    poi.Name,
                    poi.Description,
                    location.Name,
                    poi.Type,
                    $"Examine {poi.Name}",
                    AssignRandomPosition(),
                    new List<string>(),
                    new List<string>()
                );

                location.Spots.Add(newSpot);
            }
        }

        // Ensure we have at least basic spots even if AI didn't provide enough information
        EnsureBasicLocationSpots(location);
    }


    private List<PointOfInterest> ParsePointsOfInterest(string poiText)
    {
        List<PointOfInterest> result = new List<PointOfInterest>();

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
        string[] lines = poiText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        PointOfInterest currentPoi = null;

        foreach (string line in lines)
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
        List<LocationSpot> standardSpots = GetStandardSpotsForLocationType(locationType);

        foreach (LocationSpot spot in standardSpots)
        {
            // Skip if we already have a similar spot
            if (location.Spots.Any(s => s.Name.Equals(spot.Name, StringComparison.OrdinalIgnoreCase)))
                continue;

            location.Spots.Add(spot);
        }
    }


    public async Task AdvanceOpportunity(string opportunityId, string progress)
    {
        //Opportunity opportunity = worldState.GetOpportunity(opportunityId);

        //// Create context for AI
        //var context = new OpportunityContext
        //{
        //    Opportunity = opportunity,
        //    Progress = progress,
        //    Player = CreatePlayerSummary(),
        //    RelatedEntities = GetOpportunityRelatedEntities(opportunityId)
        //};

        //// Get next steps from AI
        //var nextStep = await NarrativeService.GenerateOpportunityProgression(context);

        //// Update opportunity with next step
        //opportunity.DetailedDescription = nextStep.UpdatedDescription;
        //opportunity.Status = nextStep.NewStatus;

        //// If completed, apply rewards
        //if (nextStep.NewStatus == "Completed")
        //{
        //    ApplyOpportunityRewards(opportunity);
        //}

        //// If failed, apply consequences
        //if (nextStep.NewStatus == "Failed")
        //{
        //    ApplyOpportunityConsequences(opportunity);
        //}

        //// Update world state
        //worldState.GetOpportunity(opportunityId) = opportunity;
    }


    private void UpdateLocationConnections(Location newLocation)
    {
        throw new NotImplementedException();
    }

    private List<StrategicTag> GenerateStrategicTags(List<IEnvironmentalProperty> environmentalProperties)
    {
        throw new NotImplementedException();
    }

    private string AssignRandomPosition()
    {
        throw new NotImplementedException();
    }
    private string InferPoiType(string trimmedLine)
    {
        throw new NotImplementedException();
    }

    private string InferLocationType(string name, string description)
    {
        throw new NotImplementedException();
    }

    private List<LocationSpot> GetStandardSpotsForLocationType(string locationType)
    {
        throw new NotImplementedException();
    }


    private List<string> GetLocationInteractionHistory(string locationId)
    {
        throw new NotImplementedException();
    }

    private Dictionary<string, string> GetRelatedEntities(string locationId)
    {
        throw new NotImplementedException();
    }

    internal void RecordLocationInteraction(string name, string encounterOutcome)
    {
        throw new NotImplementedException();
    }

    internal void RecordCharacterInteraction(string name, string encounterOutcome)
    {
        throw new NotImplementedException();
    }
}

public class PointOfInterest
{
    public string Name { get; set; }
    public string Type { get; set; }
    public string Description { get; set; }
}