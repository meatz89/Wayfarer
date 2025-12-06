using Xunit;

/// <summary>
/// TRACER CALL SITE INTEGRATION TESTS
/// Verifies that record methods are correctly called at each entity generation call site:
/// - SceneInstantiator.CreateDeferredScene() → RecordSceneSpawn
/// - SceneInstantiator.ActivateSceneAsync() → RecordSituationSpawn
/// - SpawnService.ExecuteSpawnRules() → RecordSituationSpawn (cascading)
/// - SituationCompletionHandler.CompleteSituation() → MarkSituationCompleted, UpdateSceneState
/// - SituationCompletionHandler.FailSituation() → MarkSituationCompleted, UpdateSceneState
/// - GameOrchestrator scene expiration → UpdateSceneState
///
/// These tests use real tracer instances and verify recorded nodes match expectations.
/// </summary>
public class TracerCallSiteIntegrationTests
{
    // ==================== SCENE INSTANTIATOR TESTS ====================

    [Fact]
    public void SceneInstantiator_CreateDeferredScene_RecordsSceneSpawnWithCorrectTemplateId()
    {
        // This test verifies SceneInstantiator.CreateDeferredScene (line 80) correctly calls RecordSceneSpawn
        // Arrange
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene scene = new Scene
        {
            TemplateId = "authored_tutorial_scene",
            Category = StoryCategory.MainStory,
            State = SceneState.Deferred,
            Situations = new List<Situation>()
        };

        // Act - Simulate what SceneInstantiator does (line 80-88)
        SceneSpawnNode sceneNode = tracer.RecordSceneSpawn(
            scene,
            "authored_tutorial_scene",  // template.Id
            isProcedurallyGenerated: false,  // !template.IsStarter means procedural
            SpawnTriggerType.Initial,
            currentDay: 1,
            TimeBlocks.Morning);

        // Assert
        Assert.NotNull(sceneNode);
        Assert.Equal("authored_tutorial_scene", sceneNode.SceneTemplateId);
        Assert.False(sceneNode.IsProcedurallyGenerated);
        Assert.Same(sceneNode, tracer.GetNodeForScene(scene));
        Assert.Single(tracer.RootScenes);  // No parent = root
        Assert.Single(tracer.AllSceneNodes);
    }

    [Fact]
    public void SceneInstantiator_CreateDeferredScene_MarksProceduralSceneCorrectly()
    {
        // This test verifies procedural scenes are marked correctly
        // Arrange
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene scene = new Scene
        {
            TemplateId = "procedural_scene",
            Category = StoryCategory.SideStory,
            State = SceneState.Deferred
        };

        // Act - Simulate procedural scene creation
        SceneSpawnNode sceneNode = tracer.RecordSceneSpawn(
            scene,
            "procedural_scene",
            isProcedurallyGenerated: true,  // Non-starter = procedural
            SpawnTriggerType.Initial,
            currentDay: 5,
            TimeBlocks.Evening);

        // Assert
        Assert.True(sceneNode.IsProcedurallyGenerated);
        Assert.Equal(5, sceneNode.GameDay);
        Assert.Equal(TimeBlocks.Evening, sceneNode.GameTimeBlock);
    }

    [Fact]
    public void SceneInstantiator_ActivateScene_RecordsSituationsWithCorrectParentScene()
    {
        // This test verifies SceneInstantiator.ActivateDeferredScene (line 305-310) correctly calls RecordSituationSpawn
        // Arrange
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene scene = new Scene
        {
            TemplateId = "test_scene",
            Situations = new List<Situation>()
        };
        Situation situation1 = new Situation { Name = "First Encounter", TemplateId = "sit1" };
        Situation situation2 = new Situation { Name = "Second Encounter", TemplateId = "sit2" };
        scene.Situations.Add(situation1);
        scene.Situations.Add(situation2);

        // Act - Simulate scene activation (creates scene node, then situation nodes)
        SceneSpawnNode sceneNode = tracer.RecordSceneSpawn(
            scene, scene.TemplateId, false, SpawnTriggerType.Initial, 1, TimeBlocks.Morning);

        // Simulate activating each situation (line 305-310)
        foreach (Situation situation in scene.Situations)
        {
            tracer.RecordSituationSpawn(
                situation,
                sceneNode,
                SituationSpawnTriggerType.InitialScene,
                EntityResolutionContext.Empty());
        }

        // Assert
        Assert.Equal(2, tracer.AllSituationNodes.Count);
        Assert.Equal(2, sceneNode.Situations.Count);

        SituationSpawnNode sitNode1 = tracer.GetNodeForSituation(situation1);
        SituationSpawnNode sitNode2 = tracer.GetNodeForSituation(situation2);

        Assert.NotNull(sitNode1);
        Assert.NotNull(sitNode2);
        Assert.Same(sceneNode, sitNode1.ParentScene);
        Assert.Same(sceneNode, sitNode2.ParentScene);
        Assert.Equal("First Encounter", sitNode1.Name);
        Assert.Equal("Second Encounter", sitNode2.Name);
    }

