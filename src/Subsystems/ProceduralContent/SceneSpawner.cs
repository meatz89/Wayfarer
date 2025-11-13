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

        // Markers and tracking properties deleted in 5-system architecture
        // Dependent resources discovered via query: template.DependentLocations/DependentItems

        return new SpawnResult
        {
            Success = true,
            SceneId = sceneId,
            CreatedSituationIds = situationIds
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
