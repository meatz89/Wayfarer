using Xunit;

/// <summary>
/// PROCEDURAL CONTENT TRACING TESTS
/// Comprehensive test suite for SpawnGraph tracing system
/// Tests that record methods correctly create nodes, link relationships, and update state
/// </summary>
public class ProceduralTracingTests
{
    // ==================== LAYER 1: RECORD SCENE SPAWN TESTS ====================

    [Fact]
    public void RecordSceneSpawn_CreatesNodeWithCorrectProperties()
    {
        // Arrange
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene scene = CreateTestScene();

        // Act
        SceneSpawnNode node = tracer.RecordSceneSpawn(
            scene,
            "test_scene_template",
            isProcedurallyGenerated: false,
            SpawnTriggerType.Initial,
            currentDay: 1,
            TimeBlocks.Morning);

        // Assert
        Assert.NotNull(node);
        Assert.Equal("test_scene_template", node.SceneTemplateId);
        Assert.False(node.IsProcedurallyGenerated);
        Assert.Equal(SpawnTriggerType.Initial, node.SpawnTrigger);
        Assert.Equal(1, node.GameDay);
        Assert.Equal(TimeBlocks.Morning, node.GameTimeBlock);
        Assert.Null(node.ParentChoice);
        Assert.Null(node.ParentSituation);
        Assert.Null(node.ParentScene);
        Assert.Empty(node.SpawnedScenes);
        Assert.Empty(node.Situations);
    }

    [Fact]
    public void RecordSceneSpawn_AddsToAllSceneNodesCollection()
    {
        // Arrange
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene scene = CreateTestScene();

        // Act
        SceneSpawnNode node = tracer.RecordSceneSpawn(
            scene, "test_scene", false, SpawnTriggerType.Initial, 1, TimeBlocks.Morning);

        // Assert
        Assert.Single(tracer.AllSceneNodes);
        Assert.Same(node, tracer.AllSceneNodes[0]);
    }

    [Fact]
    public void RecordSceneSpawn_AddsToRootScenesWhenNoParent()
    {
        // Arrange
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene scene = CreateTestScene();

        // Act
        SceneSpawnNode node = tracer.RecordSceneSpawn(
            scene, "test_scene", false, SpawnTriggerType.Initial, 1, TimeBlocks.Morning);

        // Assert - Scene without parent should be in RootScenes
        Assert.Single(tracer.RootScenes);
        Assert.Same(node, tracer.RootScenes[0]);
    }

    [Fact]
    public void RecordSceneSpawn_NotInRootScenesWhenHasParentChoice()
    {
        // Arrange
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene parentScene = CreateTestScene();
        Situation situation = CreateTestSituation();
        ChoiceTemplate choice = CreateTestChoice();
        Scene childScene = CreateTestScene();

        // Record parent chain
        SceneSpawnNode parentSceneNode = tracer.RecordSceneSpawn(
            parentScene, "parent_scene", false, SpawnTriggerType.Initial, 1, TimeBlocks.Morning);
        SituationSpawnNode situationNode = tracer.RecordSituationSpawn(
            situation, parentSceneNode, SituationSpawnTriggerType.InitialScene, EntityResolutionContext.Empty());
        ChoiceExecutionNode choiceNode = tracer.RecordChoiceExecution(
            choice, situationNode, "Test Action", true);

        // Push choice context and spawn child scene
        tracer.PushChoiceContext(choiceNode);
        SceneSpawnNode childSceneNode = tracer.RecordSceneSpawn(
            childScene, "child_scene", true, SpawnTriggerType.ChoiceReward, 1, TimeBlocks.Morning);
        tracer.PopChoiceContext();

        // Assert - Child scene NOT in RootScenes
        Assert.Single(tracer.RootScenes);  // Only parent
        Assert.Same(parentSceneNode, tracer.RootScenes[0]);
        Assert.Equal(2, tracer.AllSceneNodes.Count);  // Both scenes in all nodes
    }

    [Fact]
    public void RecordSceneSpawn_MapsEntityToNodeForLaterLookup()
    {
        // Arrange
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene scene = CreateTestScene();

        // Act
        SceneSpawnNode originalNode = tracer.RecordSceneSpawn(
            scene, "test_scene", false, SpawnTriggerType.Initial, 1, TimeBlocks.Morning);

        // Assert - GetNodeForScene returns same node
        SceneSpawnNode retrievedNode = tracer.GetNodeForScene(scene);
        Assert.Same(originalNode, retrievedNode);
    }

