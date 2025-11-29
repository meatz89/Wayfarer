/// <summary>
/// Centralized game configuration containing all game constants and rules.
/// Loaded from game-config.json at startup.
/// </summary>
public class GameConfiguration
{
    public TokenEconomyConfig TokenEconomy { get; set; } = new();
    public WorkRewardConfig WorkRewards { get; set; } = new();
    public TimeConfig Time { get; set; } = new();
    public StaminaConfig Stamina { get; set; } = new();
    public QueueManagementConfig QueueManagement { get; set; } = new();
    public LetterPaymentConfig LetterPayment { get; set; } = new();
    public RelationshipConfig Relationships { get; set; } = new();
    public TravelConfig Travel { get; set; } = new();
    public DebtConfig Debt { get; set; } = new();

}

public class TokenEconomyConfig
{
    public int BaseTokenChance { get; set; } // Percentage (100 = 100%)
    public int TokenChancePerExisting { get; set; } // Percentage (100 = 100%)
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
    public int SegmentCostStandardAction { get; set; }
    public int SegmentCostTravel { get; set; }
    public int SegmentCostDeepAction { get; set; }
    public int MaxDays { get; set; } = 30; // 30-day narrative arc
    public Dictionary<string, TimeBlockDefinition> TimeBlocks { get; set; } = new();
}

public class TimeBlockDefinition
{
    public int StartSegment { get; set; }
    public int SegmentCount { get; set; }
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
    public int UrgentBonus { get; set; } // DDR-007: Flat coin bonus for urgent letters
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
    public int EncounterChance { get; set; } // Percentage (100 = 100%)
    public Dictionary<string, int> TerrainStaminaAdjustments { get; set; } = new(); // DDR-007: Flat stamina adjustments per terrain
}

public class DebtConfig
{
    public int BorrowMoneyAmount { get; set; }
    public int BorrowMoneyCost { get; set; }
    public int IllegalWorkAmount { get; set; }
    public int IllegalWorkCost { get; set; }
    public int DebtThresholdForEmergencyActions { get; set; }
}