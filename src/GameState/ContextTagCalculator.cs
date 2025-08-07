using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Calculates context tags based on current GameWorld state.
/// This is the bridge between game mechanics and narrative presentation.
/// Updates SceneContext with appropriate tags for literary UI.
/// </summary>
public class ContextTagCalculator
{
    private readonly GameWorld _gameWorld;
    private readonly ConnectionTokenManager _tokenManager;
    private readonly LetterQueueManager _letterQueueManager;
    private readonly StandingObligationManager _obligationManager;
    private readonly TimeManager _timeManager;

    public ContextTagCalculator(
        GameWorld gameWorld,
        ConnectionTokenManager tokenManager,
        LetterQueueManager letterQueueManager,
        StandingObligationManager obligationManager,
        TimeManager timeManager)
    {
        _gameWorld = gameWorld;
        _tokenManager = tokenManager;
        _letterQueueManager = letterQueueManager;
        _obligationManager = obligationManager;
        _timeManager = timeManager;
    }

    /// <summary>
    /// Populate a SceneContext with calculated tags based on current game state
    /// </summary>
    public void PopulateContextTags(SceneContext context)
    {
        if (context == null) return;

        // Calculate pressure tags
        context.PressureTags = CalculatePressureTags();

        // Calculate relationship tags if NPC is present
        if (context.TargetNPC != null)
        {
            context.RelationshipTags = CalculateRelationshipTags(context.TargetNPC.ID);
            context.DiscoveryTags = CalculateDiscoveryTags(context.TargetNPC.ID);
        }

        // Calculate resource tags
        context.ResourceTags = CalculateResourceTags();

        // Calculate feeling tags for location
        context.FeelingTags = CalculateFeelingTags(context.LocationName, context.LocationSpotName);

        // Set specific values
        context.MinutesUntilDeadline = CalculateMinutesUntilDeadline();
        context.LetterQueueSize = _letterQueueManager?.GetPlayerQueue()?.Count(l => l != null) ?? 0;

        // Initialize attention manager if not present
        if (context.AttentionManager == null)
        {
            context.AttentionManager = new AttentionManager();
            context.AttentionManager.ResetForNewScene();
        }
    }

    private List<PressureTag> CalculatePressureTags()
    {
        List<PressureTag> tags = new List<PressureTag>();

        // Check deadline pressure
        int minutesUntilDeadline = CalculateMinutesUntilDeadline();
        if (minutesUntilDeadline < 180) // Less than 3 hours
            tags.Add(PressureTag.DEADLINE_IMMINENT);

        // Check queue pressure
        var queueSize = _letterQueueManager?.GetPlayerQueue()?.Count(l => l != null) ?? 0;
        if (queueSize >= 6)
            tags.Add(PressureTag.QUEUE_OVERFLOW);

        // Check debt pressure
        if (_tokenManager != null)
        {
            bool hasDebt = false;
            bool hasCriticalDebt = false;

            foreach (ConnectionType tokenType in Enum.GetValues(typeof(ConnectionType)))
            {
                var totalTokens = _tokenManager.GetTokenCount(tokenType);
                if (totalTokens < 0)
                {
                    hasDebt = true;
                    if (totalTokens <= -3)
                        hasCriticalDebt = true;
                }
            }

            if (hasCriticalDebt)
                tags.Add(PressureTag.DEBT_CRITICAL);
            else if (hasDebt)
                tags.Add(PressureTag.DEBT_PRESENT);
        }

        // Check obligations
        if (_obligationManager?.GetActiveObligations()?.Any() == true)
            tags.Add(PressureTag.OBLIGATION_ACTIVE);

        return tags;
    }

    private List<RelationshipTag> CalculateRelationshipTags(string npcId)
    {
        List<RelationshipTag> tags = new List<RelationshipTag>();

        if (_tokenManager == null || string.IsNullOrEmpty(npcId))
            return tags;

        // Check each token type
        var npcTokens = _tokenManager.GetTokensWithNPC(npcId);
        int trustTokens = npcTokens.GetValueOrDefault(ConnectionType.Trust);
        int commerceTokens = npcTokens.GetValueOrDefault(ConnectionType.Commerce);
        int statusTokens = npcTokens.GetValueOrDefault(ConnectionType.Status);
        int shadowTokens = npcTokens.GetValueOrDefault(ConnectionType.Shadow);

        // Trust tags
        if (trustTokens >= 4)
            tags.Add(RelationshipTag.TRUST_HIGH);
        else if (trustTokens < 0)
            tags.Add(RelationshipTag.TRUST_NEGATIVE);

        // Commerce tags
        if (commerceTokens >= 2)
            tags.Add(RelationshipTag.COMMERCE_ESTABLISHED);
        else if (commerceTokens < 0)
            tags.Add(RelationshipTag.COMMERCE_INDEBTED);

        // Status tags
        if (statusTokens >= 3)
            tags.Add(RelationshipTag.STATUS_RECOGNIZED);
        else if (statusTokens < 0)
            tags.Add(RelationshipTag.STATUS_SCORNED);

        // Shadow tags
        if (shadowTokens > 0)
            tags.Add(RelationshipTag.SHADOW_COMPLICIT);
        else if (shadowTokens < 0)
            tags.Add(RelationshipTag.SHADOW_THREATENED);

        // Check if first meeting
        int totalTokens = Math.Abs(trustTokens) + Math.Abs(commerceTokens) +
                         Math.Abs(statusTokens) + Math.Abs(shadowTokens);
        if (totalTokens == 0)
            tags.Add(RelationshipTag.STRANGER);

        return tags;
    }

