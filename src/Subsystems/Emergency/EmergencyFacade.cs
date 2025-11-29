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
    private readonly ResourceFacade _resourceFacade;
    private readonly TimeFacade _timeFacade;
    private readonly TokenFacade _tokenFacade;

    public EmergencyFacade(
        GameWorld gameWorld,
        MessageSystem messageSystem,
        ResourceFacade resourceFacade,
        TimeFacade timeFacade,
        TokenFacade tokenFacade)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _messageSystem = messageSystem ?? throw new ArgumentNullException(nameof(messageSystem));
        _resourceFacade = resourceFacade ?? throw new ArgumentNullException(nameof(resourceFacade));
        _timeFacade = timeFacade ?? throw new ArgumentNullException(nameof(timeFacade));
        _tokenFacade = tokenFacade ?? throw new ArgumentNullException(nameof(tokenFacade));
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
        int currentSegment = _timeFacade.GetCurrentSegment();
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
                        SystemMessageTypes.Warning);
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
        int currentSegment = _timeFacade.GetCurrentSegment();
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
            TimeDisplay = _timeFacade.GetTimeString()
        };
    }

    /// <summary>
    /// Select a response to an emergency situation
    /// HIGHLANDER: Accepts ActiveEmergencyState, mutates state not template.
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

        // Validate resources
        if (player.Stamina < response.StaminaCost)
            return EmergencyResult.Failed($"Not enough Stamina (need {response.StaminaCost}, have {player.Stamina})");

        if (player.Health < response.HealthCost)
            return EmergencyResult.Failed($"Not enough Health (need {response.HealthCost}, have {player.Health})");

        if (player.Coins < response.CoinCost)
            return EmergencyResult.Failed($"Not enough Coins (need {response.CoinCost}, have {player.Coins})");

        // Apply costs
        if (response.StaminaCost > 0)
        {
            player.Stamina -= response.StaminaCost;
        }

        if (response.HealthCost > 0)
        {
            _resourceFacade.TakeDamage(response.HealthCost, "Emergency response");
        }

        if (response.CoinCost > 0)
        {
            _resourceFacade.SpendCoins(response.CoinCost, "Emergency response");
        }

        if (response.TimeCost > 0)
        {
            _timeFacade.AdvanceSegments(response.TimeCost);
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

        // Grant/remove coins
        if (outcome.CoinReward != 0)
        {
            if (outcome.CoinReward > 0)
            {
                _resourceFacade.AddCoins(outcome.CoinReward, "Emergency outcome");
            }
            else
            {
                _resourceFacade.SpendCoins(-outcome.CoinReward, "Emergency outcome");
            }
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
