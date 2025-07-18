using Microsoft.AspNetCore.Components;

namespace Wayfarer.Pages
{

public class PlayerStatusViewBase : ComponentBase
{
    [Inject] public GameWorld GameWorld { get; set; }
    [Inject] public GameWorldManager GameManager { get; set; }
    [Inject] public ItemRepository ItemRepository { get; set; }
    [Inject] public LocationRepository LocationRepository { get; set; }
    [Inject] public RouteRepository RouteRepository { get; set; }
    [Inject] public NavigationManager NavigationManager { get; set; }
    [Inject] public LetterCarryingManager LetterCarryingManager { get; set; }

    [Parameter] public EventCallback OnClose { get; set; }

    public Player PlayerState => GameWorld.GetPlayer();
    public Location CurrentLocation => LocationRepository.GetCurrentLocation();
    public int CurrentStamina => PlayerState.Stamina;
    public int CurrentConcentration => PlayerState.Concentration;

    protected override void OnInitialized()
    {
        base.OnInitialized();
    }

    public async Task ClosePlayerStatus()
    {
        if (OnClose.HasDelegate)
        {
            await OnClose.InvokeAsync();
        }
        else
        {
            NavigationManager.NavigateTo("/");
        }
    }

    public string GetArchetypePortrait()
    {
        string gender = PlayerState.Gender.ToString().ToLower();
        string archetype = PlayerState.Archetype.ToString().ToLower();
        return $"/images/characters/{gender}_{archetype}.png";
    }

    public string GetArchetypeIcon(Professions archetype)
    {
        return archetype switch
        {
            Professions.Soldier => "⚔️",
            Professions.Merchant => "💰",
            Professions.Thief => "🗡️",
            Professions.Scholar => "📚",
            _ => "👤"
        };
    }

    public string GetHealthIcon(PhysicalCondition condition)
    {
        return condition switch
        {
            PhysicalCondition.Excellent => "💚",
            PhysicalCondition.Good => "💚",
            PhysicalCondition.Tired => "💛",
            PhysicalCondition.Exhausted => "🧡",
            PhysicalCondition.Injured => "❤️",
            PhysicalCondition.Sick => "🤒",
            PhysicalCondition.Recovered => "💪",
            _ => "❓"
        };
    }

    public string GetHealthStatusClass(PhysicalCondition condition)
    {
        return condition switch
        {
            PhysicalCondition.Excellent => "health-excellent",
            PhysicalCondition.Good => "health-good",
            PhysicalCondition.Tired => "health-tired",
            PhysicalCondition.Exhausted => "health-exhausted",
            PhysicalCondition.Injured => "health-injured",
            PhysicalCondition.Sick => "health-sick",
            PhysicalCondition.Recovered => "health-recovered",
            _ => ""
        };
    }

    public string GetPhysicalConditionDescription(PhysicalCondition condition)
    {
        return condition switch
        {
            PhysicalCondition.Excellent => "You feel at peak performance",
            PhysicalCondition.Good => "You feel healthy and strong",
            PhysicalCondition.Tired => "You feel somewhat tired but capable",
            PhysicalCondition.Exhausted => "You are completely exhausted",
            PhysicalCondition.Injured => "You are physically impaired",
            PhysicalCondition.Sick => "You feel ill and weak",
            PhysicalCondition.Recovered => "You are recovering from recent exertion",
            _ => "Unknown condition"
        };
    }

    public string GetPhysicalConditionEffects(PhysicalCondition condition)
    {
        return condition switch
        {
            PhysicalCondition.Tired => "Slightly reduced effectiveness",
            PhysicalCondition.Exhausted => "Cannot perform strenuous activities",
            PhysicalCondition.Injured => "Severe penalties to physical actions",
            PhysicalCondition.Sick => "Reduced stamina recovery, slower movement",
            _ => "No effects"
        };
    }

    public int GetStaminaCost()
    {
        // This would check for pending actions that consume stamina
        return 0;
    }

    public string GetWeightStatus(int totalWeight)
    {
        if (totalWeight <= 3) return "Light load";
        if (totalWeight <= 6) return "Medium load (+1 stamina)";
        return "Heavy load (+2 stamina)";
    }

    public string GetWeightStatusClass(int totalWeight)
    {
        if (totalWeight <= 3) return "weight-light";
        if (totalWeight <= 6) return "weight-medium";
        return "weight-heavy";
    }

    public List<ItemCategory> GetCurrentEquipmentCategories()
    {
        var categories = new List<ItemCategory>();

        foreach (string itemName in PlayerState.Inventory.ItemSlots)
        {
            if (itemName != null)
            {
                Item item = ItemRepository.GetItemByName(itemName);
                if (item != null)
                {
                    categories.AddRange(item.Categories);
                }
            }
        }

        return categories.Distinct().ToList();
    }

