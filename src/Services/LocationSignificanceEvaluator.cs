/// <summary>
/// Evaluates location significance for lifecycle management.
/// Significance is emergent - determined by gameplay patterns, not authoring-time flags.
/// </summary>
public class LocationSignificanceEvaluator
{
    /// <summary>
    /// Evaluate location significance based on visits, references, and provenance.
    /// </summary>
    public LocationSignificance EvaluateSignificance(Location location, GameWorld gameWorld)
    {
        // Authored content always critical (never cleanup)
        if (location.Provenance == null)
        {
            return LocationSignificance.Critical;
        }

        // Player visited = persistent (spatial memory matters)
        // Once visited, location becomes part of player's mental map
        if (location.HasBeenVisited)
        {
            return LocationSignificance.Persistent;
        }

        // Count active scene references
        int activeSceneReferences = CountActiveSceneReferences(location.Id, gameWorld);

        // Multiple active scenes reference this location = persistent
        // Shared dependency indicates structural importance
        if (activeSceneReferences > 1)
        {
            return LocationSignificance.Persistent;
        }

        // Single active scene reference = keep until scene completes
        if (activeSceneReferences == 1)
        {
            return LocationSignificance.Temporary;
        }

        // No references, never visited = cleanup eligible
        return LocationSignificance.Temporary;
    }

    /// <summary>
    /// Count how many active scenes reference this location.
    /// Checks both scene placement and situation required locations.
    /// </summary>
    private int CountActiveSceneReferences(string locationId, GameWorld gameWorld)
    {
        int count = 0;

        foreach (Scene scene in gameWorld.Scenes.Where(s => s.State == SceneState.Active))
        {
            // Check if scene placed at this location
            if (scene.PlacementType == PlacementType.Location && scene.PlacementId == locationId)
            {
                count++;
                continue;
            }

            // Check if any situation requires this location
            foreach (Situation situation in scene.Situations)
            {
                // Resolve marker if needed
                string requiredLocationId = situation.RequiredLocationId;
                if (!string.IsNullOrEmpty(requiredLocationId))
                {
                    // Check marker resolution
                    if (requiredLocationId.StartsWith("generated:") && scene.MarkerResolutionMap != null)
                    {
                        scene.MarkerResolutionMap.TryGetValue(requiredLocationId, out requiredLocationId);
                    }

                    if (requiredLocationId == locationId)
                    {
                        count++;
                        break; // Found reference in this scene, move to next scene
                    }
                }
            }
        }

        return count;
    }
}