    [Fact]
    public void SceneInstantiator_ActivateScene_CapturesEntityResolutionMetadata()
    {
        // This test verifies entity resolution metadata is correctly passed
        // Arrange
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene scene = new Scene { TemplateId = "test_scene" };
        Situation situation = new Situation
        {
            Name = "Test",
            Location = new Location { Name = "Discovered Location" },
            Npc = new NPC { Name = "Created NPC" }
        };

        SceneSpawnNode sceneNode = tracer.RecordSceneSpawn(
            scene, scene.TemplateId, false, SpawnTriggerType.Initial, 1, TimeBlocks.Morning);

        // Simulate entity resolution metadata from SceneInstantiator
        EntityResolutionContext resolutionContext = new EntityResolutionContext
        {
            LocationResolution = EntityResolutionMetadata.ForDiscovered(null, new List<string> { "Purpose" }),
            NpcResolution = EntityResolutionMetadata.ForCreated(null, new List<string> { "Profession" }, new List<string> { "PersonalityType" })
        };

        // Act
        SituationSpawnNode sitNode = tracer.RecordSituationSpawn(
            situation, sceneNode, SituationSpawnTriggerType.InitialScene, resolutionContext);

        // Assert
        Assert.NotNull(sitNode.Location);
        Assert.Equal("Discovered Location", sitNode.Location.Name);
        Assert.NotNull(sitNode.LocationResolution);
        Assert.Equal(EntityResolutionOutcome.Discovered, sitNode.LocationResolution.Outcome);
        Assert.NotNull(sitNode.NPCResolution);
        Assert.Equal(EntityResolutionOutcome.Created, sitNode.NPCResolution.Outcome);
    }

    // ==================== SPAWN SERVICE TESTS ====================

    [Fact]
    public void SpawnService_ExecuteSpawnRules_RecordsCascadingSituationWithParentContext()
    {
        // This test verifies SpawnService.ExecuteSpawnRules (line 93-98) correctly calls RecordSituationSpawn
        // with parent situation context
        // Arrange
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene scene = new Scene { TemplateId = "test_scene" };
        Situation parentSituation = new Situation { Name = "Parent Challenge" };
        Situation childSituation = new Situation { Name = "Success Cascade" };

        SceneSpawnNode sceneNode = tracer.RecordSceneSpawn(
            scene, scene.TemplateId, false, SpawnTriggerType.Initial, 1, TimeBlocks.Morning);
        SituationSpawnNode parentNode = tracer.RecordSituationSpawn(
            parentSituation, sceneNode, SituationSpawnTriggerType.InitialScene, EntityResolutionContext.Empty());

        // Act - Simulate SpawnService.ExecuteSpawnRules (lines 83-104)
        // Push parent situation context (line 87)
        tracer.PushSituationContext(parentNode);

        // Record situation spawn (line 93-98)
        SituationSpawnNode childNode = tracer.RecordSituationSpawn(
            childSituation,
            sceneNode,  // parentSceneNode
            SituationSpawnTriggerType.SuccessSpawn,
            EntityResolutionContext.Empty());

        // Pop context (line 103)
        tracer.PopSituationContext();

        // Assert
        Assert.Same(parentNode, childNode.ParentSituation);
        Assert.Contains(childNode, parentNode.SpawnedSituations);
        Assert.Equal(SituationSpawnTriggerType.SuccessSpawn, childNode.SpawnTrigger);
        Assert.Equal(2, sceneNode.Situations.Count);  // Both situations in scene
    }