    [Fact]
    public void RecordSceneSpawn_LinksToParentChoiceViaContextStack()
    {
        // Arrange
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene parentScene = CreateTestScene();
        Situation situation = CreateTestSituation();
        ChoiceTemplate choice = CreateTestChoice();
        Scene childScene = CreateTestScene();

        // Setup parent chain
        SceneSpawnNode parentSceneNode = tracer.RecordSceneSpawn(
            parentScene, "parent_scene", false, SpawnTriggerType.Initial, 1, TimeBlocks.Morning);
        SituationSpawnNode situationNode = tracer.RecordSituationSpawn(
            situation, parentSceneNode, SituationSpawnTriggerType.InitialScene, EntityResolutionContext.Empty());
        ChoiceExecutionNode choiceNode = tracer.RecordChoiceExecution(
            choice, situationNode, "Test Action", true);

        // Act - Push choice context, spawn child, pop context
        tracer.PushChoiceContext(choiceNode);
        SceneSpawnNode childSceneNode = tracer.RecordSceneSpawn(
            childScene, "child_scene", true, SpawnTriggerType.ChoiceReward, 1, TimeBlocks.Midday);
        tracer.PopChoiceContext();

        // Assert - Child linked to parent choice
        Assert.Same(choiceNode, childSceneNode.ParentChoice);
        Assert.Contains(childSceneNode, choiceNode.SpawnedScenes);
    }

    // ==================== LAYER 2: RECORD SITUATION SPAWN TESTS ====================

    [Fact]
    public void RecordSituationSpawn_CreatesNodeWithCorrectProperties()
    {
        // Arrange
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene scene = CreateTestScene();
        Situation situation = CreateTestSituation();

        SceneSpawnNode sceneNode = tracer.RecordSceneSpawn(
            scene, "test_scene", false, SpawnTriggerType.Initial, 1, TimeBlocks.Morning);

        // Act
        SituationSpawnNode node = tracer.RecordSituationSpawn(
            situation,
            sceneNode,
            SituationSpawnTriggerType.InitialScene,
            EntityResolutionContext.Empty());

        // Assert
        Assert.NotNull(node);
        Assert.Equal("Test Situation", node.Name);
        Assert.Equal(SituationType.Normal, node.Type);
        Assert.Equal(TacticalSystemType.Social, node.SystemType);
        Assert.Equal(SituationSpawnTriggerType.InitialScene, node.SpawnTrigger);
        Assert.Same(sceneNode, node.ParentScene);
        Assert.Null(node.ParentSituation);  // No parent situation
    }

    [Fact]
    public void RecordSituationSpawn_AddsToAllSituationNodesCollection()
    {
        // Arrange
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene scene = CreateTestScene();
        Situation situation = CreateTestSituation();

        SceneSpawnNode sceneNode = tracer.RecordSceneSpawn(
            scene, "test_scene", false, SpawnTriggerType.Initial, 1, TimeBlocks.Morning);

        // Act
        SituationSpawnNode node = tracer.RecordSituationSpawn(
            situation, sceneNode, SituationSpawnTriggerType.InitialScene, EntityResolutionContext.Empty());

        // Assert
        Assert.Single(tracer.AllSituationNodes);
        Assert.Same(node, tracer.AllSituationNodes[0]);
    }

    [Fact]
    public void RecordSituationSpawn_LinksToParentScene()
    {
        // Arrange
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene scene = CreateTestScene();
        Situation situation = CreateTestSituation();

        SceneSpawnNode sceneNode = tracer.RecordSceneSpawn(
            scene, "test_scene", false, SpawnTriggerType.Initial, 1, TimeBlocks.Morning);

        // Act
        SituationSpawnNode situationNode = tracer.RecordSituationSpawn(
            situation, sceneNode, SituationSpawnTriggerType.InitialScene, EntityResolutionContext.Empty());

        // Assert - Bidirectional link
        Assert.Same(sceneNode, situationNode.ParentScene);
        Assert.Contains(situationNode, sceneNode.Situations);
    }

    [Fact]
    public void RecordSituationSpawn_LinksToParentSituationViaSituationContext()
    {
        // Arrange
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene scene = CreateTestScene();
        Situation parentSituation = CreateTestSituation();
        Situation childSituation = CreateTestSituation();
        childSituation.Name = "Child Situation";

        SceneSpawnNode sceneNode = tracer.RecordSceneSpawn(
            scene, "test_scene", false, SpawnTriggerType.Initial, 1, TimeBlocks.Morning);
        SituationSpawnNode parentNode = tracer.RecordSituationSpawn(
            parentSituation, sceneNode, SituationSpawnTriggerType.InitialScene, EntityResolutionContext.Empty());

        // Act - Push parent situation context, spawn child
        tracer.PushSituationContext(parentNode);
        SituationSpawnNode childNode = tracer.RecordSituationSpawn(
            childSituation, sceneNode, SituationSpawnTriggerType.SuccessSpawn, EntityResolutionContext.Empty());
        tracer.PopSituationContext();

        // Assert - Child linked to parent situation
        Assert.Same(parentNode, childNode.ParentSituation);
        Assert.Contains(childNode, parentNode.SpawnedSituations);
    }

