
/// <summary>
/// STRATEGIC LAYER FACADE - Handles situation selection, requirement validation, and strategic cost consumption
///
/// CRITICAL ARCHITECTURAL PRINCIPLE: Strategic/Tactical Layer Separation
///
/// This facade operates at the STRATEGIC layer:
/// - Validates CompoundRequirements (can player unlock this situation?)
/// - Consumes STRATEGIC costs: Resolve, Time, Coins (cost of CHOOSING)
/// - Routes to appropriate subsystem based on InteractionType
/// - Does NOT know about tactical costs (Focus/Stamina - those are consumed by challenge facades)
///
/// Tactical layer (Mental/Physical/Social facades) operates independently:
/// - Receives challenge payload only (deck, target, GoalCards)
/// - Consumes TACTICAL costs: Focus, Stamina (cost of EXECUTING)
/// - Returns success/failure result
/// - Does NOT know about Resolve, CompoundRequirements, Scales, Spawn rules
///
/// This separation enables three situation types:
/// 1. Instant: Strategic cost → Immediate consequences (no challenge)
/// 2. Challenge: Strategic cost → Tactical challenge → Consequences
/// 3. Navigation: Strategic cost → Movement (no challenge)
/// </summary>
public class SituationFacade
{
    private readonly GameWorld _gameWorld;
    private readonly MessageSystem _messageSystem;
    private readonly TimeFacade _timeFacade;
    private readonly MentalFacade _mentalFacade;
    private readonly PhysicalFacade _physicalFacade;
    private readonly SocialFacade _socialFacade;
    private readonly ConsequenceFacade _consequenceFacade;

    public SituationFacade(
        GameWorld gameWorld,
        MessageSystem messageSystem,
        TimeFacade timeFacade,
        MentalFacade mentalFacade,
        PhysicalFacade physicalFacade,
        SocialFacade socialFacade,
        ConsequenceFacade consequenceFacade)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _messageSystem = messageSystem ?? throw new ArgumentNullException(nameof(messageSystem));
        _timeFacade = timeFacade ?? throw new ArgumentNullException(nameof(timeFacade));
        _mentalFacade = mentalFacade ?? throw new ArgumentNullException(nameof(mentalFacade));
        _physicalFacade = physicalFacade ?? throw new ArgumentNullException(nameof(physicalFacade));
        _socialFacade = socialFacade ?? throw new ArgumentNullException(nameof(socialFacade));
        _consequenceFacade = consequenceFacade ?? throw new ArgumentNullException(nameof(consequenceFacade));
    }

    /// <summary>
    /// Select and execute a situation - STRATEGIC LAYER ENTRY POINT
    /// Validates requirements, consumes strategic costs, routes to appropriate subsystem
    /// PHASE 4: Accept Situation object instead of ID
    /// </summary>
    public SituationSelectionResult SelectAndExecuteSituation(Situation situation)
    {
        if (situation == null)
            return SituationSelectionResult.Failed("Situation is null");

        Player player = _gameWorld.GetPlayer();

        // Validate CompoundRequirements (if present)
        if (situation.CompoundRequirement != null && situation.CompoundRequirement.OrPaths.Count > 0)
        {
            bool requirementsMet = situation.CompoundRequirement.IsAnySatisfied(player, _gameWorld);
            if (!requirementsMet)
            {
                return SituationSelectionResult.Failed("Requirements not met for this situation");
            }
        }

        // Validate strategic costs (player has enough resources)
        if (player.Resolve < situation.Costs.Resolve)
        {
            return SituationSelectionResult.Failed($"Not enough Resolve (need {situation.Costs.Resolve}, have {player.Resolve})");
        }

        if (player.Coins < situation.Costs.Coins)
        {
            return SituationSelectionResult.Failed($"Not enough Coins (need {situation.Costs.Coins}, have {player.Coins})");
        }

        // Consume strategic costs (RESOLVE, TIME, COINS)
        // NOTE: Focus/Stamina are TACTICAL costs consumed by challenge facades
        if (situation.Costs.Resolve > 0)
        {
            player.Resolve -= situation.Costs.Resolve;
            _messageSystem.AddSystemMessage($"Resolve consumed: {situation.Costs.Resolve} (now {player.Resolve})", SystemMessageTypes.Warning);
        }

        if (situation.Costs.Coins > 0)
        {
            player.Coins -= situation.Costs.Coins;
            _messageSystem.AddSystemMessage($"Coins spent: {situation.Costs.Coins}", SystemMessageTypes.Info);
        }

        if (situation.Costs.Time > 0)
        {
            _timeFacade.AdvanceSegments(situation.Costs.Time);
            _messageSystem.AddSystemMessage($"Time passed: {situation.Costs.Time} segments", SystemMessageTypes.Info);
        }

        // Mark situation as in progress
        situation.LifecycleStatus = LifecycleStatus.InProgress;

        // Route based on InteractionType
        return situation.InteractionType switch
        {
            SituationInteractionType.Instant => ResolveInstantSituation(situation),
            SituationInteractionType.Mental => InitiateMentalChallenge(situation),
            SituationInteractionType.Physical => InitiatePhysicalChallenge(situation),
            SituationInteractionType.Social => InitiateSocialChallenge(situation),
            SituationInteractionType.Navigation => HandleNavigation(situation),
            _ => SituationSelectionResult.Failed($"Unknown interaction type: {situation.InteractionType}")
        };
    }

    /// <summary>
    /// Resolve instant situation - apply consequences immediately without challenge
    /// Used for quick decisions, work actions, simple events
    /// </summary>
    private SituationSelectionResult ResolveInstantSituation(Situation situation)
    {
        // ProjectedConsequences DELETED - stored projection pattern violates architecture
        // NEW ARCHITECTURE: Consequences applied from Consequence when choice executed

        // Mark situation as completed
        situation.Complete();

        situation.Lifecycle.CompletedDay = _timeFacade.GetCurrentDay();
        situation.Lifecycle.CompletedTimeBlock = _timeFacade.GetCurrentTimeBlock();
        situation.Lifecycle.CompletedSegment = _timeFacade.GetCurrentSegment();

        return SituationSelectionResult.InstantResolution(situation);
    }

    /// <summary>
    /// Initiate Mental challenge - route to MentalFacade with challenge payload
    /// MentalFacade consumes tactical costs (Focus) during challenge execution
    /// PHASE 4: Pass object references instead of IDs
    /// </summary>
    private SituationSelectionResult InitiateMentalChallenge(Situation situation)
    {
        // Tactical layer receives payload only (deck, target, goal cards)
        // MentalFacade will consume Focus (tactical cost) during challenge
        // SituationFacade has already consumed Resolve (strategic cost)

        return SituationSelectionResult.LaunchChallenge(
            TacticalSystemType.Mental,
            situation,
            situation.Deck,
            npc: null,
            location: situation.Location
        );
    }

    /// <summary>
    /// Initiate Physical challenge - route to PhysicalFacade with challenge payload
    /// PhysicalFacade consumes tactical costs (Stamina) during challenge execution
    /// PHASE 4: Pass object references instead of IDs
    /// </summary>
    private SituationSelectionResult InitiatePhysicalChallenge(Situation situation)
    {
        // Tactical layer receives payload only
        // PhysicalFacade will consume Stamina (tactical cost) during challenge
        // SituationFacade has already consumed Resolve (strategic cost)

        return SituationSelectionResult.LaunchChallenge(
            TacticalSystemType.Physical,
            situation,
            situation.Deck,
            npc: null,
            location: situation.Location
        );
    }

    /// <summary>
    /// Initiate Social challenge - route to SocialFacade with challenge payload
    /// SocialFacade consumes tactical costs during challenge execution
    /// PHASE 4: Pass object references instead of IDs
    /// </summary>
    private SituationSelectionResult InitiateSocialChallenge(Situation situation)
    {
        // Tactical layer receives payload only
        // SocialFacade will consume tactical costs during challenge
        // SituationFacade has already consumed Resolve (strategic cost)

        return SituationSelectionResult.LaunchChallenge(
            TacticalSystemType.Social,
            situation,
            situation.Deck,
            npc: situation.Npc,
            location: null
        );
    }

    /// <summary>
    /// Handle navigation situation - move player to destination
    /// May trigger scene at destination
    /// PHASE 4: Pass object reference instead of ID
    /// </summary>
    private SituationSelectionResult HandleNavigation(Situation situation)
    {
        if (situation.NavigationPayload == null)
        {
            return SituationSelectionResult.Failed("Navigation situation missing NavigationPayload");
        }

        // Mark situation as completed
        situation.Complete();

        situation.Lifecycle.CompletedDay = _timeFacade.GetCurrentDay();
        situation.Lifecycle.CompletedTimeBlock = _timeFacade.GetCurrentTimeBlock();
        situation.Lifecycle.CompletedSegment = _timeFacade.GetCurrentSegment();

        // HIGHLANDER: NavigationPayload.Destination is object reference (no ID lookup needed)
        return SituationSelectionResult.Navigation(
            situation.NavigationPayload.Destination,
            situation.NavigationPayload.AutoTriggerScene
        );
    }
}

