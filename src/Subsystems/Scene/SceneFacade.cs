using System;
using System.Collections.Generic;
using System.Linq;
using Wayfarer.GameState;
using Wayfarer.GameState.Enums;

/// <summary>
/// SCENE FACADE - Generates Scene instances for location/NPC interactions
///
/// CRITICAL ARCHITECTURAL PRINCIPLE: Scene as Fundamental Content Unit
///
/// Scene is the ACTIVE DATA SOURCE that populates LocationContent UI:
/// - Locations/NPCs/LocationContent are PERSISTENT (they stay)
/// - Scene is ACTIVE/EPHEMERAL (generated fresh each visit)
/// - Scene evaluates CompoundRequirements (available vs locked separation)
/// - Scene provides perfect information (locked situations visible with requirements)
/// - Scene generates contextual intro narrative (not static description)
///
/// Default/Generic Scene:
/// - If no authored scene exists, generic scene is generated
/// - Generic scene = minimal atmospheric content
/// - Generic scene = all available situations at location (no special filtering)
/// </summary>
public class SceneFacade
{
    private readonly GameWorld _gameWorld;
    private readonly TimeFacade _timeFacade;

    public SceneFacade(
        GameWorld gameWorld,
        TimeFacade timeFacade)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _timeFacade = timeFacade ?? throw new ArgumentNullException(nameof(timeFacade));
    }

    /// <summary>
    /// Generate location scene - ephemeral instance created fresh each visit
    /// Evaluates requirements and separates available vs locked situations
    /// </summary>
    public Scene GenerateLocationScene(string locationId)
    {
        Location location = _gameWorld.GetLocation(locationId);
        if (location == null)
            return null;

        Player player = _gameWorld.GetPlayer();

        // Query ALL situations at this location (from GameWorld.Situations)
        List<Situation> allSituationsAtLocation = _gameWorld.Situations
            .Where(s => s.PlacementLocation?.Id == locationId)
            .Where(s => s.Status != SituationStatus.Completed || s.Repeatable) // Include repeatable completed situations
            .ToList();

        // Also include situations from NPCs present at this location
        List<NPC> npcsAtLocation = _gameWorld.NPCs
            .Where(n => n.Location?.Id == locationId)
            .ToList();

        foreach (NPC npc in npcsAtLocation)
        {
            List<Situation> npcSituations = _gameWorld.Situations
                .Where(s => s.PlacementNpc?.ID == npc.ID)
                .Where(s => s.Status != SituationStatus.Completed || s.Repeatable)
                .ToList();

            allSituationsAtLocation.AddRange(npcSituations);
        }

        // Separate available vs locked based on CompoundRequirement evaluation
        List<Situation> availableSituations = new List<Situation>();
        List<SituationWithLockReason> lockedSituations = new List<SituationWithLockReason>();

        foreach (Situation situation in allSituationsAtLocation)
        {
            // Check if situation has requirements
            if (situation.CompoundRequirement == null || situation.CompoundRequirement.OrPaths.Count == 0)
            {
                // No requirements = always available
                availableSituations.Add(situation);
            }
            else
            {
                // Evaluate requirements
                bool isUnlocked = situation.CompoundRequirement.IsAnySatisfied(player, _gameWorld);

                if (isUnlocked)
                {
                    availableSituations.Add(situation);
                }
                else
                {
                    // Locked - create lock reason with detailed requirement info
                    SituationWithLockReason lockedSituation = CreateLockedSituation(situation, player);
                    lockedSituations.Add(lockedSituation);
                }
            }
        }

        // Generate contextual intro narrative
        string introNarrative = GenerateIntroNarrative(location, npcsAtLocation, player);

        // Create Scene instance (ephemeral, not stored in GameWorld)
        Scene scene = new Scene
        {
            LocationId = locationId,
            DisplayName = location.Name,
            IntroNarrative = introNarrative,
            AvailableSituations = availableSituations,
            LockedSituations = lockedSituations,
            CurrentDay = _timeFacade.GetCurrentDay(),
            CurrentTimeBlock = _timeFacade.GetCurrentTimeBlock(),
            CurrentSegment = _timeFacade.GetCurrentSegment()
        };

        return scene;
    }

    /// <summary>
    /// Create locked situation with detailed requirement explanation
    /// Perfect information pattern: player sees what they need to unlock
    /// Populates strongly-typed requirement gaps for type-specific UI rendering
    /// </summary>
    private SituationWithLockReason CreateLockedSituation(Situation situation, Player player)
    {
        List<string> pathDescriptions = new List<string>();
        SituationWithLockReason lockedSituation = new SituationWithLockReason
        {
            Situation = situation
        };

        // For each OR path, collect unmet requirements
        foreach (OrPath path in situation.CompoundRequirement.OrPaths)
        {
            List<string> pathRequirements = new List<string>();
            bool pathSatisfied = true;

            foreach (NumericRequirement requirement in path.NumericRequirements)
            {
                bool met = requirement.IsSatisfied(player, _gameWorld);
                if (!met)
                {
                    pathSatisfied = false;
                    pathRequirements.Add(requirement.Label);

                    // Populate strongly-typed requirement based on Type
                    PopulateTypedRequirement(lockedSituation, requirement, player);
                }
            }

            if (!pathSatisfied)
            {
                // Path not satisfied - show what's needed
                string pathDescription = path.Label;
                if (pathRequirements.Count > 0)
                {
                    pathDescription += $": {string.Join(" AND ", pathRequirements)}";
                }
                pathDescriptions.Add(pathDescription);
            }
        }

        // Create human-readable lock reason
        lockedSituation.LockReason = pathDescriptions.Count > 0
            ? string.Join(" OR ", pathDescriptions)
            : "Requirements not met";

        return lockedSituation;
    }

    /// <summary>
    /// Populate strongly-typed requirement in SituationWithLockReason based on requirement type
    /// Each type populates different contextual property for UI rendering
    /// </summary>
    private void PopulateTypedRequirement(SituationWithLockReason lockedSituation, NumericRequirement requirement, Player player)
    {
        switch (requirement.Type)
        {
            case "BondStrength":
                // Get NPC for bond requirement
                NPC npc = _gameWorld.NPCs.FirstOrDefault(n => n.ID == requirement.Context);
                int currentBond = npc?.BondStrength ?? 0;

                // Only add if not already present (avoid duplicates from multiple paths)
                if (!lockedSituation.UnmetBonds.Any(b => b.NpcId == requirement.Context))
                {
                    lockedSituation.UnmetBonds.Add(new UnmetBondRequirement
                    {
                        NpcId = requirement.Context,
                        NpcName = npc?.Name ?? "Unknown",
                        Required = requirement.Threshold,
                        Current = currentBond
                    });
                }
                break;

            case "Scale":
                // Get current scale value
                int currentScale = GetPlayerScaleValue(player, requirement.Context);

                if (!lockedSituation.UnmetScales.Any(s => s.ScaleType.ToString() == requirement.Context))
                {
                    if (Enum.TryParse<ScaleType>(requirement.Context, true, out ScaleType scaleType))
                    {
                        lockedSituation.UnmetScales.Add(new UnmetScaleRequirement
                        {
                            ScaleType = scaleType,
                            Required = requirement.Threshold,
                            Current = currentScale
                        });
                    }
                }
                break;

            case "Resolve":
                if (lockedSituation.UnmetResolve.Count == 0)  // Only one resolve requirement needed
                {
                    lockedSituation.UnmetResolve.Add(new UnmetResolveRequirement
                    {
                        Required = requirement.Threshold,
                        Current = player.Resolve
                    });
                }
                break;

            case "Coins":
                if (lockedSituation.UnmetCoins.Count == 0)  // Only one coins requirement needed
                {
                    lockedSituation.UnmetCoins.Add(new UnmetCoinsRequirement
                    {
                        Required = requirement.Threshold,
                        Current = player.Coins
                    });
                }
                break;

            case "CompletedSituations":
                if (lockedSituation.UnmetSituationCount.Count == 0)
                {
                    lockedSituation.UnmetSituationCount.Add(new UnmetSituationCountRequirement
                    {
                        Required = requirement.Threshold,
                        Current = player.CompletedSituationIds.Count
                    });
                }
                break;

            case "Achievement":
                if (!lockedSituation.UnmetAchievements.Any(a => a.AchievementId == requirement.Context))
                {
                    lockedSituation.UnmetAchievements.Add(new UnmetAchievementRequirement
                    {
                        AchievementId = requirement.Context,
                        MustHave = requirement.Threshold > 0
                    });
                }
                break;

            case "State":
                if (!lockedSituation.UnmetStates.Any(s => s.StateType.ToString() == requirement.Context))
                {
                    if (Enum.TryParse<StateType>(requirement.Context, true, out StateType stateType))
                    {
                        lockedSituation.UnmetStates.Add(new UnmetStateRequirement
                        {
                            StateType = stateType,
                            MustHave = requirement.Threshold > 0
                        });
                    }
                }
                break;
        }
    }

    /// <summary>
    /// Get current scale value for player by scale name
    /// </summary>
    private int GetPlayerScaleValue(Player player, string scaleName)
    {
        return scaleName switch
        {
            "Morality" => player.Scales.Morality,
            "Lawfulness" => player.Scales.Lawfulness,
            "Method" => player.Scales.Method,
            "Caution" => player.Scales.Caution,
            "Transparency" => player.Scales.Transparency,
            "Fame" => player.Scales.Fame,
            _ => 0
        };
    }

    /// <summary>
    /// Generate contextual intro narrative based on current game state
    /// Reflects time of day, NPCs present, player state
    /// TODO: Integrate AI narrative service for dynamic generation
    /// </summary>
    private string GenerateIntroNarrative(Location location, List<NPC> npcsPresent, Player player)
    {
        // Simple template-based narrative for now
        // TODO: Replace with AI service integration

        TimeBlocks currentTime = _timeFacade.GetCurrentTimeBlock();
        string timeDescription = currentTime switch
        {
            TimeBlocks.Morning => "The morning sun illuminates",
            TimeBlocks.Midday => "The midday light fills",
            TimeBlocks.Afternoon => "The afternoon shadows lengthen across",
            TimeBlocks.Evening => "The evening light fades over",
            _ => "You find yourself at"
        };

        string narrative = $"{timeDescription} {location.Name}.";

        // Add NPC presence
        if (npcsPresent.Count > 0)
        {
            List<string> npcNames = npcsPresent.Select(n => n.Name).ToList();
            if (npcNames.Count == 1)
            {
                narrative += $" {npcNames[0]} is here.";
            }
            else if (npcNames.Count == 2)
            {
                narrative += $" {npcNames[0]} and {npcNames[1]} are here.";
            }
            else
            {
                string lastNpc = npcNames[npcNames.Count - 1];
                List<string> otherNpcs = npcNames.Take(npcNames.Count - 1).ToList();
                narrative += $" {string.Join(", ", otherNpcs)}, and {lastNpc} are here.";
            }
        }

        return narrative;
    }

    /// <summary>
    /// Generate NPC scene - for direct NPC interaction
    /// Similar to location scene but focused on single NPC's situations
    /// </summary>
    public Scene GenerateNPCScene(string npcId)
    {
        NPC npc = _gameWorld.NPCs.FirstOrDefault(n => n.ID == npcId);
        if (npc == null)
            return null;

        Player player = _gameWorld.GetPlayer();

        // Query situations for this NPC
        List<Situation> allNpcSituations = _gameWorld.Situations
            .Where(s => s.PlacementNpc?.ID == npcId)
            .Where(s => s.Status != SituationStatus.Completed || s.Repeatable)
            .ToList();

        // Separate available vs locked
        List<Situation> availableSituations = new List<Situation>();
        List<SituationWithLockReason> lockedSituations = new List<SituationWithLockReason>();

        foreach (Situation situation in allNpcSituations)
        {
            if (situation.CompoundRequirement == null || situation.CompoundRequirement.OrPaths.Count == 0)
            {
                availableSituations.Add(situation);
            }
            else
            {
                bool isUnlocked = situation.CompoundRequirement.IsAnySatisfied(player, _gameWorld);
                if (isUnlocked)
                {
                    availableSituations.Add(situation);
                }
                else
                {
                    SituationWithLockReason lockedSituation = CreateLockedSituation(situation, player);
                    lockedSituations.Add(lockedSituation);
                }
            }
        }

        // Generate NPC-specific intro
        string introNarrative = $"Conversation with {npc.Name}.";
        // TODO: Add bond strength context, connection state, etc.

        Scene scene = new Scene
        {
            NpcId = npcId,
            DisplayName = $"Conversation with {npc.Name}",
            IntroNarrative = introNarrative,
            AvailableSituations = availableSituations,
            LockedSituations = lockedSituations,
            CurrentDay = _timeFacade.GetCurrentDay(),
            CurrentTimeBlock = _timeFacade.GetCurrentTimeBlock(),
            CurrentSegment = _timeFacade.GetCurrentSegment()
        };

        return scene;
    }
}
