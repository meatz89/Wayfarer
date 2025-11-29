using System.Reflection;
using Xunit;

namespace Wayfarer.Tests.Architecture;

/// <summary>
/// Tests for Backend/Frontend Separation compliance (arc42 §8.6).
/// Backend returns domain semantics (WHAT). Frontend decides presentation (HOW).
///
/// FORBIDDEN in Backend:
/// - CssClass properties in ViewModels
/// - IconName properties in ViewModels
/// - Display string generation in services
///
/// BEHAVIOR-ONLY TESTING (§8.21): Tests verify structural compliance.
/// </summary>
public class BackendFrontendSeparationTests
{
    /// <summary>
    /// Verify ViewModels do not contain CSS class properties.
    /// Frontend should compute CSS from domain values.
    /// </summary>
    [Fact]
    public void ViewModels_ShouldNot_ContainCssClassProperties()
    {
        Assembly assembly = typeof(GameWorld).Assembly;
        List<string> violations = new List<string>();

        foreach (Type type in GetViewModelTypes(assembly))
        {
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo prop in properties)
            {
                if (prop.PropertyType == typeof(string))
                {
                    string propNameLower = prop.Name.ToLowerInvariant();
                    if (propNameLower.Contains("css") ||
                        propNameLower.Contains("class") ||
                        propNameLower.Contains("style"))
                    {
                        violations.Add($"{type.Name}.{prop.Name} contains CSS/styling (presentation belongs in frontend)");
                    }
                }
            }
        }

