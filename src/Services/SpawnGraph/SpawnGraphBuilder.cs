using Blazor.Diagrams;
using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using Microsoft.JSInterop;

public class SpawnGraphBuilder
{
    private IJSRuntime JSRuntime { get; }

    public SpawnGraphBuilder(IJSRuntime jsRuntime)
    {
        JSRuntime = jsRuntime;
    }

    public async Task<SpawnGraphBuildResult> BuildGraphAsync(ProceduralContentTracer tracer, BlazorDiagram diagram)
    {
        if (tracer == null || !tracer.IsEnabled || tracer.AllSceneNodes.Count == 0)
        {
            return new SpawnGraphBuildResult
            {
                Success = false,
                Message = "No tracer data available"
            };
        }

        diagram.Nodes.Clear();
        diagram.Links.Clear();

        DagreGraphData graphData = BuildDagreGraphData(tracer);

        DagreLayoutResult layoutResult = await ComputeLayoutAsync(graphData);

        if (layoutResult == null)
        {
            return BuildWithFallbackLayout(tracer, diagram);
        }

        ApplyLayoutToDiagram(tracer, diagram, layoutResult);

        return new SpawnGraphBuildResult
        {
            Success = true,
            NodeCount = diagram.Nodes.Count,
            LinkCount = diagram.Links.Count,
            GraphWidth = layoutResult.GraphWidth,
            GraphHeight = layoutResult.GraphHeight
        };
    }