    [Fact]
    public void RecordSituationSpawn_CapturesEntityResolutionMetadata()
    {
        // Arrange
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene scene = CreateTestScene();
        Situation situation = CreateTestSituation();
        situation.Location = new Location { Name = "Test Location" };

        SceneSpawnNode sceneNode = tracer.RecordSceneSpawn(
            scene, "test_scene", false, SpawnTriggerType.Initial, 1, TimeBlocks.Morning);

        EntityResolutionContext resolutionContext = new EntityResolutionContext
        {
            LocationResolution = EntityResolutionMetadata.ForDiscovered(null, new List<string> { "Purpose", "Privacy" }),
            NpcResolution = EntityResolutionMetadata.ForCreated(null, new List<string> { "Profession" }, new List<string> { "PersonalityType" })
        };

        // Act
        SituationSpawnNode node = tracer.RecordSituationSpawn(
            situation, sceneNode, SituationSpawnTriggerType.InitialScene, resolutionContext);

        // Assert
        Assert.NotNull(node.Location);  // Snapshot created from situation.Location
        Assert.NotNull(node.LocationResolution);
        Assert.Equal(EntityResolutionOutcome.Discovered, node.LocationResolution.Outcome);
        Assert.NotNull(node.NPCResolution);
        Assert.Equal(EntityResolutionOutcome.Created, node.NPCResolution.Outcome);
    }

    [Fact]
    public void RecordSituationSpawn_MapsEntityToNodeForLaterLookup()
    {
        // Arrange
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene scene = CreateTestScene();
        Situation situation = CreateTestSituation();

        SceneSpawnNode sceneNode = tracer.RecordSceneSpawn(
            scene, "test_scene", false, SpawnTriggerType.Initial, 1, TimeBlocks.Morning);

        // Act
        SituationSpawnNode originalNode = tracer.RecordSituationSpawn(
            situation, sceneNode, SituationSpawnTriggerType.InitialScene, EntityResolutionContext.Empty());

        // Assert
        SituationSpawnNode retrievedNode = tracer.GetNodeForSituation(situation);
        Assert.Same(originalNode, retrievedNode);
    }

    // ==================== LAYER 3: RECORD CHOICE EXECUTION TESTS ====================

    [Fact]
    public void RecordChoiceExecution_CreatesNodeWithCorrectProperties()
    {
        // Arrange
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene scene = CreateTestScene();
        Situation situation = CreateTestSituation();
        ChoiceTemplate choice = CreateTestChoice();

        SceneSpawnNode sceneNode = tracer.RecordSceneSpawn(
            scene, "test_scene", false, SpawnTriggerType.Initial, 1, TimeBlocks.Morning);
        SituationSpawnNode situationNode = tracer.RecordSituationSpawn(
            situation, sceneNode, SituationSpawnTriggerType.InitialScene, EntityResolutionContext.Empty());

        // Act
        ChoiceExecutionNode node = tracer.RecordChoiceExecution(
            choice, situationNode, "Perform action", playerMetRequirements: true);

        // Assert
        Assert.NotNull(node);
        Assert.Equal("test_choice", node.ChoiceId);
        Assert.Equal("Perform action", node.ActionText);
        Assert.Equal(ChoiceActionType.Instant, node.ActionType);
        Assert.Equal(ChoicePathType.InstantSuccess, node.PathType);
        Assert.True(node.PlayerMetRequirements);
    }

    [Fact]
    public void RecordChoiceExecution_AddsToAllChoiceNodesCollection()
    {
        // Arrange
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene scene = CreateTestScene();
        Situation situation = CreateTestSituation();
        ChoiceTemplate choice = CreateTestChoice();

        SceneSpawnNode sceneNode = tracer.RecordSceneSpawn(
            scene, "test_scene", false, SpawnTriggerType.Initial, 1, TimeBlocks.Morning);
        SituationSpawnNode situationNode = tracer.RecordSituationSpawn(
            situation, sceneNode, SituationSpawnTriggerType.InitialScene, EntityResolutionContext.Empty());

        // Act
        ChoiceExecutionNode node = tracer.RecordChoiceExecution(
            choice, situationNode, "Test Action", true);

        // Assert
        Assert.Single(tracer.AllChoiceNodes);
        Assert.Same(node, tracer.AllChoiceNodes[0]);
    }

