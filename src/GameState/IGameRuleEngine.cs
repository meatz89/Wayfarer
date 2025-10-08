using System.Collections.Generic;

/// <summary>
/// Central interface for all game mechanics and rule calculations.
/// This separates game logic from UI and infrastructure concerns.
/// </summary>
public interface IGameRuleEngine
{
    // Travel mechanics
    int CalculateTravelStamina(RouteOption route);
    bool CanTravel(Player player, RouteOption route);

    // Time management
    TimeBlocks GetTimeBlock(int segment);
    int GetActiveSegmentsRemaining(int currentSegment);
    bool IsNPCAvailable(NPC npc, TimeBlocks timeBlock);

    // Stamina and recovery
    int CalculateRestRecovery(string lodgingType);
    int CalculateMaxStamina(Player player);

}
