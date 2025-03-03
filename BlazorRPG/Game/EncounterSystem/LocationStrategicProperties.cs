using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Defines which tags are available for a location and their activation conditions
/// </summary>
public class LocationStrategicProperties
{
    public string LocationId { get; }
    public string LocationName { get; }
    public List<SignatureElementTypes> FavoredElements { get; }
    public List<SignatureElementTypes> DisfavoredElements { get; }
    public List<string> AvailableTagIds { get; }

    public LocationStrategicProperties(string id, string name,
                                      List<SignatureElementTypes> favored,
                                      List<SignatureElementTypes> disfavored,
                                      List<string> availableTagIds)
    {
        LocationId = id;
        LocationName = name;
        FavoredElements = favored;
        DisfavoredElements = disfavored;
        AvailableTagIds = availableTagIds;
    }
}