    [Fact]
    public void RecordChoiceExecution_LinksToParentSituation()
    {
        // Arrange
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene scene = CreateTestScene();
        Situation situation = CreateTestSituation();
        ChoiceTemplate choice = CreateTestChoice();

        SceneSpawnNode sceneNode = tracer.RecordSceneSpawn(
            scene, "test_scene", false, SpawnTriggerType.Initial, 1, TimeBlocks.Morning);
        SituationSpawnNode situationNode = tracer.RecordSituationSpawn(
            situation, sceneNode, SituationSpawnTriggerType.InitialScene, EntityResolutionContext.Empty());

        // Act
        ChoiceExecutionNode choiceNode = tracer.RecordChoiceExecution(
            choice, situationNode, "Test Action", true);

        // Assert - Bidirectional link
        Assert.Same(situationNode, choiceNode.ParentSituation);
        Assert.Contains(choiceNode, situationNode.Choices);
    }

    [Fact]
    public void RecordChoiceExecution_CapturesRequirementSnapshot()
    {
        // Arrange
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene scene = CreateTestScene();
        Situation situation = CreateTestSituation();
        ChoiceTemplate choice = CreateTestChoiceWithRequirements();

        SceneSpawnNode sceneNode = tracer.RecordSceneSpawn(
            scene, "test_scene", false, SpawnTriggerType.Initial, 1, TimeBlocks.Morning);
        SituationSpawnNode situationNode = tracer.RecordSituationSpawn(
            situation, sceneNode, SituationSpawnTriggerType.InitialScene, EntityResolutionContext.Empty());

        // Act
        ChoiceExecutionNode node = tracer.RecordChoiceExecution(
            choice, situationNode, "Test Action", true);

        // Assert
        Assert.NotNull(node.RequirementSnapshot);
        Assert.Equal(5, node.RequirementSnapshot.RequiredInsight);
    }

    [Fact]
    public void RecordChoiceExecution_CapturesRewardSnapshots()
    {
        // Arrange
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene scene = CreateTestScene();
        Situation situation = CreateTestSituation();
        ChoiceTemplate choice = CreateTestChoiceWithRewards();

        SceneSpawnNode sceneNode = tracer.RecordSceneSpawn(
            scene, "test_scene", false, SpawnTriggerType.Initial, 1, TimeBlocks.Morning);
        SituationSpawnNode situationNode = tracer.RecordSituationSpawn(
            situation, sceneNode, SituationSpawnTriggerType.InitialScene, EntityResolutionContext.Empty());

        // Act
        ChoiceExecutionNode node = tracer.RecordChoiceExecution(
            choice, situationNode, "Test Action", true);

        // Assert
        Assert.NotNull(node.RewardSnapshot);
        Assert.Equal(10, node.RewardSnapshot.CoinsGained);
        Assert.Equal(2, node.RewardSnapshot.InsightGained);
    }

    // ==================== LAYER 4: STATE UPDATE TESTS ====================

    [Fact]
    public void UpdateSceneState_SetsCurrentState()
    {
        // Arrange
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene scene = CreateTestScene();
        scene.State = SceneState.Deferred;

        SceneSpawnNode node = tracer.RecordSceneSpawn(
            scene, "test_scene", false, SpawnTriggerType.Initial, 1, TimeBlocks.Morning);

        // Act
        tracer.UpdateSceneState(scene, SceneState.Active, DateTime.UtcNow);

        // Assert
        Assert.Equal(SceneState.Active, node.CurrentState);
    }

    [Fact]
    public void UpdateSceneState_SetsActivatedTimestamp()
    {
        // Arrange
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene scene = CreateTestScene();
        scene.State = SceneState.Deferred;

        SceneSpawnNode node = tracer.RecordSceneSpawn(
            scene, "test_scene", false, SpawnTriggerType.Initial, 1, TimeBlocks.Morning);
        DateTime activationTime = DateTime.UtcNow;

        // Act
        tracer.UpdateSceneState(scene, SceneState.Active, activationTime);

        // Assert
        Assert.NotNull(node.ActivatedTimestamp);
        Assert.Equal(activationTime, node.ActivatedTimestamp.Value);
    }

