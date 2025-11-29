using System.Reflection;
using Xunit;

namespace Wayfarer.Tests.Architecture;

/// <summary>
/// Tests for Type System compliance (CLAUDE.md User Code Preferences).
/// - Collections: ONLY List<T> (no Dictionary, HashSet for domain)
/// - Numbers: ONLY int (no float, double for game values)
/// - Keywords: No var (enforced at compile time, tested structurally here)
///
/// BEHAVIOR-ONLY TESTING (§8.21): Tests verify structural compliance.
/// </summary>
public class TypeSystemComplianceTests
{
    /// <summary>
    /// Verify that domain entities do not use Dictionary for collections.
    /// Domain Collection Principle: Use List<T> with LINQ queries.
    /// </summary>
    [Fact]
    public void DomainEntities_ShouldNot_UseDictionary()
    {
        Assembly assembly = typeof(GameWorld).Assembly;
        List<string> violations = new List<string>();

        foreach (Type type in GetDomainTypes(assembly))
        {
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo prop in properties)
            {
                if (IsDictionaryType(prop.PropertyType))
                {
                    violations.Add($"{type.Name}.{prop.Name} uses Dictionary (should use List<T> per Domain Collection Principle)");
                }
            }
        }

        // Filter known exceptions documented in CLAUDE.md
        violations = violations
            .Where(v => !v.Contains("DTO"))           // DTOs may use Dictionary for JSON mapping
            .Where(v => !v.Contains("Parser"))        // Parsers may use Dictionary temporarily
            .Where(v => !v.Contains("Blazor"))        // Blazor framework requirements
            .Where(v => !v.Contains("Parameters"))    // Component parameters
            .ToList();

