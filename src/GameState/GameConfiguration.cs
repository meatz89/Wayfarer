using System.Collections.Generic;

/// <summary>
/// Centralized game configuration containing all game constants and rules.
/// Loaded from game-config.json at startup.
/// </summary>
public class GameConfiguration
{
    public LetterQueueConfig LetterQueue { get; set; } = new();
    public TokenEconomyConfig TokenEconomy { get; set; } = new();
    public WorkRewardConfig WorkRewards { get; set; } = new();
    public TimeConfig Time { get; set; } = new();
    public StaminaConfig Stamina { get; set; } = new();
    public QueueManagementConfig QueueManagement { get; set; } = new();
    public LetterPaymentConfig LetterPayment { get; set; } = new();
    public RelationshipConfig Relationships { get; set; } = new();
    public TravelConfig Travel { get; set; } = new();
    public PatronConfig Patron { get; set; } = new();
    public DebtConfig Debt { get; set; } = new();

    /// <summary>
    /// Create default configuration with all standard game rules
    /// </summary>
    public static GameConfiguration CreateDefault()
    {
        return new GameConfiguration
        {
            LetterQueue = new LetterQueueConfig
            {
                MaxQueueSize = 8,
                BasePositions = new Dictionary<ConnectionType, int>
                {
                    { ConnectionType.Noble, 3 },
                    { ConnectionType.Trade, 5 },
                    { ConnectionType.Shadow, 5 },
                    { ConnectionType.Common, 7 },
                    { ConnectionType.Trust, 7 }
                },
                LeverageMultiplier = 1,
                SkipCostPerPosition = 1,
                PurgeCostTokens = 3,
                ExtendDeadlineDays = 2,
                ExtendCostTokens = 2,
                PriorityCostTokens = 5,
                DeadlinePenaltyTokens = 2,
                PatronLetterMinPosition = 1,
                PatronLetterMaxPosition = 3
            },
            TokenEconomy = new TokenEconomyConfig
            {
                BaseTokenChance = 0.25f,
                TokenChancePerExisting = 0.05f,
                StrangerThreshold = 0,
                BasicLetterThreshold = 1,
                QualityLetterThreshold = 3,
                PremiumLetterThreshold = 5,
                MaxTokensPerNPC = 10,
                TokenLossOnExpiry = 2,
                TokenGainOnDelivery = 1
            },
            WorkRewards = new WorkRewardConfig
            {
                DefaultReward = new WorkReward { BaseCoins = 3, BonusCoins = 0, Stamina = -2 },
                ByProfession = new Dictionary<Professions, WorkReward>
                {
                    { Professions.Merchant, new WorkReward { BaseCoins = 4, BonusCoins = 0, Stamina = -2 } },
                    { Professions.TavernKeeper, new WorkReward { BaseCoins = 4, BonusCoins = 0, Stamina = -2 } },
                    { Professions.Scribe, new WorkReward { BaseCoins = 3, BonusCoins = 0, Stamina = -1 } },
                    { Professions.Innkeeper, new WorkReward { BaseCoins = 4, BonusCoins = 0, Stamina = -2 } }
                },
                SpecialRewards = new Dictionary<string, WorkReward>
                {
                    { "baker_dawn", new WorkReward { BaseCoins = 2, BonusCoins = 0, Stamina = -1, Bonus = "bread" } }
                }
            },
            Time = new TimeConfig
            {
                HoursPerDay = 24,
                ActiveDayStartHour = 6,
                ActiveDayEndHour = 22,
                HourCostStandardAction = 1,
                HourCostTravel = 2,
                HourCostDeepAction = 2,
                TimeBlocks = new Dictionary<string, TimeBlockDefinition>
                {
                    { "Dawn", new TimeBlockDefinition { StartHour = 6, EndHour = 8 } },
                    { "Morning", new TimeBlockDefinition { StartHour = 8, EndHour = 12 } },
                    { "Afternoon", new TimeBlockDefinition { StartHour = 12, EndHour = 16 } },
                    { "Evening", new TimeBlockDefinition { StartHour = 16, EndHour = 20 } },
                    { "Night", new TimeBlockDefinition { StartHour = 20, EndHour = 6 } }
                }
            },
            Stamina = new StaminaConfig
            {
                MaxStamina = 10,
                StartingStamina = 7,
                CostTravel = 2,
                CostWork = 2,
                CostDeliver = 1,
                RecoveryRest = 3,
                RecoverySleep = 6,
                RecoveryByLodging = new Dictionary<string, int>
                {
                    { "rough", 2 },
                    { "common", 4 },
                    { "private", 6 },
                    { "noble", 8 }
                }
            },
            QueueManagement = new QueueManagementConfig
            {
                MorningSwapAvailable = true,
                MorningSwapTimeBlock = "Dawn",
                LettersPerDayMin = 1,
                LettersPerDayMax = 2,
                ChainLetterDeadlineBonus = 1,
                ChainLetterPaymentMultiplier = 1.2f
            },
            LetterPayment = new LetterPaymentConfig
            {
                BasicMin = 3,
                BasicMax = 5,
                QualityMin = 8,
                QualityMax = 12,
                PremiumMin = 15,
                PremiumMax = 20,
                ReturnLetterMin = 8,
                ReturnLetterMax = 16,
                UrgentMultiplier = 1.5f,
                LateDeliveryPenaltyPerDay = 1
            },
            Relationships = new RelationshipConfig
            {
                SkipThresholdForLeverage = 2,
                ExpiredThresholdForRefusal = 3,
                RouteDiscoveryCostTokens = 3,
                FavorMinimumRelationship = 3
            },
            Travel = new TravelConfig
            {
                BaseStaminaCost = 2,
                EncounterChance = 0.3f,
                TerrainStaminaModifiers = new Dictionary<string, float>
                {
                    { "Road", 1.0f },
                    { "Trail", 1.2f },
                    { "Wilderness", 1.5f },
                    { "Mountain", 2.0f }
                }
            },
            Patron = new PatronConfig
            {
                MinDaysBetweenLetters = 5,
                BaseChancePercent = 10,
                ChanceIncreasePerDay = 15,
                MaxChancePercent = 90,
                FundRequestAmount = 30,
                FundRequestCost = 1,
                EquipmentRequestCost = 2
            },
            Debt = new DebtConfig
            {
                BorrowMoneyAmount = 20,
                BorrowMoneyCost = 2,
                IllegalWorkAmount = 30,
                IllegalWorkCost = 1,
                DebtThresholdForEmergencyActions = 10
            }
        };
    }
}

