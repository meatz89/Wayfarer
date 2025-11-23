using Xunit;

/// <summary>
/// HIGHLANDER REFACTORING VERIFICATION TESTS
/// Tests that procedural tracing uses direct object references, no NodeId strings
/// </summary>
public class ProceduralTracingTests
{
    // ==================== LAYER 1: UNIT TESTS (Core Tracer Logic) ====================

    [Fact]
    public void RecordSceneSpawn_CreatesNodeWithObjectReferences()
    {
        // Arrange
        GameWorld gameWorld = CreateTestGameWorld();
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene scene = CreateTestScene();
        Player player = gameWorld.GetPlayer();

        // Act
        SceneSpawnNode node = tracer.RecordSceneSpawn(
            scene,
            "test_scene",
            false,
            SpawnTriggerType.Initial,
            player
        );

        // Assert - HIGHLANDER: No NodeId property
        Assert.NotNull(node);
        Assert.Equal("test_scene", node.SceneTemplateId);
        Assert.Null(node.ParentChoice); // No parent
        Assert.Null(node.ParentSituation);
        Assert.Null(node.ParentScene);
        Assert.Empty(node.SpawnedScenes); // No children yet
        Assert.Empty(node.Situations); // No situations yet
    }

    [Fact]
    public void RecordSituationSpawn_CreatesNodeWithParentSceneReference()
    {
        // Arrange
        GameWorld gameWorld = CreateTestGameWorld();
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene scene = CreateTestScene();
        Situation situation = CreateTestSituation();
        Player player = gameWorld.GetPlayer();

        // Record parent scene first
        SceneSpawnNode sceneNode = tracer.RecordSceneSpawn(scene, "test_scene", false, SpawnTriggerType.Initial, player);

        // Act
        SituationSpawnNode situationNode = tracer.RecordSituationSpawn(
            situation,
            sceneNode, // HIGHLANDER: Pass object, not string
            SituationSpawnTriggerType.InitialScene
        );

        // Assert - HIGHLANDER: Direct object reference
        Assert.NotNull(situationNode);
        Assert.Same(sceneNode, situationNode.ParentScene); // Object identity
        Assert.Contains(situationNode, sceneNode.Situations); // Bidirectional link
    }

    [Fact]
    public void RecordChoiceExecution_CreatesNodeWithParentSituationReference()
    {
        // Arrange
        GameWorld gameWorld = CreateTestGameWorld();
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene scene = CreateTestScene();
        Situation situation = CreateTestSituation();
        ChoiceTemplate choice = CreateTestChoice();
        Player player = gameWorld.GetPlayer();

        SceneSpawnNode sceneNode = tracer.RecordSceneSpawn(scene, "test_scene", false, SpawnTriggerType.Initial, player);
        SituationSpawnNode situationNode = tracer.RecordSituationSpawn(situation, sceneNode, SituationSpawnTriggerType.InitialScene);

        // Act
        ChoiceExecutionNode choiceNode = tracer.RecordChoiceExecution(
            choice,
            situationNode, // HIGHLANDER: Pass object, not string
            "Test Action",
            true
        );

        // Assert - HIGHLANDER: Direct object reference
        Assert.NotNull(choiceNode);
        Assert.Same(situationNode, choiceNode.ParentSituation); // Object identity
        Assert.Contains(choiceNode, situationNode.Choices); // Bidirectional link
    }

    [Fact]
    public void ContextStack_PushPop_WorksWithObjects()
    {
        // Arrange
        GameWorld gameWorld = CreateTestGameWorld();
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene scene = CreateTestScene();
        Situation situation = CreateTestSituation();
        ChoiceTemplate choice = CreateTestChoice();
        Player player = gameWorld.GetPlayer();

        SceneSpawnNode sceneNode = tracer.RecordSceneSpawn(scene, "test_scene", false, SpawnTriggerType.Initial, player);
        SituationSpawnNode situationNode = tracer.RecordSituationSpawn(situation, sceneNode, SituationSpawnTriggerType.InitialScene);
        ChoiceExecutionNode choiceNode = tracer.RecordChoiceExecution(choice, situationNode, "Test", true);

        // Act - Push choice context
        tracer.PushChoiceContext(choiceNode);
        ChoiceExecutionNode currentChoice = tracer.GetCurrentChoiceContext();

        // Assert - Same object reference
        Assert.Same(choiceNode, currentChoice);

        // Act - Pop context
        tracer.PopChoiceContext();
        ChoiceExecutionNode afterPop = tracer.GetCurrentChoiceContext();

        // Assert - Context cleared
        Assert.Null(afterPop);
    }