    [Fact]
    public void UpdateSceneState_SetsCompletedTimestamp()
    {
        // Arrange
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene scene = CreateTestScene();

        SceneSpawnNode node = tracer.RecordSceneSpawn(
            scene, "test_scene", false, SpawnTriggerType.Initial, 1, TimeBlocks.Morning);
        DateTime completionTime = DateTime.UtcNow;

        // Act
        tracer.UpdateSceneState(scene, SceneState.Completed, completionTime);

        // Assert
        Assert.NotNull(node.CompletedTimestamp);
        Assert.Equal(completionTime, node.CompletedTimestamp.Value);
    }

    [Fact]
    public void UpdateSceneState_SetsExpiredTimestamp()
    {
        // Arrange
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene scene = CreateTestScene();

        SceneSpawnNode node = tracer.RecordSceneSpawn(
            scene, "test_scene", false, SpawnTriggerType.Initial, 1, TimeBlocks.Morning);
        DateTime expirationTime = DateTime.UtcNow;

        // Act
        tracer.UpdateSceneState(scene, SceneState.Expired, expirationTime);

        // Assert
        Assert.NotNull(node.ExpiredTimestamp);
        Assert.Equal(expirationTime, node.ExpiredTimestamp.Value);
    }

    [Fact]
    public void UpdateSceneState_DoesNotSetTimestampTwice()
    {
        // Arrange
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene scene = CreateTestScene();

        SceneSpawnNode node = tracer.RecordSceneSpawn(
            scene, "test_scene", false, SpawnTriggerType.Initial, 1, TimeBlocks.Morning);
        DateTime firstActivation = DateTime.UtcNow;
        DateTime secondActivation = firstActivation.AddMinutes(5);

        // Act - Activate twice
        tracer.UpdateSceneState(scene, SceneState.Active, firstActivation);
        tracer.UpdateSceneState(scene, SceneState.Active, secondActivation);

        // Assert - First timestamp preserved
        Assert.Equal(firstActivation, node.ActivatedTimestamp.Value);
    }

    [Fact]
    public void MarkSituationCompleted_SetsLifecycleStatusAndTimestamp()
    {
        // Arrange
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene scene = CreateTestScene();
        Situation situation = CreateTestSituation();

        SceneSpawnNode sceneNode = tracer.RecordSceneSpawn(
            scene, "test_scene", false, SpawnTriggerType.Initial, 1, TimeBlocks.Morning);
        SituationSpawnNode node = tracer.RecordSituationSpawn(
            situation, sceneNode, SituationSpawnTriggerType.InitialScene, EntityResolutionContext.Empty());

        // Act
        tracer.MarkSituationCompleted(situation, challengeSucceeded: true);

        // Assert
        Assert.Equal(LifecycleStatus.Completed, node.LifecycleStatus);
        Assert.NotNull(node.CompletedTimestamp);
        Assert.True(node.LastChallengeSucceeded);
    }

    [Fact]
    public void MarkSituationCompleted_RecordsChallengeFailure()
    {
        // Arrange
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene scene = CreateTestScene();
        Situation situation = CreateTestSituation();

        SceneSpawnNode sceneNode = tracer.RecordSceneSpawn(
            scene, "test_scene", false, SpawnTriggerType.Initial, 1, TimeBlocks.Morning);
        SituationSpawnNode node = tracer.RecordSituationSpawn(
            situation, sceneNode, SituationSpawnTriggerType.InitialScene, EntityResolutionContext.Empty());

        // Act
        tracer.MarkSituationCompleted(situation, challengeSucceeded: false);

        // Assert
        Assert.Equal(LifecycleStatus.Completed, node.LifecycleStatus);
        Assert.False(node.LastChallengeSucceeded);
    }

    [Fact]
    public void MarkSituationCompleted_RecordsNullChallengeOutcome()
    {
        // Arrange
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene scene = CreateTestScene();
        Situation situation = CreateTestSituation();

        SceneSpawnNode sceneNode = tracer.RecordSceneSpawn(
            scene, "test_scene", false, SpawnTriggerType.Initial, 1, TimeBlocks.Morning);
        SituationSpawnNode node = tracer.RecordSituationSpawn(
            situation, sceneNode, SituationSpawnTriggerType.InitialScene, EntityResolutionContext.Empty());

        // Act - No challenge outcome (instant action)
        tracer.MarkSituationCompleted(situation, challengeSucceeded: null);

        // Assert
        Assert.Equal(LifecycleStatus.Completed, node.LifecycleStatus);
        Assert.Null(node.LastChallengeSucceeded);
    }

    // ==================== LAYER 5: CONTEXT STACK TESTS ====================

