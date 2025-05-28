public class EncounterUIController
{
    private PlayerChoiceSelection currentSelection;
    private bool waitingForChoice;
    private string narrativeText;
    private object choiceButtons;

    public void ShowMessage(string message)
    {
        narrativeText.text = message;
    }

    public PlayerChoiceSelection WaitForPlayerChoice(List<EncounterChoice> choices)
    {
        DisplayChoices(choices);
        waitingForChoice = true;

        while (waitingForChoice)
        {
            System.Threading.Thread.Sleep(50);
        }

        return currentSelection;
    }

    private void DisplayChoices(List<EncounterChoice> choices)
    {
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (i < choices.Count)
            {
                EncounterChoice choice = choices[i];
                choiceButtons[i].gameObject.SetActive(true);

                Text buttonText = choiceButtons[i].GetComponentInChildren<Text>();
                string displayText = choice.NarrativeText + " (Focus: " + choice.FocusCost + ")";

                if (!choice.IsAffordable)
                {
                    displayText += " [Cannot Afford]";
                    choiceButtons[i].interactable = false;
                }
                else
                {
                    choiceButtons[i].interactable = true;
                }

                buttonText.text = displayText;
            }
            else
            {
                choiceButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private void OnChoiceSelected(int index)
    {
        // Show skill options for selected choice
        // For now, auto-select first available skill option
        EncounterChoice selectedChoice = GetCurrentChoices()[index];
        SkillOption bestOption = FindBestSkillOption(selectedChoice.SkillOptions);

        currentSelection = new PlayerChoiceSelection();
        currentSelection.Choice = selectedChoice;
        currentSelection.SelectedOption = bestOption;

        waitingForChoice = false;
    }

    private SkillOption FindBestSkillOption(List<SkillOption> options)
    {
        SkillOption best = options[0];
        for (int i = 1; i < options.Count; i++)
        {
            if (options[i].SuccessChance > best.SuccessChance)
            {
                best = options[i];
            }
        }
        return best;
    }

    public void DisplaySkillCheckResult(SkillCheckResult result)
    {
        string message = "Skill Check: " + result.SkillName + "\n";
        message += "Level " + result.PlayerLevel + " vs Difficulty " + result.RequiredLevel + "\n";
        message += result.IsSuccess ? "SUCCESS!" : "FAILURE!";

        if (result.IsUntrained)
        {
            message += " (Untrained Attempt)";
        }

        ShowMessage(message);
    }

    public void UpdateEncounterDisplay(EncounterState state)
    {
        focusDisplay.text = "Focus: " + state.FocusPoints + "/" + state.MaxFocusPoints;

        StringBuilder flagText = new StringBuilder("Active Effects:\n");
        List<FlagStates> activeFlags = state.FlagManager.GetActiveFlags();

        for (int i = 0; i < activeFlags.Count; i++)
        {
            flagText.AppendLine("• " + GetFriendlyFlagName(activeFlags[i]));
        }

        flagsDisplay.text = flagText.ToString();
    }

    private string GetFriendlyFlagName(FlagStates flag)
    {
        switch (flag)
        {
            case FlagStates.TrustEstablished: return "Trust Established";
            case FlagStates.PathCleared: return "Path Cleared";
            case FlagStates.InsightGained: return "Insight Gained";
            default: return flag.ToString();
        }
    }
}