    [Fact]
    public void SceneSpawn_WithChoiceContext_LinksToParentChoice()
    {
        // Arrange
        GameWorld gameWorld = CreateTestGameWorld();
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene parentScene = CreateTestScene();
        Situation situation = CreateTestSituation();
        ChoiceTemplate choice = CreateTestChoice();
        Scene childScene = CreateTestScene();
        Player player = gameWorld.GetPlayer();

        SceneSpawnNode parentSceneNode = tracer.RecordSceneSpawn(parentScene, "parent_scene", false, SpawnTriggerType.Initial, player);
        SituationSpawnNode situationNode = tracer.RecordSituationSpawn(situation, parentSceneNode, SituationSpawnTriggerType.InitialScene);
        ChoiceExecutionNode choiceNode = tracer.RecordChoiceExecution(choice, situationNode, "Test", true);

        // Act - Push choice context, then spawn child scene
        tracer.PushChoiceContext(choiceNode);
        SceneSpawnNode childSceneNode = tracer.RecordSceneSpawn(childScene, "child_scene", true, SpawnTriggerType.ChoiceReward, player);
        tracer.PopChoiceContext();

        // Assert - HIGHLANDER: Child links to parent via object reference
        Assert.Same(choiceNode, childSceneNode.ParentChoice);
        Assert.Contains(childSceneNode, choiceNode.SpawnedScenes);
    }

    [Fact]
    public void GetNodeForScene_ReturnsObjectReference()
    {
        // Arrange
        GameWorld gameWorld = CreateTestGameWorld();
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene scene = CreateTestScene();
        Player player = gameWorld.GetPlayer();

        SceneSpawnNode originalNode = tracer.RecordSceneSpawn(scene, "test_scene", false, SpawnTriggerType.Initial, player);

        // Act - HIGHLANDER: Get node by entity (ConditionalWeakTable lookup)
        SceneSpawnNode retrievedNode = tracer.GetNodeForScene(scene);

        // Assert - Same object reference
        Assert.Same(originalNode, retrievedNode);
    }

    [Fact]
    public void GetNodeForSituation_ReturnsObjectReference()
    {
        // Arrange
        GameWorld gameWorld = CreateTestGameWorld();
        ProceduralContentTracer tracer = new ProceduralContentTracer();
        Scene scene = CreateTestScene();
        Situation situation = CreateTestSituation();
        Player player = gameWorld.GetPlayer();

        SceneSpawnNode sceneNode = tracer.RecordSceneSpawn(scene, "test_scene", false, SpawnTriggerType.Initial, player);
        SituationSpawnNode originalNode = tracer.RecordSituationSpawn(situation, sceneNode, SituationSpawnTriggerType.InitialScene);

        // Act
        SituationSpawnNode retrievedNode = tracer.GetNodeForSituation(situation);

        // Assert - Same object reference
        Assert.Same(originalNode, retrievedNode);
    }

    // ==================== LAYER 2: INTEGRATION TESTS (Backend Hooks) ====================

    [Fact]
    public async Task InstantChoice_RecordsChoiceAndSpawnsScene()
    {
        // Arrange
        GameWorld gameWorld = CreateTestGameWorld();
        gameWorld.ProceduralTracer = new ProceduralContentTracer();
        gameWorld.ProceduralTracer.IsEnabled = true;

        Scene scene = CreateTestSceneWithSituation(gameWorld);
        Situation situation = scene.Situations[0];
        ChoiceTemplate choice = CreateInstantChoiceWithSceneReward();

        // Act - Execute choice (simulates SceneContent.HandleChoiceSelected)
        SituationSpawnNode situationNode = gameWorld.ProceduralTracer.GetNodeForSituation(situation);
        ChoiceExecutionNode choiceNode = gameWorld.ProceduralTracer.RecordChoiceExecution(choice, situationNode, "Instant Action", true);

        gameWorld.ProceduralTracer.PushChoiceContext(choiceNode);
        Scene spawnedScene = await ApplyChoiceReward(choice.RewardTemplate, gameWorld);
        gameWorld.ProceduralTracer.PopChoiceContext();

        // Assert - Choice recorded, scene spawned and linked
        Assert.NotNull(choiceNode);
        Assert.Same(situationNode, choiceNode.ParentSituation);

        SceneSpawnNode spawnedSceneNode = gameWorld.ProceduralTracer.GetNodeForScene(spawnedScene);
        Assert.NotNull(spawnedSceneNode);
        Assert.Same(choiceNode, spawnedSceneNode.ParentChoice);
        Assert.Contains(spawnedSceneNode, choiceNode.SpawnedScenes);
    }

