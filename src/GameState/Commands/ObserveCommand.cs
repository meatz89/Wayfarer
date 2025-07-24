/// <summary>
/// Command to observe a location for information and opportunities
/// </summary>
public class ObserveCommand : BaseGameCommand
{
    private readonly string _locationSpotId;
    private readonly NPCRepository _npcRepository;
    private readonly LocationRepository _locationRepository;
    private readonly MessageSystem _messageSystem;


    public ObserveCommand(
        string locationSpotId,
        NPCRepository npcRepository,
        LocationRepository locationRepository,
        MessageSystem messageSystem)
    {
        _locationSpotId = locationSpotId ?? throw new ArgumentNullException(nameof(locationSpotId));
        _npcRepository = npcRepository ?? throw new ArgumentNullException(nameof(npcRepository));
        _locationRepository = locationRepository ?? throw new ArgumentNullException(nameof(locationRepository));
        _messageSystem = messageSystem ?? throw new ArgumentNullException(nameof(messageSystem));

        Description = "Observe location for opportunities";
    }

    public override CommandValidationResult CanExecute(GameWorld gameWorld)
    {
        Player player = gameWorld.GetPlayer();

        // Check if player is at a location
        if (player.CurrentLocationSpot == null)
        {
            return CommandValidationResult.Failure("Not at any location");
        }

        // Check if at the right spot
        if (player.CurrentLocationSpot.SpotID != _locationSpotId)
        {
            return CommandValidationResult.Failure("Not at the specified location");
        }

        // Commands don't check time - that's handled by the service executing them
        // This command requires 1 hour to execute

        return CommandValidationResult.Success();
    }

    public override async Task<CommandResult> ExecuteAsync(GameWorld gameWorld)
    {
        Player player = gameWorld.GetPlayer();
        LocationSpot spot = player.CurrentLocationSpot;
        TimeBlocks currentTime = gameWorld.CurrentTimeBlock;

        // Start observation report
        _messageSystem.AddSystemMessage(
            $"🔍 Observing {spot.Name}...",
            SystemMessageTypes.Info
        );

        var observationData = new
        {
            Location = spot.Name,
            Type = spot.Type.ToString(),
            CurrentTime = currentTime.ToString(),
            Observations = new List<string>()
        };

        // Describe the location
        if (!string.IsNullOrEmpty(spot.Description))
        {
            _messageSystem.AddSystemMessage(
                $"📍 {spot.Description}",
                SystemMessageTypes.Info
            );
            observationData.Observations.Add($"Location: {spot.Description}");
        }

        // List NPCs present
        List<NPC> npcsHere = _npcRepository.GetNPCsForLocationSpotAndTime(spot.SpotID, currentTime);
        if (npcsHere.Any())
        {
            _messageSystem.AddSystemMessage(
                $"👥 People here:",
                SystemMessageTypes.Info
            );

            foreach (NPC npc in npcsHere)
            {
                string activities = GetNPCActivities(npc);
                _messageSystem.AddSystemMessage(
                    $"  • {npc.Name} - {activities}",
                    SystemMessageTypes.Info
                );
                observationData.Observations.Add($"{npc.Name} is here ({activities})");
            }
        }
        else
        {
            _messageSystem.AddSystemMessage(
                "👥 Nobody is here at this time.",
                SystemMessageTypes.Info
            );
            observationData.Observations.Add("No one is present at this time");
        }

        // Check for special features
        if (spot.Type == LocationSpotTypes.FEATURE)
        {
            _messageSystem.AddSystemMessage(
                "🌿 This location has natural resources that could be gathered.",
                SystemMessageTypes.Info
            );
            observationData.Observations.Add("Natural resources available for gathering");
        }

        // Show time-based opportunities
        ShowTimeBasedOpportunities(spot, currentTime);

        // Show connections to other spots
        Location parentLocation = _locationRepository.GetLocation(spot.LocationId);
        if (parentLocation != null)
        {
            int otherSpotCount = parentLocation.LocationSpotIds.Count - 1; // Exclude current spot
            if (otherSpotCount > 0)
            {
                _messageSystem.AddSystemMessage(
                    $"🗺️ {otherSpotCount} other location{(otherSpotCount > 1 ? "s" : "")} nearby in {parentLocation.Name}",
                    SystemMessageTypes.Info
                );
            }
        }

        return CommandResult.Success(
            "Observation completed",
            new
            {
                observationData.Location,
                observationData.Type,
                observationData.CurrentTime,
                observationData.Observations,
                TimeCost = 1  // This command costs 1 hour
            }
        );
    }


    private string GetNPCActivities(NPC npc)
    {
        List<string> activities = new List<string>();

        if (npc.LetterTokenTypes?.Any() == true)
            activities.Add($"handles {string.Join("/", npc.LetterTokenTypes)} letters");

        return activities.Any() ? string.Join(", ", activities) : "resting";
    }

    private void ShowTimeBasedOpportunities(LocationSpot spot, TimeBlocks currentTime)
    {
        // Show what's available at different times
        Dictionary<TimeBlocks, List<string>> timeOpportunities = new Dictionary<TimeBlocks, List<string>>
        {
            { TimeBlocks.Dawn, new List<string> { "Morning letter swaps available", "Early workers arriving" } },
            { TimeBlocks.Morning, new List<string> { "Markets opening", "Most NPCs available" } },
            { TimeBlocks.Afternoon, new List<string> { "Peak activity time", "All services available" } },
            { TimeBlocks.Evening, new List<string> { "Markets closing soon", "Social activities beginning" } },
            { TimeBlocks.Night, new List<string> { "Most shops closed", "Taverns active" } },
            { TimeBlocks.LateNight, new List<string> { "Quiet time", "Shadow dealings possible" } }
        };

        if (timeOpportunities.ContainsKey(currentTime))
        {
            _messageSystem.AddSystemMessage(
                $"⏰ At {currentTime}: {string.Join(", ", timeOpportunities[currentTime])}",
                SystemMessageTypes.Info
            );
        }
    }
}