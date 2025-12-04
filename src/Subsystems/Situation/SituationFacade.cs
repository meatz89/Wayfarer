
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
    private readonly TimeManager _timeManager;
    private readonly RewardApplicationService _rewardApplicationService;

    public SituationFacade(
        GameWorld gameWorld,
        MessageSystem messageSystem,
        TimeManager timeManager,
        RewardApplicationService rewardApplicationService)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _messageSystem = messageSystem ?? throw new ArgumentNullException(nameof(messageSystem));
        _timeManager = timeManager ?? throw new ArgumentNullException(nameof(timeManager));
        _rewardApplicationService = rewardApplicationService ?? throw new ArgumentNullException(nameof(rewardApplicationService));
    }

    /// <summary>
    /// Select and execute a situation - STRATEGIC LAYER ENTRY POINT
    /// Validates requirements, consumes strategic costs, routes to appropriate subsystem
    /// TWO PILLARS: Uses CompoundRequirement for checks, Consequence for mutations
    /// </summary>
    public async Task<SituationSelectionResult> SelectAndExecuteSituation(Situation situation)
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

        // HIGHLANDER: EntryCost uses negative values for costs
        // Extract positive cost amounts for validation
        int resolveCost = situation.EntryCost.Resolve < 0 ? -situation.EntryCost.Resolve : 0;
        int coinsCost = situation.EntryCost.Coins < 0 ? -situation.EntryCost.Coins : 0;
        int timeCost = situation.EntryCost.TimeSegments;

        // TWO PILLARS: Validate strategic costs via CompoundRequirement
        if (resolveCost > 0 || coinsCost > 0)
        {
            CompoundRequirement costRequirement = new CompoundRequirement();
            OrPath costPath = new OrPath
            {
                ResolveRequired = resolveCost,
                CoinsRequired = coinsCost
            };
            costRequirement.OrPaths.Add(costPath);
            if (!costRequirement.IsAnySatisfied(player, _gameWorld))
            {
                return SituationSelectionResult.Failed("Not enough resources for this situation");
            }
        }

        // HIGHLANDER: Apply EntryCost directly (already negative values)
        // NOTE: Focus/Stamina are TACTICAL costs consumed by challenge facades
        if (resolveCost > 0 || coinsCost > 0)
        {
            Consequence strategicCosts = new Consequence
            {
                Resolve = situation.EntryCost.Resolve,
                Coins = situation.EntryCost.Coins
            };
            await _rewardApplicationService.ApplyConsequence(strategicCosts, situation);

            if (resolveCost > 0)
                _messageSystem.AddSystemMessage($"Resolve consumed: {resolveCost} (now {player.Resolve})", SystemMessageTypes.Warning, null);
            if (coinsCost > 0)
                _messageSystem.AddSystemMessage($"Coins spent: {coinsCost}", SystemMessageTypes.Info, null);
        }

        if (timeCost > 0)
        {
            _timeManager.AdvanceSegments(timeCost);
            _messageSystem.AddSystemMessage($"Time passed: {timeCost} segments", SystemMessageTypes.Info, null);
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

        situation.Lifecycle.CompletedDay = _timeManager.CurrentDay;
        situation.Lifecycle.CompletedTimeBlock = _timeManager.CurrentTimeBlock;
        situation.Lifecycle.CompletedSegment = _timeManager.CurrentSegment;

        return SituationSelectionResult.InstantResolution(situation);
    }

    /// <summary>
    /// Initiate Mental challenge - returns result for GameOrchestrator to route
    /// Mental subsystem consumes tactical costs (Focus) during challenge execution
    /// </summary>
    private SituationSelectionResult InitiateMentalChallenge(Situation situation)
    {
        // Tactical layer receives payload only (deck, target, goal cards)
        // Mental subsystem will consume Focus (tactical cost) during challenge
        // Strategic cost (Resolve) already consumed above

        return SituationSelectionResult.LaunchMentalChallenge(
            situation,
            situation.Deck,
            situation.Location
        );
    }

    /// <summary>
    /// Initiate Physical challenge - returns result for GameOrchestrator to route
    /// Physical subsystem consumes tactical costs (Stamina) during challenge execution
    /// </summary>
    private SituationSelectionResult InitiatePhysicalChallenge(Situation situation)
    {
        // Tactical layer receives payload only
        // Physical subsystem will consume Stamina (tactical cost) during challenge
        // Strategic cost (Resolve) already consumed above

        return SituationSelectionResult.LaunchPhysicalChallenge(
            situation,
            situation.Deck,
            situation.Location
        );
    }

    /// <summary>
    /// Initiate Social challenge - returns result for GameOrchestrator to route
    /// Social subsystem consumes tactical costs during challenge execution
    /// </summary>
    private SituationSelectionResult InitiateSocialChallenge(Situation situation)
    {
        // Tactical layer receives payload only
        // Social subsystem will consume tactical costs during challenge
        // Strategic cost (Resolve) already consumed above

        return SituationSelectionResult.LaunchSocialChallenge(
            situation,
            situation.Deck,
            situation.Npc
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

        situation.Lifecycle.CompletedDay = _timeManager.CurrentDay;
        situation.Lifecycle.CompletedTimeBlock = _timeManager.CurrentTimeBlock;
        situation.Lifecycle.CompletedSegment = _timeManager.CurrentSegment;

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

    /// <summary>
    /// HIGHLANDER: Launch Social challenge - requires NPC
    /// </summary>
    public static SituationSelectionResult LaunchSocialChallenge(
        Situation situation,
        object deck,
        NPC npc)
    {
        return new SituationSelectionResult
        {
            Success = true,
            ResultType = SituationResultType.LaunchChallenge,
            ChallengeType = TacticalSystemType.Social,
            ChallengeSituation = situation,
            ChallengeDeck = deck,
            ChallengeNpc = npc,
            ChallengeLocation = null
        };
    }

    /// <summary>
    /// HIGHLANDER: Launch Mental challenge - requires Location
    /// </summary>
    public static SituationSelectionResult LaunchMentalChallenge(
        Situation situation,
        object deck,
        Location location)
    {
        return new SituationSelectionResult
        {
            Success = true,
            ResultType = SituationResultType.LaunchChallenge,
            ChallengeType = TacticalSystemType.Mental,
            ChallengeSituation = situation,
            ChallengeDeck = deck,
            ChallengeNpc = null,
            ChallengeLocation = location
        };
    }

    /// <summary>
    /// HIGHLANDER: Launch Physical challenge - requires Location
    /// </summary>
    public static SituationSelectionResult LaunchPhysicalChallenge(
        Situation situation,
        object deck,
        Location location)
    {
        return new SituationSelectionResult
        {
            Success = true,
            ResultType = SituationResultType.LaunchChallenge,
            ChallengeType = TacticalSystemType.Physical,
            ChallengeSituation = situation,
            ChallengeDeck = deck,
            ChallengeNpc = null,
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
