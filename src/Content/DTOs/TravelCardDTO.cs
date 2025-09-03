using System.Collections.Generic;

/// <summary>
/// DTO for travel card data from JSON packages
/// </summary>
public class TravelCardDTO
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Category { get; set; }
    public int? Focus { get; set; }
    public List<string> Requirements { get; set; }
    public string Type { get; set; }
    public string DisplayName { get; set; }
    public int? BaseFlow { get; set; }
    public string Persistence { get; set; }
    public string ConnectionType { get; set; }
    public TravelCardMechanicsDTO Mechanics { get; set; }
    public TravelCardContextDTO Context { get; set; }
}

/// <summary>
/// DTO for travel card mechanics
/// </summary>
public class TravelCardMechanicsDTO
{
    public bool? RequiresPermit { get; set; }
    public bool? DelayTravel { get; set; }
    public bool? RequiresCaution { get; set; }
    public bool? RequiresPayment { get; set; }
    public bool? RequiresDetour { get; set; }
    public bool? ProvidesProtection { get; set; }
    public bool? TradingOpening { get; set; }
    public bool? ShelterRequired { get; set; }
}

/// <summary>
/// DTO for travel card context
/// </summary>
public class TravelCardContextDTO
{
    public string TerrainType { get; set; }
    public string Weather { get; set; }
    public string TimeOfDay { get; set; }
}