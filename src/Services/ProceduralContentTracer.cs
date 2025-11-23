using System.Runtime.CompilerServices;

/// <summary>
/// Procedural Content Tracing System - tracks complete spawn graph
/// Captures scene spawning, situation creation, and choice execution with parent-child relationships
/// Enables debugging visualization of procedural generation
/// SYNCHRONOUS: No async/await - all operations are immediate
/// </summary>
public class ProceduralContentTracer
{
    // ==================== COLLECTIONS ====================

    /// <summary>
    /// Root scenes (no parent) - entry points to spawn trees
    /// </summary>
    public List<SceneSpawnNode> RootScenes { get; private set; } = new List<SceneSpawnNode>();

    /// <summary>
    /// All scene nodes (flat index for lookup by NodeId)
    /// </summary>
    public List<SceneSpawnNode> AllSceneNodes { get; private set; } = new List<SceneSpawnNode>();

    /// <summary>
    /// All situation nodes (flat index for lookup by NodeId)
    /// </summary>
    public List<SituationSpawnNode> AllSituationNodes { get; private set; } = new List<SituationSpawnNode>();

    /// <summary>
    /// All choice execution nodes (flat index for lookup by NodeId)
    /// </summary>
    public List<ChoiceExecutionNode> AllChoiceNodes { get; private set; } = new List<ChoiceExecutionNode>();

    /// <summary>
    /// Enable/disable tracing (can disable for performance)
    /// When false, all recording methods return immediately
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    // ==================== ENTITY MAPPING (ConditionalWeakTable for state updates) ====================

    /// <summary>
    /// Maps Scene entities to their NodeIds
    /// Uses weak references - allows GC to collect scenes
    /// Enables state updates: when scene.State changes, find NodeId and update trace
    /// </summary>
    private ConditionalWeakTable<Scene, string> sceneToNodeId = new ConditionalWeakTable<Scene, string>();

    /// <summary>
    /// Maps Situation entities to their NodeIds
    /// Uses weak references - allows GC
    /// </summary>
    private ConditionalWeakTable<Situation, string> situationToNodeId = new ConditionalWeakTable<Situation, string>();

    // ==================== CONTEXT STACKS (non-invasive parent linking) ====================

    /// <summary>
    /// Stack of current choice context NodeIds
    /// Enables automatic parent linking without changing method signatures
    /// Push before spawning, pop after spawning complete
    /// </summary>
    private Stack<string> choiceContextStack = new Stack<string>();

    /// <summary>
    /// Stack of current situation context NodeIds
    /// Enables automatic parent linking for situation cascades
    /// </summary>
    private Stack<string> situationContextStack = new Stack<string>();

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
            string nodeId = Guid.NewGuid().ToString();