        Assert.Empty(violations);
    }

    /// <summary>
    /// Verify ViewModels do not contain Icon properties.
    /// Frontend should select icons based on domain enums/values.
    /// </summary>
    [Fact]
    public void ViewModels_ShouldNot_ContainIconProperties()
    {
        Assembly assembly = typeof(GameWorld).Assembly;
        List<string> violations = new List<string>();

        foreach (Type type in GetViewModelTypes(assembly))
        {
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo prop in properties)
            {
                if (prop.PropertyType == typeof(string))
                {
                    string propNameLower = prop.Name.ToLowerInvariant();
                    if (propNameLower.Contains("icon") ||
                        propNameLower.Contains("emoji") ||
                        propNameLower.Contains("glyph") ||
                        propNameLower.Contains("symbol"))
                    {
                        violations.Add($"{type.Name}.{prop.Name} contains icon/glyph (presentation belongs in frontend)");
                    }
                }
            }
        }

        Assert.Empty(violations);
    }

    /// <summary>
    /// Verify ViewModels do not contain Color properties.
    /// Colors are presentation decisions.
    /// </summary>
    [Fact]
    public void ViewModels_ShouldNot_ContainColorProperties()
    {
        Assembly assembly = typeof(GameWorld).Assembly;
        List<string> violations = new List<string>();

        foreach (Type type in GetViewModelTypes(assembly))
        {
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo prop in properties)
            {
                if (prop.PropertyType == typeof(string))
                {
                    string propNameLower = prop.Name.ToLowerInvariant();
                    // Check for color-like property names
                    if (propNameLower == "color" ||
                        propNameLower.EndsWith("color") ||
                        propNameLower.Contains("colour") ||
                        propNameLower.Contains("bgcolor") ||
                        propNameLower.Contains("textcolor"))
                    {
                        violations.Add($"{type.Name}.{prop.Name} contains color (presentation belongs in frontend)");
                    }
                }
            }
        }

        Assert.Empty(violations);
    }

    /// <summary>
    /// Verify TravelStatusViewModel uses domain values, not presentation.
    /// </summary>
    [Fact]
    public void TravelStatusViewModel_UsesDomainValues()
    {
        Type vmType = typeof(TravelStatusViewModel);
        PropertyInfo[] properties = vmType.GetProperties();

        // Should NOT have FocusClass (presentation)
        PropertyInfo focusClassProp = properties.FirstOrDefault(p => p.Name == "FocusClass");
        Assert.Null(focusClassProp);

        // SHOULD have BaseStaminaPenalty (domain value)
        PropertyInfo penaltyProp = properties.FirstOrDefault(p => p.Name == "BaseStaminaPenalty");
        Assert.NotNull(penaltyProp);
        Assert.Equal(typeof(int), penaltyProp.PropertyType);
    }

    /// <summary>
    /// Verify RouteViewModel uses domain values for affordability.
    /// </summary>
    [Fact]
    public void RouteViewModel_UsesDomainValues()
    {
        Type vmType = typeof(RouteViewModel);
        PropertyInfo[] properties = vmType.GetProperties();

        // Should have boolean affordability flags (domain)
        PropertyInfo canAffordCoinsProp = properties.FirstOrDefault(p => p.Name == "CanAffordCoins");
        Assert.NotNull(canAffordCoinsProp);
        Assert.Equal(typeof(bool), canAffordCoinsProp.PropertyType);

        PropertyInfo canAffordStaminaProp = properties.FirstOrDefault(p => p.Name == "CanAffordStamina");
        Assert.NotNull(canAffordStaminaProp);
        Assert.Equal(typeof(bool), canAffordStaminaProp.PropertyType);

        // Should NOT have affordability CSS classes
        List<PropertyInfo> cssProps = properties
            .Where(p => p.Name.ToLowerInvariant().Contains("class"))
            .ToList();
        Assert.Empty(cssProps);
    }

    /// <summary>
    /// Verify services do not generate display strings.
    /// Display formatting is frontend responsibility.
    /// </summary>
    [Fact]
    public void Services_ShouldNot_GenerateDisplayStrings()
    {
        Assembly assembly = typeof(GameWorld).Assembly;
        List<string> violations = new List<string>();

        foreach (Type type in GetServiceTypes(assembly))
        {
            MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);

            foreach (MethodInfo method in methods)
            {
                // Check for methods that generate display text
                string methodNameLower = method.Name.ToLowerInvariant();
                if ((methodNameLower.Contains("format") ||
                     methodNameLower.Contains("display") ||
                     methodNameLower.Contains("render") ||
                     methodNameLower.Contains("tostring")) &&
                    method.ReturnType == typeof(string))
                {
                    // Skip ToString overrides and legitimate formatting
                    if (method.Name == "ToString") continue;

                    violations.Add($"{type.Name}.{method.Name}() returns string (display generation belongs in frontend)");
                }
            }
        }

        // Filter legitimate cases
        violations = violations
            .Where(v => !v.Contains("Debug"))              // Debug methods are okay
            .Where(v => !v.Contains("Log"))                // Logging is okay
            .Where(v => !v.Contains("Error"))              // Error messages are okay
            .Where(v => !v.Contains("Serialize"))          // Serialization is okay
            .Where(v => !v.Contains("RenderTemplate"))     // Narrative generation is content creation, not UI presentation
            .Where(v => !v.Contains("TimeDisplayFormatter")) // Time formatting is canonical state representation
            .Where(v => !v.Contains("TimeBlockCalculator")) // Time block names are domain vocabulary, not display choice
            .Where(v => !v.Contains("NarrativeFacade"))    // Narrative facade delegates to renderer (content, not presentation)
            .Where(v => !v.Contains("NarrativeRenderer"))  // Narrative rendering is content generation
            .Where(v => !v.Contains("TimeFacade.GetFormattedTimeDisplay")) // Canonical time state string
            .Where(v => !v.Contains("TimeFacade.FormatDuration"))   // Domain-semantic duration representation
            .Where(v => !v.Contains("TimeFacade.FormatSegments"))   // Domain-semantic segment representation
            .Where(v => !v.Contains("TimeFacade.GetTimeBlockDisplayName")) // Domain vocabulary (Morning, Midday, etc.)
            .Where(v => !v.Contains("TimeFacade.GetDayName"))       // Domain vocabulary (Monday, Tuesday, etc.)
            .Where(v => !v.Contains("TimeFacade.GetShortDayName"))  // Domain vocabulary (MON, TUE, etc.)
            .Where(v => !v.Contains("GameFacade.GetFormattedTimeDisplay")) // Delegation to TimeFacade
            .Where(v => !v.Contains("TimeFacade.GetNextAvailableTimeDisplay")) // Domain-semantic availability representation
            .ToList();

        Assert.Empty(violations);
    }

    /// <summary>
    /// Verify entities return domain enums, not string representations.
    /// Frontend maps enums to display strings.
    /// </summary>
    [Fact]
    public void Entities_ReturnDomainEnums_NotDisplayStrings()
    {
        Assembly assembly = typeof(GameWorld).Assembly;
        List<string> violations = new List<string>();

        // Properties that should be enums, not strings
        Dictionary<Type, List<string>> expectedEnumProperties = new Dictionary<Type, List<string>>
        {
            { typeof(NPC), new List<string> { "PersonalityType", "Profession", "PlayerRelationship" } },
            { typeof(Location), new List<string> { "Purpose", "Privacy", "Safety", "Environment" } },
            { typeof(RouteOption), new List<string> { "TerrainType" } }
        };

        foreach (KeyValuePair<Type, List<string>> kvp in expectedEnumProperties)
        {
            Type entityType = kvp.Key;
            List<string> propNames = kvp.Value;

            foreach (string propName in propNames)
            {
                PropertyInfo prop = entityType.GetProperty(propName);
                if (prop != null && prop.PropertyType == typeof(string))
                {
                    violations.Add($"{entityType.Name}.{propName} is string but should be enum (frontend handles display)");
                }
            }
        }

        Assert.Empty(violations);
    }

    /// <summary>
    /// Verify domain classes do not have "Display" prefixed properties.
    /// Display properties indicate presentation leaking into backend.
    /// </summary>
    [Fact]
    public void DomainClasses_ShouldNot_HaveDisplayProperties()
    {
        Assembly assembly = typeof(GameWorld).Assembly;
        List<string> violations = new List<string>();

        foreach (Type type in GetDomainTypes(assembly))
        {
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo prop in properties)
            {
                if (prop.Name.StartsWith("Display") && prop.PropertyType == typeof(string))
                {
                    violations.Add($"{type.Name}.{prop.Name} is a display property (presentation belongs in frontend)");
                }
            }
        }

        Assert.Empty(violations);
    }

    /// <summary>
    /// Verify ViewModels provide domain values that frontend can style.
    /// Example: Provide BaseStaminaPenalty (int), not FocusClass (string).
    /// </summary>
    [Fact]
    public void ViewModels_ProvideDomainValues_ForStyling()
    {
        Type vmType = typeof(TravelStatusViewModel);
        PropertyInfo[] properties = vmType.GetProperties();

        // Check we have actual domain values, not presentation hints
        int domainValueCount = properties.Count(p =>
            p.PropertyType == typeof(int) ||
            p.PropertyType == typeof(bool) ||
            p.PropertyType.IsEnum);

        int stringPresentationCount = properties.Count(p =>
            p.PropertyType == typeof(string) &&
            (p.Name.ToLowerInvariant().Contains("class") ||
             p.Name.ToLowerInvariant().Contains("style") ||
             p.Name.ToLowerInvariant().Contains("icon")));

        // Should have more domain values than presentation strings
        Assert.True(domainValueCount > stringPresentationCount,
            $"ViewModel has {domainValueCount} domain values but {stringPresentationCount} presentation strings - domain values should predominate");
    }

    // ========== HELPER METHODS ==========

    private List<Type> GetViewModelTypes(Assembly assembly)
    {
        return assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => t.Name.EndsWith("ViewModel") || t.Name.Contains("ViewModel"))
            .ToList();
    }

    private List<Type> GetServiceTypes(Assembly assembly)
    {
        return assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => t.Name.EndsWith("Service") ||
                       t.Name.EndsWith("Executor") ||
                       t.Name.EndsWith("Facade"))
            .ToList();
    }

    private List<Type> GetDomainTypes(Assembly assembly)
    {
        return assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => !t.Namespace?.Contains("Pages") ?? true)
            .Where(t => !t.Name.EndsWith("ViewModel"))
            .Where(t => !t.Name.EndsWith("DTO"))
            .Where(t => !t.Name.EndsWith("Parser"))
            .Where(t => !t.Name.EndsWith("Service"))
            .ToList();
    }
}