// Configuration classes for different game systems
public class LetterQueueConfig
{
    public int MaxQueueSize { get; set; }
    public Dictionary<ConnectionType, int> BasePositions { get; set; } = new();
    public int LeverageMultiplier { get; set; }
    public int SkipCostPerPosition { get; set; }
    public int PurgeCostTokens { get; set; }
    public int ExtendDeadlineDays { get; set; }
    public int ExtendCostTokens { get; set; }
    public int PriorityCostTokens { get; set; }
    public int DeadlinePenaltyTokens { get; set; }
    public int PatronLetterMinPosition { get; set; }
    public int PatronLetterMaxPosition { get; set; }
}

public class TokenEconomyConfig
{
    public float BaseTokenChance { get; set; }
    public float TokenChancePerExisting { get; set; }
    public int StrangerThreshold { get; set; }
    public int BasicLetterThreshold { get; set; }
    public int QualityLetterThreshold { get; set; }
    public int PremiumLetterThreshold { get; set; }
    public int MaxTokensPerNPC { get; set; }
    public int TokenLossOnExpiry { get; set; }
    public int TokenGainOnDelivery { get; set; }
}

public class WorkRewardConfig
{
    public WorkReward DefaultReward { get; set; } = new();
    public Dictionary<Professions, WorkReward> ByProfession { get; set; } = new();
    public Dictionary<string, WorkReward> SpecialRewards { get; set; } = new();
}

public class WorkReward
{
    public int BaseCoins { get; set; }
    public int BonusCoins { get; set; }
    public int Stamina { get; set; }
    public string Bonus { get; set; }

    // For backward compatibility or simplified usage
    public int Coins
    {
        get
        {
            return BaseCoins + BonusCoins;
        }

        set
        {
            BaseCoins = value;
        }
    }
}

public class TimeConfig
{
    public int HoursPerDay { get; set; }
    public int ActiveDayStartHour { get; set; }
    public int ActiveDayEndHour { get; set; }
    public int HourCostStandardAction { get; set; }
    public int HourCostTravel { get; set; }
    public int HourCostDeepAction { get; set; }
    public Dictionary<string, TimeBlockDefinition> TimeBlocks { get; set; } = new();
}

public class TimeBlockDefinition
{
    public int StartHour { get; set; }
    public int EndHour { get; set; }
}

public class StaminaConfig
{
    public int MaxStamina { get; set; }
    public int StartingStamina { get; set; }
    public int CostTravel { get; set; }
    public int CostWork { get; set; }
    public int CostDeliver { get; set; }
    public int RecoveryRest { get; set; }
    public int RecoverySleep { get; set; }
    public Dictionary<string, int> RecoveryByLodging { get; set; } = new();
}

public class QueueManagementConfig
{
    public bool MorningSwapAvailable { get; set; }
    public string MorningSwapTimeBlock { get; set; }
    public int LettersPerDayMin { get; set; }
    public int LettersPerDayMax { get; set; }
    public int ChainLetterDeadlineBonus { get; set; }
    public float ChainLetterPaymentMultiplier { get; set; }
}

public class LetterPaymentConfig
{
    public int BasicMin { get; set; }
    public int BasicMax { get; set; }
    public int QualityMin { get; set; }
    public int QualityMax { get; set; }
    public int PremiumMin { get; set; }
    public int PremiumMax { get; set; }
    public int ReturnLetterMin { get; set; }
    public int ReturnLetterMax { get; set; }
    public float UrgentMultiplier { get; set; }
    public int LateDeliveryPenaltyPerDay { get; set; }
}

public class RelationshipConfig
{
    public int SkipThresholdForLeverage { get; set; }
    public int ExpiredThresholdForRefusal { get; set; }
    public int RouteDiscoveryCostTokens { get; set; }
    public int FavorMinimumRelationship { get; set; }
}

public class TravelConfig
{
    public int BaseStaminaCost { get; set; }
    public float EncounterChance { get; set; }
    public Dictionary<string, float> TerrainStaminaModifiers { get; set; } = new();
}

public class PatronConfig
{
    public int MinDaysBetweenLetters { get; set; }
    public int BaseChancePercent { get; set; }
    public int ChanceIncreasePerDay { get; set; }
    public int MaxChancePercent { get; set; }
    public int FundRequestAmount { get; set; }
    public int FundRequestCost { get; set; }
    public int EquipmentRequestCost { get; set; }
    public int PatronLetterMinPosition { get; set; } = 1;
    public int PatronLetterMaxPosition { get; set; } = 3;
}

public class DebtConfig
{
    public int BorrowMoneyAmount { get; set; }
    public int BorrowMoneyCost { get; set; }
    public int IllegalWorkAmount { get; set; }
    public int IllegalWorkCost { get; set; }
    public int DebtThresholdForEmergencyActions { get; set; }
}