            SceneSpawnNode node = new SceneSpawnNode
            {
                NodeId = nodeId,
                SceneTemplateId = sceneTemplateId,
                DisplayName = scene.Template?.DisplayName ?? scene.TemplateId ?? "Unknown Scene",
                SpawnTimestamp = DateTime.UtcNow,
                GameDay = player.CurrentDay,
                GameTimeBlock = player.CurrentTimeBlock,
                SpawnTrigger = spawnTrigger,
                ParentChoiceNodeId = GetCurrentChoiceContext(),
                ParentSituationNodeId = GetCurrentSituationContext(),
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
            if (string.IsNullOrEmpty(node.ParentChoiceNodeId) && string.IsNullOrEmpty(node.ParentSituationNodeId))
            {
                RootScenes.Add(node);
            }

            // Map entity to NodeId for later updates
            sceneToNodeId.Add(scene, nodeId);

            // Link to parent choice (if exists)
            if (!string.IsNullOrEmpty(node.ParentChoiceNodeId))
            {
                ChoiceExecutionNode parentChoice = AllChoiceNodes.FirstOrDefault(c => c.NodeId == node.ParentChoiceNodeId);
                if (parentChoice != null)
                {
                    parentChoice.SpawnedSceneNodeIds.Add(nodeId);
                }
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
        string parentSceneNodeId,
        SituationSpawnTriggerType spawnTrigger)
    {
        if (!IsEnabled) return null;

        try
        {
            string nodeId = Guid.NewGuid().ToString();

            SituationSpawnNode node = new SituationSpawnNode
            {
                NodeId = nodeId,
                SituationTemplateId = situation.TemplateId,
                Name = situation.Name,
                Description = situation.Description,
                SpawnTimestamp = DateTime.UtcNow,
                ParentSceneNodeId = parentSceneNodeId,
                SpawnTrigger = spawnTrigger,
                ParentSituationNodeId = GetCurrentSituationContext(),
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

            // Map entity to NodeId
            situationToNodeId.Add(situation, nodeId);

            // Link to parent scene
            if (!string.IsNullOrEmpty(parentSceneNodeId))
            {
                SceneSpawnNode parentScene = AllSceneNodes.FirstOrDefault(s => s.NodeId == parentSceneNodeId);
                if (parentScene != null)
                {
                    parentScene.Situations.Add(node);
                }
            }

            // Link to parent situation (if cascade)
            if (!string.IsNullOrEmpty(node.ParentSituationNodeId))
            {
                SituationSpawnNode parentSituation = AllSituationNodes.FirstOrDefault(s => s.NodeId == node.ParentSituationNodeId);
                if (parentSituation != null)
                {
                    parentSituation.SpawnedSituationNodeIds.Add(nodeId);
                }
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
        string parentSituationNodeId,
        string actionText,
        bool playerMetRequirements)
    {
        if (!IsEnabled) return null;

        try
        {
            string nodeId = Guid.NewGuid().ToString();

            ChoiceExecutionNode node = new ChoiceExecutionNode
            {
                NodeId = nodeId,
                ChoiceId = choiceTemplate.Id,
                ActionText = actionText,
                ExecutionTimestamp = DateTime.UtcNow,
                ParentSituationNodeId = parentSituationNodeId,
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
            if (!string.IsNullOrEmpty(parentSituationNodeId))
            {
                SituationSpawnNode parentSituation = AllSituationNodes.FirstOrDefault(s => s.NodeId == parentSituationNodeId);
                if (parentSituation != null)
                {
                    parentSituation.Choices.Add(node);
                }
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
            if (sceneToNodeId.TryGetValue(scene, out string nodeId))
            {
                SceneSpawnNode node = AllSceneNodes.FirstOrDefault(n => n.NodeId == nodeId);
                if (node != null)
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
            if (situationToNodeId.TryGetValue(situation, out string nodeId))
            {
                SituationSpawnNode node = AllSituationNodes.FirstOrDefault(n => n.NodeId == nodeId);
                if (node != null)
                {
                    node.CompletedTimestamp = DateTime.UtcNow;
                    node.LastChallengeSucceeded = challengeSucceeded;
                    node.LifecycleStatus = LifecycleStatus.Completed;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ProceduralContentTracer.MarkSituationCompleted failed: {ex.Message}");
        }
    }

    // ==================== CONTEXT STACK METHODS ====================

    public void PushChoiceContext(string choiceNodeId)
    {
        if (!IsEnabled) return;
        choiceContextStack.Push(choiceNodeId);
    }

    public void PopChoiceContext()
    {
        if (!IsEnabled) return;
        if (choiceContextStack.Count > 0)
        {
            choiceContextStack.Pop();
        }
    }

    public string GetCurrentChoiceContext()
    {
        if (!IsEnabled) return null;
        return choiceContextStack.Count > 0 ? choiceContextStack.Peek() : null;
    }

    public void PushSituationContext(string situationNodeId)
    {
        if (!IsEnabled) return;
        situationContextStack.Push(situationNodeId);
    }

    public void PopSituationContext()
    {
        if (!IsEnabled) return;
        if (situationContextStack.Count > 0)
        {
            situationContextStack.Pop();
        }
    }

    public string GetCurrentSituationContext()
    {
        if (!IsEnabled) return null;
        return situationContextStack.Count > 0 ? situationContextStack.Peek() : null;
    }

    // ==================== QUERY METHODS ====================

    /// <summary>
    /// Get NodeId for scene entity (if exists in trace)
    /// </summary>
    public string GetNodeIdForScene(Scene scene)
    {
        if (!IsEnabled) return null;
        sceneToNodeId.TryGetValue(scene, out string nodeId);
        return nodeId;
    }

    /// <summary>
    /// Get NodeId for situation entity (if exists in trace)
    /// </summary>
    public string GetNodeIdForSituation(Situation situation)
    {
        if (!IsEnabled) return null;
        situationToNodeId.TryGetValue(situation, out string nodeId);
        return nodeId;
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
        sceneToNodeId = new ConditionalWeakTable<Scene, string>();
        situationToNodeId = new ConditionalWeakTable<Situation, string>();
        choiceContextStack.Clear();
        situationContextStack.Clear();
    }
}