    private DagreGraphData BuildDagreGraphData(ProceduralContentTracer tracer)
    {
        DagreGraphData graphData = new DagreGraphData();

        Dictionary<SceneSpawnNode, string> sceneIds = new Dictionary<SceneSpawnNode, string>();
        Dictionary<SituationSpawnNode, string> situationIds = new Dictionary<SituationSpawnNode, string>();
        Dictionary<ChoiceExecutionNode, string> choiceIds = new Dictionary<ChoiceExecutionNode, string>();
        Dictionary<string, string> entityIds = new Dictionary<string, string>();

        int nodeIndex = 0;

        foreach (SceneSpawnNode sceneNode in tracer.AllSceneNodes)
        {
            string nodeId = "scene_" + nodeIndex++;
            sceneIds[sceneNode] = nodeId;

            graphData.Nodes.Add(new DagreNode
            {
                Id = nodeId,
                Label = sceneNode.DisplayName,
                Type = "scene",
                Width = 200,
                Height = 100
            });
        }

        foreach (SituationSpawnNode situationNode in tracer.AllSituationNodes)
        {
            string nodeId = "situation_" + nodeIndex++;
            situationIds[situationNode] = nodeId;

            graphData.Nodes.Add(new DagreNode
            {
                Id = nodeId,
                Label = situationNode.Name ?? "Situation",
                Type = "situation",
                Width = 180,
                Height = 80
            });
        }

        foreach (ChoiceExecutionNode choiceNode in tracer.AllChoiceNodes)
        {
            string nodeId = "choice_" + nodeIndex++;
            choiceIds[choiceNode] = nodeId;

            graphData.Nodes.Add(new DagreNode
            {
                Id = nodeId,
                Label = TruncateText(choiceNode.ActionText, 30),
                Type = "choice",
                Width = 160,
                Height = 70
            });
        }

        foreach (SituationSpawnNode situationNode in tracer.AllSituationNodes)
        {
            if (situationNode.Location != null)
            {
                string entityKey = "loc_" + situationNode.Location.Name;
                if (!entityIds.ContainsKey(entityKey))
                {
                    string entityNodeId = "entity_" + nodeIndex++;
                    entityIds[entityKey] = entityNodeId;

                    graphData.Nodes.Add(new DagreNode
                    {
                        Id = entityNodeId,
                        Label = situationNode.Location.Name,
                        Type = "entity",
                        Width = 150,
                        Height = 60
                    });
                }
            }

            if (situationNode.NPC != null)
            {
                string entityKey = "npc_" + situationNode.NPC.Name;
                if (!entityIds.ContainsKey(entityKey))
                {
                    string entityNodeId = "entity_" + nodeIndex++;
                    entityIds[entityKey] = entityNodeId;

                    graphData.Nodes.Add(new DagreNode
                    {
                        Id = entityNodeId,
                        Label = situationNode.NPC.Name,
                        Type = "entity",
                        Width = 150,
                        Height = 60
                    });
                }
            }

            if (situationNode.Route != null)
            {
                string entityKey = "route_" + situationNode.Route.OriginLocationName + "_" + situationNode.Route.DestinationLocationName;
                if (!entityIds.ContainsKey(entityKey))
                {
                    string entityNodeId = "entity_" + nodeIndex++;
                    entityIds[entityKey] = entityNodeId;

                    graphData.Nodes.Add(new DagreNode
                    {
                        Id = entityNodeId,
                        Label = situationNode.Route.OriginLocationName + " -> " + situationNode.Route.DestinationLocationName,
                        Type = "entity",
                        Width = 150,
                        Height = 60
                    });
                }
            }
        }

        foreach (SceneSpawnNode sceneNode in tracer.AllSceneNodes)
        {
            string sceneId = sceneIds[sceneNode];

            foreach (SituationSpawnNode situationNode in sceneNode.Situations)
            {
                if (situationIds.TryGetValue(situationNode, out string situationId))
                {
                    graphData.Edges.Add(new DagreEdge { Source = sceneId, Target = situationId });
                }
            }
        }

        foreach (SituationSpawnNode situationNode in tracer.AllSituationNodes)
        {
            if (situationIds.TryGetValue(situationNode, out string situationId))
            {
                foreach (ChoiceExecutionNode choiceNode in situationNode.Choices)
                {
                    if (choiceIds.TryGetValue(choiceNode, out string choiceId))
                    {
                        graphData.Edges.Add(new DagreEdge { Source = situationId, Target = choiceId });
                    }
                }

                if (situationNode.Location != null)
                {
                    string entityKey = "loc_" + situationNode.Location.Name;
                    if (entityIds.TryGetValue(entityKey, out string entityNodeId))
                    {
                        graphData.Edges.Add(new DagreEdge { Source = situationId, Target = entityNodeId });
                    }
                }

                if (situationNode.NPC != null)
                {
                    string entityKey = "npc_" + situationNode.NPC.Name;
                    if (entityIds.TryGetValue(entityKey, out string entityNodeId))
                    {
                        graphData.Edges.Add(new DagreEdge { Source = situationId, Target = entityNodeId });
                    }
                }

                if (situationNode.Route != null)
                {
                    string entityKey = "route_" + situationNode.Route.OriginLocationName + "_" + situationNode.Route.DestinationLocationName;
                    if (entityIds.TryGetValue(entityKey, out string entityNodeId))
                    {
                        graphData.Edges.Add(new DagreEdge { Source = situationId, Target = entityNodeId });
                    }
                }
            }
        }

        foreach (ChoiceExecutionNode choiceNode in tracer.AllChoiceNodes)
        {
            if (choiceIds.TryGetValue(choiceNode, out string choiceId))
            {
                foreach (SceneSpawnNode spawnedScene in choiceNode.SpawnedScenes)
                {
                    if (sceneIds.TryGetValue(spawnedScene, out string sceneId))
                    {
                        graphData.Edges.Add(new DagreEdge { Source = choiceId, Target = sceneId });
                    }
                }
            }
        }

        return graphData;
    }

    private async Task<DagreLayoutResult> ComputeLayoutAsync(DagreGraphData graphData)
    {
        try
        {
            return await JSRuntime.InvokeAsync<DagreLayoutResult>("DagreLayout.computeLayout", graphData);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SpawnGraphBuilder.ComputeLayoutAsync failed: {ex.Message}");
            return null;
        }
    }

