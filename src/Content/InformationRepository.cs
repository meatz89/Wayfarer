using Wayfarer.Game.MainSystem;
using Wayfarer.Game.ActionSystem;

namespace Wayfarer.Content;

/// <summary>
/// Stateless repository for accessing information data from GameWorld
/// Follows the Repository-Mediated Access architectural pattern
/// </summary>
public class InformationRepository
{
    private readonly GameWorld _gameWorld;

    public InformationRepository(GameWorld gameWorld)
    {
        _gameWorld = gameWorld;
    }

    /// <summary>
    /// Get information by ID from WorldState
    /// </summary>
    public Information? GetInformationById(string id)
    {
        return _gameWorld.WorldState.Informations.FirstOrDefault(info => info.Id == id);
    }

    /// <summary>
    /// Get all information from WorldState
    /// </summary>
    public List<Information> GetAllInformation()
    {
        return _gameWorld.WorldState.Informations.ToList();
    }

    /// <summary>
    /// Get information by type category
    /// </summary>
    public List<Information> GetInformationByType(InformationType type)
    {
        return _gameWorld.WorldState.Informations
            .Where(info => info.Type == type)
            .ToList();
    }

    /// <summary>
    /// Get information by minimum quality level
    /// </summary>
    public List<Information> GetInformationByQuality(InformationQuality minimumQuality)
    {
        return _gameWorld.WorldState.Informations
            .Where(info => info.Quality >= minimumQuality)
            .ToList();
    }

    /// <summary>
    /// Get information by minimum freshness level
    /// </summary>
    public List<Information> GetInformationByFreshness(InformationFreshness minimumFreshness)
    {
        return _gameWorld.WorldState.Informations
            .Where(info => info.Freshness >= minimumFreshness)
            .ToList();
    }

    /// <summary>
    /// Get information about a specific location
    /// </summary>
    public List<Information> GetLocationInformation(string locationId)
    {
        return _gameWorld.WorldState.Informations
            .Where(info => info.IsAbout(locationId))
            .ToList();
    }

    /// <summary>
    /// Get information concerning a specific NPC
    /// </summary>
    public List<Information> GetNPCInformation(string npcId)
    {
        return _gameWorld.WorldState.Informations
            .Where(info => info.ConcernsNPC(npcId))
            .ToList();
    }

    /// <summary>
    /// Get information related to a specific item
    /// </summary>
    public List<Information> GetItemInformation(string itemId)
    {
        return _gameWorld.WorldState.Informations
            .Where(info => info.RelatesToItem(itemId))
            .ToList();
    }

    /// <summary>
    /// Find information matching categorical requirements
    /// </summary>
    public List<Information> FindInformationMatching(
        InformationType? requiredType = null,
        InformationQuality? minimumQuality = null,
        InformationFreshness? minimumFreshness = null)
    {
        return _gameWorld.WorldState.Informations
            .Where(info => info.MeetsRequirements(requiredType, minimumQuality, minimumFreshness))
            .ToList();
    }

    /// <summary>
    /// Get public information (common knowledge)
    /// </summary>
    public List<Information> GetPublicInformation()
    {
        return _gameWorld.WorldState.Informations
            .Where(info => info.IsPublic)
            .ToList();
    }

    /// <summary>
    /// Get valuable information (high trade value)
    /// </summary>
    public List<Information> GetValuableInformation(int minimumValue = 10)
    {
        return _gameWorld.WorldState.Informations
            .Where(info => info.CalculateCurrentValue() >= minimumValue)
            .OrderByDescending(info => info.CalculateCurrentValue())
            .ToList();
    }

    /// <summary>
    /// Add information to WorldState (single source of truth)
    /// </summary>
    public void AddInformation(Information information)
    {
        if (information == null) return;
        
        // Avoid duplicates
        if (!_gameWorld.WorldState.Informations.Any(info => info.Id == information.Id))
        {
            _gameWorld.WorldState.Informations.Add(information);
        }
    }

    /// <summary>
    /// Add multiple information pieces to WorldState
    /// </summary>
    public void AddInformationRange(List<Information> informationList)
    {
        if (informationList == null) return;
        
        foreach (var information in informationList)
        {
            AddInformation(information);
        }
    }

    /// <summary>
    /// Update information quality/freshness in WorldState
    /// </summary>
    public void UpdateInformation(string id, InformationQuality? newQuality = null, 
                                InformationFreshness? newFreshness = null, string? newSource = null)
    {
        var information = GetInformationById(id);
        if (information == null) return;

        if (newQuality.HasValue)
            information.Quality = newQuality.Value;
            
        if (newFreshness.HasValue)
            information.Freshness = newFreshness.Value;
            
        if (!string.IsNullOrEmpty(newSource))
            information.Source = newSource;
    }

    /// <summary>
    /// Remove information from WorldState
    /// </summary>
    public void RemoveInformation(string id)
    {
        var information = GetInformationById(id);
        if (information != null)
        {
            _gameWorld.WorldState.Informations.Remove(information);
        }
    }

    /// <summary>
    /// Update freshness for all information based on time passage
    /// </summary>
    public void UpdateAllInformationFreshness(int daysPassed)
    {
        foreach (var information in _gameWorld.WorldState.Informations)
        {
            information.UpdateFreshness(daysPassed);
        }
    }

    /// <summary>
    /// Clear all information (for testing/reset)
    /// </summary>
    public void ClearAllInformation()
    {
        _gameWorld.WorldState.Informations.Clear();
    }
}