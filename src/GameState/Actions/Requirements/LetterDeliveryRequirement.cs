namespace Wayfarer.GameState.Actions.Requirements;

/// <summary>
/// Requirement for delivering a letter - checks position and collection status
/// </summary>
public class LetterDeliveryRequirement : IActionRequirement
{
    private readonly string _recipientNpcId;
    private readonly NPCRepository _npcRepository;
    
    public LetterDeliveryRequirement(string recipientNpcId, NPCRepository npcRepository)
    {
        _recipientNpcId = recipientNpcId;
        _npcRepository = npcRepository;
    }
    
    public bool IsSatisfied(Player player, GameWorld world)
    {
        // Letter must be in position 1
        var letterAtPosition1 = player.LetterQueue[0];
        if (letterAtPosition1 == null) return false;
        
        // Must be collected
        if (letterAtPosition1.State != LetterState.Collected) return false;
        
        // Must be for this recipient
        return letterAtPosition1.RecipientId == _recipientNpcId;
    }
    
    public string GetDescription()
    {
        var npc = _npcRepository.GetNPCById(_recipientNpcId);
        var npcName = npc?.Name ?? "recipient";
        
        return $"Letter for {npcName} in position 1 and collected";
    }
    
    public string GetFailureReason(Player player, GameWorld world)
    {
        var letterAtPosition1 = player.LetterQueue[0];
        var npc = _npcRepository.GetNPCById(_recipientNpcId);
        var npcName = npc?.Name ?? "this recipient";
        
        if (letterAtPosition1 == null)
        {
            return $"No letter in position 1 to deliver to {npcName}";
        }
        
        if (letterAtPosition1.RecipientId != _recipientNpcId)
        {
            return $"Letter in position 1 is not for {npcName}";
        }
        
        if (letterAtPosition1.State != LetterState.Collected)
        {
            return "Letter must be collected before delivery";
        }
        
        return "Cannot deliver this letter";
    }
    
    public bool CanBeRemedied => true;
    
    public string GetRemediationHint()
    {
        // Note: We now get the player state via the IsSatisfied/GetFailureReason methods that receive GameWorld
        // This method provides general hints without needing current state
        return GetGeneralRemediationHint();
    }
    
    private string GetGeneralRemediationHint()
    {
        return "Get a letter for this recipient, collect it from the sender, and move it to position 1";
    }
    
    public double GetProgress(Player player, GameWorld world)
    {
        var letterAtPosition1 = player.LetterQueue[0];
        
        if (letterAtPosition1 == null) return 0.0;
        
        if (letterAtPosition1.RecipientId != _recipientNpcId) return 0.25; // Wrong letter
        
        if (letterAtPosition1.State == LetterState.Accepted) return 0.5; // Right letter, not collected
        
        if (letterAtPosition1.State == LetterState.Collected) return 1.0; // Ready to deliver
        
        return 0.0;
    }
}