    [Fact]
    public async Task ChallengeChoice_StoresContextThenAppliesReward()
    {
        // Arrange
        GameWorld gameWorld = CreateTestGameWorld();
        gameWorld.ProceduralTracer = new ProceduralContentTracer();
        gameWorld.ProceduralTracer.IsEnabled = true;

        Scene scene = CreateTestSceneWithSituation(gameWorld);
        Situation situation = scene.Situations[0];
        ChoiceTemplate choice = CreateChallengeChoiceWithSuccessReward();

        // Act - Part 1: Record choice, store in context (simulates SceneContent)
        SituationSpawnNode situationNode = gameWorld.ProceduralTracer.GetNodeForSituation(situation);
        ChoiceExecutionNode choiceNode = gameWorld.ProceduralTracer.RecordChoiceExecution(choice, situationNode, "Challenge Action", true);

        gameWorld.PendingSocialContext = new SocialChallengeContext
        {
            Situation = situation,
            CompletionReward = choice.OnSuccessReward,
            ChoiceExecution = choiceNode // HIGHLANDER: Store object, not string
        };

        // Simulate challenge success
        gameWorld.LastSocialOutcome = new SocialOutcome { Success = true };

        // Act - Part 2: Process outcome (simulates GameFacade.ProcessSocialChallengeOutcome)
        ChoiceExecutionNode storedChoice = gameWorld.PendingSocialContext.ChoiceExecution;
        Assert.Same(choiceNode, storedChoice); // Verify object identity

        gameWorld.ProceduralTracer.PushChoiceContext(storedChoice);
        Scene spawnedScene = await ApplyChoiceReward(choice.OnSuccessReward, gameWorld);
        gameWorld.ProceduralTracer.PopChoiceContext();

        // Assert - Scene spawned by choice
        SceneSpawnNode spawnedSceneNode = gameWorld.ProceduralTracer.GetNodeForScene(spawnedScene);
        Assert.NotNull(spawnedSceneNode);
        Assert.Same(choiceNode, spawnedSceneNode.ParentChoice);
    }

    [Fact]
    public void CascadingSituationSpawn_LinksToParentSituation()
    {
        // Arrange
        GameWorld gameWorld = CreateTestGameWorld();
        gameWorld.ProceduralTracer = new ProceduralContentTracer();
        gameWorld.ProceduralTracer.IsEnabled = true;

        Scene scene = CreateTestSceneWithSituation(gameWorld);
        Situation parentSituation = scene.Situations[0];
        Situation childSituation = CreateTestSituation();

        SceneSpawnNode sceneNode = gameWorld.ProceduralTracer.GetNodeForScene(scene);
        SituationSpawnNode parentNode = gameWorld.ProceduralTracer.GetNodeForSituation(parentSituation);

        // Act - Simulate SpawnFacade.ExecuteSpawnRules
        gameWorld.ProceduralTracer.PushSituationContext(parentNode);
        SituationSpawnNode childNode = gameWorld.ProceduralTracer.RecordSituationSpawn(
            childSituation,
            sceneNode,
            SituationSpawnTriggerType.SuccessSpawn
        );
        gameWorld.ProceduralTracer.PopSituationContext();

        // Assert - HIGHLANDER: Child links to parent via object
        Assert.Same(parentNode, childNode.ParentSituation);
        Assert.Contains(childNode, parentNode.SpawnedSituations);
    }

    // ==================== HELPER METHODS ====================

    private GameWorld CreateTestGameWorld()
    {
        GameWorld gameWorld = new GameWorld();
        Player player = new Player();
        gameWorld.Player = player;
        return gameWorld;
    }

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
            Type = SituationType.Normal,
            SystemType = TacticalSystemType.Social
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

    private Scene CreateTestSceneWithSituation(GameWorld gameWorld)
    {
        Scene scene = CreateTestScene();
        Situation situation = CreateTestSituation();
        situation.ParentScene = scene;
        scene.Situations.Add(situation);

        gameWorld.Scenes.Add(scene);

        // Record in tracer
        Player player = gameWorld.GetPlayer();
        SceneSpawnNode sceneNode = gameWorld.ProceduralTracer.RecordSceneSpawn(scene, scene.TemplateId, false, SpawnTriggerType.Initial, player);
        gameWorld.ProceduralTracer.RecordSituationSpawn(situation, sceneNode, SituationSpawnTriggerType.InitialScene);

        return scene;
    }

    private ChoiceTemplate CreateInstantChoiceWithSceneReward()
    {
        ChoiceTemplate choice = CreateTestChoice();
        choice.RewardTemplate = new ChoiceReward
        {
            ScenesToSpawn = new List<string> { "reward_scene" }
        };
        return choice;
    }

    private ChoiceTemplate CreateChallengeChoiceWithSuccessReward()
    {
        ChoiceTemplate choice = CreateTestChoice();
        choice.ActionType = ChoiceActionType.StartChallenge;
        choice.ChallengeType = TacticalSystemType.Social;
        choice.OnSuccessReward = new ChoiceReward
        {
            ScenesToSpawn = new List<string> { "success_scene" }
        };
        return choice;
    }

    private async Task<Scene> ApplyChoiceReward(ChoiceReward reward, GameWorld gameWorld)
    {
        // Simplified reward application - in real code this goes through RewardApplicationService
        Scene spawnedScene = new Scene
        {
            TemplateId = reward.ScenesToSpawn[0],
            Category = StoryCategory.SideStory,
            State = SceneState.Provisional
        };

        gameWorld.Scenes.Add(spawnedScene);

        // Record in tracer
        Player player = gameWorld.GetPlayer();
        gameWorld.ProceduralTracer.RecordSceneSpawn(spawnedScene, spawnedScene.TemplateId, true, SpawnTriggerType.ChoiceReward, player);

        return spawnedScene;
    }
}
