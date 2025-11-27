
/// <summary>
/// Handles situation completion lifecycle - marking complete, removing from ActiveSituations if DeleteOnSuccess, and obligation progress
/// </summary>
public class SituationCompletionHandler
{
    private readonly GameWorld _gameWorld;
    private readonly ObligationActivity _obligationActivity;
    private readonly TimeManager _timeManager;
    private readonly ConsequenceFacade _consequenceFacade;
    private readonly SpawnFacade _spawnFacade;

    public SituationCompletionHandler(
        GameWorld gameWorld,
        ObligationActivity obligationActivity,
        TimeManager timeManager,
        ConsequenceFacade consequenceFacade,
        SpawnFacade spawnFacade)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _obligationActivity = obligationActivity ?? throw new ArgumentNullException(nameof(obligationActivity));
        _timeManager = timeManager ?? throw new ArgumentNullException(nameof(timeManager));
        _consequenceFacade = consequenceFacade ?? throw new ArgumentNullException(nameof(consequenceFacade));
        _spawnFacade = spawnFacade ?? throw new ArgumentNullException(nameof(spawnFacade));
    }

    /// <summary>
    /// Complete a situation - mark as complete, remove from ActiveSituations if DeleteOnSuccess=true, and check obligation progress
    /// </summary>
    /// <param name="situation">Situation that was successfully completed</param>
    public async Task CompleteSituation(Situation situation)
    {
        if (situation == null)
            throw new ArgumentNullException(nameof(situation));

        // Mark situation as complete
        situation.Complete();

        // TRANSITION TRACKING: Set LastChallengeSucceeded if completing from challenge
        // Challenge facades call CompleteSituation when player plays SituationCard (success)
        // This enables OnSuccess/OnFailure transitions in scene state machine
        // NOTE: ChallengeContext.SituationId is DOMAIN VIOLATION - should be Situation object reference
        // Workaround: Check if any challenge context exists (only one challenge can be pending at a time)
        bool? challengeSucceeded = null;
        if (_gameWorld.PendingMentalContext != null ||
            _gameWorld.PendingPhysicalContext != null ||
            _gameWorld.PendingSocialContext != null)
        {
            situation.LastChallengeSucceeded = true;
            challengeSucceeded = true;
            Console.WriteLine($"[SituationCompletionHandler] Challenge succeeded for situation '{situation.Name}'");
        }

        // PROCEDURAL CONTENT TRACING: Mark situation completed in trace
        if (_gameWorld.ProceduralTracer != null && _gameWorld.ProceduralTracer.IsEnabled)
        {
            _gameWorld.ProceduralTracer.MarkSituationCompleted(situation, challengeSucceeded);
        }

        // ProjectedConsequences DELETED - stored projection pattern violates architecture
        // NEW ARCHITECTURE: Consequences applied from Consequence when choice executed, not from Situation

        // Apply rewards from all achieved situation cards (idempotent - only if not already achieved)
        ApplySituationCardRewards(situation);

        // NOTE: ActiveSituationIds DELETED from NPC/Location - situations embedded in scenes
        // DeleteOnSuccess behavior: Situation marked complete, query filters it out
        // No need to remove from separate collections - situations queried from scenes by lifecycle status

        // Check for simple obligation completion (Player.ActiveObligationIds system)
        CheckSimpleObligationCompletion(situation);

        // Check for obligation progress (phase-based ObligationJournal system)
        if (situation.Obligation != null)
        {
            await CheckObligationProgress(situation, situation.Obligation);
        }

        // PHASE 3: Execute SuccessSpawns - recursive situation spawning
        if (situation.SuccessSpawns != null && situation.SuccessSpawns.Count > 0)
        {
            _spawnFacade.ExecuteSpawnRules(situation.SuccessSpawns, situation);
        }

        // PHASE 1.3: Scene state machine - advance to next situation
        // Scene owns its lifecycle, not facades
        if (situation.ParentScene != null)
        {
            Scene scene = situation.ParentScene;
            SceneRoutingDecision routingDecision = scene.AdvanceToNextSituation(situation);

            // Store routing decision on situation for UI to query
            situation.RoutingDecision = routingDecision;

            // SCENE COMPLETE: Update MainStory sequence tracking
            // Player position NEVER changes here - travel uses TravelFacade/MovementValidator
            if (routingDecision == SceneRoutingDecision.SceneComplete)
            {
                // MAINSTORY SEQUENCE INCREMENT: Track player's A-story progress
                // When a MainStory scene completes, increment sequence for next spawn
                if (scene.Category == StoryCategory.MainStory && scene.MainStorySequence.HasValue)
                {
                    Player player = _gameWorld.GetPlayer();
                    player.CurrentMainStorySequence = scene.MainStorySequence.Value;
                    Console.WriteLine($"[SituationCompletionHandler] MainStory scene '{scene.DisplayName}' complete - advanced player sequence to {player.CurrentMainStorySequence}");
                }

                // HIGHLANDER: Player position NEVER changes via scene completion
                // Player travels via TravelFacade (inter-venue) or moves via MovementValidator (intra-venue)
                // Scene completion only updates scene state - player position is their concern, not ours
            }

            // PROCEDURAL CONTENT TRACING: Update scene state if it transitioned to Completed
            if (_gameWorld.ProceduralTracer != null && _gameWorld.ProceduralTracer.IsEnabled)
            {
                _gameWorld.ProceduralTracer.UpdateSceneState(scene, scene.State, DateTime.UtcNow);
            }
        }
    }

    /// <summary>
    /// Check for obligation progress when situation completes
    /// Handles both intro actions (Discovered → Active) and regular situations (phase progression)
    /// </summary>
    private async Task CheckObligationProgress(Situation situation, Obligation obligation)
    {
        // Check if this is an intro action (Discovered → Active transition)
        if (situation != null && situation.IsIntroAction)
        {
            // This is intro completion - activate obligation and spawn Phase 1
            await _obligationActivity.CompleteIntroAction(obligation);
            return;
        }

        // Regular situation completion
        ObligationProgressResult progressResult = await _obligationActivity.CompleteSituation(situation, obligation);

        // Log progress for UI modal display (UI will handle modal)

        // Check if obligation is now complete
        ObligationCompleteResult completeResult = _obligationActivity.CheckObligationCompletion(obligation);
        if (completeResult != null)
        {
            // Obligation complete - UI will display completion modal
        }
    }

    /// <summary>
    /// Handle situation failure - situation always remains in ActiveSituations for retry
    /// PHASE 3: Execute FailureSpawns for recursive situation spawning
    /// TRANSITION TRACKING: Set LastChallengeSucceeded = false for OnFailure transitions
    /// </summary>
    /// <param name="situation">Situation that failed</param>
    public void FailSituation(Situation situation)
    {
        if (situation == null)
            throw new ArgumentNullException(nameof(situation));

        // Mark situation as failed
        situation.LifecycleStatus = LifecycleStatus.Failed;

        // TRANSITION TRACKING: Set LastChallengeSucceeded if failing from challenge
        // Challenge facades call FailSituation when player escapes/abandons (failure)
        // This enables OnFailure transitions in scene state machine
        // NOTE: ChallengeContext.SituationId is DOMAIN VIOLATION - should be Situation object reference
        // Workaround: Check if any challenge context exists (only one challenge can be pending at a time)
        bool? challengeSucceeded = null;
        if (_gameWorld.PendingMentalContext != null ||
            _gameWorld.PendingPhysicalContext != null ||
            _gameWorld.PendingSocialContext != null)
        {
            situation.LastChallengeSucceeded = false;
            challengeSucceeded = false;
            Console.WriteLine($"[SituationCompletionHandler] Challenge failed for situation '{situation.Name}'");
        }

        // PROCEDURAL CONTENT TRACING: Mark situation as failed in trace
        if (_gameWorld.ProceduralTracer != null && _gameWorld.ProceduralTracer.IsEnabled)
        {
            _gameWorld.ProceduralTracer.MarkSituationCompleted(situation, challengeSucceeded);
        }

        // PHASE 3: Execute FailureSpawns - recursive situation spawning on failure
        if (situation.FailureSpawns != null && situation.FailureSpawns.Count > 0)
        {
            _spawnFacade.ExecuteSpawnRules(situation.FailureSpawns, situation);
        }

        // PHASE 1.3: Scene state machine - advance to next situation with OnFailure
        // Scene owns its lifecycle, not facades
        if (situation.ParentScene != null)
        {
            Scene scene = situation.ParentScene;
            SceneRoutingDecision routingDecision = scene.AdvanceToNextSituation(situation);

            // Store routing decision on situation for UI to query
            situation.RoutingDecision = routingDecision;

            // PROCEDURAL CONTENT TRACING: Update scene state if it transitioned to Completed
            if (_gameWorld.ProceduralTracer != null && _gameWorld.ProceduralTracer.IsEnabled)
            {
                _gameWorld.ProceduralTracer.UpdateSceneState(scene, scene.State, DateTime.UtcNow);
            }
        }

        // Situations remain in ActiveSituations on failure regardless of DeleteOnSuccess
        // Player can retry the situation
    }

    /// <summary>
    /// Apply rewards from all achieved situation cards (idempotent - only applies rewards once per card)
    /// </summary>
    private void ApplySituationCardRewards(Situation situation)
    {
        Player player = _gameWorld.GetPlayer();

        foreach (SituationCard situationCard in situation.SituationCards)
        {
            // Only apply rewards if card is achieved and not already rewarded (idempotent)
            if (!situationCard.IsAchieved)
            {
                continue;
            }

            SituationCardRewards rewards = situationCard.Rewards;

            // COINS - direct player currency
            if (rewards.Coins.HasValue && rewards.Coins.Value > 0)
            {
                player.AddCoins(rewards.Coins.Value);
            }

            // ITEM REWARD - add to player inventory by Item object
            // HIGHLANDER: Add object reference, not ID string
            if (rewards.Item != null)
            {
                player.Inventory.Add(rewards.Item); // Object reference ONLY
            }

            // OBLIGATION CUBES - grant to situation's placement Location (localized mastery)
            // ARCHITECTURAL CHANGE: Direct property access (situation owns placement)
            if (rewards.InvestigationCubes.HasValue && rewards.InvestigationCubes.Value > 0)
            {
                if (situation.Location != null)
                {
                    _gameWorld.GrantLocationCubes(situation.Location, rewards.InvestigationCubes.Value);
                    string locationName = situation.Location.Name;
                }
            }

            // STORY CUBES - grant to situation's placement NPC (localized mastery)
            // ARCHITECTURAL CHANGE: Direct property access (situation owns placement)
            if (rewards.StoryCubes.HasValue && rewards.StoryCubes.Value > 0)
            {
                if (situation.Npc != null)
                {
                    _gameWorld.GrantNPCCubes(situation.Npc, rewards.StoryCubes.Value);
                    string npcName = situation.Npc.Name;
                }
            }

            // EXPLORATION CUBES - grant to route (requires route context from situation)
            // ARCHITECTURAL CHANGE: Direct property access (situation owns placement)
            if (rewards.ExplorationCubes.HasValue && rewards.ExplorationCubes.Value > 0)
            {
                if (situation.Route != null)
                {
                    _gameWorld.GrantRouteCubes(situation.Route, rewards.ExplorationCubes.Value);
                    string routeName = situation.Route.Name;
                }
                // Situation without route context - exploration cubes can't be granted
                // This is expected for non-route situations
            }

            // CREATE OBLIGATION - grant StoryCubes to patron NPC (RESOURCE-BASED PATTERN)
            // PRINCIPLE 4: No boolean gates - visibility based on resource thresholds
            if (rewards.CreateObligationData != null)
            {
                CreateObligationReward data = rewards.CreateObligationData;

                // NOTE: data.PatronNpc is object reference (HIGHLANDER)
                NPC patron = data.PatronNpc;
                if (patron == null)
                {
                }
                else
                {
                    // Grant StoryCubes to patron (max 10)
                    int previousCubes = patron.StoryCubes;
                    patron.StoryCubes = Math.Min(10, patron.StoryCubes + data.StoryCubesGranted);
                }
            }

            // OBLIGATION - activate existing obligation (SelfDiscovered or NPCCommissioned)
            // Adds to Player.ActiveObligationIds and sets deadline if NPCCommissioned
            // NOTE: rewards.Obligation is object reference (HIGHLANDER)
            if (rewards.Obligation != null)
            {
                // Activate obligation (adds to Player.ActiveObligationIds, sets deadline if NPCCommissioned)
                // GameWorld.ActivateObligation still uses string obligationId - Template.Id is allowed
                _gameWorld.ActivateObligation(rewards.Obligation.Id, _timeManager);
            }

            // ROUTE SEGMENT UNLOCK - reveal hidden PathCard by setting ExplorationThreshold to 0
            if (rewards.RouteSegmentUnlock != null)
            {
                RouteSegmentUnlock unlock = rewards.RouteSegmentUnlock;
                // ARCHITECTURAL FIX: unlock.Route and unlock.Path are object references (HIGHLANDER)
                RouteOption route = unlock.Route;
                if (route != null)
                {
                    if (unlock.SegmentPosition >= 0 && unlock.SegmentPosition < route.Segments.Count)
                    {
                        RouteSegment segment = route.Segments[unlock.SegmentPosition];

                        // HIGHLANDER: unlock.Path is PathCard object (domain entity)
                        // PathCollection contains PathCardDTOs (parse-time data)
                        // Match by Name to find the DTO to modify
                        PathCardCollectionDTO collection = segment.PathCollection;
                        if (collection != null && unlock.Path != null)
                        {
                            // Find PathCardDTO by matching Name with domain PathCard
                            PathCardDTO pathCard = collection.PathCards.FirstOrDefault(p => p.Name == unlock.Path.Name);
                            if (pathCard != null)
                            {
                                // Reveal hidden PathCard by setting ExplorationThreshold to 0 (always visible)
                                int previousThreshold = pathCard.ExplorationThreshold;
                                pathCard.ExplorationThreshold = 0;
                            }
                        }
                    }
                    else
                    {
                    }
                }
                else
                {
                }
            }

        }
    }


    /// <summary>
    /// Check if completing this situation completes an active obligation (simple Player.ActiveObligationIds system)
    /// Query all situations with matching ObligationId to determine completion
    /// </summary>
    private void CheckSimpleObligationCompletion(Situation completedSituation)
    {
        // Only check if situation is part of an obligation
        if (completedSituation.Obligation == null)
            return;

        // Only check if obligation is in Player.ActiveObligations
        Player player = _gameWorld.GetPlayer();
        if (!player.ActiveObligations.Contains(completedSituation.Obligation))
            return;

        // Find all situations for this obligation (cross-scene query)
        List<Situation> obligationSituations = _gameWorld.Scenes
            .SelectMany(s => s.Situations)
            .Where(sit => sit.Obligation == completedSituation.Obligation)
            .ToList();

        // Check if ALL situations are complete
        bool allSituationsComplete = obligationSituations.All(g => g.IsCompleted);

        if (allSituationsComplete)
        {
            // Complete the obligation (grants rewards, removes from ActiveObligationIds)
            _gameWorld.CompleteObligation(completedSituation.Obligation?.Id, _timeManager);
        }
    }
}
