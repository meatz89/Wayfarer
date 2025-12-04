/// <summary>
/// Public facade for emergency situation operations.
/// Handles urgent situations that interrupt normal gameplay and demand immediate response.
/// This is the public interface for the Emergency subsystem.
/// HIGHLANDER: Mutable state (ActiveEmergencyState) separated from immutable templates (EmergencySituation).
/// </summary>
public class EmergencyFacade
{
    private readonly GameWorld _gameWorld;
    private readonly MessageSystem _messageSystem;
    private readonly TimeManager _timeManager;
    private readonly ConnectionTokenManager _connectionTokenManager;
    private readonly RewardApplicationService _rewardApplicationService;

    public EmergencyFacade(
        GameWorld gameWorld,
        MessageSystem messageSystem,
        TimeManager timeManager,
        ConnectionTokenManager connectionTokenManager,
        RewardApplicationService rewardApplicationService)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _messageSystem = messageSystem ?? throw new ArgumentNullException(nameof(messageSystem));
        _timeManager = timeManager ?? throw new ArgumentNullException(nameof(timeManager));
        _connectionTokenManager = connectionTokenManager ?? throw new ArgumentNullException(nameof(connectionTokenManager));
        _rewardApplicationService = rewardApplicationService ?? throw new ArgumentNullException(nameof(rewardApplicationService));
    }

    /// <summary>
    /// Get or create ActiveEmergencyState for a template.
    /// HIGHLANDER: Mutable state separated from immutable template.
    /// </summary>
    private ActiveEmergencyState GetOrCreateState(EmergencySituation template)
    {
        ActiveEmergencyState state = _gameWorld.EmergencyStates.FirstOrDefault(s => s.Template == template);
        if (state == null)
        {
            state = new ActiveEmergencyState { Template = template };
            _gameWorld.EmergencyStates.Add(state);
        }
        return state;
    }

    /// <summary>
    /// Check for active emergencies that should trigger
    /// Called at sync points (time advancement, location entry)
    /// HIGHLANDER: Use Location objects, not string IDs.
    /// Returns ActiveEmergencyState which holds Template reference + mutable state.
    /// </summary>
    public ActiveEmergencyState CheckForActiveEmergency()
    {
        int currentDay = _gameWorld.CurrentDay;
        int currentSegment = _timeManager.CurrentSegment;
        Player player = _gameWorld.GetPlayer();
        Location currentLocation = _gameWorld.GetPlayerCurrentLocation();

        foreach (EmergencySituation template in _gameWorld.EmergencySituations)
        {
            ActiveEmergencyState state = GetOrCreateState(template);

            // Skip if already resolved
            if (state.IsResolved) continue;

            // Check if should trigger
            if (!state.IsTriggered)
            {
                bool shouldTrigger = false;

                // Check day trigger
                if (template.TriggerDay.HasValue && currentDay == template.TriggerDay.Value)
                {
                    // Check segment if specified
                    if (!template.TriggerSegment.HasValue || currentSegment == template.TriggerSegment.Value)
                    {
                        // Check location if specified (HIGHLANDER: object equality)
                        if (template.TriggerLocations.Count == 0 ||
                            template.TriggerLocations.Contains(currentLocation))
                        {
                            shouldTrigger = true;
                        }
                    }
                }

                if (shouldTrigger)
                {
                    state.IsTriggered = true;
                    state.TriggeredAtSegment = currentSegment;
                    return state;
                }
            }
            else
            {
                // Check if still active (within response window)
                int segmentsPassed = currentSegment - (state.TriggeredAtSegment ?? 0);
                if (segmentsPassed < template.ResponseWindowSegments)
                {
                    return state; // Still active
                }
                else
                {
                    // Window expired - apply ignore outcome
                    ApplyOutcome(template.IgnoreOutcome, template);
                    state.IsResolved = true;
                    _messageSystem.AddSystemMessage(
                        "Emergency situation expired - " + (template.IgnoreOutcome?.NarrativeResult ?? "situation resolved without your involvement"),
                        SystemMessageTypes.Warning,
                        null);
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Create context for an emergency screen
    /// HIGHLANDER: Accepts ActiveEmergencyState, accesses Template for immutable data.
    /// </summary>
    public EmergencyContext CreateContext(ActiveEmergencyState emergencyState)
    {
        if (emergencyState == null)
        {
            return new EmergencyContext
            {
                IsValid = false,
                ErrorMessage = "Emergency situation not found"
            };
        }

        if (!emergencyState.IsTriggered)
        {
            return new EmergencyContext
            {
                IsValid = false,
                ErrorMessage = "This emergency has not been triggered yet"
            };
        }

        if (emergencyState.IsResolved)
        {
            return new EmergencyContext
            {
                IsValid = false,
                ErrorMessage = "This emergency has already been resolved"
            };
        }

        EmergencySituation template = emergencyState.Template;
        Player player = _gameWorld.GetPlayer();
        int currentSegment = _timeManager.CurrentSegment;
        int triggeredAt = emergencyState.TriggeredAtSegment ?? currentSegment;
        int segmentsPassed = currentSegment - triggeredAt;
        int segmentsRemaining = template.ResponseWindowSegments - segmentsPassed;

        return new EmergencyContext
        {
            IsValid = true,
            EmergencyState = emergencyState,
            Emergency = template,
            CurrentStamina = player.Stamina,
            MaxStamina = player.MaxStamina,
            CurrentHealth = player.Health,
            MaxHealth = player.MaxHealth,
            CurrentCoins = player.Coins,
            CurrentFocus = player.Focus,
            MaxFocus = player.MaxFocus,
            CurrentSegment = currentSegment,
            ResponseDeadlineSegment = triggeredAt + template.ResponseWindowSegments,
            SegmentsRemaining = Math.Max(0, segmentsRemaining),
            LocationName = _gameWorld.GetPlayerCurrentLocation()?.Name ?? "Unknown",
            TimeDisplay = _timeManager.GetSegmentDisplay()
        };
    }

    /// <summary>
    /// Select a response to an emergency situation
    /// HIGHLANDER: Accepts ActiveEmergencyState, mutates state not template.
    /// TWO PILLARS: Uses CompoundRequirement for availability, Consequence for costs
    /// </summary>
    public EmergencyResult SelectResponse(ActiveEmergencyState emergencyState, EmergencyResponse response)
    {
        if (emergencyState == null)
            return EmergencyResult.Failed("Emergency situation not found");

        if (!emergencyState.IsTriggered)
            return EmergencyResult.Failed("Emergency has not been triggered");

        if (emergencyState.IsResolved)
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

        // TWO PILLARS: Apply costs directly (sync pattern for emergency responses)
        if (response.StaminaCost > 0)
            player.Stamina = Math.Max(0, player.Stamina - response.StaminaCost);
        if (response.HealthCost > 0)
            player.Health = Math.Max(0, player.Health - response.HealthCost);
        if (response.CoinCost > 0)
            player.Coins -= response.CoinCost;

        if (response.TimeCost > 0)
        {
            _timeManager.AdvanceSegments(response.TimeCost);
        }

        // Apply outcome
        ApplyOutcome(response.Outcome, emergencyState.Template);

        // Mark as resolved (mutate state, not template)
        emergencyState.IsResolved = true;

        return EmergencyResult.Resolved(response.Outcome?.NarrativeResult ?? "Emergency resolved");
    }

    /// <summary>
    /// Ignore the emergency (apply ignore outcome)
    /// HIGHLANDER: Accepts ActiveEmergencyState, mutates state not template.
    /// </summary>
    public EmergencyResult IgnoreEmergency(ActiveEmergencyState emergencyState)
    {
        if (emergencyState == null)
            return EmergencyResult.Failed("Emergency situation not found");

        if (!emergencyState.IsTriggered)
            return EmergencyResult.Failed("Emergency has not been triggered");

        if (emergencyState.IsResolved)
            return EmergencyResult.Failed("Emergency has already been resolved");

        EmergencySituation template = emergencyState.Template;

        // Apply ignore outcome
        ApplyOutcome(template.IgnoreOutcome, template);

        // Mark as resolved (mutate state, not template)
        emergencyState.IsResolved = true;

        return EmergencyResult.Resolved(template.IgnoreOutcome?.NarrativeResult ?? "You chose not to respond");
    }

    private void ApplyOutcome(EmergencyOutcome outcome, EmergencySituation template)
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
                    _connectionTokenManager.AddTokensToNPC(ConnectionType.Trust, outcome.RelationshipDelta, npc);
                }
                else
                {
                    _connectionTokenManager.RemoveTokensFromNPC(ConnectionType.Trust, -outcome.RelationshipDelta, npc);
                }
            }

            string deltaText = outcome.RelationshipDelta > 0
                ? $"+{outcome.RelationshipDelta}"
                : outcome.RelationshipDelta.ToString();
            _messageSystem.AddSystemMessage(
                $"General reputation: {deltaText}",
                SystemMessageTypes.Info,
                null);
        }

        // Apply specific NPC relationship changes
        // DOMAIN COLLECTION PRINCIPLE: List<NPCRelationshipDelta> with strongly-typed entries
        foreach (NPCRelationshipDelta entry in outcome.NPCRelationshipDeltas)
        {
            NPC npc = entry.Npc;
            int delta = entry.Delta;

            if (delta > 0)
            {
                _connectionTokenManager.AddTokensToNPC(ConnectionType.Trust, delta, npc);
            }
            else if (delta < 0)
            {
                _connectionTokenManager.RemoveTokensFromNPC(ConnectionType.Trust, -delta, npc);
            }

            string deltaText = delta > 0 ? $"+{delta}" : delta.ToString();
            _messageSystem.AddSystemMessage(
                $"Relationship with {npc.Name}: {deltaText}",
                SystemMessageTypes.Info,
                null);
        }

        // Grant knowledge
        foreach (string knowledge in outcome.GrantedKnowledge)
        {
            if (!player.Knowledge.Contains(knowledge))
            {
                player.Knowledge.Add(knowledge);
                _messageSystem.AddSystemMessage($"Learned: {knowledge}", SystemMessageTypes.Info, null);
            }
        }

        foreach (Item item in outcome.GrantedItems)
        {
            _messageSystem.AddSystemMessage($"Received item: {item.Name}", SystemMessageTypes.Info, null);
        }

        // Grant/remove coins directly (sync pattern)
        if (outcome.CoinReward != 0)
        {
            player.Coins += outcome.CoinReward;
            string rewardText = outcome.CoinReward > 0
                ? $"+{outcome.CoinReward} coins"
                : $"{outcome.CoinReward} coins";
            _messageSystem.AddSystemMessage(rewardText, SystemMessageTypes.Info, null);
        }

        foreach (Situation situation in outcome.SpawnedSituations)
        {
            _messageSystem.AddSystemMessage($"New situation available: {situation.Name}", SystemMessageTypes.Info, null);
        }

        // Display narrative result
        if (!string.IsNullOrEmpty(outcome.NarrativeResult))
        {
            _messageSystem.AddSystemMessage(outcome.NarrativeResult, SystemMessageTypes.Info, null);
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