    private void ApplyLayoutToDiagram(ProceduralContentTracer tracer, BlazorDiagram diagram, DagreLayoutResult layoutResult)
    {
        Dictionary<string, DagreLayoutNode> positionMap = new Dictionary<string, DagreLayoutNode>();
        foreach (DagreLayoutNode layoutNode in layoutResult.Nodes)
        {
            positionMap[layoutNode.Id] = layoutNode;
        }

        Dictionary<SceneSpawnNode, SceneNodeModel> sceneModelMap = new Dictionary<SceneSpawnNode, SceneNodeModel>();
        Dictionary<SituationSpawnNode, SituationNodeModel> situationModelMap = new Dictionary<SituationSpawnNode, SituationNodeModel>();
        Dictionary<ChoiceExecutionNode, ChoiceNodeModel> choiceModelMap = new Dictionary<ChoiceExecutionNode, ChoiceNodeModel>();
        Dictionary<string, EntityNodeModel> entityModelMap = new Dictionary<string, EntityNodeModel>();

        int nodeIndex = 0;

        foreach (SceneSpawnNode sceneNode in tracer.AllSceneNodes)
        {
            string nodeId = "scene_" + nodeIndex++;
            Point position = GetPositionFromLayout(positionMap, nodeId, 50, 100 + (nodeIndex * 200));

            SceneNodeModel model = new SceneNodeModel(position, sceneNode);
            diagram.Nodes.Add(model);
            sceneModelMap[sceneNode] = model;
        }

        foreach (SituationSpawnNode situationNode in tracer.AllSituationNodes)
        {
            string nodeId = "situation_" + nodeIndex++;
            Point position = GetPositionFromLayout(positionMap, nodeId, 300, 100);

            SituationNodeModel model = new SituationNodeModel(position, situationNode);
            diagram.Nodes.Add(model);
            situationModelMap[situationNode] = model;
        }

        foreach (ChoiceExecutionNode choiceNode in tracer.AllChoiceNodes)
        {
            string nodeId = "choice_" + nodeIndex++;
            Point position = GetPositionFromLayout(positionMap, nodeId, 550, 100);

            ChoiceNodeModel model = new ChoiceNodeModel(position, choiceNode);
            diagram.Nodes.Add(model);
            choiceModelMap[choiceNode] = model;
        }

        foreach (SituationSpawnNode situationNode in tracer.AllSituationNodes)
        {
            if (situationNode.Location != null)
            {
                string entityKey = "loc_" + situationNode.Location.Name;
                if (!entityModelMap.ContainsKey(entityKey))
                {
                    string entityNodeId = "entity_" + nodeIndex++;
                    Point position = GetPositionFromLayout(positionMap, entityNodeId, 800, 100);

                    EntityNodeModel model = new EntityNodeModel(position, situationNode.Location);
                    diagram.Nodes.Add(model);
                    entityModelMap[entityKey] = model;
                }
            }

            if (situationNode.NPC != null)
            {
                string entityKey = "npc_" + situationNode.NPC.Name;
                if (!entityModelMap.ContainsKey(entityKey))
                {
                    string entityNodeId = "entity_" + nodeIndex++;
                    Point position = GetPositionFromLayout(positionMap, entityNodeId, 800, 180);

                    EntityNodeModel model = new EntityNodeModel(position, situationNode.NPC);
                    diagram.Nodes.Add(model);
                    entityModelMap[entityKey] = model;
                }
            }

            if (situationNode.Route != null)
            {
                string entityKey = "route_" + situationNode.Route.OriginLocationName + "_" + situationNode.Route.DestinationLocationName;
                if (!entityModelMap.ContainsKey(entityKey))
                {
                    string entityNodeId = "entity_" + nodeIndex++;
                    Point position = GetPositionFromLayout(positionMap, entityNodeId, 800, 260);

                    EntityNodeModel model = new EntityNodeModel(position, situationNode.Route);
                    diagram.Nodes.Add(model);
                    entityModelMap[entityKey] = model;
                }
            }
        }

        foreach (SceneSpawnNode sceneNode in tracer.AllSceneNodes)
        {
            if (!sceneModelMap.TryGetValue(sceneNode, out SceneNodeModel sceneModel)) continue;

            foreach (SituationSpawnNode situationNode in sceneNode.Situations)
            {
                if (situationModelMap.TryGetValue(situationNode, out SituationNodeModel situationModel))
                {
                    diagram.Links.Add(new SpawnGraphLinkModel(sceneModel, situationModel, SpawnGraphLinkType.Hierarchy));
                }
            }
        }

        foreach (SituationSpawnNode situationNode in tracer.AllSituationNodes)
        {
            if (!situationModelMap.TryGetValue(situationNode, out SituationNodeModel situationModel)) continue;

            foreach (ChoiceExecutionNode choiceNode in situationNode.Choices)
            {
                if (choiceModelMap.TryGetValue(choiceNode, out ChoiceNodeModel choiceModel))
                {
                    diagram.Links.Add(new SpawnGraphLinkModel(situationModel, choiceModel, SpawnGraphLinkType.Hierarchy));
                }
            }

            if (situationNode.Location != null)
            {
                string entityKey = "loc_" + situationNode.Location.Name;
                if (entityModelMap.TryGetValue(entityKey, out EntityNodeModel entityModel))
                {
                    diagram.Links.Add(new SpawnGraphLinkModel(situationModel, entityModel, SpawnGraphLinkType.EntityLocation));
                }
            }

            if (situationNode.NPC != null)
            {
                string entityKey = "npc_" + situationNode.NPC.Name;
                if (entityModelMap.TryGetValue(entityKey, out EntityNodeModel entityModel))
                {
                    diagram.Links.Add(new SpawnGraphLinkModel(situationModel, entityModel, SpawnGraphLinkType.EntityNpc));
                }
            }

            if (situationNode.Route != null)
            {
                string entityKey = "route_" + situationNode.Route.OriginLocationName + "_" + situationNode.Route.DestinationLocationName;
                if (entityModelMap.TryGetValue(entityKey, out EntityNodeModel entityModel))
                {
                    diagram.Links.Add(new SpawnGraphLinkModel(situationModel, entityModel, SpawnGraphLinkType.EntityRoute));
                }
            }
        }

        foreach (ChoiceExecutionNode choiceNode in tracer.AllChoiceNodes)
        {
            if (!choiceModelMap.TryGetValue(choiceNode, out ChoiceNodeModel choiceModel)) continue;

            foreach (SceneSpawnNode spawnedScene in choiceNode.SpawnedScenes)
            {
                if (sceneModelMap.TryGetValue(spawnedScene, out SceneNodeModel sceneModel))
                {
                    diagram.Links.Add(new SpawnGraphLinkModel(choiceModel, sceneModel, SpawnGraphLinkType.SpawnScene));
                }
            }

            foreach (SituationSpawnNode spawnedSituation in choiceNode.SpawnedSituations)
            {
                if (situationModelMap.TryGetValue(spawnedSituation, out SituationNodeModel situationModel))
                {
                    diagram.Links.Add(new SpawnGraphLinkModel(choiceModel, situationModel, SpawnGraphLinkType.SpawnSituation));
                }
            }
        }
    }

