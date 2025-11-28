/// <summary>
/// Public facade for emergency situation operations.
/// Handles urgent situations that interrupt normal gameplay and demand immediate response.
/// This is the public interface for the Emergency subsystem.
/// </summary>
public class EmergencyFacade
{
    private readonly GameWorld _gameWorld;
    private readonly MessageSystem _messageSystem;
    private readonly ResourceFacade _resourceFacade;
    private readonly TimeFacade _timeFacade;
    private readonly TokenFacade _tokenFacade;
    private readonly RewardApplicationService _rewardApplicationService;

    public EmergencyFacade(
        GameWorld gameWorld,
        MessageSystem messageSystem,
        ResourceFacade resourceFacade,
        TimeFacade timeFacade,
        TokenFacade tokenFacade,
        RewardApplicationService rewardApplicationService)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _messageSystem = messageSystem ?? throw new ArgumentNullException(nameof(messageSystem));
        _resourceFacade = resourceFacade ?? throw new ArgumentNullException(nameof(resourceFacade));
        _timeFacade = timeFacade ?? throw new ArgumentNullException(nameof(timeFacade));
        _tokenFacade = tokenFacade ?? throw new ArgumentNullException(nameof(tokenFacade));
        _rewardApplicationService = rewardApplicationService ?? throw new ArgumentNullException(nameof(rewardApplicationService));
    }

    /// <summary>
    /// Check for active emergencies that should trigger
    /// Called at sync points (time advancement, location entry)
    /// HIGHLANDER: Use Location objects, not string IDs
    /// </summary>
    public async Task<EmergencySituation> CheckForActiveEmergency()
    {
        int currentDay = _gameWorld.CurrentDay;
        int currentSegment = _timeFacade.GetCurrentSegment();
        Player player = _gameWorld.GetPlayer();
        Location currentLocation = _gameWorld.GetPlayerCurrentLocation();

        foreach (EmergencySituation emergency in _gameWorld.EmergencySituations)
        {
            // Skip if already resolved
            if (emergency.IsResolved) continue;

            // Check if should trigger
            if (!emergency.IsTriggered)
            {
                bool shouldTrigger = false;

                // Check day trigger
                if (emergency.TriggerDay.HasValue && currentDay == emergency.TriggerDay.Value)
                {
                    // Check segment if specified
                    if (!emergency.TriggerSegment.HasValue || currentSegment == emergency.TriggerSegment.Value)
                    {
                        // Check location if specified (HIGHLANDER: object equality)
                        if (emergency.TriggerLocations.Count == 0 ||
                            emergency.TriggerLocations.Contains(currentLocation))
                        {
                            shouldTrigger = true;
                        }
                    }
                }

                if (shouldTrigger)
                {
                    emergency.IsTriggered = true;
                    emergency.TriggeredAtSegment = currentSegment;
                    return emergency;
                }
            }
            else
            {
                // Check if still active (within response window)
                int segmentsPassed = currentSegment - (emergency.TriggeredAtSegment ?? 0);
                if (segmentsPassed < emergency.ResponseWindowSegments)
                {
                    return emergency; // Still active
                }
                else
                {
                    // Window expired - apply ignore outcome
                    await ApplyOutcome(emergency.IgnoreOutcome, emergency);
                    emergency.IsResolved = true;
                    _messageSystem.AddSystemMessage(
                        "Emergency situation expired - " + (emergency.IgnoreOutcome?.NarrativeResult ?? "situation resolved without your involvement"),
                        SystemMessageTypes.Warning);
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Create context for an emergency screen
    /// </summary>
    public EmergencyContext CreateContext(EmergencySituation emergency)
    {
        if (emergency == null)
        {
            return new EmergencyContext
            {
                IsValid = false,
                ErrorMessage = "Emergency situation not found"
            };
        }

        if (!emergency.IsTriggered)
        {
            return new EmergencyContext
            {
                IsValid = false,
                ErrorMessage = "This emergency has not been triggered yet"
            };
        }

        if (emergency.IsResolved)
        {
            return new EmergencyContext
            {
                IsValid = false,
                ErrorMessage = "This emergency has already been resolved"
            };
        }

        Player player = _gameWorld.GetPlayer();
        int currentSegment = _timeFacade.GetCurrentSegment();
        int triggeredAt = emergency.TriggeredAtSegment ?? currentSegment;
        int segmentsPassed = currentSegment - triggeredAt;
        int segmentsRemaining = emergency.ResponseWindowSegments - segmentsPassed;

        return new EmergencyContext
        {
            IsValid = true,
            Emergency = emergency,
            CurrentStamina = player.Stamina,
            MaxStamina = player.MaxStamina,
            CurrentHealth = player.Health,
            MaxHealth = player.MaxHealth,
            CurrentCoins = player.Coins,
            CurrentFocus = player.Focus,
            MaxFocus = player.MaxFocus,
            CurrentSegment = currentSegment,
            ResponseDeadlineSegment = triggeredAt + emergency.ResponseWindowSegments,
            SegmentsRemaining = Math.Max(0, segmentsRemaining),
            LocationName = _gameWorld.GetPlayerCurrentLocation()?.Name ?? "Unknown",
            TimeDisplay = _timeFacade.GetTimeString()
        };
    }

    /// <summary>
    /// Select a response to an emergency situation
    /// TWO PILLARS: Uses CompoundRequirement for availability, Consequence for costs
    /// </summary>
    public async Task<EmergencyResult> SelectResponse(EmergencySituation emergency, EmergencyResponse response)
    {
        if (emergency == null)
            return EmergencyResult.Failed("Emergency situation not found");

        if (!emergency.IsTriggered)
            return EmergencyResult.Failed("Emergency has not been triggered");

        if (emergency.IsResolved)
            return EmergencyResult.Failed("Emergency has already been resolved");

        if (response == null)
            return EmergencyResult.Failed("Response option not found");

        Player player = _gameWorld.GetPlayer();

        // TWO PILLARS: Validate resources via CompoundRequirement
        CompoundRequirement costRequirement = new CompoundRequirement();
        OrPath costPath = new OrPath
        {
            StaminaRequired = response.StaminaCost,
            HealthRequired = response.HealthCost,
            CoinsRequired = response.CoinCost
        };
        costRequirement.OrPaths.Add(costPath);

        if (!costRequirement.IsAnySatisfied(player, _gameWorld))
        {
            return EmergencyResult.Failed("Insufficient resources for this response");
        }

        // TWO PILLARS: Apply costs via Consequence class
        Consequence responseCosts = new Consequence
        {
            Stamina = -response.StaminaCost,
            Health = -response.HealthCost,
            Coins = -response.CoinCost
        };
        await _rewardApplicationService.ApplyConsequence(responseCosts, null);

        if (response.TimeCost > 0)
        {
            _timeFacade.AdvanceSegments(response.TimeCost);
        }

        // Apply outcome
        await ApplyOutcome(response.Outcome, emergency);

        // Mark as resolved
        emergency.IsResolved = true;

        return EmergencyResult.Resolved(response.Outcome?.NarrativeResult ?? "Emergency resolved");
    }

    /// <summary>
    /// Ignore the emergency (apply ignore outcome)
    /// </summary>
    public async Task<EmergencyResult> IgnoreEmergency(EmergencySituation emergency)
    {
        if (emergency == null)
            return EmergencyResult.Failed("Emergency situation not found");

        if (!emergency.IsTriggered)
            return EmergencyResult.Failed("Emergency has not been triggered");

        if (emergency.IsResolved)
            return EmergencyResult.Failed("Emergency has already been resolved");

        // Apply ignore outcome
        await ApplyOutcome(emergency.IgnoreOutcome, emergency);

        // Mark as resolved
        emergency.IsResolved = true;

        return EmergencyResult.Resolved(emergency.IgnoreOutcome?.NarrativeResult ?? "You chose not to respond");
    }

    private async Task ApplyOutcome(EmergencyOutcome outcome, EmergencySituation emergency)
    {
        if (outcome == null) return;

        Player player = _gameWorld.GetPlayer();

        // Apply general relationship change
        if (outcome.RelationshipDelta != 0)
        {
            // Apply to all NPCs
            foreach (NPC npc in _gameWorld.NPCs)
            {
                if (outcome.RelationshipDelta > 0)
                {
                    _tokenFacade.AddTokensToNPC(ConnectionType.Trust, outcome.RelationshipDelta, npc);
                }
                else
                {
                    _tokenFacade.RemoveTokensFromNPC(ConnectionType.Trust, -outcome.RelationshipDelta, npc);
                }
            }

            string deltaText = outcome.RelationshipDelta > 0
                ? $"+{outcome.RelationshipDelta}"
                : outcome.RelationshipDelta.ToString();
            _messageSystem.AddSystemMessage(
                $"General reputation: {deltaText}",
                SystemMessageTypes.Info);
        }

        // Apply specific NPC relationship changes
        // HIGHLANDER: NPCRelationshipDeltas is Dictionary<NPC, int> with object keys
        foreach (KeyValuePair<NPC, int> kvp in outcome.NPCRelationshipDeltas)
        {
            NPC npc = kvp.Key;
            int delta = kvp.Value;

            if (delta > 0)
            {
                _tokenFacade.AddTokensToNPC(ConnectionType.Trust, delta, npc);
            }
            else if (delta < 0)
            {
                _tokenFacade.RemoveTokensFromNPC(ConnectionType.Trust, -delta, npc);
            }

            string deltaText = delta > 0 ? $"+{delta}" : delta.ToString();
            _messageSystem.AddSystemMessage(
                $"Relationship with {npc.Name}: {deltaText}",
                SystemMessageTypes.Info);
        }

        // Grant knowledge
        foreach (string knowledge in outcome.GrantedKnowledge)
        {
            if (!player.Knowledge.Contains(knowledge))
            {
                player.Knowledge.Add(knowledge);
                _messageSystem.AddSystemMessage($"Learned: {knowledge}", SystemMessageTypes.Info);
            }
        }

        foreach (Item item in outcome.GrantedItems)
        {
            _messageSystem.AddSystemMessage($"Received item: {item.Name}", SystemMessageTypes.Info);
        }

        // TWO PILLARS: Grant/remove coins via Consequence class
        if (outcome.CoinReward != 0)
        {
            Consequence coinReward = new Consequence { Coins = outcome.CoinReward };
            await _rewardApplicationService.ApplyConsequence(coinReward, null);
        }

        foreach (Situation situation in outcome.SpawnedSituations)
        {
            _messageSystem.AddSystemMessage($"New situation available: {situation.Name}", SystemMessageTypes.Info);
        }

        // Display narrative result
        if (!string.IsNullOrEmpty(outcome.NarrativeResult))
        {
            _messageSystem.AddSystemMessage(outcome.NarrativeResult, SystemMessageTypes.Info);
        }
    }
}

/// <summary>
/// Result of an emergency response
/// </summary>
public class EmergencyResult
{
    public bool Success { get; set; }
    public string Message { get; set; }

    public static EmergencyResult Failed(string message)
    {
        return new EmergencyResult { Success = false, Message = message };
    }

    public static EmergencyResult Resolved(string message)
    {
        return new EmergencyResult { Success = true, Message = message };
    }
}