/// <summary>
/// Result of situation selection at strategic layer
/// Tells UI what to do next (instant resolution, launch challenge, navigate)
/// PHASE 4: ID properties replaced with object references
/// </summary>
public class SituationSelectionResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public SituationResultType ResultType { get; set; }

    // For instant resolution
    public Situation ResolvedSituation { get; set; }

    // For challenge launch - PHASE 4: Object references instead of IDs
    public TacticalSystemType? ChallengeType { get; set; }
    public Situation ChallengeSituation { get; set; }
    public object ChallengeDeck { get; set; }  // MentalChallengeDeck | PhysicalChallengeDeck | SocialChallengeDeck
    public NPC ChallengeNpc { get; set; }  // For Social challenges
    public Location ChallengeLocation { get; set; }  // For Mental/Physical challenges

    // For navigation - PHASE 4: Object reference instead of ID
    public Location NavigationDestination { get; set; }
    public bool NavigationAutoTriggerScene { get; set; }

    public static SituationSelectionResult Failed(string message)
    {
        return new SituationSelectionResult
        {
            Success = false,
            Message = message,
            ResultType = SituationResultType.Failure
        };
    }

    public static SituationSelectionResult InstantResolution(Situation situation)
    {
        return new SituationSelectionResult
        {
            Success = true,
            ResultType = SituationResultType.InstantResolution,
            ResolvedSituation = situation
        };
    }

    public static SituationSelectionResult LaunchChallenge(
        TacticalSystemType challengeType,
        Situation situation,
        object deck,
        NPC npc = null,
        Location location = null)
    {
        return new SituationSelectionResult
        {
            Success = true,
            ResultType = SituationResultType.LaunchChallenge,
            ChallengeType = challengeType,
            ChallengeSituation = situation,
            ChallengeDeck = deck,
            ChallengeNpc = npc,
            ChallengeLocation = location
        };
    }

    public static SituationSelectionResult Navigation(Location destination, bool autoTriggerScene)
    {
        return new SituationSelectionResult
        {
            Success = true,
            ResultType = SituationResultType.Navigation,
            NavigationDestination = destination,
            NavigationAutoTriggerScene = autoTriggerScene
        };
    }
}

public enum SituationResultType
{
    Failure,
    InstantResolution,
    LaunchChallenge,
    Navigation
}