    [Fact]
    public void ChoiceContextStack_PushPopWorksCorrectly()
    {
        // Arrange
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene scene = CreateTestScene();
        Situation situation = CreateTestSituation();
        ChoiceTemplate choice = CreateTestChoice();

        SceneSpawnNode sceneNode = tracer.RecordSceneSpawn(
            scene, "test_scene", false, SpawnTriggerType.Initial, 1, TimeBlocks.Morning);
        SituationSpawnNode situationNode = tracer.RecordSituationSpawn(
            situation, sceneNode, SituationSpawnTriggerType.InitialScene, EntityResolutionContext.Empty());
        ChoiceExecutionNode choiceNode = tracer.RecordChoiceExecution(
            choice, situationNode, "Test", true);

        // Act & Assert - Push
        tracer.PushChoiceContext(choiceNode);
        Assert.Same(choiceNode, tracer.GetCurrentChoiceContext());

        // Act & Assert - Pop
        tracer.PopChoiceContext();
        Assert.Null(tracer.GetCurrentChoiceContext());
    }

    [Fact]
    public void ChoiceContextStack_SupportsNestedContexts()
    {
        // Arrange
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene scene = CreateTestScene();
        Situation situation1 = CreateTestSituation();
        Situation situation2 = CreateTestSituation();
        ChoiceTemplate choice1 = CreateTestChoice();
        ChoiceTemplate choice2 = CreateTestChoice();

        SceneSpawnNode sceneNode = tracer.RecordSceneSpawn(
            scene, "test_scene", false, SpawnTriggerType.Initial, 1, TimeBlocks.Morning);
        SituationSpawnNode sitNode1 = tracer.RecordSituationSpawn(
            situation1, sceneNode, SituationSpawnTriggerType.InitialScene, EntityResolutionContext.Empty());
        SituationSpawnNode sitNode2 = tracer.RecordSituationSpawn(
            situation2, sceneNode, SituationSpawnTriggerType.InitialScene, EntityResolutionContext.Empty());
        ChoiceExecutionNode choiceNode1 = tracer.RecordChoiceExecution(choice1, sitNode1, "First", true);
        ChoiceExecutionNode choiceNode2 = tracer.RecordChoiceExecution(choice2, sitNode2, "Second", true);

        // Act & Assert - Nested push/pop
        tracer.PushChoiceContext(choiceNode1);
        Assert.Same(choiceNode1, tracer.GetCurrentChoiceContext());

        tracer.PushChoiceContext(choiceNode2);
        Assert.Same(choiceNode2, tracer.GetCurrentChoiceContext());

        tracer.PopChoiceContext();
        Assert.Same(choiceNode1, tracer.GetCurrentChoiceContext());

        tracer.PopChoiceContext();
        Assert.Null(tracer.GetCurrentChoiceContext());
    }

    [Fact]
    public void SituationContextStack_PushPopWorksCorrectly()
    {
        // Arrange
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene scene = CreateTestScene();
        Situation situation = CreateTestSituation();

        SceneSpawnNode sceneNode = tracer.RecordSceneSpawn(
            scene, "test_scene", false, SpawnTriggerType.Initial, 1, TimeBlocks.Morning);
        SituationSpawnNode situationNode = tracer.RecordSituationSpawn(
            situation, sceneNode, SituationSpawnTriggerType.InitialScene, EntityResolutionContext.Empty());

        // Act & Assert - Push
        tracer.PushSituationContext(situationNode);
        Assert.Same(situationNode, tracer.GetCurrentSituationContext());

        // Act & Assert - Pop
        tracer.PopSituationContext();
        Assert.Null(tracer.GetCurrentSituationContext());
    }

    [Fact]
    public void PopChoiceContext_SafeWhenEmpty()
    {
        // Arrange
        ProceduralContentTracer tracer = new ProceduralContentTracer();

        // Act - Pop when empty (should not throw)
        tracer.PopChoiceContext();

        // Assert - Still null
        Assert.Null(tracer.GetCurrentChoiceContext());
    }

    [Fact]
    public void PopSituationContext_SafeWhenEmpty()
    {
        // Arrange
        ProceduralContentTracer tracer = new ProceduralContentTracer();

        // Act - Pop when empty (should not throw)
        tracer.PopSituationContext();

        // Assert - Still null
        Assert.Null(tracer.GetCurrentSituationContext());
    }

    // ==================== LAYER 6: CLEAR AND LOOKUP TESTS ====================

