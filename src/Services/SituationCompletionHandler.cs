
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
        if (_gameWorld.PendingMentalContext?.SituationId == situation.Id ||
            _gameWorld.PendingPhysicalContext?.SituationId == situation.Id ||
            _gameWorld.PendingSocialContext?.SituationId == situation.Id)
        {
            situation.LastChallengeSucceeded = true;
            Console.WriteLine($"[SituationCompletionHandler] Challenge succeeded for situation '{situation.Id}'");
        }

        // Scene-Situation Architecture: Apply ProjectedConsequences (bonds, scales, states)
        _consequenceFacade.ApplyConsequences(
            situation.ProjectedBondChanges,
            situation.ProjectedScaleShifts,
            situation.ProjectedStates
        );

        // Apply rewards from all achieved situation cards (idempotent - only if not already achieved)
        ApplySituationCardRewards(situation);

        // If DeleteOnSuccess, remove from ActiveSituations
        if (situation.DeleteOnSuccess)
        {
            RemoveSituationFromActiveSituations(situation);
        }

        // Check for simple obligation completion (Player.ActiveObligationIds system)
        CheckSimpleObligationCompletion(situation);

        // Check for obligation progress (phase-based ObligationJournal system)
        if (!string.IsNullOrEmpty(situation.Obligation?.Id))
        {
            await CheckObligationProgress(situation.Id, situation.Obligation?.Id);
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

            // AUTOMATIC SPAWNING ORCHESTRATION - Scene completion trigger (cascade spawning + infinite A-story generation)
            // If scene just completed, check for procedural scenes that become eligible
            // CRITICAL: A-story completion triggers next A-scene template generation here
            if (scene.State == SceneState.Completed)
            {
                // TAG-BASED PROGRESSION: Apply GrantsTags and track completion (DDR-002)
                TrackSceneCompletion(scene);

                // NOTE: All generated locations persist forever (no cleanup)
                // Budget validation happens at generation time (fail-fast)
                // Spawn eligible scenes (including A-story continuation)
                await _spawnFacade.CheckAndSpawnEligibleScenes(SpawnTriggerType.Scene, scene.Id);
            }
        }
    }

    /// <summary>
    /// Check for obligation progress when situation completes
    /// Handles both intro actions (Discovered → Active) and regular situations (phase progression)
    /// </summary>
    private async Task CheckObligationProgress(string situationId, string obligationId)
    {
        // Check if this is an intro action (Discovered → Active transition)
        Situation situation = _gameWorld.Scenes
            .SelectMany(s => s.Situations)
            .FirstOrDefault(sit => sit.Id == situationId);
        if (situation != null && situation.IsIntroAction)
        {
            // This is intro completion - activate obligation and spawn Phase 1
            await _obligationActivity.CompleteIntroAction(obligationId);
            return;
        }

        // Regular situation completion
        ObligationProgressResult progressResult = await _obligationActivity.CompleteSituation(situationId, obligationId);

        // Log progress for UI modal display (UI will handle modal)

        // Check if obligation is now complete
        ObligationCompleteResult completeResult = _obligationActivity.CheckObligationCompletion(obligationId);
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
        if (_gameWorld.PendingMentalContext?.SituationId == situation.Id ||
            _gameWorld.PendingPhysicalContext?.SituationId == situation.Id ||
            _gameWorld.PendingSocialContext?.SituationId == situation.Id)
        {
            situation.LastChallengeSucceeded = false;
            Console.WriteLine($"[SituationCompletionHandler] Challenge failed for situation '{situation.Id}'");
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

            // ITEM - add to player inventory by ID
            if (!string.IsNullOrEmpty(rewards.EquipmentId))
            {
                player.Inventory.AddItem(rewards.EquipmentId);
            }

            // OBLIGATION CUBES - grant to situation's placement Location (localized mastery)
            // PHASE 0.2: Query ParentScene for placement using GetPlacementId() helper
            if (rewards.InvestigationCubes.HasValue && rewards.InvestigationCubes.Value > 0)
            {
                string locationId = situation.GetPlacementId(PlacementType.Location);
                if (!string.IsNullOrEmpty(locationId))
                {
                    _gameWorld.GrantLocationCubes(locationId, rewards.InvestigationCubes.Value);
                    Location location = _gameWorld.GetLocation(locationId);
                    string locationName = location.Name;
                }
                else
                {
                }
            }

            // STORY CUBES - grant to situation's placement NPC (localized mastery)
            // PHASE 0.2: Query ParentScene for placement using GetPlacementId() helper
            if (rewards.StoryCubes.HasValue && rewards.StoryCubes.Value > 0)
            {
                string npcId = situation.GetPlacementId(PlacementType.NPC);
                if (!string.IsNullOrEmpty(npcId))
                {
                    _gameWorld.GrantNPCCubes(npcId, rewards.StoryCubes.Value);
                    NPC npc = _gameWorld.NPCs.FirstOrDefault(n => n.ID == npcId);
                    string npcName = npc.Name;
                }
                else
                {
                }
            }

            // EXPLORATION CUBES - grant to route (requires route context from situation)
            // PHASE 0.2: Query ParentScene for placement using GetPlacementId() helper
            if (rewards.ExplorationCubes.HasValue && rewards.ExplorationCubes.Value > 0)
            {
                string routeId = situation.GetPlacementId(PlacementType.Route);
                if (!string.IsNullOrEmpty(routeId))
                {
                    _gameWorld.GrantRouteCubes(routeId, rewards.ExplorationCubes.Value);
                    RouteOption route = _gameWorld.Routes.FirstOrDefault(r => r.Id == routeId);
                    string routeName = route?.Name ?? "Unknown Route";
                }
                else
                {
                    // Situation without route context - exploration cubes can't be granted
                    // This is expected for non-route situations
                }
            }

            // CREATE OBLIGATION - grant StoryCubes to patron NPC (RESOURCE-BASED PATTERN)
            // PRINCIPLE 4: No boolean gates - visibility based on resource thresholds
            if (rewards.CreateObligationData != null)
            {
                CreateObligationReward data = rewards.CreateObligationData;

                NPC patron = _gameWorld.NPCs.FirstOrDefault(n => n.ID == data.PatronNpcId);
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

            // OBLIGATION ID - activate existing obligation (SelfDiscovered or NPCCommissioned)
            // Adds to Player.ActiveObligationIds and sets deadline if NPCCommissioned
            if (!string.IsNullOrEmpty(rewards.ObligationId))
            {
                Obligation obligation = _gameWorld.Obligations.FirstOrDefault(i => i.Id == rewards.ObligationId);
                if (obligation != null)
                {
                    // Activate obligation (adds to Player.ActiveObligationIds, sets deadline if NPCCommissioned)
                    _gameWorld.ActivateObligation(rewards.ObligationId, _timeManager);
                }
            }

            // ROUTE SEGMENT UNLOCK - reveal hidden PathCard by setting ExplorationThreshold to 0
            if (rewards.RouteSegmentUnlock != null)
            {
                RouteSegmentUnlock unlock = rewards.RouteSegmentUnlock;
                RouteOption route = _gameWorld.Routes.FirstOrDefault(r => r.Id == unlock.RouteId);
                if (route != null)
                {
                    if (unlock.SegmentPosition >= 0 && unlock.SegmentPosition < route.Segments.Count)
                    {
                        RouteSegment segment = route.Segments[unlock.SegmentPosition];

                        // Get PathCard collection for this segment
                        if (!string.IsNullOrEmpty(segment.PathCollectionId))
                        {
                            PathCardCollectionDTO collection = _gameWorld.GetPathCollection(segment.PathCollectionId);
                            if (collection != null)
                            {
                                PathCardDTO pathCard = collection.PathCards.FirstOrDefault(p => p.Id == unlock.PathId);
                                if (pathCard != null)
                                {
                                    // Reveal hidden PathCard by setting ExplorationThreshold to 0 (always visible)
                                    int previousThreshold = pathCard.ExplorationThreshold;
                                    pathCard.ExplorationThreshold = 0;
                                }
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
    /// Remove situation from NPC.ActiveSituationIds or Location.ActiveSituationIds
    /// PHASE 0.2: Query ParentScene for placement using GetPlacementId() helper
    /// </summary>
    private void RemoveSituationFromActiveSituations(Situation situation)
    {
        string npcId = situation.GetPlacementId(PlacementType.NPC);
        if (!string.IsNullOrEmpty(npcId))
        {
            // Remove from NPC.ActiveSituationIds
            NPC npc = _gameWorld.NPCs.FirstOrDefault(n => n.ID == npcId);
            if (npc != null)
            {
                if (npc.ActiveSituationIds.Contains(situation.Id))
                {
                    npc.ActiveSituationIds.Remove(situation.Id);
                }
            }
        }
        else
        {
            string locationId = situation.GetPlacementId(PlacementType.Location);
            if (!string.IsNullOrEmpty(locationId))
            {
                // Remove from Location.ActiveSituationIds
                Location location = _gameWorld.GetLocation(locationId);
                if (location != null)
                {
                    if (location.ActiveSituationIds.Contains(situation.Id))
                    {
                        location.ActiveSituationIds.Remove(situation.Id);
                    }
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
        if (string.IsNullOrEmpty(completedSituation.Obligation?.Id))
            return;

        // Only check if obligation is in Player.ActiveObligationIds
        Player player = _gameWorld.GetPlayer();
        if (!player.ActiveObligationIds.Contains(completedSituation.Obligation?.Id))
            return;

        // Find all situations for this obligation (cross-scene query)
        List<Situation> obligationSituations = _gameWorld.Scenes
            .SelectMany(s => s.Situations)
            .Where(sit => sit.Obligation?.Id == completedSituation.Obligation?.Id)
            .ToList();

        // Check if ALL situations are complete
        bool allSituationsComplete = obligationSituations.All(g => g.IsCompleted);

        if (allSituationsComplete)
        {
            // Complete the obligation (grants rewards, removes from ActiveObligationIds)
            _gameWorld.CompleteObligation(completedSituation.Obligation?.Id, _timeManager);
        }
    }

    /// <summary>
    /// Track scene completion: apply GrantsTags and record completion
    /// TAG-BASED PROGRESSION SYSTEM (DDR-002): Enables flexible branching via tag unlock graphs
    /// </summary>
    private void TrackSceneCompletion(Scene scene)
    {
        if (scene == null || scene.Template == null)
            return;

        Player player = _gameWorld.GetPlayer();

        // Apply GrantsTags to player (tag-based progression unlock)
        if (scene.Template.GrantsTags != null && scene.Template.GrantsTags.Count > 0)
        {
            foreach (string tag in scene.Template.GrantsTags)
            {
                if (!player.GrantedTags.Contains(tag))
                {
                    player.GrantedTags.Add(tag);
                    Console.WriteLine($"[SituationCompletionHandler] Granted tag '{tag}' to player from scene '{scene.Id}'");
                }
            }
        }

        // Track scene completion (for CompletedScenes spawn condition checking)
        if (!string.IsNullOrEmpty(scene.TemplateId) && !player.CompletedSceneIds.Contains(scene.TemplateId))
        {
            player.CompletedSceneIds.Add(scene.TemplateId);
            Console.WriteLine($"[SituationCompletionHandler] Tracked completion of scene template '{scene.TemplateId}'");
        }
    }
}
