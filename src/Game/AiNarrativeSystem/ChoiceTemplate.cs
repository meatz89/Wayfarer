public class ChoiceTemplate
{
    // Template identity
    public string TemplateName { get; private set; }
    public string TemplatePurpose { get; private set; }
    public int Weight { get; private set; }

    // Input mechanics
    public InputMechanics InputMechanics { get; private set; }

    // Direct effect class references
    public IMechanicalEffect SuccessEffect { get; private set; }
    public IMechanicalEffect FailureEffect { get; private set; }

    // Letter offer specific properties
    public string Purpose { get; set; }
    public string Description { get; set; }
    public ConnectionType? TokenType { get; set; }
    public LetterCategory? Category { get; set; }

    public ChoiceTemplate(
        string templateName,
        string strategicPurpose,
        int weight,
        InputMechanics inputMechanics,
        IMechanicalEffect successEffect,
        IMechanicalEffect failureEffect)
    {

        TemplateName = templateName;
        TemplatePurpose = strategicPurpose;
        Weight = weight;
        InputMechanics = inputMechanics;
        SuccessEffect = successEffect;
        FailureEffect = failureEffect;
    }

    // Parameterless constructor for dynamic creation
    public ChoiceTemplate()
    {
    }
}
