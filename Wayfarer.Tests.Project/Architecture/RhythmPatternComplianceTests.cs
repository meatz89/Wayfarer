using System.Reflection;
using System.Text.RegularExpressions;
using Xunit;

namespace Wayfarer.Tests.Architecture;

/// <summary>
/// Tests for Sir Brante Rhythm Pattern compliance (arc42 §8.26).
/// HIGHLANDER: ONE generation path for ALL archetypes.
/// RhythmPattern (Building/Crisis/Mixed) determines choice structure.
///
/// Design Goal: ANY archetype + ANY rhythm produces appropriate choices.
/// - Building scenes grant stats (identity formation, no requirements)
/// - Crisis scenes gate penalty avoidance (test investments)
/// - Mixed scenes offer standard trade-offs
///
/// All scene archetypes MUST:
/// 1. Use GenerateChoiceTemplatesWithContext (context REQUIRED)
/// 2. Pass GenerationContext to choice generation
/// 3. NOT bypass rhythm pattern via duplicate implementations
/// </summary>
public class RhythmPatternComplianceTests
{
    /// <summary>
    /// Verify SituationArchetypeCatalog has only ONE choice generation method.
    /// HIGHLANDER: GenerateChoiceTemplatesWithContext with REQUIRED context.
    /// No GenerateChoiceTemplates method without context should exist.
    /// </summary>
    [Fact]
    public void SituationArchetypeCatalog_HasOnlyOneGenerationMethod()
    {
        Type catalogType = typeof(SituationArchetypeCatalog);

        MethodInfo[] allMethods = catalogType.GetMethods(BindingFlags.Public | BindingFlags.Static);
        List<MethodInfo> generationMethods = allMethods
            .Where(m => m.Name.StartsWith("GenerateChoiceTemplates"))
            .ToList();

        Assert.Single(generationMethods);
        Assert.Equal("GenerateChoiceTemplatesWithContext", generationMethods[0].Name);
    }

    /// <summary>
    /// Verify GenerateChoiceTemplatesWithContext requires GenerationContext.
    /// Context must NOT have default value (= null) - fail-fast principle.
    /// </summary>
    [Fact]
    public void GenerateChoiceTemplatesWithContext_RequiresContext()
    {
        Type catalogType = typeof(SituationArchetypeCatalog);

        MethodInfo method = catalogType.GetMethod(
            "GenerateChoiceTemplatesWithContext",
            BindingFlags.Public | BindingFlags.Static);

        Assert.NotNull(method);

        ParameterInfo[] parameters = method.GetParameters();
        ParameterInfo contextParam = parameters.FirstOrDefault(p => p.Name == "context");

        Assert.NotNull(contextParam);
        Assert.Equal(typeof(GenerationContext), contextParam.ParameterType);
        Assert.False(contextParam.HasDefaultValue, "Context parameter must be REQUIRED (no default value)");
    }

    /// <summary>
    /// Verify SceneArchetypeCatalog and its dispatched archetype files use GenerateChoiceTemplatesWithContext.
    /// No calls to GenerateChoiceTemplates (without context) should exist.
    /// Source code scan for HIGHLANDER compliance across all archetype files.
    /// </summary>
    [Fact]
    public void SceneArchetypeCatalog_UsesOnlyContextAwareGeneration()
    {
        string catalogsDir = GetSourceFilePath("src/Content/Catalogs");
        string[] archetypeFiles = Directory.GetFiles(catalogsDir, "*Archetypes.cs");

        int totalContextAwareCalls = 0;
        int totalNonContextCalls = 0;

        foreach (string file in archetypeFiles)
        {
            string sourceCode = File.ReadAllText(file);
            totalContextAwareCalls += Regex.Matches(sourceCode, @"GenerateChoiceTemplatesWithContext\(").Count;
            totalNonContextCalls += Regex.Matches(sourceCode, @"GenerateChoiceTemplates\([^W]").Count;
        }

        Assert.True(totalContextAwareCalls > 0,
            "Archetype files (JourneyArchetypes, ExplorationArchetypes, EncounterArchetypes) should use GenerateChoiceTemplatesWithContext");
        Assert.Equal(0, totalNonContextCalls);
    }

