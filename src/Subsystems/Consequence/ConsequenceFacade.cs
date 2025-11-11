/// <summary>
/// CONSEQUENCE FACADE - Applies situation consequences to game state
///
/// Scene-Situation Architecture: Situations have ProjectedConsequences
/// - BondChanges: Modify NPC.BondStrength
/// - ScaleShifts: Modify Player.Scales
/// - StateApplications: Add/Remove Player.ActiveStates
///
/// Called by:
/// - SituationFacade.ResolveInstantSituation() - for instant situations
/// - SituationCompletionHandler.CompleteSituation() - after challenge completion
///
/// NO EVENTS - Synchronous application orchestrated by GameFacade
/// </summary>
public class ConsequenceFacade
{
    private readonly GameWorld _gameWorld;
    private readonly TimeFacade _timeFacade;

    public ConsequenceFacade(GameWorld gameWorld, TimeFacade timeFacade)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _timeFacade = timeFacade ?? throw new ArgumentNullException(nameof(timeFacade));
    }

    /// <summary>
    /// Apply all consequences from a situation to game state
    /// Called after situation completion (success or failure)
    /// </summary>
    public void ApplyConsequences(List<BondChange> bondChanges, List<ScaleShift> scaleShifts, List<StateApplication> stateApplications)
    {
        if (bondChanges != null && bondChanges.Count > 0)
        {
            ApplyBondChanges(bondChanges);
        }

        if (scaleShifts != null && scaleShifts.Count > 0)
        {
            ApplyScaleShifts(scaleShifts);
        }

        if (stateApplications != null && stateApplications.Count > 0)
        {
            ApplyStateApplications(stateApplications);
        }
    }

    /// <summary>
    /// Apply bond strength changes to NPCs
    /// Updates NPC.BondStrength (0-30 range)
    /// </summary>
    public void ApplyBondChanges(List<BondChange> bondChanges)
    {
        Player player = _gameWorld.GetPlayer();

        foreach (BondChange change in bondChanges)
        {
            NPC npc = _gameWorld.NPCs.FirstOrDefault(n => n.ID == change.NpcId);
            if (npc == null)
            {
                // NPC not found - skip this bond change
                continue;
            }

            // Apply delta to bond strength
            int oldBond = npc.BondStrength;
            npc.BondStrength += change.Delta;

            // Clamp to valid range (0-30)
            if (npc.BondStrength < 0)
                npc.BondStrength = 0;
            if (npc.BondStrength > 30)
                npc.BondStrength = 30;

            // TODO: Add narrative feedback about bond change
            // Example: "Your bond with {npc.Name} has {increased|decreased} to {npc.BondStrength}"
        }
    }

    /// <summary>
    /// Apply scale shifts to player behavioral reputation
    /// Updates Player.Scales (-10 to +10 range for each scale)
    /// </summary>
    public void ApplyScaleShifts(List<ScaleShift> scaleShifts)
    {
        Player player = _gameWorld.GetPlayer();

        foreach (ScaleShift shift in scaleShifts)
        {
            int oldValue = GetScaleValue(player, shift.ScaleType);
            int newValue = oldValue + shift.Delta;

            // Clamp to valid range (-10 to +10)
            if (newValue < -10)
                newValue = -10;
            if (newValue > 10)
                newValue = 10;

            SetScaleValue(player, shift.ScaleType, newValue);

            // TODO: Add narrative feedback about scale shift
            // Example: "Your {ScaleType} reputation shifts toward {positive|negative}"
        }
    }

    /// <summary>
    /// Apply state applications to player (add or remove temporary states)
    /// Updates Player.ActiveStates
    /// </summary>
    public void ApplyStateApplications(List<StateApplication> stateApplications)
    {
        Player player = _gameWorld.GetPlayer();

        foreach (StateApplication application in stateApplications)
        {
            if (application.Apply)
            {
                // Add state (if not already active)
                if (!player.ActiveStates.Any(s => s.Type == application.StateType))
                {
                    // TODO: Get state category from StateType (requires StateType → StateCategory mapping)
                    StateCategory category = GetStateCategoryForType(application.StateType);

                    player.ActiveStates.Add(new ActiveState
                    {
                        Type = application.StateType,
                        Category = category,
                        AppliedDay = _timeFacade.GetCurrentDay(),
                        AppliedTimeBlock = _timeFacade.GetCurrentTimeBlock(),
                        AppliedSegment = _timeFacade.GetCurrentSegment(),
                        DurationSegments = application.DurationSegments ?? 48  // Use specified duration or default to 3 days
                    });

                    // TODO: Add narrative feedback about state gained
                    // Example: "You are now {StateType}: {state description}"
                }
            }
            else
            {
                // Remove state (if active)
                ActiveState existingState = player.ActiveStates.FirstOrDefault(s => s.Type == application.StateType);
                if (existingState != null)
                {
                    player.ActiveStates.Remove(existingState);

                    // TODO: Add narrative feedback about state removed
                    // Example: "You are no longer {StateType}"
                }
            }
        }
    }

    /// <summary>
    /// Get current scale value for player
    /// </summary>
    private int GetScaleValue(Player player, ScaleType scaleType)
    {
        return scaleType switch
        {
            ScaleType.Morality => player.Scales.Morality,
            ScaleType.Lawfulness => player.Scales.Lawfulness,
            ScaleType.Method => player.Scales.Method,
            ScaleType.Caution => player.Scales.Caution,
            ScaleType.Transparency => player.Scales.Transparency,
            ScaleType.Fame => player.Scales.Fame,
            _ => 0
        };
    }

    /// <summary>
    /// Set scale value for player
    /// </summary>
    private void SetScaleValue(Player player, ScaleType scaleType, int value)
    {
        switch (scaleType)
        {
            case ScaleType.Morality:
                player.Scales.Morality = value;
                break;
            case ScaleType.Lawfulness:
                player.Scales.Lawfulness = value;
                break;
            case ScaleType.Method:
                player.Scales.Method = value;
                break;
            case ScaleType.Caution:
                player.Scales.Caution = value;
                break;
            case ScaleType.Transparency:
                player.Scales.Transparency = value;
                break;
            case ScaleType.Fame:
                player.Scales.Fame = value;
                break;
        }
    }

    /// <summary>
    /// Get state category for a given state type
    /// Maps StateType to StateCategory (Physical, Mental, Social)
    /// </summary>
    private StateCategory GetStateCategoryForType(StateType stateType)
    {
        // Physical States
        if (stateType == StateType.Wounded ||
            stateType == StateType.Exhausted ||
            stateType == StateType.Sick ||
            stateType == StateType.Injured ||
            stateType == StateType.Starving ||
            stateType == StateType.Armed ||
            stateType == StateType.Provisioned ||
            stateType == StateType.Rested)
        {
            return StateCategory.Physical;
        }

        // Mental States
        if (stateType == StateType.Confused ||
            stateType == StateType.Traumatized ||
            stateType == StateType.Inspired ||
            stateType == StateType.Focused ||
            stateType == StateType.Obsessed)
        {
            return StateCategory.Mental;
        }

        // Social States
        if (stateType == StateType.Wanted ||
            stateType == StateType.Celebrated ||
            stateType == StateType.Shunned ||
            stateType == StateType.Humiliated ||
            stateType == StateType.Disguised ||
            stateType == StateType.Indebted ||
            stateType == StateType.Trusted)
        {
            return StateCategory.Social;
        }

        // Default to Mental if unknown
        return StateCategory.Mental;
    }
}
