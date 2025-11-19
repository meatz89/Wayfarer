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
            .Select(st => $"situation_{st.Id}_{Guid.NewGuid():N}")
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

    /// <summary>
    /// Check if scene can be spawned
    /// HIGHLANDER: Accept objects, no string ID checks
    /// </summary>
    private static bool CanSpawn(SpawnRequest request, IGameStateQuery gameState, out string reason)
    {
        if (gameState.HasScene(request.Template.Id))
        {
            reason = $"Scene '{request.Template.Id}' already spawned";
            return false;
        }

        if (request.Location != null && !gameState.HasLocation(request.Location))
        {
            reason = $"Location '{request.Location.Name}' not found";
            return false;
        }

        if (request.Npc != null && !gameState.HasNPC(request.Npc))
        {
            reason = $"NPC '{request.Npc.Name}' not found";
            return false;
        }

        reason = null;
        return true;
    }
}
