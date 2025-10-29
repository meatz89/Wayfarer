/// <summary>
/// Tracks progress for a single stat (level and XP)
/// </summary>
public class StatProgress
{
    public int Level { get; set; } = 1;
    public int XP { get; set; } = 0;

    /// <summary>
    /// Get XP required to reach the next level (extended to level 8)
    /// </summary>
    public int GetXPToNextLevel()
    {
        return Level switch
        {
            1 => 10,
            2 => 25,
            3 => 50,
            4 => 100,
            5 => 175,
            6 => 275,
            7 => 400,
            _ => int.MaxValue // Level 8 is max
        };
    }

    /// <summary>
    /// Get progress percentage toward next level (0-100)
    /// </summary>
    public int GetProgressPercent()
    {
        if (Level >= 8) return 100; // Extended to level 8

        int toNext = GetXPToNextLevel();
        return (int)((float)XP / toNext * 100);
    }

    /// <summary>
    /// Create a deep copy of this stat progress
    /// </summary>
    public StatProgress Clone()
    {
        return new StatProgress
        {
            Level = this.Level,
            XP = this.XP
        };
    }
}

/// <summary>
/// Manages all five player stats and their progression
/// Replaces the old PlayerSkills system with stat-based progression
/// </summary>
public class PlayerStats
{
    private Dictionary<PlayerStatType, StatProgress> _stats = new();

    public PlayerStats()
    {
        // Initialize all stats at level 1 with 0 XP
        foreach (PlayerStatType stat in Enum.GetValues<PlayerStatType>())
        {
            _stats[stat] = new StatProgress();
        }
    }

    /// <summary>
    /// Get the current level for a stat (1-8)
    /// </summary>
    public int GetLevel(PlayerStatType stat)
    {
        return _stats[stat].Level;
    }

    /// <summary>
    /// Get depth bonus for stat specialization in conversation system refactor
    /// Levels 1-3: +0 bonus, Levels 4-6: +1 bonus, Levels 7-9: +2 bonus, Level 10: +3 bonus
    /// </summary>
    public int GetDepthBonus(PlayerStatType stat)
    {
        int level = GetLevel(stat);
        return level switch
        {
            >= 10 => 3,
            >= 7 => 2,
            >= 4 => 1,
            _ => 0
        };
    }

    /// <summary>
    /// Get the current XP for a stat
    /// </summary>
    public int GetXP(PlayerStatType stat)
    {
        return _stats[stat].XP;
    }

    /// <summary>
    /// Get XP required to reach next level
    /// </summary>
    public int GetXPToNextLevel(PlayerStatType stat)
    {
        return _stats[stat].GetXPToNextLevel();
    }

    /// <summary>
    /// Get progress percentage toward next level (0-100)
    /// </summary>
    public int GetProgressPercent(PlayerStatType stat)
    {
        return _stats[stat].GetProgressPercent();
    }

    /// <summary>
    /// Add XP to a stat and handle automatic level ups
    /// </summary>
    public void AddXP(PlayerStatType stat, int amount)
    {
        if (amount <= 0) return;

        StatProgress progress = _stats[stat];
        progress.XP += amount;

        // Check for level ups (max level 8)
        while (progress.Level < 8 && progress.XP >= progress.GetXPToNextLevel())
        {
            progress.XP -= progress.GetXPToNextLevel();
            progress.Level++;
        }

        // Cap XP at level 8
        if (progress.Level >= 8)
        {
            progress.XP = 0;
        }
    }

    /// <summary>
    /// Get success rate bonus for a stat based on level (updated for new system)
    /// Level 1: +0%, Level 2: +10%, Level 3: +20%, Level 4: +30%, Level 5: +40%, etc.
    /// </summary>
    public int GetSuccessBonus(PlayerStatType stat)
    {
        return (_stats[stat].Level - 1) * 10; // 10% per level as per implementation plan
    }

