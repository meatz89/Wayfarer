
public static class ChoiceTemplateLibrary
{
    public static List<ChoiceTemplate> GetAllTemplates()
    {
        return new List<ChoiceTemplate>
        {
            new ChoiceTemplate(
                templateName: "EstablishTrust",
                strategicPurpose: "Build relationship with NPC",
                weight: 10,
                inputMechanics: new InputMechanics(
                    new FocusCost(1),
                    new SkillCheckRequirement(SkillCategories.Social, 3)
                ),
                successEffect: new NoEffect(),
                failureEffect: new NoEffect(),
                conceptualOutput: "Player attempts to build trust with NPC",
                successOutcomeNarrativeGuidance: "NPC becomes more trusting toward player",
                failureOutcomeNarrativeGuidance: "NPC becomes suspicious of player's intentions"
            ),

            new ChoiceTemplate(
                templateName: "GatherInformation",
                strategicPurpose: "Acquire knowledge about situation",
                weight: 8,
                inputMechanics: new InputMechanics(
                    new FocusCost(1),
                    new SkillCheckRequirement(SkillCategories.Intellectual, 3)
                ),
                successEffect: new NoEffect(),
                failureEffect: new NoEffect(),
                conceptualOutput: "Player attempts to gather information",
                successOutcomeNarrativeGuidance: "Player gains valuable insight",
                failureOutcomeNarrativeGuidance: "Player becomes confused by conflicting information"
            ),

            new ChoiceTemplate(
                templateName: "RecoverFocus",
                strategicPurpose: "Regain lost Focus Points",
                weight: 5,
                inputMechanics: new InputMechanics(
                    new FocusCost(0),
                    new SkillCheckRequirement(SkillCategories.Physical, 2)
                ),
                successEffect: new NoEffect(),
                failureEffect: new NoEffect(),
                conceptualOutput: "Player attempts to recover focus",
                successOutcomeNarrativeGuidance: "Player regains composure and energy",
                failureOutcomeNarrativeGuidance: "Player wastes time without recovering"
            )
            
            // Additional templates would be defined here...
        };
    }

    public static ChoiceTemplate GetEffect(string id)
    {
        return GetAllTemplates().FirstOrDefault(t => t.TemplateName == id.ToString());
    }

    public static bool HasEffect(string? id)
    {
        return GetAllTemplates().Any(t => t.TemplateName == id);
    }
}