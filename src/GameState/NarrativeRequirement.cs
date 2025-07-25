using System;

/// <summary>
/// Validates whether commands are allowed based on active narrative state.
/// Integrates with the command validation system to filter actions during tutorials/quests.
/// </summary>
public class NarrativeRequirement
{
    private readonly NarrativeManager _narrativeManager;
    
    public NarrativeRequirement(NarrativeManager narrativeManager)
    {
        _narrativeManager = narrativeManager ?? throw new ArgumentNullException(nameof(narrativeManager));
    }
    
    /// <summary>
    /// Check if a command is allowed based on narrative state
    /// </summary>
    public CommandValidationResult ValidateCommand(IGameCommand command)
    {
        // If no active narratives, all commands are allowed
        if (!_narrativeManager.HasActiveNarrative())
        {
            return CommandValidationResult.Success();
        }
        
        // Get the command type
        string commandType = GetCommandType(command);
        
        // Check each active narrative
        foreach (var narrativeId in _narrativeManager.GetActiveNarratives())
        {
            var currentStep = _narrativeManager.GetCurrentStep(narrativeId);
            if (currentStep != null && currentStep.AllowedActions.Count > 0)
            {
                // This step has restricted actions
                if (!currentStep.AllowedActions.Contains(commandType))
                {
                    return CommandValidationResult.Failure(
                        $"This action is not available during '{currentStep.Name}'",
                        canBeRemedied: false,
                        remediationHint: currentStep.GuidanceText ?? "Follow the tutorial guidance"
                    );
                }
            }
        }
        
        return CommandValidationResult.Success();
    }
    
    /// <summary>
    /// Check if an NPC interaction is allowed
    /// </summary>
    public bool IsNPCInteractionAllowed(string npcId)
    {
        return _narrativeManager.IsNPCVisible(npcId);
    }
    
    /// <summary>
    /// Get contextual failure message for narrative restrictions
    /// </summary>
    public string GetNarrativeRestrictionMessage()
    {
        if (!_narrativeManager.HasActiveNarrative())
        {
            return null;
        }
        
        foreach (var narrativeId in _narrativeManager.GetActiveNarratives())
        {
            var currentStep = _narrativeManager.GetCurrentStep(narrativeId);
            if (currentStep != null)
            {
                return currentStep.GuidanceText ?? $"Complete '{currentStep.Name}' to continue";
            }
        }
        
        return "Complete the current narrative step to unlock more actions";
    }
    
    private string GetCommandType(IGameCommand command)
    {
        // Map command types to action names used in narrative definitions
        return command switch
        {
            TravelCommand => "Travel",
            ConverseCommand => "Converse",
            WorkCommand => "Work",
            RestCommand => "Rest",
            CollectLetterCommand => "CollectLetter",
            DeliverLetterCommand => "DeliverLetter",
            LetterQueueActionCommand => "QueueAction",
            SocializeCommand => "Socialize",
            BorrowMoneyCommand => "BorrowMoney",
            GatherResourcesCommand => "Gather",
            BrowseCommand => "Browse",
            ObserveCommand => "Observe",
            PatronFundsCommand => "PatronFunds",
            _ => command.GetType().Name.Replace("Command", "")
        };
    }
}