    /// <summary>
    /// Get success bonus as decimal (for calculations)
    /// </summary>
    public decimal GetSuccessBonusDecimal(PlayerStatType stat)
    {
        return GetLevel(stat) * 0.10m; // 10% per level
    }

    /// <summary>
    /// Check if stat has persistence bonus (level 3+)
    /// Cards bound to this stat gain Statement persistence
    /// </summary>
    public bool HasPersistenceBonus(PlayerStatType stat)
    {
        return _stats[stat].Level >= 3;
    }

    /// <summary>
    /// Get maximum accessible card depth for a stat
    /// </summary>
    public int GetMaxAccessibleDepth(PlayerStatType stat)
    {
        return GetLevel(stat); // Your stat level = maximum depth accessible
    }

    /// <summary>
    /// Check if player can access cards of specific depth for a stat
    /// </summary>
    public bool CanAccessCardDepth(PlayerStatType stat, CardDepth depth)
    {
        return GetLevel(stat) >= (int)depth;
    }

    /// <summary>
    /// Get all accessible depths for a stat (for UI display)
    /// </summary>
    public List<CardDepth> GetAccessibleDepths(PlayerStatType stat)
    {
        int maxDepth = GetMaxAccessibleDepth(stat);
        List<CardDepth> depths = new();

        for (int i = 1; i <= Math.Min(maxDepth, 10); i++)
        {
            depths.Add((CardDepth)i);
        }

        return depths;
    }

    /// <summary>
    /// Get all stats as a dictionary for serialization
    /// </summary>
    public Dictionary<PlayerStatType, StatProgress> GetAllStats()
    {
        return new Dictionary<PlayerStatType, StatProgress>(_stats);
    }

    /// <summary>
    /// Set stats from a dictionary (for deserialization)
    /// </summary>
    public void SetStats(Dictionary<PlayerStatType, StatProgress> stats)
    {
        _stats = new Dictionary<PlayerStatType, StatProgress>();
        foreach (KeyValuePair<PlayerStatType, StatProgress> kvp in stats)
        {
            _stats[kvp.Key] = kvp.Value.Clone();
        }
    }

    /// <summary>
    /// Create a deep copy of the player stats
    /// </summary>
    public PlayerStats Clone()
    {
        PlayerStats clone = new PlayerStats();
        foreach (KeyValuePair<PlayerStatType, StatProgress> kvp in _stats)
        {
            clone._stats[kvp.Key] = kvp.Value.Clone();
        }
        return clone;
    }

    /// <summary>
    /// Get total level across all stats (for progression tracking)
    /// </summary>
    public int GetTotalLevel()
    {
        return _stats.Values.Sum(s => s.Level);
    }

    /// <summary>
    /// Get total XP earned across all stats
    /// </summary>
    public int GetTotalXP()
    {
        int total = 0;
        foreach (StatProgress stat in _stats.Values)
        {
            total += stat.XP;
            // Add XP spent on previous levels (extended to level 8)
            for (int level = 1; level < stat.Level; level++)
            {
                total += level switch
                {
                    1 => 10,
                    2 => 25,
                    3 => 50,
                    4 => 100,
                    5 => 175,
                    6 => 275,
                    7 => 400,
                    _ => 0
                };
            }
        }
        return total;
    }

    /// <summary>
    /// Check if player meets stat requirement for obligation approaches
    /// </summary>
    public bool MeetsStatRequirement(PlayerStatType stat, int requiredLevel)
    {
        return GetLevel(stat) >= requiredLevel;
    }

    /// <summary>
    /// Get the level of the highest stat
    /// CONVERSATION SYSTEM: Used to calculate starting Understanding/Momentum/Initiative
    /// Starting Understanding = 2 + floor(highest_stat/3)
    /// Starting Momentum = 2 + floor(highest_stat/3)
    /// Starting Initiative = 3 + floor(highest_stat/3)
    /// </summary>
    public int GetHighestLevel()
    {
        return _stats.Values.Max(s => s.Level);
    }

}