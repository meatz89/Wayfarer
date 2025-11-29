using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Xunit;

namespace Wayfarer.Tests.Architecture;

/// <summary>
/// Tests for DDR-007 (Intentional Numeric Design) compliance.
/// Three principles:
/// 1. Mental Math Design - Values small enough for head calculation (±20 range)
/// 2. Deterministic Arithmetic - No randomness in strategic outcomes
/// 3. Absolute Modifiers - Bonuses stack additively, never multiplicatively
///
/// BEHAVIOR-ONLY TESTING (§8.21): Tests verify structural and pattern compliance.
/// </summary>
public class DDR007ComplianceTests
{
    // ========== PRINCIPLE 1: MENTAL MATH DESIGN ==========

    /// <summary>
    /// Verify PriceManager uses flat adjustments in mental math range.
    /// </summary>
    [Fact]
    public void PriceManager_UsesAbsoluteSpread_NotPercentage()
    {
        Type priceManagerType = typeof(PriceManager);
        FieldInfo[] fields = priceManagerType.GetFields(BindingFlags.NonPublic | BindingFlags.Static);

        List<string> violations = new List<string>();

        foreach (FieldInfo field in fields)
        {
            string fieldName = field.Name.ToUpperInvariant();

            if (fieldName.Contains("PERCENT") || fieldName.Contains("BP"))
            {
                violations.Add($"PriceManager.{field.Name} violates DDR-007: percentage/basis point naming");
            }
        }

        Assert.Empty(violations);
    }

    /// <summary>
    /// Verify that PriceManager's BUY_SELL_SPREAD is a flat value (not percentage).
    /// </summary>
    [Fact]
    public void PriceManager_BuySellSpread_IsFlatValue()
    {
        Type priceManagerType = typeof(PriceManager);

        FieldInfo spreadField = priceManagerType.GetField(
            "BUY_SELL_SPREAD",
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(spreadField);

        object value = spreadField.GetValue(null);
        int spreadValue = (int)value;

        Assert.True(spreadValue >= 1 && spreadValue <= 10,
            $"BUY_SELL_SPREAD should be small absolute value (1-10), got {spreadValue}");
    }

    /// <summary>
    /// Verify PricingInfo uses flat adjustment properties.
    /// </summary>
    [Fact]
    public void PricingInfo_UsesFlatAdjustments()
    {
        Type pricingInfoType = typeof(PriceManager).GetNestedType("PricingInfo");
        Assert.NotNull(pricingInfoType);

        PropertyInfo[] properties = pricingInfoType.GetProperties();
        List<string> violations = new List<string>();

        foreach (PropertyInfo prop in properties)
        {
            string propName = prop.Name;

            if (propName.Contains("Modifier") && propName.Contains("BP"))
            {
                violations.Add($"PricingInfo.{propName} uses basis points (should use flat Adjustment)");
            }

            if (propName.Contains("Percent") || propName.Contains("Multiplier"))
            {
                violations.Add($"PricingInfo.{propName} uses percentage/multiplier naming");
            }
        }

        Assert.Empty(violations);
    }

    // ========== PRINCIPLE 2: DETERMINISTIC ARITHMETIC ==========

    /// <summary>
    /// Verify strategic services don't use System.Random.
    /// Tactical layer (Pile.cs card shuffling) is exempt.
    /// </summary>
    [Fact]
    public void StrategicServices_DoNotUseRandom()
    {
        Assembly assembly = typeof(GameWorld).Assembly;
        List<string> violations = new List<string>();

        List<string> strategicServicePatterns = new List<string>
        {
            "DialogueGeneration",
            "TravelManager",
            "HexRouteGenerator",
            "ObservationFacade",
            "SkeletonGenerator",
            "NarrativeService",
            "PriceManager",
            "TokenEffect"
        };

        foreach (Type type in assembly.GetTypes())
        {
            bool isStrategicService = strategicServicePatterns.Any(p =>
                type.Name.Contains(p));

            if (!isStrategicService) continue;

            FieldInfo[] fields = type.GetFields(
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.Static);

            foreach (FieldInfo field in fields)
            {
                if (field.FieldType == typeof(Random))
                {
                    violations.Add($"{type.Name} has Random field '{field.Name}' - violates DDR-007 Deterministic Arithmetic");
                }
            }
        }

        Assert.Empty(violations);
    }

    /// <summary>
    /// Verify Pile.cs (tactical layer) is the ONLY place with Random.
    /// This is the documented exception for card shuffling.
    /// </summary>
    [Fact]
    public void OnlyPile_UsesRandom_TacticalLayerException()
    {
        Assembly assembly = typeof(GameWorld).Assembly;
        List<string> typesWithRandom = new List<string>();

        foreach (Type type in assembly.GetTypes())
        {
            if (type.IsInterface || type.IsEnum) continue;

            FieldInfo[] fields = type.GetFields(
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.Static);

            bool hasRandom = fields.Any(f => f.FieldType == typeof(Random));

            if (hasRandom)
            {
                typesWithRandom.Add(type.Name);
            }
        }

        List<string> violations = typesWithRandom
            .Where(t => t != "Pile")
            .ToList();

        Assert.Empty(violations);
    }

    // ========== PRINCIPLE 3: ABSOLUTE MODIFIERS ==========

    /// <summary>
    /// Verify no properties named with "BP" (basis points) pattern.
    /// </summary>
    [Fact]
    public void DomainTypes_DoNotUseBasisPointProperties()
    {
        Assembly assembly = typeof(GameWorld).Assembly;
        List<string> violations = new List<string>();

        foreach (Type type in GetDomainTypes(assembly))
        {
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo prop in properties)
            {
                if (Regex.IsMatch(prop.Name, @"BP$|BasisPoint|^BP[A-Z]"))
                {
                    violations.Add($"{type.Name}.{prop.Name} uses basis point naming (violates DDR-007)");
                }
            }
        }

        Assert.Empty(violations);
    }