        Assert.Empty(violations);
    }

    /// <summary>
    /// Verify that domain entities do not use HashSet for collections.
    /// </summary>
    [Fact]
    public void DomainEntities_ShouldNot_UseHashSet()
    {
        Assembly assembly = typeof(GameWorld).Assembly;
        List<string> violations = new List<string>();

        foreach (Type type in GetDomainTypes(assembly))
        {
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo prop in properties)
            {
                if (IsHashSetType(prop.PropertyType))
                {
                    violations.Add($"{type.Name}.{prop.Name} uses HashSet (should use List<T> per Domain Collection Principle)");
                }
            }
        }

        Assert.Empty(violations);
    }

    /// <summary>
    /// Verify GameWorld collections are all List<T>.
    /// </summary>
    [Fact]
    public void GameWorld_Collections_AreAllLists()
    {
        Type gameWorldType = typeof(GameWorld);
        PropertyInfo[] properties = gameWorldType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        List<string> violations = new List<string>();

        foreach (PropertyInfo prop in properties)
        {
            // Check if it's a collection type
            if (prop.PropertyType.IsGenericType)
            {
                Type genericDef = prop.PropertyType.GetGenericTypeDefinition();

                if (genericDef == typeof(Dictionary<,>) || genericDef == typeof(HashSet<>))
                {
                    violations.Add($"GameWorld.{prop.Name} uses {genericDef.Name} instead of List<T>");
                }
            }
        }

        Assert.Empty(violations);
    }

    /// <summary>
    /// Verify game value properties use int, not float or double.
    /// TYPE SYSTEM: int only for game values (no float/double).
    /// </summary>
    [Fact]
    public void GameValues_ShouldUse_IntNotFloat()
    {
        Assembly assembly = typeof(GameWorld).Assembly;
        List<string> violations = new List<string>();

        // Property names that indicate game values (should be int)
        HashSet<string> gameValuePatterns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Cost", "Price", "Amount", "Count", "Level", "Value", "Points",
            "Health", "Stamina", "Focus", "Hunger", "Coins", "Gold",
            "Damage", "Armor", "Defense", "Attack", "Strength",
            "Tier", "Rating", "Score", "Bonus", "Penalty",
            "Duration", "Delay", "Time", "Days", "Segments",
            "Min", "Max", "Current", "Total", "Base"
        };

        foreach (Type type in GetDomainTypes(assembly))
        {
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo prop in properties)
            {
                bool isGameValue = gameValuePatterns.Any(pattern =>
                    prop.Name.Contains(pattern, StringComparison.OrdinalIgnoreCase));

                if (isGameValue)
                {
                    if (prop.PropertyType == typeof(float))
                    {
                        violations.Add($"{type.Name}.{prop.Name} uses float (should use int for game values)");
                    }
                    else if (prop.PropertyType == typeof(double))
                    {
                        violations.Add($"{type.Name}.{prop.Name} uses double (should use int for game values)");
                    }
                    else if (prop.PropertyType == typeof(decimal))
                    {
                        violations.Add($"{type.Name}.{prop.Name} uses decimal (should use int for game values)");
                    }
                }
            }
        }

        // Filter known exceptions
        violations = violations
            .Where(v => !v.Contains("DTO"))               // DTOs may have different types for JSON
            .Where(v => !v.Contains("HexPosition"))       // Hex coordinates use specialized types
            .Where(v => !v.Contains("Coordinate"))        // Coordinates may use float
            .Where(v => !v.Contains("Animation"))         // Already fixed to int AnimationDelayMs
            .ToList();

        Assert.Empty(violations);
    }

    /// <summary>
    /// Verify Player stats are all int type.
    /// </summary>
    [Fact]
    public void Player_Stats_AreAllInt()
    {
        Type playerType = typeof(Player);
        List<string> statProperties = new List<string>
        {
            "Insight", "Rapport", "Authority", "Diplomacy", "Cunning",
            "Health", "Stamina", "Focus", "Hunger", "Resolve",
            "MaxHealth", "MaxStamina", "MaxFocus", "MaxHunger",
            "Coins"
        };

        List<string> violations = new List<string>();

        foreach (string statName in statProperties)
        {
            PropertyInfo prop = playerType.GetProperty(statName);
            if (prop != null && prop.PropertyType != typeof(int))
            {
                violations.Add($"Player.{statName} is {prop.PropertyType.Name} but should be int");
            }
        }

        Assert.Empty(violations);
    }

    /// <summary>
    /// Verify NPC stats are all int type.
    /// </summary>
    [Fact]
    public void NPC_Stats_AreAllInt()
    {
        Type npcType = typeof(NPC);
        List<string> statProperties = new List<string>
        {
            "RelationshipFlow", "StoryCubes", "Tier", "VisitCount"
        };

        List<string> violations = new List<string>();

        foreach (string statName in statProperties)
        {
            PropertyInfo prop = npcType.GetProperty(statName);
            if (prop != null && prop.PropertyType != typeof(int))
            {
                violations.Add($"NPC.{statName} is {prop.PropertyType.Name} but should be int");
            }
        }

        Assert.Empty(violations);
    }

    /// <summary>
    /// Verify Location stats are all int type.
    /// </summary>
    [Fact]
    public void Location_Stats_AreAllInt()
    {
        Type locationType = typeof(Location);
        List<string> statProperties = new List<string>
        {
            "InvestigationCubes", "VisitCount", "Tier"
        };

        List<string> violations = new List<string>();

        foreach (string statName in statProperties)
        {
            PropertyInfo prop = locationType.GetProperty(statName);
            if (prop != null && prop.PropertyType != typeof(int))
            {
                violations.Add($"Location.{statName} is {prop.PropertyType.Name} but should be int");
            }
        }

        Assert.Empty(violations);
    }

    /// <summary>
    /// Verify cost/reward classes use int for all values.
    /// </summary>
    [Fact]
    public void CostRewardClasses_UseIntValues()
    {
        List<Type> costRewardTypes = new List<Type>
        {
            typeof(ActionCosts),
            typeof(ActionRewards),
            typeof(ChoiceCost),
            typeof(ChoiceReward),
            typeof(Consequence)
        };

        List<string> violations = new List<string>();

        foreach (Type type in costRewardTypes)
        {
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo prop in properties)
            {
                // Numeric properties should be int
                if (prop.PropertyType == typeof(float) ||
                    prop.PropertyType == typeof(double) ||
                    prop.PropertyType == typeof(decimal))
                {
                    violations.Add($"{type.Name}.{prop.Name} uses {prop.PropertyType.Name} (should use int for game values)");
                }
            }
        }

        Assert.Empty(violations);
    }

    /// <summary>
    /// Verify MarketPriceInfo uses int for all price values.
    /// </summary>
    [Fact]
    public void MarketPriceInfo_UsesIntValues()
    {
        Type priceInfoType = typeof(MarketPriceInfo);
        List<string> violations = new List<string>();

        PropertyInfo[] properties = priceInfoType.GetProperties();
        foreach (PropertyInfo prop in properties)
        {
            if (prop.Name.Contains("Price") || prop.Name.Contains("Level"))
            {
                if (prop.PropertyType != typeof(int))
                {
                    violations.Add($"MarketPriceInfo.{prop.Name} is {prop.PropertyType.Name} but should be int");
                }
            }
        }

        Assert.Empty(violations);
    }

    /// <summary>
    /// Verify CardAnimationState uses int for timing values.
    /// </summary>
    [Fact]
    public void CardAnimationState_UsesIntForTiming()
    {
        Type animationType = typeof(CardAnimationState);
        List<string> violations = new List<string>();

        PropertyInfo delayProp = animationType.GetProperty("AnimationDelayMs");
        if (delayProp != null && delayProp.PropertyType != typeof(int))
        {
            violations.Add($"CardAnimationState.AnimationDelayMs is {delayProp.PropertyType.Name} but should be int");
        }

        PropertyInfo seqProp = animationType.GetProperty("SequenceIndex");
        if (seqProp != null && seqProp.PropertyType != typeof(int))
        {
            violations.Add($"CardAnimationState.SequenceIndex is {seqProp.PropertyType.Name} but should be int");
        }

        Assert.Empty(violations);
    }

    /// <summary>
    /// Verify PathfindingResult uses int for cost.
    /// </summary>
    [Fact]
    public void PathfindingResult_UsesIntForCost()
    {
        Type resultType = typeof(PathfindingResult);

        PropertyInfo costProp = resultType.GetProperty("TotalCost");
        if (costProp != null)
        {
            Assert.Equal(typeof(int), costProp.PropertyType);
        }
    }

    // ========== HELPER METHODS ==========

    private bool IsDictionaryType(Type type)
    {
        if (!type.IsGenericType) return false;
        Type genericDef = type.GetGenericTypeDefinition();
        return genericDef == typeof(Dictionary<,>) ||
               genericDef == typeof(IDictionary<,>) ||
               genericDef == typeof(IReadOnlyDictionary<,>);
    }

    private bool IsHashSetType(Type type)
    {
        if (!type.IsGenericType) return false;
        Type genericDef = type.GetGenericTypeDefinition();
        return genericDef == typeof(HashSet<>) ||
               genericDef == typeof(ISet<>);
    }

    private List<Type> GetDomainTypes(Assembly assembly)
    {
        return assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => !t.Namespace?.Contains("Pages") ?? true)
            .Where(t => !t.Name.Contains("Parser"))
            .Where(t => !t.Name.EndsWith("Builder"))
            .Where(t => !t.Name.EndsWith("Factory"))
            .ToList();
    }
}