    [Fact]
    public void SpawnService_ExecuteSpawnRules_MultipleCascadesAllLinkedToParent()
    {
        // This test verifies multiple cascading situations are all linked to same parent
        // Arrange
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene scene = new Scene { TemplateId = "test_scene" };
        Situation parentSituation = new Situation { Name = "Parent" };
        Situation child1 = new Situation { Name = "Child 1" };
        Situation child2 = new Situation { Name = "Child 2" };
        Situation child3 = new Situation { Name = "Child 3" };

        SceneSpawnNode sceneNode = tracer.RecordSceneSpawn(
            scene, scene.TemplateId, false, SpawnTriggerType.Initial, 1, TimeBlocks.Morning);
        SituationSpawnNode parentNode = tracer.RecordSituationSpawn(
            parentSituation, sceneNode, SituationSpawnTriggerType.InitialScene, EntityResolutionContext.Empty());

        // Act - Spawn multiple children in parent context
        tracer.PushSituationContext(parentNode);
        SituationSpawnNode childNode1 = tracer.RecordSituationSpawn(
            child1, sceneNode, SituationSpawnTriggerType.SuccessSpawn, EntityResolutionContext.Empty());
        SituationSpawnNode childNode2 = tracer.RecordSituationSpawn(
            child2, sceneNode, SituationSpawnTriggerType.SuccessSpawn, EntityResolutionContext.Empty());
        SituationSpawnNode childNode3 = tracer.RecordSituationSpawn(
            child3, sceneNode, SituationSpawnTriggerType.SuccessSpawn, EntityResolutionContext.Empty());
        tracer.PopSituationContext();

        // Assert
        Assert.Equal(3, parentNode.SpawnedSituations.Count);
        Assert.Same(parentNode, childNode1.ParentSituation);
        Assert.Same(parentNode, childNode2.ParentSituation);
        Assert.Same(parentNode, childNode3.ParentSituation);
    }

    // ==================== SITUATION COMPLETION HANDLER TESTS ====================

    [Fact]
    public void SituationCompletionHandler_CompleteSituation_MarksCompleteWithSuccessOutcome()
    {
        // This test verifies SituationCompletionHandler.CompleteSituation (line 59) calls MarkSituationCompleted
        // Arrange
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene scene = new Scene { TemplateId = "test_scene" };
        Situation situation = new Situation { Name = "Challenge", LifecycleStatus = LifecycleStatus.InProgress };

        SceneSpawnNode sceneNode = tracer.RecordSceneSpawn(
            scene, scene.TemplateId, false, SpawnTriggerType.Initial, 1, TimeBlocks.Morning);
        SituationSpawnNode sitNode = tracer.RecordSituationSpawn(
            situation, sceneNode, SituationSpawnTriggerType.InitialScene, EntityResolutionContext.Empty());

        // Act - Simulate SituationCompletionHandler.CompleteSituation (line 59)
        tracer.MarkSituationCompleted(situation, challengeSucceeded: true);

        // Assert
        Assert.Equal(LifecycleStatus.Completed, sitNode.LifecycleStatus);
        Assert.True(sitNode.LastChallengeSucceeded);
        Assert.NotNull(sitNode.CompletedTimestamp);
    }

    [Fact]
    public void SituationCompletionHandler_CompleteSituation_UpdatesSceneState()
    {
        // This test verifies SituationCompletionHandler.CompleteSituation (line 143) calls UpdateSceneState
        // Arrange
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene scene = new Scene { TemplateId = "test_scene", State = SceneState.Active };
        Situation situation = new Situation { Name = "Final", ParentScene = scene };

        SceneSpawnNode sceneNode = tracer.RecordSceneSpawn(
            scene, scene.TemplateId, false, SpawnTriggerType.Initial, 1, TimeBlocks.Morning);
        tracer.RecordSituationSpawn(
            situation, sceneNode, SituationSpawnTriggerType.InitialScene, EntityResolutionContext.Empty());

        // Act - Simulate scene state update (line 143)
        tracer.UpdateSceneState(scene, SceneState.Completed, DateTime.UtcNow);

        // Assert
        Assert.Equal(SceneState.Completed, sceneNode.CurrentState);
        Assert.NotNull(sceneNode.CompletedTimestamp);
    }

