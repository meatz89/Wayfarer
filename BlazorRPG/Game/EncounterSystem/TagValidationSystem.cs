/// <summary>
/// Validates that all tags in the TagRegistry are properly initialized in the EncounterTagRepository
/// </summary>
public static class TagValidationSystem
{
    /// <summary>
    /// Validates that all tags defined in TagRegistry are initialized in the repository
    /// </summary>
    /// <param name="repository">The repository to validate</param>
    /// <returns>True if validation passes, false otherwise</returns>
    public static bool ValidateRepository(EncounterTagRepository repository)
    {
        List<string> missingTags = new List<string>();
        bool isValid = true;

        // Check all Dominance tags
        foreach (string tagId in TagRegistry.Dominance.AllTags)
        {
            if (repository.GetTag(tagId) == null)
            {
                missingTags.Add(tagId);
                isValid = false;
            }
        }

        // Check all Rapport tags
        foreach (string tagId in TagRegistry.Rapport.AllTags)
        {
            if (repository.GetTag(tagId) == null)
            {
                missingTags.Add(tagId);
                isValid = false;
            }
        }

        // Check all Analysis tags
        foreach (string tagId in TagRegistry.Analysis.AllTags)
        {
            if (repository.GetTag(tagId) == null)
            {
                missingTags.Add(tagId);
                isValid = false;
            }
        }

        // Check all Precision tags
        foreach (string tagId in TagRegistry.Precision.AllTags)
        {
            if (repository.GetTag(tagId) == null)
            {
                missingTags.Add(tagId);
                isValid = false;
            }
        }

        // Check all Concealment tags
        foreach (string tagId in TagRegistry.Concealment.AllTags)
        {
            if (repository.GetTag(tagId) == null)
            {
                missingTags.Add(tagId);
                isValid = false;
            }
        }

        // Check all location reaction tags
        foreach (string tagId in TagRegistry.LocationReaction.MerchantGuild)
        {
            if (repository.GetTag(tagId) == null)
            {
                missingTags.Add(tagId);
                isValid = false;
            }
        }

        foreach (string tagId in TagRegistry.LocationReaction.BanditCamp)
        {
            if (repository.GetTag(tagId) == null)
            {
                missingTags.Add(tagId);
                isValid = false;
            }
        }

        // Log the missing tags if any
        if (missingTags.Count > 0)
        {
            Console.WriteLine("WARNING: The following tags are defined in TagRegistry but not initialized in EncounterTagRepository:");
            foreach (string tagId in missingTags)
            {
                Console.WriteLine($"  - {tagId}");
            }
        }

        return isValid;
    }

    /// <summary>
    /// Checks for any tag IDs in the repository that aren't defined in the TagRegistry
    /// </summary>
    /// <param name="repository">The repository to check</param>
    /// <returns>True if no undefined tags exist, false otherwise</returns>
    public static bool CheckForUndefinedTags(EncounterTagRepository repository)
    {
        // Get all possible tag IDs from TagRegistry
        HashSet<string> definedTagIds = new HashSet<string>();
        definedTagIds.UnionWith(TagRegistry.Dominance.AllTags);
        definedTagIds.UnionWith(TagRegistry.Rapport.AllTags);
        definedTagIds.UnionWith(TagRegistry.Analysis.AllTags);
        definedTagIds.UnionWith(TagRegistry.Precision.AllTags);
        definedTagIds.UnionWith(TagRegistry.Concealment.AllTags);
        definedTagIds.UnionWith(TagRegistry.LocationReaction.MerchantGuild);
        definedTagIds.UnionWith(TagRegistry.LocationReaction.BanditCamp);

        // Get all tag IDs from the repository
        List<string> undefinedTags = new List<string>();
        foreach (EncounterTag tag in repository.GetAllTags())
        {
            if (!definedTagIds.Contains(tag.Id))
            {
                undefinedTags.Add(tag.Id);
            }
        }

        // Log the undefined tags if any
        if (undefinedTags.Count > 0)
        {
            Console.WriteLine("WARNING: The following tags are initialized in EncounterTagRepository but not defined in TagRegistry:");
            foreach (string tagId in undefinedTags)
            {
                Console.WriteLine($"  - {tagId}");
            }
            return false;
        }

        return true;
    }
}