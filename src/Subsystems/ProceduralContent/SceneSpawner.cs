public static class SceneSpawner
{
    public static SpawnResult CalculateSpawn(SpawnRequest request, IGameStateQuery gameState)
    {
        if (!CanSpawn(request, gameState, out string reason))
        {
            return SpawnResult.Failure(reason);
        }

        string sceneId = $"scene_{Guid.NewGuid():N}";

        List<string> situationIds = request.Template.SituationTemplates
            .Select(st => $"{sceneId}_{st.Id}")
            .ToList();

        List<string> locationIds = request.Template.DependentLocations
            ?.Select(spec => $"{sceneId}_{spec.TemplateId}")
            .ToList() ?? new List<string>();

        List<string> itemIds = request.Template.DependentItems
            ?.Select(spec => $"{sceneId}_{spec.TemplateId}")
            .ToList() ?? new List<string>();

        Dictionary<string, string> markerMap = new();

        if (request.Template.DependentLocations != null)
        {
            foreach (DependentLocationSpec spec in request.Template.DependentLocations)
            {
                string actualId = $"{sceneId}_{spec.TemplateId}";
                markerMap[$"generated:{spec.TemplateId}"] = actualId;
            }
        }

        if (request.Template.DependentItems != null)
        {
            foreach (DependentItemSpec spec in request.Template.DependentItems)
            {
                string actualId = $"{sceneId}_{spec.TemplateId}";
                markerMap[$"generated:{spec.TemplateId}"] = actualId;
            }
        }

        return new SpawnResult
        {
            Success = true,
            SceneId = sceneId,
            CreatedSituationIds = situationIds,
            CreatedLocationIds = locationIds,
            CreatedItemIds = itemIds,
            MarkerResolutionMap = markerMap
        };
    }

    private static bool CanSpawn(SpawnRequest request, IGameStateQuery gameState, out string reason)
    {
        if (gameState.HasScene(request.Template.Id))
        {
            reason = $"Scene '{request.Template.Id}' already spawned";
            return false;
        }

        if (!string.IsNullOrEmpty(request.LocationId) && !gameState.HasLocation(request.LocationId))
        {
            reason = $"Location '{request.LocationId}' not found";
            return false;
        }

        if (!string.IsNullOrEmpty(request.NpcId) && !gameState.HasNPC(request.NpcId))
        {
            reason = $"NPC '{request.NpcId}' not found";
            return false;
        }

        reason = null;
        return true;
    }
}