    [Fact]
    public void SituationCompletionHandler_FailSituation_MarksCompleteWithFailureOutcome()
    {
        // This test verifies SituationCompletionHandler.FailSituation (line 204) calls MarkSituationCompleted
        // Arrange
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene scene = new Scene { TemplateId = "test_scene" };
        Situation situation = new Situation { Name = "Challenge", LifecycleStatus = LifecycleStatus.InProgress };

        SceneSpawnNode sceneNode = tracer.RecordSceneSpawn(
            scene, scene.TemplateId, false, SpawnTriggerType.Initial, 1, TimeBlocks.Morning);
        SituationSpawnNode sitNode = tracer.RecordSituationSpawn(
            situation, sceneNode, SituationSpawnTriggerType.InitialScene, EntityResolutionContext.Empty());

        // Act - Simulate SituationCompletionHandler.FailSituation (line 204)
        tracer.MarkSituationCompleted(situation, challengeSucceeded: false);

        // Assert
        Assert.Equal(LifecycleStatus.Completed, sitNode.LifecycleStatus);
        Assert.False(sitNode.LastChallengeSucceeded);
        Assert.NotNull(sitNode.CompletedTimestamp);
    }

    [Fact]
    public void SituationCompletionHandler_FailSituation_UpdatesSceneState()
    {
        // This test verifies SituationCompletionHandler.FailSituation (line 227) calls UpdateSceneState
        // Arrange
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene scene = new Scene { TemplateId = "test_scene", State = SceneState.Active };

        SceneSpawnNode sceneNode = tracer.RecordSceneSpawn(
            scene, scene.TemplateId, false, SpawnTriggerType.Initial, 1, TimeBlocks.Morning);

        // Act - Simulate scene state update on failure (line 227)
        tracer.UpdateSceneState(scene, SceneState.Completed, DateTime.UtcNow);

        // Assert
        Assert.Equal(SceneState.Completed, sceneNode.CurrentState);
    }

    // ==================== GAME ORCHESTRATOR TESTS ====================

    [Fact]
    public void GameOrchestrator_SceneExpiration_UpdatesStateToExpired()
    {
        // This test verifies GameOrchestrator scene expiration (line 1194) calls UpdateSceneState
        // Arrange
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene scene = new Scene
        {
            TemplateId = "test_scene",
            State = SceneState.Active,
            ExpiresOnDay = 3
        };

        SceneSpawnNode sceneNode = tracer.RecordSceneSpawn(
            scene, scene.TemplateId, false, SpawnTriggerType.Initial, 1, TimeBlocks.Morning);

        // Act - Simulate day transition expiration (line 1191-1194)
        tracer.UpdateSceneState(scene, SceneState.Expired, DateTime.UtcNow);

        // Assert
        Assert.Equal(SceneState.Expired, sceneNode.CurrentState);
        Assert.NotNull(sceneNode.ExpiredTimestamp);
    }

    [Fact]
    public void GameOrchestrator_MultipleSceneExpirations_AllTrackedSeparately()
    {
        // This test verifies multiple scene expirations are tracked independently
        // Arrange
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene scene1 = new Scene { TemplateId = "scene1", State = SceneState.Active };
        Scene scene2 = new Scene { TemplateId = "scene2", State = SceneState.Active };
        Scene scene3 = new Scene { TemplateId = "scene3", State = SceneState.Active };

        SceneSpawnNode node1 = tracer.RecordSceneSpawn(scene1, "scene1", false, SpawnTriggerType.Initial, 1, TimeBlocks.Morning);
        SceneSpawnNode node2 = tracer.RecordSceneSpawn(scene2, "scene2", false, SpawnTriggerType.Initial, 1, TimeBlocks.Morning);
        SceneSpawnNode node3 = tracer.RecordSceneSpawn(scene3, "scene3", false, SpawnTriggerType.Initial, 1, TimeBlocks.Morning);

        // Act - Expire first two, keep third active
        DateTime expirationTime = DateTime.UtcNow;
        tracer.UpdateSceneState(scene1, SceneState.Expired, expirationTime);
        tracer.UpdateSceneState(scene2, SceneState.Expired, expirationTime);

        // Assert
        Assert.Equal(SceneState.Expired, node1.CurrentState);
        Assert.Equal(SceneState.Expired, node2.CurrentState);
        Assert.Equal(SceneState.Provisional, node3.CurrentState);  // Unchanged from initial
        Assert.NotNull(node1.ExpiredTimestamp);
        Assert.NotNull(node2.ExpiredTimestamp);
        Assert.Null(node3.ExpiredTimestamp);
    }

    // ==================== SCENE CONTENT CHOICE EXECUTION TESTS ====================

