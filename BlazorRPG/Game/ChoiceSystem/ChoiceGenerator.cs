//public class ChoiceGenerator
//{
//    private readonly GameState gameState;
//    private readonly List<(ChoiceArchetypes archetype, ChoiceApproaches approach)> alreadyUsedCombinations;

//    public ChoiceGenerator(GameState gameState)
//    {
//        this.gameState = gameState;
//        this.alreadyUsedCombinations = new List<(ChoiceArchetypes, ChoiceApproaches)>();
//    }

//    public List<EncounterChoice> CreateEncounterChoices(Encounter encounter, EncounterContext context, EncounterStageContext encounterStageContext, ChoiceSetTemplate template)
//    {
//        alreadyUsedCombinations.Clear();

//        PlayerState playerState = gameState.Player;
//        EncounterStageState initialValues = encounterStageContext.StageValues;

//        List<EncounterChoice> choices = new List<EncounterChoice>();

//        choices = GenerateBaseChoices(initialValues, playerState, template.NumberOfChoices, template.pattern);
//        return choices;
//    }

//    private List<EncounterChoice> GenerateBaseChoices(
//        EncounterStageState values,
//        PlayerState playerState,
//        int desiredChoiceCount,
//        CompositionPattern pattern)
//    {
//        List<EncounterChoice> choices = new();

//        // First: Generate Primary Archetype choices
//        int primaryChoiceCount = Math.Max(1, desiredChoiceCount / 2);
//        GenerateArchetypeChoices(
//            pattern.PrimaryArchetype,
//            primaryChoiceCount,
//            values,
//            playerState,
//            pattern,
//            choices);

//        // Second: Generate Secondary Archetype choices
//        int secondaryChoiceCount = Math.Max(1, desiredChoiceCount / 3);
//        GenerateArchetypeChoices(
//            pattern.SecondaryArchetype,
//            secondaryChoiceCount,
//            values,
//            playerState,
//            pattern,
//            choices);

//        // Fill remaining slots with flexible choices
//        while (choices.Count < desiredChoiceCount)
//        {
//            EncounterChoice flexibleChoice = TryGenerateFlexibleChoice(pattern, values, playerState);
//            if (flexibleChoice == null)
//                break;

//            choices.Add(flexibleChoice);
//            alreadyUsedCombinations.Add((flexibleChoice.Archetype, flexibleChoice.Approach));
//        }

//        return choices;
//    }

//    private void GenerateArchetypeChoices(
//        ChoiceArchetypes archetype,
//        int count,
//        EncounterStageState values,
//        PlayerState playerState,
//        CompositionPattern pattern,
//        List<EncounterChoice> choices)
//    {
//        List<ChoiceApproaches> availableApproaches = GetUnusedAvailableApproaches(archetype, values, playerState, pattern);
//        List<ChoiceApproaches> priorityOrder = GameRules.GetPriorityOrder(archetype, values);

//        for (int i = 0; i < count && availableApproaches.Any(); i++)
//        {
//            ChoiceApproaches? approach = priorityOrder
//                .FirstOrDefault(a => availableApproaches.Contains(a));

//            if (approach == null)
//                break;

//            EncounterChoice choice = CreateChoice(choices.Count + 1, archetype, approach.Value);
//            choices.Add(choice);
//            alreadyUsedCombinations.Add((archetype, approach.Value));
//            availableApproaches.Remove(approach.Value);
//        }
//    }

//    private EncounterChoice TryGenerateFlexibleChoice(
//        CompositionPattern pattern,
//        EncounterStageState values,
//        PlayerState playerState)
//    {
//        // Try primary archetype first
//        EncounterChoice primaryChoice = TryGenerateChoiceForArchetype(
//            pattern.PrimaryArchetype,
//            values,
//            playerState,
//            pattern);

//        if (primaryChoice != null)
//            return primaryChoice;

//        // Then try secondary archetype
//        EncounterChoice secondaryChoice = TryGenerateChoiceForArchetype(
//                    pattern.SecondaryArchetype,
//                    values,
//                    playerState,
//                    pattern);

//        if (secondaryChoice != null)
//            return secondaryChoice;

//        return null;
//    }

//    private List<ChoiceApproaches> GetUnusedAvailableApproaches(
//        ChoiceArchetypes archetype,
//        EncounterStageState values,
//        PlayerState playerState,
//        CompositionPattern pattern)
//    {
//        List<ChoiceApproaches> choiceApproaches = GameRules.GetAvailableApproaches(archetype, values, playerState);
//        List<ChoiceApproaches> unusedChoiceApproaches = choiceApproaches
//                    .Where(approach => !alreadyUsedCombinations.Any(used =>
//                        used.archetype == archetype && used.approach == approach))
//                    .ToList();
//        return unusedChoiceApproaches;
//    }

//    private EncounterChoice TryGenerateChoiceForArchetype(
//        ChoiceArchetypes archetype,
//        EncounterStageState values,
//        PlayerState playerState,
//        CompositionPattern pattern)
//    {
//        List<ChoiceApproaches> availableApproaches = GetUnusedAvailableApproaches(
//            archetype, values, playerState, pattern);

//        if (!availableApproaches.Any())
//            return null;

//        List<ChoiceApproaches> priorityOrder = GameRules.GetPriorityOrder(archetype, values);
//        ChoiceApproaches? approach = priorityOrder.FirstOrDefault(a => availableApproaches.Contains(a));

//        if (approach == null)
//            return null;

//        if (!alreadyUsedCombinations.Any(c => c.archetype == pattern.PrimaryArchetype && c.approach == approach))
//        {
//            EncounterChoice choice = CreateChoice(alreadyUsedCombinations.Count + 1, archetype, approach.Value);
//            return choice;
//        }

//        return null;
//    }

//    private EncounterChoice CreateChoice(int index, ChoiceArchetypes archetype, ChoiceApproaches approach)
//    {
//        EncounterChoice choice = new EncounterChoice(
//            index,
//            $"{archetype} - {approach}",
//            $"{archetype} - {approach}",
//            archetype,
//            approach);

//        choice.CalculationResult = new ChoiceCalculationResult(
//            new List<ValueModification>(),
//            new List<Requirement>(),
//            new List<Outcome>(),
//            new List<Outcome>()
//            );

//        return choice;
//    }
//}