    /// <summary>
    /// Verify TokenEffectProcessor uses additive bonuses, not multiplicative modifiers.
    /// </summary>
    [Fact]
    public void TokenEffectProcessor_UsesAdditiveBonuses()
    {
        Type processorType = typeof(TokenEffectProcessor);

        FieldInfo[] fields = processorType.GetFields(
            BindingFlags.Public | BindingFlags.NonPublic |
            BindingFlags.Instance | BindingFlags.Static);

        List<string> violations = new List<string>();

        foreach (FieldInfo field in fields)
        {
            string fieldName = field.Name.ToLowerInvariant();

            if (fieldName.Contains("multiplier") || fieldName.Contains("modifier"))
            {
                if (field.FieldType.IsGenericType)
                {
                    Type valueType = field.FieldType.GetGenericArguments().LastOrDefault();
                    if (valueType == typeof(int))
                    {
                        continue;
                    }
                }

                violations.Add($"TokenEffectProcessor.{field.Name} may use multiplicative pattern");
            }
        }

        Assert.Empty(violations);
    }

    /// <summary>
    /// Verify SituationArchetypeCatalog archetypes use integer-only values.
    /// </summary>
    [Fact]
    public void SituationArchetypeCatalog_ArchetypesUseIntegerValues()
    {
        Type archetypeType = typeof(SituationArchetype);

        PropertyInfo[] properties = archetypeType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        List<string> violations = new List<string>();

        foreach (PropertyInfo prop in properties)
        {
            if (prop.PropertyType == typeof(double) ||
                prop.PropertyType == typeof(float) ||
                prop.PropertyType == typeof(decimal))
            {
                violations.Add($"SituationArchetype.{prop.Name} uses {prop.PropertyType.Name} (should use int)");
            }
        }

        Assert.Empty(violations);
    }

    /// <summary>
    /// Verify archetype values are in mental math range.
    /// </summary>
    [Fact]
    public void SituationArchetypeCatalog_ValuesInMentalMathRange()
    {
        List<string> violations = new List<string>();

        foreach (SituationArchetypeType archetypeType in Enum.GetValues<SituationArchetypeType>())
        {
            SituationArchetype archetype = SituationArchetypeCatalog.GetArchetype(archetypeType);

            if (archetype.StatThreshold < 0 || archetype.StatThreshold > 10)
            {
                violations.Add($"{archetypeType}.StatThreshold = {archetype.StatThreshold} (should be 0-10)");
            }

            if (archetype.CoinCost < 0 || archetype.CoinCost > 30)
            {
                violations.Add($"{archetypeType}.CoinCost = {archetype.CoinCost} (should be 0-30)");
            }

            if (archetype.ResolveCost < 0 || archetype.ResolveCost > 15)
            {
                violations.Add($"{archetypeType}.ResolveCost = {archetype.ResolveCost} (should be 0-15)");
            }

            if (archetype.FallbackTimeCost < 0 || archetype.FallbackTimeCost > 5)
            {
                violations.Add($"{archetypeType}.FallbackTimeCost = {archetype.FallbackTimeCost} (should be 0-5)");
            }
        }

        Assert.Empty(violations);
    }

    /// <summary>
    /// Verify card effect catalogues use absolute integer values.
    /// </summary>
    [Fact]
    public void CardEffectCatalogues_UseAbsoluteValues()
    {
        Assembly assembly = typeof(GameWorld).Assembly;

        List<Type> catalogTypes = assembly.GetTypes()
            .Where(t => t.Name.Contains("CardEffect") && t.Name.Contains("Catalog"))
            .ToList();

        List<string> violations = new List<string>();

        foreach (Type catalogType in catalogTypes)
        {
            FieldInfo[] fields = catalogType.GetFields(
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.Static);

            foreach (FieldInfo field in fields)
            {
                if (field.FieldType == typeof(double) ||
                    field.FieldType == typeof(float) ||
                    field.FieldType == typeof(decimal))
                {
                    violations.Add($"{catalogType.Name}.{field.Name} uses {field.FieldType.Name} (should use int)");
                }
            }
        }

        Assert.Empty(violations);
    }