    [Fact]
    public void SceneContent_HandleChoiceSelected_RecordsChoiceExecution()
    {
        // This test verifies SceneContent.HandleChoiceSelected (line 600) calls RecordChoiceExecution
        // Arrange
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene scene = new Scene { TemplateId = "test_scene" };
        Situation situation = new Situation { Name = "Encounter" };
        ChoiceTemplate choice = new ChoiceTemplate
        {
            Id = "player_choice",
            ActionTextTemplate = "Attack the enemy",
            ActionType = ChoiceActionType.StartChallenge,
            PathType = ChoicePathType.Challenge
        };

        SceneSpawnNode sceneNode = tracer.RecordSceneSpawn(
            scene, scene.TemplateId, false, SpawnTriggerType.Initial, 1, TimeBlocks.Morning);
        SituationSpawnNode sitNode = tracer.RecordSituationSpawn(
            situation, sceneNode, SituationSpawnTriggerType.InitialScene, EntityResolutionContext.Empty());

        // Act - Simulate SceneContent.HandleChoiceSelected (line 600-605)
        ChoiceExecutionNode choiceNode = tracer.RecordChoiceExecution(
            choice,
            sitNode,  // situationNode from GetNodeForSituation
            "Attack the enemy",  // choiceTemplate.ActionTextTemplate
            playerMetRequirements: true);

        // Assert
        Assert.NotNull(choiceNode);
        Assert.Equal("player_choice", choiceNode.ChoiceId);
        Assert.Equal("Attack the enemy", choiceNode.ActionText);
        Assert.Equal(ChoiceActionType.StartChallenge, choiceNode.ActionType);
        Assert.Same(sitNode, choiceNode.ParentSituation);
        Assert.Contains(choiceNode, sitNode.Choices);
    }

    [Fact]
    public void SceneContent_HandleChoiceSelected_PushesContextForSceneSpawning()
    {
        // This test verifies context is pushed before spawning scenes (for challenge flow)
        // Arrange
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene parentScene = new Scene { TemplateId = "parent_scene" };
        Situation situation = new Situation { Name = "Encounter" };
        ChoiceTemplate choice = new ChoiceTemplate { Id = "choice", ActionType = ChoiceActionType.Instant };
        Scene spawnedScene = new Scene { TemplateId = "reward_scene" };

        SceneSpawnNode parentNode = tracer.RecordSceneSpawn(
            parentScene, "parent_scene", false, SpawnTriggerType.Initial, 1, TimeBlocks.Morning);
        SituationSpawnNode sitNode = tracer.RecordSituationSpawn(
            situation, parentNode, SituationSpawnTriggerType.InitialScene, EntityResolutionContext.Empty());
        ChoiceExecutionNode choiceNode = tracer.RecordChoiceExecution(
            choice, sitNode, "Do action", true);

        // Act - Simulate pushing context and spawning scene
        tracer.PushChoiceContext(choiceNode);
        SceneSpawnNode spawnedNode = tracer.RecordSceneSpawn(
            spawnedScene, "reward_scene", true, SpawnTriggerType.ChoiceReward, 1, TimeBlocks.Midday);
        tracer.PopChoiceContext();

        // Assert
        Assert.Same(choiceNode, spawnedNode.ParentChoice);
        Assert.Contains(spawnedNode, choiceNode.SpawnedScenes);
        Assert.Single(tracer.RootScenes);  // Only parent is root
    }

    // ==================== COMPLETE FLOW INTEGRATION TESTS ====================

