using Wayfarer.GameState.Enums;

namespace Wayfarer.Content.Generators;

public class GenerationContext
{
    public int Tier { get; set; }
    public PersonalityType? NpcPersonality { get; set; }
    public string NpcLocationId { get; set; }
    public string NpcId { get; set; }
    public string NpcName { get; set; }
    public int PlayerCoins { get; set; }
    public List<LocationPropertyType> LocationProperties { get; set; } = new();

    public static GenerationContext Categorical(int tier)
    {
        return new GenerationContext
        {
            Tier = tier,
            NpcPersonality = null,
            NpcLocationId = null,
            NpcId = null,
            NpcName = "",
            PlayerCoins = 0,
            LocationProperties = new()
        };
    }

    public static GenerationContext FromEntities(
        int tier,
        NPC npc,
        Location location,
        Player player)
    {
        return new GenerationContext
        {
            Tier = tier,
            NpcPersonality = npc?.PersonalityType,
            NpcLocationId = npc?.Location?.Id,
            NpcId = npc?.ID,
            NpcName = npc?.Name ?? "",
            PlayerCoins = player?.Coins ?? 0,
            LocationProperties = location?.LocationProperties ?? new()
        };
    }
}