    [Fact]
    public void Clear_RemovesAllTrackedData()
    {
        // Arrange
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene scene = CreateTestScene();
        Situation situation = CreateTestSituation();
        ChoiceTemplate choice = CreateTestChoice();

        SceneSpawnNode sceneNode = tracer.RecordSceneSpawn(
            scene, "test_scene", false, SpawnTriggerType.Initial, 1, TimeBlocks.Morning);
        SituationSpawnNode situationNode = tracer.RecordSituationSpawn(
            situation, sceneNode, SituationSpawnTriggerType.InitialScene, EntityResolutionContext.Empty());
        ChoiceExecutionNode choiceNode = tracer.RecordChoiceExecution(choice, situationNode, "Test", true);
        tracer.PushChoiceContext(choiceNode);
        tracer.PushSituationContext(situationNode);

        // Act
        tracer.Clear();

        // Assert - All collections cleared
        Assert.Empty(tracer.RootScenes);
        Assert.Empty(tracer.AllSceneNodes);
        Assert.Empty(tracer.AllSituationNodes);
        Assert.Empty(tracer.AllChoiceNodes);
        Assert.Null(tracer.GetCurrentChoiceContext());
        Assert.Null(tracer.GetCurrentSituationContext());
        Assert.Null(tracer.GetNodeForScene(scene));  // Entity mapping cleared
        Assert.Null(tracer.GetNodeForSituation(situation));
    }

