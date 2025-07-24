/// <summary>
/// Command to update the player's relationship with an NPC.
/// </summary>
public class UpdateNPCRelationshipCommand : BaseGameCommand
{
    private readonly string _npcId;
    private readonly NPCRelationship _newRelationship;
    private readonly string _reason;
    private readonly NPCRepository _npcRepository;

    public UpdateNPCRelationshipCommand(string npcId, NPCRelationship newRelationship, NPCRepository npcRepository, string reason = "interaction")
    {
        if (string.IsNullOrWhiteSpace(npcId))
            throw new ArgumentException("NPC ID cannot be empty", nameof(npcId));

        _npcId = npcId;
        _newRelationship = newRelationship;
        _npcRepository = npcRepository ?? throw new ArgumentNullException(nameof(npcRepository));
        _reason = reason;

        CommandType = "UpdateRelationship";
        Description = $"Update relationship with NPC {_npcId} to {_newRelationship} ({_reason})";
    }

    public override CommandValidationResult CanExecute(GameWorld gameWorld)
    {
        // Check if NPC exists
        NPC npc = _npcRepository.GetById(_npcId);

        if (npc == null)
        {
            return CommandValidationResult.Failure(
                $"NPC '{_npcId}' not found",
                canBeRemedied: false
            );
        }

        // Validate relationship progression is reasonable
        NPCRelationship currentRelationship = npc.PlayerRelationship;
        int levelChange = Math.Abs(GetRelationshipLevel(_newRelationship) - GetRelationshipLevel(currentRelationship));

        if (levelChange > 2)
        {
            return CommandValidationResult.Failure(
                $"Relationship change too drastic: {currentRelationship} to {_newRelationship}",
                canBeRemedied: true,
                remediationHint: "Relationship changes should be gradual"
            );
        }

        return CommandValidationResult.Success();
    }

    public override Task<CommandResult> ExecuteAsync(GameWorld gameWorld)
    {
        CommandValidationResult validation = CanExecute(gameWorld);
        if (!validation.IsValid)
        {
            return Task.FromResult(CommandResult.Failure(validation.FailureReason));
        }

        // Find the NPC
        NPC npc = _npcRepository.GetById(_npcId);

        // Get previous relationship for change description
        NPCRelationship previousRelationship = npc.PlayerRelationship;
        NPCState previousState = NPCState.FromNPC(npc);

        // Apply the relationship change
        NPCOperationResult result = NPCStateOperations.UpdateRelationship(previousState, _newRelationship);
        if (!result.IsSuccess)
        {
            return Task.FromResult(CommandResult.Failure(result.Message));
        }

        // Update the mutable NPC (temporary until full immutability)
        npc.PlayerRelationship = _newRelationship;

        // Add relationship change message
        string changeDescription = GetRelationshipChangeDescription(previousRelationship, _newRelationship);
        gameWorld.SystemMessages.Add(new SystemMessage($"{npc.Name}: {changeDescription} ({_reason})"));

        return Task.FromResult(CommandResult.Success(
            result.Message,
            new
            {
                NPCId = _npcId,
                NPCName = npc.Name,
                PreviousRelationship = previousRelationship.ToString(),
                NewRelationship = _newRelationship.ToString(),
                Reason = _reason
            }
        ));
    }


    private static int GetRelationshipLevel(NPCRelationship relationship)
    {
        return relationship switch
        {
            NPCRelationship.Hostile => -2,
            NPCRelationship.Unfriendly => -1,
            NPCRelationship.Neutral => 0,
            NPCRelationship.Friendly => 1,
            NPCRelationship.Allied => 2,
            _ => 0
        };
    }

    private static string GetRelationshipChangeDescription(NPCRelationship from, NPCRelationship to)
    {
        int fromLevel = GetRelationshipLevel(from);
        int toLevel = GetRelationshipLevel(to);

        if (toLevel > fromLevel)
        {
            return to switch
            {
                NPCRelationship.Allied => "You've become close allies!",
                NPCRelationship.Friendly => "Your relationship has improved to friendly.",
                NPCRelationship.Neutral => "Relations have normalized.",
                _ => $"Relationship improved to {to}"
            };
        }
        else
        {
            return to switch
            {
                NPCRelationship.Hostile => "You've become enemies!",
                NPCRelationship.Unfriendly => "Your relationship has soured.",
                NPCRelationship.Neutral => "Relations have cooled.",
                _ => $"Relationship declined to {to}"
            };
        }
    }
}