    /// <summary>
    /// Verify SceneTemplateParser does not have duplicate choice generation.
    /// HIGHLANDER: Parser should delegate to catalogue, not duplicate logic.
    /// </summary>
    [Fact]
    public void SceneTemplateParser_NosDuplicateChoiceGeneration()
    {
        string sourceFile = GetSourceFilePath("src/Content/Parsers/SceneTemplateParser.cs");
        string sourceCode = File.ReadAllText(sourceFile);

        bool hasGenerateChoiceTemplatesFromArchetype = sourceCode.Contains("GenerateChoiceTemplatesFromArchetype");
        bool hasGenerateSingleSituation = sourceCode.Contains("GenerateSingleSituationFromArchetype");

        Assert.False(hasGenerateChoiceTemplatesFromArchetype,
            "SceneTemplateParser should not have GenerateChoiceTemplatesFromArchetype (HIGHLANDER violation)");
        Assert.False(hasGenerateSingleSituation,
            "SceneTemplateParser should not have GenerateSingleSituationFromArchetype (HIGHLANDER violation)");
    }

    /// <summary>
    /// Get path to source file by finding solution root directory.
    /// Works from test bin directory by walking up to find .sln file.
    /// </summary>
    private static string GetSourceFilePath(string relativePath)
    {
        string currentDir = Directory.GetCurrentDirectory();
        DirectoryInfo dir = new DirectoryInfo(currentDir);

        while (dir != null)
        {
            if (dir.GetFiles("*.sln").Length > 0)
            {
                return Path.Combine(dir.FullName, relativePath);
            }
            dir = dir.Parent;
        }

        throw new InvalidOperationException($"Could not find solution root from {currentDir}");
    }

    /// <summary>
    /// Verify GenerationContext has Rhythm property.
    /// RhythmPattern must flow through context to choice generation.
    /// </summary>
    [Fact]
    public void GenerationContext_HasRhythmProperty()
    {
        Type contextType = typeof(GenerationContext);

        PropertyInfo rhythmProp = contextType.GetProperty("Rhythm");
        Assert.NotNull(rhythmProp);
        Assert.Equal(typeof(RhythmPattern), rhythmProp.PropertyType);
    }

    /// <summary>
    /// Verify RhythmPattern enum has all required values.
    /// Sir Brante pattern: Building, Crisis, Mixed.
    /// </summary>
    [Fact]
    public void RhythmPattern_HasRequiredValues()
    {
        Array values = Enum.GetValues(typeof(RhythmPattern));
        List<string> valueNames = values.Cast<RhythmPattern>().Select(v => v.ToString()).ToList();

        Assert.Contains("Building", valueNames);
        Assert.Contains("Crisis", valueNames);
        Assert.Contains("Mixed", valueNames);
    }

    /// <summary>
    /// Verify tutorial JSON templates have rhythmPattern authored.
    /// Fail-fast: Missing rhythmPattern should fail at parse time, not silently default.
    /// </summary>
    [Fact]
    public void TutorialJSON_HasRhythmPatternAuthored()
    {
        string tutorialFile = GetSourceFilePath("src/Content/Core/22_a_story_tutorial.json");
        string tutorialJson = File.ReadAllText(tutorialFile);

        int buildingCount = Regex.Matches(tutorialJson, @"""rhythmPattern""\s*:\s*""Building""", RegexOptions.IgnoreCase).Count;
        int mixedCount = Regex.Matches(tutorialJson, @"""rhythmPattern""\s*:\s*""Mixed""", RegexOptions.IgnoreCase).Count;
        int crisisCount = Regex.Matches(tutorialJson, @"""rhythmPattern""\s*:\s*""Crisis""", RegexOptions.IgnoreCase).Count;

        Assert.True(buildingCount >= 1, "Tutorial should have at least one Building rhythm scene (A1)");
        Assert.True(mixedCount >= 1, "Tutorial should have at least one Mixed rhythm scene (A2)");
        Assert.True(crisisCount >= 1, "Tutorial should have at least one Crisis rhythm scene (A3)");
    }
}