    [Fact]
    public void GetNodeForScene_ReturnsNullForUnknownScene()
    {
        // Arrange
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene unknownScene = CreateTestScene();

        // Act
        SceneSpawnNode result = tracer.GetNodeForScene(unknownScene);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetNodeForSituation_ReturnsNullForUnknownSituation()
    {
        // Arrange
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Situation unknownSituation = CreateTestSituation();

        // Act
        SituationSpawnNode result = tracer.GetNodeForSituation(unknownSituation);

        // Assert
        Assert.Null(result);
    }

    // ==================== LAYER 7: GRAPH CONSTRUCTION TESTS ====================

    [Fact]
    public void FullGraph_CorrectlyBuildsSceneSituationChoiceHierarchy()
    {
        // Arrange
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene scene = CreateTestScene();
        Situation situation1 = CreateTestSituation();
        situation1.Name = "Situation 1";
        Situation situation2 = CreateTestSituation();
        situation2.Name = "Situation 2";
        ChoiceTemplate choice = CreateTestChoice();

        // Act - Build hierarchy
        SceneSpawnNode sceneNode = tracer.RecordSceneSpawn(
            scene, "test_scene", false, SpawnTriggerType.Initial, 1, TimeBlocks.Morning);
        SituationSpawnNode sitNode1 = tracer.RecordSituationSpawn(
            situation1, sceneNode, SituationSpawnTriggerType.InitialScene, EntityResolutionContext.Empty());
        SituationSpawnNode sitNode2 = tracer.RecordSituationSpawn(
            situation2, sceneNode, SituationSpawnTriggerType.InitialScene, EntityResolutionContext.Empty());
        ChoiceExecutionNode choiceNode = tracer.RecordChoiceExecution(
            choice, sitNode1, "Test Action", true);

        // Assert - Graph structure
        Assert.Single(tracer.RootScenes);
        Assert.Single(tracer.AllSceneNodes);
        Assert.Equal(2, tracer.AllSituationNodes.Count);
        Assert.Single(tracer.AllChoiceNodes);

        // Assert - Hierarchical relationships
        Assert.Equal(2, sceneNode.Situations.Count);
        Assert.Contains(sitNode1, sceneNode.Situations);
        Assert.Contains(sitNode2, sceneNode.Situations);
        Assert.Single(sitNode1.Choices);
        Assert.Same(choiceNode, sitNode1.Choices[0]);
        Assert.Empty(sitNode2.Choices);
    }

    [Fact]
    public void FullGraph_CascadingSpawnCreatesCorrectLinks()
    {
        // Arrange
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene scene = CreateTestScene();
        Situation parentSituation = CreateTestSituation();
        parentSituation.Name = "Parent";
        Situation childSituation1 = CreateTestSituation();
        childSituation1.Name = "Child 1";
        Situation childSituation2 = CreateTestSituation();
        childSituation2.Name = "Child 2";

        // Act - Create scene, parent situation, then cascading children
        SceneSpawnNode sceneNode = tracer.RecordSceneSpawn(
            scene, "test_scene", false, SpawnTriggerType.Initial, 1, TimeBlocks.Morning);
        SituationSpawnNode parentNode = tracer.RecordSituationSpawn(
            parentSituation, sceneNode, SituationSpawnTriggerType.InitialScene, EntityResolutionContext.Empty());

        // Spawn children via situation context
        tracer.PushSituationContext(parentNode);
        SituationSpawnNode childNode1 = tracer.RecordSituationSpawn(
            childSituation1, sceneNode, SituationSpawnTriggerType.SuccessSpawn, EntityResolutionContext.Empty());
        SituationSpawnNode childNode2 = tracer.RecordSituationSpawn(
            childSituation2, sceneNode, SituationSpawnTriggerType.SuccessSpawn, EntityResolutionContext.Empty());
        tracer.PopSituationContext();

        // Assert - Cascading links
        Assert.Same(parentNode, childNode1.ParentSituation);
        Assert.Same(parentNode, childNode2.ParentSituation);
        Assert.Equal(2, parentNode.SpawnedSituations.Count);
        Assert.Contains(childNode1, parentNode.SpawnedSituations);
        Assert.Contains(childNode2, parentNode.SpawnedSituations);

        // Assert - All still linked to scene
        Assert.Equal(3, sceneNode.Situations.Count);
    }

    [Fact]
    public void FullGraph_ChoiceSpawnsSceneCreatesCorrectLinks()
    {
        // Arrange
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene parentScene = CreateTestScene();
        Situation situation = CreateTestSituation();
        ChoiceTemplate choice = CreateTestChoice();
        Scene childScene1 = CreateTestScene();
        Scene childScene2 = CreateTestScene();

        // Act - Build graph with choice spawning multiple scenes
        SceneSpawnNode parentSceneNode = tracer.RecordSceneSpawn(
            parentScene, "parent", false, SpawnTriggerType.Initial, 1, TimeBlocks.Morning);
        SituationSpawnNode sitNode = tracer.RecordSituationSpawn(
            situation, parentSceneNode, SituationSpawnTriggerType.InitialScene, EntityResolutionContext.Empty());
        ChoiceExecutionNode choiceNode = tracer.RecordChoiceExecution(
            choice, sitNode, "Spawn scenes", true);

        // Push choice context and spawn children
        tracer.PushChoiceContext(choiceNode);
        SceneSpawnNode childSceneNode1 = tracer.RecordSceneSpawn(
            childScene1, "child1", true, SpawnTriggerType.ChoiceReward, 1, TimeBlocks.Midday);
        SceneSpawnNode childSceneNode2 = tracer.RecordSceneSpawn(
            childScene2, "child2", true, SpawnTriggerType.ChoiceReward, 1, TimeBlocks.Midday);
        tracer.PopChoiceContext();

        // Assert - Child scenes linked to choice
        Assert.Same(choiceNode, childSceneNode1.ParentChoice);
        Assert.Same(choiceNode, childSceneNode2.ParentChoice);
        Assert.Equal(2, choiceNode.SpawnedScenes.Count);
        Assert.Contains(childSceneNode1, choiceNode.SpawnedScenes);
        Assert.Contains(childSceneNode2, choiceNode.SpawnedScenes);

        // Assert - Only parent is root
        Assert.Single(tracer.RootScenes);
        Assert.Same(parentSceneNode, tracer.RootScenes[0]);
        Assert.Equal(3, tracer.AllSceneNodes.Count);
    }

    // ==================== HELPER METHODS ====================

    private Scene CreateTestScene()
    {
        return new Scene
        {
            TemplateId = "test_scene",
            Category = StoryCategory.MainStory,
            State = SceneState.Provisional,
            Situations = new List<Situation>()
        };
    }

    private Situation CreateTestSituation()
    {
        return new Situation
        {
            Name = "Test Situation",
            TemplateId = "test_situation",
            Type = SituationType.Normal,
            SystemType = TacticalSystemType.Social,
            LifecycleStatus = LifecycleStatus.Selectable
        };
    }

    private ChoiceTemplate CreateTestChoice()
    {
        return new ChoiceTemplate
        {
            Id = "test_choice",
            ActionTextTemplate = "Test Action",
            ActionType = ChoiceActionType.Instant,
            PathType = ChoicePathType.InstantSuccess
        };
    }

    private ChoiceTemplate CreateTestChoiceWithRequirements()
    {
        return new ChoiceTemplate
        {
            Id = "test_choice",
            ActionTextTemplate = "Test Action",
            ActionType = ChoiceActionType.Instant,
            PathType = ChoicePathType.InstantSuccess,
            RequirementFormula = new CompoundRequirement
            {
                OrPaths = new List<OrPath>
                {
                    new OrPath { InsightRequired = 5 }
                }
            }
        };
    }

    private ChoiceTemplate CreateTestChoiceWithRewards()
    {
        return new ChoiceTemplate
        {
            Id = "test_choice",
            ActionTextTemplate = "Test Action",
            ActionType = ChoiceActionType.Instant,
            PathType = ChoicePathType.InstantSuccess,
            Consequence = new Consequence
            {
                Coins = 10,
                Insight = 2
            }
        };
    }
}
