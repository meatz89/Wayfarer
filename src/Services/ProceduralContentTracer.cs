using System.Runtime.CompilerServices;

/// <summary>
/// Procedural Content Tracing System - tracks complete spawn graph
/// Captures scene spawning, situation creation, and choice execution with parent-child relationships
/// Enables debugging visualization of procedural generation
/// SYNCHRONOUS: No async/await - all operations are immediate
/// HIGHLANDER: Pure object references, NO NodeId strings
/// </summary>
public class ProceduralContentTracer
{
    // ==================== COLLECTIONS ====================

    /// <summary>
    /// Root scenes (no parent) - entry points to spawn trees
    /// </summary>
    public List<SceneSpawnNode> RootScenes { get; private set; } = new List<SceneSpawnNode>();

    /// <summary>
    /// All scene nodes (flat index for UI binding)
    /// </summary>
    public List<SceneSpawnNode> AllSceneNodes { get; private set; } = new List<SceneSpawnNode>();

    /// <summary>
    /// All situation nodes (flat index for UI binding)
    /// </summary>
    public List<SituationSpawnNode> AllSituationNodes { get; private set; } = new List<SituationSpawnNode>();

    /// <summary>
    /// All choice execution nodes (flat index for UI binding)
    /// </summary>
    public List<ChoiceExecutionNode> AllChoiceNodes { get; private set; } = new List<ChoiceExecutionNode>();

    /// <summary>
    /// Enable/disable tracing (can disable for performance)
    /// When false, all recording methods return immediately
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    // ==================== ENTITY MAPPING (ConditionalWeakTable for state updates) ====================

    /// <summary>
    /// Maps Scene entities to their trace nodes
    /// Uses weak references - allows GC to collect scenes
    /// Enables state updates: when scene.State changes, find node and update trace
    /// </summary>
    private ConditionalWeakTable<Scene, SceneSpawnNode> sceneToNode = new ConditionalWeakTable<Scene, SceneSpawnNode>();

    /// <summary>
    /// Maps Situation entities to their trace nodes
    /// Uses weak references - allows GC
    /// </summary>
    private ConditionalWeakTable<Situation, SituationSpawnNode> situationToNode = new ConditionalWeakTable<Situation, SituationSpawnNode>();

    // ==================== CONTEXT STACKS (non-invasive parent linking) ====================

    /// <summary>
    /// Stack of current choice context nodes
    /// Enables automatic parent linking without changing method signatures
    /// Push before spawning, pop after spawning complete
    /// HIGHLANDER: Direct object references, no NodeId strings
    /// </summary>
    private Stack<ChoiceExecutionNode> choiceContextStack = new Stack<ChoiceExecutionNode>();

    /// <summary>
    /// Stack of current situation context nodes
    /// Enables automatic parent linking for situation cascades
    /// HIGHLANDER: Direct object references, no NodeId strings
    /// </summary>
    private Stack<SituationSpawnNode> situationContextStack = new Stack<SituationSpawnNode>();

    // ==================== RECORD METHODS ====================