    private List<DiscoveryTag> CalculateDiscoveryTags(string npcId)
    {
        List<DiscoveryTag> tags = new List<DiscoveryTag>();

        // Basic discovery opportunities
        // These would be expanded based on actual game state
        tags.Add(DiscoveryTag.RUMOR_AVAILABLE);

        // Check if NPC has undiscovered information - strongly typed
        if (_gameWorld.PendingQueueState.NPCsWithSecrets.Contains(npcId))
            tags.Add(DiscoveryTag.SECRET_PRESENT);

        return tags;
    }

    private List<ResourceTag> CalculateResourceTags()
    {
        List<ResourceTag> tags = new List<ResourceTag>();
        Player player = _gameWorld.GetPlayer();

        if (player == null)
            return tags;

        // Coin tags
        int coins = _gameWorld.PlayerCoins;
        if (coins >= 20)
            tags.Add(ResourceTag.COINS_ABUNDANT);
        else if (coins >= 5)
            tags.Add(ResourceTag.COINS_SUFFICIENT);
        else if (coins >= 1)
            tags.Add(ResourceTag.COINS_LOW);
        else
            tags.Add(ResourceTag.COINS_NONE);

        // Stamina tags
        int stamina = _gameWorld.PlayerStamina;
        int maxStamina = 10; // Default max stamina
        int staminaPercent = (stamina * 100) / maxStamina;

        if (staminaPercent >= 100)
            tags.Add(ResourceTag.STAMINA_FULL);
        else if (staminaPercent >= 70)
            tags.Add(ResourceTag.STAMINA_RESTED);
        else if (staminaPercent >= 30)
            tags.Add(ResourceTag.STAMINA_TIRED);
        else
            tags.Add(ResourceTag.STAMINA_EXHAUSTED);

        // Inventory tags
        var inventoryItems = _gameWorld.PlayerInventory?.GetAllItems();
        if (inventoryItems == null || !inventoryItems.Any(i => !string.IsNullOrEmpty(i)))
            tags.Add(ResourceTag.INVENTORY_EMPTY);
        else if (inventoryItems.Count(i => !string.IsNullOrEmpty(i)) >= 10) // Assuming 10 is max
            tags.Add(ResourceTag.INVENTORY_FULL);

        return tags;
    }

    private int CalculateMinutesUntilDeadline()
    {
        // Get the most urgent letter deadline
        var urgentLetter = _letterQueueManager?.GetPlayerQueue()?
            .Where(l => l != null)?
            .Where(l => l.DeadlineInHours > 0)
            .OrderBy(l => l.DeadlineInHours)
            .FirstOrDefault();

        if (urgentLetter == null)
            return 9999; // No deadline

        // Calculate minutes until deadline
        // DeadlineInHours is the number of hours remaining
        var totalHours = urgentLetter.DeadlineInHours;
        
        return Math.Max(0, totalHours * 60);
    }

    private List<FeelingTag> CalculateFeelingTags(string locationName, string spotName)
    {
        List<FeelingTag> tags = new List<FeelingTag>();

        // Add weather-based feelings
        WeatherCondition weather = _gameWorld.CurrentWeather;
        switch (weather)
        {
            case WeatherCondition.Clear:
                tags.Add(FeelingTag.SUN_DRENCHED);
                break;
            case WeatherCondition.Rain:
                tags.Add(FeelingTag.RAIN_SOAKED);
                break;
            case WeatherCondition.Snow:
                tags.Add(FeelingTag.FROST_TOUCHED);
                break;
        }

        // Add location-based feelings
        if (!string.IsNullOrEmpty(spotName))
        {
            if (spotName.ToLower().Contains("tavern") || spotName.ToLower().Contains("inn"))
            {
                tags.Add(FeelingTag.HEARTH_WARMED);
                tags.Add(FeelingTag.ALE_SCENTED);
                tags.Add(FeelingTag.MUSIC_DRIFTING);
            }
            else if (spotName.ToLower().Contains("market") || spotName.ToLower().Contains("square"))
            {
                tags.Add(FeelingTag.BUSTLING);
            }
            else if (spotName.ToLower().Contains("alley") || spotName.ToLower().Contains("shadow"))
            {
                tags.Add(FeelingTag.DANGER_LURKS);
                tags.Add(FeelingTag.SILENCE_HEAVY);
            }
        }

        // Add pressure-based feelings
        if (CalculateMinutesUntilDeadline() < 180)
            tags.Add(FeelingTag.URGENCY_GNAWS);

        // Add time-based feelings
        switch (_gameWorld.CurrentTimeBlock)
        {
            case TimeBlocks.Night:
                tags.Add(FeelingTag.SILENCE_HEAVY);
                tags.Add(FeelingTag.MYSTERY_WHISPERS);
                break;
            case TimeBlocks.Evening:
                tags.Add(FeelingTag.COMFORT_EMBRACES);
                break;
        }

        return tags;
    }
}