    private Point GetPositionFromLayout(Dictionary<string, DagreLayoutNode> positionMap, string nodeId, double fallbackX, double fallbackY)
    {
        if (positionMap.TryGetValue(nodeId, out DagreLayoutNode layoutNode))
        {
            return new Point(layoutNode.X, layoutNode.Y);
        }
        return new Point(fallbackX, fallbackY);
    }

    private SpawnGraphBuildResult BuildWithFallbackLayout(ProceduralContentTracer tracer, BlazorDiagram diagram)
    {
        double xOffset = 50;
        double yOffset = 100;
        double xSpacing = 250;
        double ySpacing = 150;

        Dictionary<SceneSpawnNode, SceneNodeModel> sceneModelMap = new Dictionary<SceneSpawnNode, SceneNodeModel>();
        Dictionary<SituationSpawnNode, SituationNodeModel> situationModelMap = new Dictionary<SituationSpawnNode, SituationNodeModel>();
        Dictionary<ChoiceExecutionNode, ChoiceNodeModel> choiceModelMap = new Dictionary<ChoiceExecutionNode, ChoiceNodeModel>();
        Dictionary<string, EntityNodeModel> entityModelMap = new Dictionary<string, EntityNodeModel>();

        // Create scene nodes (column 0)
        int sceneRow = 0;
        foreach (SceneSpawnNode sceneSpawn in tracer.AllSceneNodes)
        {
            SceneNodeModel model = new SceneNodeModel(
                new Point(xOffset, yOffset + (sceneRow * ySpacing)),
                sceneSpawn);
            diagram.Nodes.Add(model);
            sceneModelMap[sceneSpawn] = model;
            sceneRow++;
        }

        // Create situation nodes (column 1)
        int situationRow = 0;
        foreach (SituationSpawnNode situationSpawn in tracer.AllSituationNodes)
        {
            SituationNodeModel model = new SituationNodeModel(
                new Point(xOffset + xSpacing, yOffset + (situationRow * ySpacing)),
                situationSpawn);
            diagram.Nodes.Add(model);
            situationModelMap[situationSpawn] = model;
            situationRow++;
        }

        // Create choice nodes (column 2)
        int choiceRow = 0;
        foreach (ChoiceExecutionNode choiceSpawn in tracer.AllChoiceNodes)
        {
            ChoiceNodeModel model = new ChoiceNodeModel(
                new Point(xOffset + (xSpacing * 2), yOffset + (choiceRow * ySpacing)),
                choiceSpawn);
            diagram.Nodes.Add(model);
            choiceModelMap[choiceSpawn] = model;
            choiceRow++;
        }

        // Create entity nodes (column 3)
        int entityRow = 0;
        foreach (SituationSpawnNode situationSpawn in tracer.AllSituationNodes)
        {
            if (situationSpawn.Location != null)
            {
                string entityKey = "loc_" + situationSpawn.Location.Name;
                if (!entityModelMap.ContainsKey(entityKey))
                {
                    EntityNodeModel model = new EntityNodeModel(
                        new Point(xOffset + (xSpacing * 3), yOffset + (entityRow * ySpacing)),
                        situationSpawn.Location);
                    diagram.Nodes.Add(model);
                    entityModelMap[entityKey] = model;
                    entityRow++;
                }
            }

            if (situationSpawn.NPC != null)
            {
                string entityKey = "npc_" + situationSpawn.NPC.Name;
                if (!entityModelMap.ContainsKey(entityKey))
                {
                    EntityNodeModel model = new EntityNodeModel(
                        new Point(xOffset + (xSpacing * 3), yOffset + (entityRow * ySpacing)),
                        situationSpawn.NPC);
                    diagram.Nodes.Add(model);
                    entityModelMap[entityKey] = model;
                    entityRow++;
                }
            }

            if (situationSpawn.Route != null)
            {
                string entityKey = "route_" + situationSpawn.Route.OriginLocationName + "_" + situationSpawn.Route.DestinationLocationName;
                if (!entityModelMap.ContainsKey(entityKey))
                {
                    EntityNodeModel model = new EntityNodeModel(
                        new Point(xOffset + (xSpacing * 3), yOffset + (entityRow * ySpacing)),
                        situationSpawn.Route);
                    diagram.Nodes.Add(model);
                    entityModelMap[entityKey] = model;
                    entityRow++;
                }
            }
        }

        // Create links (same as ApplyLayoutToDiagram)
        foreach (SceneSpawnNode sceneSpawn in tracer.AllSceneNodes)
        {
            if (!sceneModelMap.TryGetValue(sceneSpawn, out SceneNodeModel sceneModel)) continue;

            foreach (SituationSpawnNode situationSpawn in sceneSpawn.Situations)
            {
                if (situationModelMap.TryGetValue(situationSpawn, out SituationNodeModel situationModel))
                {
                    diagram.Links.Add(new SpawnGraphLinkModel(sceneModel, situationModel, SpawnGraphLinkType.Hierarchy));
                }
            }
        }

        foreach (SituationSpawnNode situationSpawn in tracer.AllSituationNodes)
        {
            if (!situationModelMap.TryGetValue(situationSpawn, out SituationNodeModel situationModel)) continue;

            foreach (ChoiceExecutionNode choiceSpawn in situationSpawn.Choices)
            {
                if (choiceModelMap.TryGetValue(choiceSpawn, out ChoiceNodeModel choiceModel))
                {
                    diagram.Links.Add(new SpawnGraphLinkModel(situationModel, choiceModel, SpawnGraphLinkType.Hierarchy));
                }
            }

            if (situationSpawn.Location != null)
            {
                string entityKey = "loc_" + situationSpawn.Location.Name;
                if (entityModelMap.TryGetValue(entityKey, out EntityNodeModel entityModel))
                {
                    diagram.Links.Add(new SpawnGraphLinkModel(situationModel, entityModel, SpawnGraphLinkType.EntityLocation));
                }
            }

            if (situationSpawn.NPC != null)
            {
                string entityKey = "npc_" + situationSpawn.NPC.Name;
                if (entityModelMap.TryGetValue(entityKey, out EntityNodeModel entityModel))
                {
                    diagram.Links.Add(new SpawnGraphLinkModel(situationModel, entityModel, SpawnGraphLinkType.EntityNpc));
                }
            }

            if (situationSpawn.Route != null)
            {
                string entityKey = "route_" + situationSpawn.Route.OriginLocationName + "_" + situationSpawn.Route.DestinationLocationName;
                if (entityModelMap.TryGetValue(entityKey, out EntityNodeModel entityModel))
                {
                    diagram.Links.Add(new SpawnGraphLinkModel(situationModel, entityModel, SpawnGraphLinkType.EntityRoute));
                }
            }
        }

        foreach (ChoiceExecutionNode choiceSpawn in tracer.AllChoiceNodes)
        {
            if (!choiceModelMap.TryGetValue(choiceSpawn, out ChoiceNodeModel choiceModel)) continue;

            foreach (SceneSpawnNode spawnedScene in choiceSpawn.SpawnedScenes)
            {
                if (sceneModelMap.TryGetValue(spawnedScene, out SceneNodeModel sceneModel))
                {
                    diagram.Links.Add(new SpawnGraphLinkModel(choiceModel, sceneModel, SpawnGraphLinkType.SpawnScene));
                }
            }

            foreach (SituationSpawnNode spawnedSituation in choiceSpawn.SpawnedSituations)
            {
                if (situationModelMap.TryGetValue(spawnedSituation, out SituationNodeModel situationModel))
                {
                    diagram.Links.Add(new SpawnGraphLinkModel(choiceModel, situationModel, SpawnGraphLinkType.SpawnSituation));
                }
            }
        }

        int maxRow = Math.Max(Math.Max(sceneRow, situationRow), Math.Max(choiceRow, entityRow));

        return new SpawnGraphBuildResult
        {
            Success = true,
            Message = "Used fallback layout (JS interop unavailable)",
            NodeCount = diagram.Nodes.Count,
            LinkCount = diagram.Links.Count,
            GraphWidth = xSpacing * 4 + 200,
            GraphHeight = maxRow * ySpacing + 200
        };
    }