    /// <summary>
    /// Record scene spawn event
    /// Creates trace node, links to parent (if context exists), adds to collections
    /// </summary>
    public SceneSpawnNode RecordSceneSpawn(
        Scene scene,
        string sceneTemplateId,
        bool isProcedurallyGenerated,
        SpawnTriggerType spawnTrigger,
        Player player)
    {
        if (!IsEnabled) return null;

        try
        {
            SceneSpawnNode node = new SceneSpawnNode
            {
                SceneTemplateId = sceneTemplateId,
                DisplayName = scene.Template?.DisplayName ?? scene.TemplateId ?? "Unknown Scene",
                SpawnTimestamp = DateTime.UtcNow,
                GameDay = player.CurrentDay,
                GameTimeBlock = player.CurrentTimeBlock,
                SpawnTrigger = spawnTrigger,
                ParentChoice = GetCurrentChoiceContext(),
                ParentSituation = GetCurrentSituationContext(),
                Category = scene.Category,
                MainStorySequence = scene.MainStorySequence,
                EstimatedDifficulty = scene.EstimatedDifficulty,
                State = scene.State,
                CurrentState = scene.State,
                ProgressionMode = scene.ProgressionMode,
                SituationCount = scene.SituationCount,
                IsProcedurallyGenerated = isProcedurallyGenerated
            };

            // Add to collections
            AllSceneNodes.Add(node);

            // Add to root if no parent
            if (node.ParentChoice == null && node.ParentSituation == null)
            {
                RootScenes.Add(node);
            }

            // Map entity to node for later updates
            sceneToNode.Add(scene, node);

            // Link to parent choice (if exists)
            if (node.ParentChoice != null)
            {
                node.ParentChoice.SpawnedScenes.Add(node);
            }

            // Link to parent scene (if spawned from parent situation)
            if (node.ParentSituation != null)
            {
                node.ParentScene = node.ParentSituation.ParentScene;
            }

            return node;
        }
        catch (Exception ex)
        {
            // Trace failures should NEVER crash the game
            Console.WriteLine($"ProceduralContentTracer.RecordSceneSpawn failed: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Record situation spawn event
    /// Creates trace node, links to parent scene and situation (if cascade)
    /// </summary>
    public SituationSpawnNode RecordSituationSpawn(
        Situation situation,
        SceneSpawnNode parentScene,
        SituationSpawnTriggerType spawnTrigger)
    {
        if (!IsEnabled) return null;

        try
        {
            SituationSpawnNode node = new SituationSpawnNode
            {
                SituationTemplateId = situation.TemplateId,
                Name = situation.Name,
                Description = situation.Description,
                SpawnTimestamp = DateTime.UtcNow,
                ParentScene = parentScene,
                SpawnTrigger = spawnTrigger,
                ParentSituation = GetCurrentSituationContext(),
                Type = situation.Type,
                SystemType = situation.SystemType,
                InteractionType = situation.InteractionType,
                Location = situation.Location != null ? SnapshotFactory.CreateLocationSnapshot(situation.Location) : null,
                NPC = situation.Npc != null ? SnapshotFactory.CreateNPCSnapshot(situation.Npc) : null,
                Route = situation.Route != null ? SnapshotFactory.CreateRouteSnapshot(situation.Route) : null,
                SegmentIndex = situation.SegmentIndex > 0 ? situation.SegmentIndex : null,
                LifecycleStatus = LifecycleStatus.Selectable
            };

            // Add to collections
            AllSituationNodes.Add(node);

            // Map entity to node
            situationToNode.Add(situation, node);

            // Link to parent scene
            if (parentScene != null)
            {
                parentScene.Situations.Add(node);
            }

            // Link to parent situation (if cascade)
            if (node.ParentSituation != null)
            {
                node.ParentSituation.SpawnedSituations.Add(node);
            }

            return node;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ProceduralContentTracer.RecordSituationSpawn failed: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Record choice execution event
    /// Creates trace node, links to parent situation
    /// </summary>
    public ChoiceExecutionNode RecordChoiceExecution(
        ChoiceTemplate choiceTemplate,
        SituationSpawnNode parentSituation,
        string actionText,
        bool playerMetRequirements)
    {
        if (!IsEnabled) return null;

        try
        {
            ChoiceExecutionNode node = new ChoiceExecutionNode
            {
                ChoiceId = choiceTemplate.Id,
                ActionText = actionText,
                ExecutionTimestamp = DateTime.UtcNow,
                ParentSituation = parentSituation,
                PathType = choiceTemplate.PathType,
                ActionType = choiceTemplate.ActionType,
                PlayerMetRequirements = playerMetRequirements,
                RequirementSnapshot = choiceTemplate.RequirementFormula != null
                    ? SnapshotFactory.CreateRequirementSnapshot(choiceTemplate.RequirementFormula)
                    : null,
                CostSnapshot = choiceTemplate.CostTemplate != null
                    ? SnapshotFactory.CreateCostSnapshot(choiceTemplate.CostTemplate)
                    : null,
                RewardSnapshot = choiceTemplate.RewardTemplate != null
                    ? SnapshotFactory.CreateRewardSnapshot(choiceTemplate.RewardTemplate)
                    : null,
                OnSuccessRewardSnapshot = choiceTemplate.OnSuccessReward != null
                    ? SnapshotFactory.CreateRewardSnapshot(choiceTemplate.OnSuccessReward)
                    : null,
                OnFailureRewardSnapshot = choiceTemplate.OnFailureReward != null
                    ? SnapshotFactory.CreateRewardSnapshot(choiceTemplate.OnFailureReward)
                    : null
            };

            // Add to collections
            AllChoiceNodes.Add(node);

            // Link to parent situation
            if (parentSituation != null)
            {
                parentSituation.Choices.Add(node);
            }

            return node;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ProceduralContentTracer.RecordChoiceExecution failed: {ex.Message}");
            return null;
        }
    }

    // ==================== STATE UPDATE METHODS ====================

    /// <summary>
    /// Update scene state when it transitions
    /// Finds node via ConditionalWeakTable mapping
    /// </summary>
    public void UpdateSceneState(Scene scene, SceneState newState, DateTime timestamp)
    {
        if (!IsEnabled) return;

        try
        {
            if (sceneToNode.TryGetValue(scene, out SceneSpawnNode node))
            {
                node.CurrentState = newState;

                if (newState == SceneState.Active && !node.ActivatedTimestamp.HasValue)
                {
                    node.ActivatedTimestamp = timestamp;
                }
                else if (newState == SceneState.Completed && !node.CompletedTimestamp.HasValue)
                {
                    node.CompletedTimestamp = timestamp;
                }
                else if (newState == SceneState.Expired && !node.ExpiredTimestamp.HasValue)
                {
                    node.ExpiredTimestamp = timestamp;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ProceduralContentTracer.UpdateSceneState failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Mark situation as completed
    /// </summary>
    public void MarkSituationCompleted(Situation situation, bool? challengeSucceeded)
    {
        if (!IsEnabled) return;

        try
        {
            if (situationToNode.TryGetValue(situation, out SituationSpawnNode node))
            {
                node.CompletedTimestamp = DateTime.UtcNow;
                node.LastChallengeSucceeded = challengeSucceeded;
                node.LifecycleStatus = LifecycleStatus.Completed;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ProceduralContentTracer.MarkSituationCompleted failed: {ex.Message}");
        }
    }

    // ==================== CONTEXT STACK METHODS ====================

    public void PushChoiceContext(ChoiceExecutionNode choiceNode)
    {
        if (!IsEnabled) return;
        choiceContextStack.Push(choiceNode);
    }

    public void PopChoiceContext()
    {
        if (!IsEnabled) return;
        if (choiceContextStack.Count > 0)
        {
            choiceContextStack.Pop();
        }
    }

    public ChoiceExecutionNode GetCurrentChoiceContext()
    {
        if (!IsEnabled) return null;
        return choiceContextStack.Count > 0 ? choiceContextStack.Peek() : null;
    }

    public void PushSituationContext(SituationSpawnNode situationNode)
    {
        if (!IsEnabled) return;
        situationContextStack.Push(situationNode);
    }

    public void PopSituationContext()
    {
        if (!IsEnabled) return;
        if (situationContextStack.Count > 0)
        {
            situationContextStack.Pop();
        }
    }

    public SituationSpawnNode GetCurrentSituationContext()
    {
        if (!IsEnabled) return null;
        return situationContextStack.Count > 0 ? situationContextStack.Peek() : null;
    }

    // ==================== QUERY METHODS ====================

    /// <summary>
    /// Get trace node for scene entity (if exists in trace)
    /// </summary>
    public SceneSpawnNode GetNodeForScene(Scene scene)
    {
        if (!IsEnabled) return null;
        sceneToNode.TryGetValue(scene, out SceneSpawnNode node);
        return node;
    }

    /// <summary>
    /// Get trace node for situation entity (if exists in trace)
    /// </summary>
    public SituationSpawnNode GetNodeForSituation(Situation situation)
    {
        if (!IsEnabled) return null;
        situationToNode.TryGetValue(situation, out SituationSpawnNode node);
        return node;
    }

    /// <summary>
    /// Clear all trace data (new game start)
    /// </summary>
    public void Clear()
    {
        RootScenes.Clear();
        AllSceneNodes.Clear();
        AllSituationNodes.Clear();
        AllChoiceNodes.Clear();
        sceneToNode = new ConditionalWeakTable<Scene, SceneSpawnNode>();
        situationToNode = new ConditionalWeakTable<Situation, SituationSpawnNode>();
        choiceContextStack.Clear();
        situationContextStack.Clear();
    }
}