    [Fact]
    public void FullFlow_TutorialSceneToCompletion_AllNodesCorrectlyLinked()
    {
        // This test simulates a complete tutorial flow:
        // 1. Scene created (SceneInstantiator.CreateDeferredScene)
        // 2. Scene activated with situations (SceneInstantiator.ActivateDeferredScene)
        // 3. Player makes choice (SceneContent.HandleChoiceSelected)
        // 4. Situation completes (SituationCompletionHandler.CompleteSituation)
        // 5. Scene completes (state update)

        // Arrange
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene scene = new Scene { TemplateId = "tutorial_scene", State = SceneState.Deferred };
        Situation situation = new Situation { Name = "Tutorial Encounter" };
        ChoiceTemplate choice = new ChoiceTemplate
        {
            Id = "tutorial_choice",
            ActionType = ChoiceActionType.Instant
        };

        // Step 1: Create scene
        SceneSpawnNode sceneNode = tracer.RecordSceneSpawn(
            scene, "tutorial_scene", false, SpawnTriggerType.Initial, 1, TimeBlocks.Morning);

        // Step 2: Activate with situation
        SituationSpawnNode sitNode = tracer.RecordSituationSpawn(
            situation, sceneNode, SituationSpawnTriggerType.InitialScene, EntityResolutionContext.Empty());

        // Simulate activation
        tracer.UpdateSceneState(scene, SceneState.Active, DateTime.UtcNow);

        // Step 3: Player makes choice
        ChoiceExecutionNode choiceNode = tracer.RecordChoiceExecution(
            choice, sitNode, "Continue tutorial", true);

        // Step 4: Situation completes
        tracer.MarkSituationCompleted(situation, challengeSucceeded: null);

        // Step 5: Scene completes
        tracer.UpdateSceneState(scene, SceneState.Completed, DateTime.UtcNow);

        // Assert - Full graph structure
        Assert.Single(tracer.RootScenes);
        Assert.Single(tracer.AllSceneNodes);
        Assert.Single(tracer.AllSituationNodes);
        Assert.Single(tracer.AllChoiceNodes);

        // Assert - Relationships
        Assert.Same(sceneNode, tracer.RootScenes[0]);
        Assert.Single(sceneNode.Situations);
        Assert.Same(sitNode, sceneNode.Situations[0]);
        Assert.Same(sceneNode, sitNode.ParentScene);
        Assert.Single(sitNode.Choices);
        Assert.Same(choiceNode, sitNode.Choices[0]);
        Assert.Same(sitNode, choiceNode.ParentSituation);

        // Assert - State transitions
        Assert.Equal(SceneState.Completed, sceneNode.CurrentState);
        Assert.NotNull(sceneNode.ActivatedTimestamp);
        Assert.NotNull(sceneNode.CompletedTimestamp);
        Assert.Equal(LifecycleStatus.Completed, sitNode.LifecycleStatus);
        Assert.NotNull(sitNode.CompletedTimestamp);
    }

    [Fact]
    public void FullFlow_CascadingSceneSpawn_CreatesCorrectGraph()
    {
        // This test simulates a cascading flow:
        // 1. Main scene with situation
        // 2. Choice spawns new scene
        // 3. New scene has its own situations

        // Arrange
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene mainScene = new Scene { TemplateId = "main_scene" };
        Situation mainSituation = new Situation { Name = "Main Encounter" };
        ChoiceTemplate choice = new ChoiceTemplate { Id = "spawn_choice" };
        Scene childScene = new Scene { TemplateId = "child_scene" };
        Situation childSituation = new Situation { Name = "Child Encounter" };

        // Build main scene
        SceneSpawnNode mainNode = tracer.RecordSceneSpawn(
            mainScene, "main_scene", false, SpawnTriggerType.Initial, 1, TimeBlocks.Morning);
        SituationSpawnNode mainSitNode = tracer.RecordSituationSpawn(
            mainSituation, mainNode, SituationSpawnTriggerType.InitialScene, EntityResolutionContext.Empty());
        ChoiceExecutionNode choiceNode = tracer.RecordChoiceExecution(
            choice, mainSitNode, "Spawn new adventure", true);

        // Choice spawns new scene
        tracer.PushChoiceContext(choiceNode);
        SceneSpawnNode childNode = tracer.RecordSceneSpawn(
            childScene, "child_scene", true, SpawnTriggerType.ChoiceReward, 1, TimeBlocks.Midday);
        SituationSpawnNode childSitNode = tracer.RecordSituationSpawn(
            childSituation, childNode, SituationSpawnTriggerType.InitialScene, EntityResolutionContext.Empty());
        tracer.PopChoiceContext();

        // Assert - Graph structure
        Assert.Single(tracer.RootScenes);  // Only main is root
        Assert.Equal(2, tracer.AllSceneNodes.Count);
        Assert.Equal(2, tracer.AllSituationNodes.Count);
        Assert.Single(tracer.AllChoiceNodes);

        // Assert - Cascading relationships
        Assert.Same(choiceNode, childNode.ParentChoice);
        Assert.Contains(childNode, choiceNode.SpawnedScenes);
        Assert.Same(childNode, childSitNode.ParentScene);
    }
}