    private string TruncateText(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text)) return "Choice";
        if (text.Length <= maxLength) return text;
        return text.Substring(0, maxLength - 3) + "...";
    }
}

public class SpawnGraphBuildResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public int NodeCount { get; set; }
    public int LinkCount { get; set; }
    public double GraphWidth { get; set; }
    public double GraphHeight { get; set; }
}

public class DagreGraphData
{
    [System.Text.Json.Serialization.JsonPropertyName("nodes")]
    public List<DagreNode> Nodes { get; set; } = new List<DagreNode>();

    [System.Text.Json.Serialization.JsonPropertyName("edges")]
    public List<DagreEdge> Edges { get; set; } = new List<DagreEdge>();
}

public class DagreNode
{
    [System.Text.Json.Serialization.JsonPropertyName("id")]
    public string Id { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("label")]
    public string Label { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("type")]
    public string Type { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("width")]
    public int Width { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("height")]
    public int Height { get; set; }
}

public class DagreEdge
{
    [System.Text.Json.Serialization.JsonPropertyName("source")]
    public string Source { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("target")]
    public string Target { get; set; }
}

public class DagreLayoutResult
{
    [System.Text.Json.Serialization.JsonPropertyName("nodes")]
    public List<DagreLayoutNode> Nodes { get; set; } = new List<DagreLayoutNode>();

    [System.Text.Json.Serialization.JsonPropertyName("edges")]
    public List<DagreLayoutEdge> Edges { get; set; } = new List<DagreLayoutEdge>();

    [System.Text.Json.Serialization.JsonPropertyName("graphWidth")]
    public double GraphWidth { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("graphHeight")]
    public double GraphHeight { get; set; }
}

public class DagreLayoutNode
{
    [System.Text.Json.Serialization.JsonPropertyName("id")]
    public string Id { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("x")]
    public double X { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("y")]
    public double Y { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("width")]
    public double Width { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("height")]
    public double Height { get; set; }
}

public class DagreLayoutEdge
{
    [System.Text.Json.Serialization.JsonPropertyName("source")]
    public string Source { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("target")]
    public string Target { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("points")]
    public List<DagrePoint> Points { get; set; } = new List<DagrePoint>();
}

public class DagrePoint
{
    [System.Text.Json.Serialization.JsonPropertyName("x")]
    public double X { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("y")]
    public double Y { get; set; }
}