    // ========== SOURCE CODE PATTERN DETECTION ==========

    /// <summary>
    /// Detect decimal multipliers in source code (e.g., * 1.15, * 0.85).
    /// These patterns violate DDR-007 Absolute Modifiers principle.
    /// </summary>
    [Fact]
    public void SourceCode_NoDecimalMultipliers()
    {
        string srcPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "src");

        if (!Directory.Exists(srcPath))
        {
            srcPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "src"));
        }

        List<string> violations = new List<string>();
        Regex multiplierPattern = new Regex(@"\*\s*[0-9]+\.[0-9]+[^f]", RegexOptions.Compiled);
        Regex exemptPattern = new Regex(@"milliseconds|timeout|delay|animation|opacity|alpha|scale|zoom", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        if (Directory.Exists(srcPath))
        {
            foreach (string file in Directory.GetFiles(srcPath, "*.cs", SearchOption.AllDirectories))
            {
                if (file.Contains("Test") || file.Contains("obj") || file.Contains("bin")) continue;

                string[] lines = File.ReadAllLines(file);
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];

                    if (multiplierPattern.IsMatch(line) && !exemptPattern.IsMatch(line))
                    {
                        string fileName = Path.GetFileName(file);
                        violations.Add($"{fileName}:{i + 1}: {line.Trim()}");
                    }
                }
            }
        }

        Assert.Empty(violations);
    }

    /// <summary>
    /// Detect percentage calculations in domain code (e.g., / 100, * 100 / x).
    /// These patterns typically indicate DDR-007 violations.
    /// </summary>
    [Fact]
    public void SourceCode_NoPercentageCalculations()
    {
        string srcPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "src");

        if (!Directory.Exists(srcPath))
        {
            srcPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "src"));
        }

        List<string> violations = new List<string>();
        Regex percentPattern = new Regex(@"\*\s*100\s*/|\s*/\s*100(?!\d)", RegexOptions.Compiled);
        Regex exemptCommentPattern = new Regex(@"^\s*//|^\s*/\*|\*\s+100\s+coins", RegexOptions.Compiled);

        if (Directory.Exists(srcPath))
        {
            foreach (string file in Directory.GetFiles(srcPath, "*.cs", SearchOption.AllDirectories))
            {
                if (file.Contains("Test") || file.Contains("obj") || file.Contains("bin")) continue;
                if (file.Contains("Parser") || file.Contains("DTO")) continue; // Allow in parsing layer

                string[] lines = File.ReadAllLines(file);
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];

                    if (percentPattern.IsMatch(line) && !exemptCommentPattern.IsMatch(line))
                    {
                        string fileName = Path.GetFileName(file);
                        violations.Add($"{fileName}:{i + 1}: {line.Trim()}");
                    }
                }
            }
        }

        Assert.Empty(violations);
    }

    // ========== INTEGRATION TESTS ==========

    /// <summary>
    /// Verify archetype scaling uses additive adjustments.
    /// PowerDynamic.Dominant = base - 2, Submissive = base + 2 (not multipliers)
    /// </summary>
    [Fact]
    public void ArchetypeScaling_UsesAdditiveAdjustments()
    {
        SituationArchetype archetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.Negotiation);
        string situationId = "test_situation";

        GenerationContext dominantContext = new GenerationContext
        {
            Power = PowerDynamic.Dominant,
            Quality = Quality.Standard,
            NpcDemeanor = NPCDemeanor.Neutral
        };

        GenerationContext submissiveContext = new GenerationContext
        {
            Power = PowerDynamic.Submissive,
            Quality = Quality.Standard,
            NpcDemeanor = NPCDemeanor.Neutral
        };

        List<ChoiceTemplate> dominantChoices = SituationArchetypeCatalog.GenerateChoiceTemplates(
            archetype, situationId, dominantContext);

        List<ChoiceTemplate> submissiveChoices = SituationArchetypeCatalog.GenerateChoiceTemplates(
            archetype, situationId, submissiveContext);

        ChoiceTemplate dominantStat = dominantChoices.First(c => c.Id.EndsWith("_stat"));
        ChoiceTemplate submissiveStat = submissiveChoices.First(c => c.Id.EndsWith("_stat"));

        int dominantThreshold = GetStatThresholdFromRequirement(dominantStat.RequirementFormula);
        int submissiveThreshold = GetStatThresholdFromRequirement(submissiveStat.RequirementFormula);

        int difference = submissiveThreshold - dominantThreshold;

        Assert.Equal(4, difference);
    }

    /// <summary>
    /// Verify quality scaling uses additive adjustments for costs.
    /// Quality.Basic = base - 3, Quality.Luxury = base + 10 (not multipliers)
    /// </summary>
    [Fact]
    public void QualityScaling_UsesAdditiveAdjustments()
    {
        SituationArchetype archetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.Negotiation);
        string situationId = "test_situation";
        int baseCoinCost = archetype.CoinCost;

        GenerationContext basicContext = new GenerationContext
        {
            Power = PowerDynamic.Equal,
            Quality = Quality.Basic,
            NpcDemeanor = NPCDemeanor.Neutral
        };

        GenerationContext luxuryContext = new GenerationContext
        {
            Power = PowerDynamic.Equal,
            Quality = Quality.Luxury,
            NpcDemeanor = NPCDemeanor.Neutral
        };

        List<ChoiceTemplate> basicChoices = SituationArchetypeCatalog.GenerateChoiceTemplates(
            archetype, situationId, basicContext);

        List<ChoiceTemplate> luxuryChoices = SituationArchetypeCatalog.GenerateChoiceTemplates(
            archetype, situationId, luxuryContext);

        ChoiceTemplate basicMoney = basicChoices.First(c => c.Id.EndsWith("_money"));
        ChoiceTemplate luxuryMoney = luxuryChoices.First(c => c.Id.EndsWith("_money"));

        int basicCost = -basicMoney.Consequence.Coins;
        int luxuryCost = -luxuryMoney.Consequence.Coins;

        Assert.Equal(baseCoinCost - 3, basicCost);
        Assert.Equal(baseCoinCost + 10, luxuryCost);
    }

    /// <summary>
    /// Verify demeanor scaling uses additive adjustments.
    /// Friendly = base - 2, Hostile = base + 2
    /// </summary>
    [Fact]
    public void DemeanorScaling_UsesAdditiveAdjustments()
    {
        SituationArchetype archetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.Negotiation);
        string situationId = "test_situation";

        GenerationContext friendlyContext = new GenerationContext
        {
            Power = PowerDynamic.Equal,
            Quality = Quality.Standard,
            NpcDemeanor = NPCDemeanor.Friendly
        };

        GenerationContext hostileContext = new GenerationContext
        {
            Power = PowerDynamic.Equal,
            Quality = Quality.Standard,
            NpcDemeanor = NPCDemeanor.Hostile
        };

        List<ChoiceTemplate> friendlyChoices = SituationArchetypeCatalog.GenerateChoiceTemplates(
            archetype, situationId, friendlyContext);

        List<ChoiceTemplate> hostileChoices = SituationArchetypeCatalog.GenerateChoiceTemplates(
            archetype, situationId, hostileContext);

        ChoiceTemplate friendlyStat = friendlyChoices.First(c => c.Id.EndsWith("_stat"));
        ChoiceTemplate hostileStat = hostileChoices.First(c => c.Id.EndsWith("_stat"));

        int friendlyThreshold = GetStatThresholdFromRequirement(friendlyStat.RequirementFormula);
        int hostileThreshold = GetStatThresholdFromRequirement(hostileStat.RequirementFormula);

        int difference = hostileThreshold - friendlyThreshold;

        Assert.Equal(4, difference);
    }

    // ========== HELPER METHODS ==========

    private List<Type> GetDomainTypes(Assembly assembly)
    {
        return assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => !t.Namespace?.Contains("Pages") ?? true)
            .Where(t => !t.Name.Contains("Parser"))
            .Where(t => !t.Name.EndsWith("DTO"))
            .Where(t => !t.Name.EndsWith("Builder"))
            .Where(t => !t.Name.EndsWith("Factory"))
            .ToList();
    }

    private int GetStatThresholdFromRequirement(CompoundRequirement requirement)
    {
        if (requirement.OrPaths.Count == 0) return 0;

        OrPath firstPath = requirement.OrPaths[0];

        // OrPath stat properties are nullable int - use null coalescing
        if (firstPath.InsightRequired.HasValue && firstPath.InsightRequired.Value > 0) return firstPath.InsightRequired.Value;
        if (firstPath.RapportRequired.HasValue && firstPath.RapportRequired.Value > 0) return firstPath.RapportRequired.Value;
        if (firstPath.AuthorityRequired.HasValue && firstPath.AuthorityRequired.Value > 0) return firstPath.AuthorityRequired.Value;
        if (firstPath.DiplomacyRequired.HasValue && firstPath.DiplomacyRequired.Value > 0) return firstPath.DiplomacyRequired.Value;
        if (firstPath.CunningRequired.HasValue && firstPath.CunningRequired.Value > 0) return firstPath.CunningRequired.Value;

        return 0;
    }
}
