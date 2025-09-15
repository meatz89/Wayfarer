using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Tracks progress for a single stat (level and XP)
/// </summary>
public class StatProgress
{
    public int Level { get; set; } = 1;
    public int XP { get; set; } = 0;

    /// <summary>
    /// Get XP required to reach the next level
    /// </summary>
    public int GetXPToNextLevel()
    {
        return Level switch
        {
            1 => 10,
            2 => 25,
            3 => 50,
            4 => 100,
            _ => int.MaxValue // Level 5 is max
        };
    }

    /// <summary>
    /// Get progress percentage toward next level (0-100)
    /// </summary>
    public int GetProgressPercent()
    {
        if (Level >= 5) return 100;

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
    /// Get the current level for a stat (1-5)
    /// </summary>
    public int GetLevel(PlayerStatType stat)
    {
        return _stats[stat].Level;
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

        var progress = _stats[stat];
        progress.XP += amount;

        // Check for level ups (max level 5)
        while (progress.Level < 5 && progress.XP >= progress.GetXPToNextLevel())
        {
            progress.XP -= progress.GetXPToNextLevel();
            progress.Level++;
        }

        // Cap XP at level 5
        if (progress.Level >= 5)
        {
            progress.XP = 0;
        }
    }

    /// <summary>
    /// Get success rate bonus for a stat based on level
    /// Level 1: +0%, Level 2: +5%, Level 3: +10%, Level 4: +15%, Level 5: +20%
    /// </summary>
    public int GetSuccessBonus(PlayerStatType stat)
    {
        return (_stats[stat].Level - 1) * 5;
    }

    /// <summary>
    /// Check if stat has persistence bonus (level 3+)
    /// Cards bound to this stat gain Thought persistence
    /// </summary>
    public bool HasPersistenceBonus(PlayerStatType stat)
    {
        return _stats[stat].Level >= 3;
    }

    /// <summary>
    /// Check if stat has mastery effect (level 5)
    /// Cards bound to this stat never force LISTEN on failure
    /// </summary>
    public bool IgnoresFailureListen(PlayerStatType stat)
    {
        return _stats[stat].Level >= 5;
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
        foreach (var kvp in stats)
        {
            _stats[kvp.Key] = kvp.Value.Clone();
        }
    }

    /// <summary>
    /// Create a deep copy of the player stats
    /// </summary>
    public PlayerStats Clone()
    {
        var clone = new PlayerStats();
        foreach (var kvp in _stats)
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
        foreach (var stat in _stats.Values)
        {
            total += stat.XP;
            // Add XP spent on previous levels
            for (int level = 1; level < stat.Level; level++)
            {
                total += level switch
                {
                    1 => 10,
                    2 => 25,
                    3 => 50,
                    4 => 100,
                    _ => 0
                };
            }
        }
        return total;
    }

    /// <summary>
    /// Check if player meets stat requirement for investigation approaches
    /// </summary>
    public bool MeetsStatRequirement(PlayerStatType stat, int requiredLevel)
    {
        return GetLevel(stat) >= requiredLevel;
    }

    /// <summary>
    /// Get highest stat level (for display/progression)
    /// </summary>
    public int GetHighestStatLevel()
    {
        return _stats.Values.Max(s => s.Level);
    }

    /// <summary>
    /// Get primary stat (highest level, breaking ties by total XP)
    /// </summary>
    public PlayerStatType GetPrimaryStat()
    {
        var maxLevel = GetHighestStatLevel();
        var primaryCandidates = _stats.Where(kvp => kvp.Value.Level == maxLevel).ToList();

        if (primaryCandidates.Count == 1)
        {
            return primaryCandidates[0].Key;
        }

        // Break ties by total XP in that stat
        return primaryCandidates.OrderByDescending(kvp =>
        {
            var stat = kvp.Value;
            int totalXp = stat.XP;
            for (int level = 1; level < stat.Level; level++)
            {
                totalXp += level switch
                {
                    1 => 10,
                    2 => 25,
                    3 => 50,
                    4 => 100,
                    _ => 0
                };
            }
            return totalXp;
        }).First().Key;
    }
}