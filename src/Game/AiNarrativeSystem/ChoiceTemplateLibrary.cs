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
                failureEffect: new NoEffect()
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
                failureEffect: new NoEffect()
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
                failureEffect: new NoEffect()
            )
            
            // Additional templates would be defined here...
        };
    }

    public static IMechanicalEffect GetEffect(string id, bool success)
    {
        List<ChoiceTemplate> choiceTemplates = GetAllTemplates();
        ChoiceTemplate? choiceTemplate = choiceTemplates.FirstOrDefault(t => t.TemplateName == id.ToString());

        if(choiceTemplate == null)
        {
            return new NoEffect();
        }
        else if (success)
        {
            return choiceTemplate.SuccessEffect;
        }
        else
        {
            return choiceTemplate.FailureEffect;
        }
    }
}