    public (int accessible, int blocked, int total) GetAccessibleRoutes()
    {
        var allRoutes = RouteRepository.GetAllRoutes();
        var equipmentCategories = GetCurrentEquipmentCategories();
        int accessible = 0;
        int blocked = 0;

        foreach (var route in allRoutes)
        {
            bool canAccess = true;

            // Check terrain requirements
            foreach (var terrain in route.TerrainCategories)
            {
                if (terrain == TerrainCategory.Requires_Climbing && !equipmentCategories.Contains(ItemCategory.Climbing_Equipment))
                {
                    canAccess = false;
                    break;
                }
                if (terrain == TerrainCategory.Wilderness_Terrain && !equipmentCategories.Contains(ItemCategory.Navigation_Tools))
                {
                    canAccess = false;
                    break;
                }
            }

            if (canAccess)
                accessible++;
            else
                blocked++;
        }

        return (accessible, blocked, allRoutes.Count);
    }

    public string GetEquipmentIcon(ItemCategory category)
    {
        return category switch
        {
            ItemCategory.Climbing_Equipment => "🧗",
            ItemCategory.Navigation_Tools => "🧭",
            ItemCategory.Weather_Protection => "☂️",
            ItemCategory.Light_Source => "🔦",
            _ => "🛠️"
        };
    }

    public string GetEquipmentEffect(ItemCategory category)
    {
        return category switch
        {
            ItemCategory.Climbing_Equipment => "Enables mountain routes",
            ItemCategory.Navigation_Tools => "Enables wilderness routes",
            ItemCategory.Weather_Protection => "Travel in bad weather",
            ItemCategory.Light_Source => "Night travel enabled",
            _ => "Special capability"
        };
    }

    public List<Location> GetDiscoveredLocations()
    {
        return GameManager.GetPlayerKnownLocations();
    }

    public string GetLocationIcon(LocationTypes type)
    {
        return type switch
        {
            LocationTypes.Town => "🏘️",
            LocationTypes.City => "🏛️",
            LocationTypes.Village => "🏚️",
            LocationTypes.Outpost => "🏕️",
            LocationTypes.Wilderness => "🌲",
            LocationTypes.Castle => "🏰",
            LocationTypes.Temple => "⛪",
            LocationTypes.Ruin => "🏛️",
            LocationTypes.Cave => "🕳️",
            LocationTypes.Market => "🏪",
            LocationTypes.Port => "⚓",
            LocationTypes.Farm => "🌾",
            LocationTypes.Mine => "⛏️",
            LocationTypes.Forest => "🌳",
            LocationTypes.Mountain => "⛰️",
            LocationTypes.Swamp => "🌿",
            LocationTypes.Beach => "🏖️",
            LocationTypes.River => "🏞️",
            LocationTypes.Lake => "🏞️",
            LocationTypes.Road => "🛤️",
            LocationTypes.Bridge => "🌉",
            LocationTypes.Crossroads => "🚏",
            LocationTypes.Tavern => "🍺",
            LocationTypes.Inn => "🏨",
            LocationTypes.Shop => "🏪",
            LocationTypes.Guild => "🛡️",
            LocationTypes.Library => "📚",
            LocationTypes.Barracks => "⚔️",
            LocationTypes.Prison => "🔒",
            LocationTypes.Palace => "👑",
            LocationTypes.Tower => "🏰",
            LocationTypes.Crypt => "⚰️",
            LocationTypes.Graveyard => "🪦",
            LocationTypes.Other => "📍",
            _ => "📍"
        };
    }

    public string GetSkillIcon(SkillTypes skill)
    {
        return skill switch
        {
            SkillTypes.BruteForce => "💪",
            SkillTypes.Finesse => "🤸",
            SkillTypes.Endurance => "🏃",
            SkillTypes.Knowledge => "🧠",
            SkillTypes.Perception => "👁️",
            SkillTypes.Reasoning => "🔍",
            SkillTypes.Charm => "😊",
            SkillTypes.Intimidation => "😠",
            SkillTypes.Deception => "🎭",
            _ => "⭐"
        };
    }

    public LetterCarryingManager GetLetterCarryingManager()
    {
        return LetterCarryingManager;
    }
}

// Supporting classes for player strategic overview
public class PlayerStrategicOverview
{
    public List<string> EquipmentCapabilities { get; set; } = new();
    public int AccessibleRoutes { get; set; }
    public int BlockedRoutes { get; set; }
    public List<string> CriticalMissingEquipment { get; set; } = new();
    public int ReadyContracts { get; set; }
    public int PendingContracts { get; set; }
    public int UrgentContracts { get; set; }
}

public class TimeAwarenessAnalysis
{
    public string CurrentStatus { get; set; } = "";
    public string Recommendation { get; set; } = "";
}

} // namespace Wayfarer.Pages