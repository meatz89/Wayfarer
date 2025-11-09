/// <summary>
/// Manages all meeting-related obligations including scheduling, completion, and deadline tracking.
/// Handles NPC meeting requests, appointment management, and meeting outcome processing.
/// </summary>
public class MeetingManager
{
private readonly GameWorld _gameWorld;
private readonly NPCRepository _npcRepository;
private readonly MessageSystem _messageSystem;
private readonly TokenMechanicsManager _tokenManager;
private readonly GameConfiguration _config;

public MeetingManager(
    GameWorld gameWorld,
    NPCRepository npcRepository,
    MessageSystem messageSystem,
    TokenMechanicsManager tokenManager,
    GameConfiguration config)
{
    _gameWorld = gameWorld;
    _npcRepository = npcRepository;
    _messageSystem = messageSystem;
    _tokenManager = tokenManager;
    _config = config;
}

/// <summary>
/// Get all active meeting obligations that haven't expired.
/// </summary>
public List<MeetingObligation> GetActiveMeetingObligations()
{
    Player player = _gameWorld.GetPlayer();
    return player.MeetingObligations
        .ToList();
}

/// <summary>
/// Get meeting obligation with a specific NPC.
/// </summary>
public MeetingObligation GetMeetingWithNPC(string npcId)
{
    return GetActiveMeetingObligations()
        .FirstOrDefault(m => m.RequesterId == npcId);
}

/// <summary>
/// Get meeting obligation by ID.
/// </summary>
public MeetingObligation GetMeetingById(string meetingId)
{
    Player player = _gameWorld.GetPlayer();
    return player.MeetingObligations
        .FirstOrDefault(m => m.Id == meetingId);
}

/// <summary>
/// Add a new meeting obligation from an NPC request.
/// </summary>
public MeetingResult AddMeetingObligation(MeetingObligation meeting)
{
    MeetingResult result = new MeetingResult
    {
        Operation = MeetingOperation.Add,
        NPCId = meeting.RequesterId,
        NPCName = meeting.RequesterName
    };

    // Validate the meeting request
    MeetingResult validation = ValidateMeetingRequest(meeting);
    if (!validation.Success)
    {
        result.ErrorMessage = validation.ErrorMessage;
        return result;
    }

    // Check if there's already a meeting with this NPC
    MeetingObligation existingMeeting = GetMeetingWithNPC(meeting.RequesterId);
    if (existingMeeting != null)
    {
        result.ErrorMessage = $"Already have a meeting scheduled with {meeting.RequesterName}";
        return result;
    }

    // Add the meeting
    _gameWorld.GetPlayer().MeetingObligations.Add(meeting);

    result.Success = true;
    result.AffectedMeeting = meeting;

    // Send appropriate message based on urgency
    MeetingUrgency urgencyLevel = GetMeetingUrgencyLevel(meeting);
    SystemMessageTypes messageType = urgencyLevel switch
    {
        MeetingUrgency.Critical => SystemMessageTypes.Danger,
        MeetingUrgency.Urgent => SystemMessageTypes.Warning,
        _ => SystemMessageTypes.Info
    };

    return result;
}

/// <summary>
/// Complete a meeting obligation when the player meets with the NPC.
/// </summary>
public MeetingResult CompleteMeeting(string meetingId)
{
    MeetingResult result = new MeetingResult
    {
        Operation = MeetingOperation.Complete
    };

    MeetingObligation meeting = GetMeetingById(meetingId);
    if (meeting == null)
    {
        result.ErrorMessage = "Meeting not found";
        return result;
    }

    result.NPCId = meeting.RequesterId;
    result.NPCName = meeting.RequesterName;

    // Validate the meeting can be completed
    MeetingResult validation = ValidateMeetingCompletion(meeting);
    if (!validation.Success)
    {
        result.ErrorMessage = validation.ErrorMessage;
        return result;
    }

    // Remove the meeting from obligations
    _gameWorld.GetPlayer().MeetingObligations.Remove(meeting);

    // Award reputation based on meeting completion timing
    AwardMeetingCompletionTokens(meeting);

    result.Success = true;
    result.AffectedMeeting = meeting;

    _messageSystem.AddSystemMessage(
        $"Met with {meeting.RequesterName}",
        SystemMessageTypes.Success
    );

    return result;
}

/// <summary>
/// Cancel a meeting obligation (typically for testing or special circumstances).
/// </summary>
public MeetingResult CancelMeeting(string meetingId)
{
    MeetingResult result = new MeetingResult
    {
        Operation = MeetingOperation.Cancel
    };

    MeetingObligation meeting = GetMeetingById(meetingId);
    if (meeting == null)
    {
        result.ErrorMessage = "Meeting not found";
        return result;
    }

    result.NPCId = meeting.RequesterId;
    result.NPCName = meeting.RequesterName;

    // Remove the meeting
    _gameWorld.GetPlayer().MeetingObligations.Remove(meeting);

    // Apply minor relationship penalty for cancellation
    if (meeting.RequesterId != null)
    {
        _tokenManager.RemoveTokensFromNPC(ConnectionType.Trust, 1, meeting.RequesterId);
        _messageSystem.AddSystemMessage(
            $"Lost 1 Trust token with {meeting.RequesterName} for canceling meeting",
            SystemMessageTypes.Warning
        );
    }

    result.Success = true;
    result.AffectedMeeting = meeting;

    _messageSystem.AddSystemMessage(
        $"Canceled meeting with {meeting.RequesterName}",
        SystemMessageTypes.Warning
    );

    return result;
}

/// <summary>
/// Process meeting deadline expiration. Called by DeadlineTracker.
/// </summary>
public List<MeetingResult> ProcessExpiredMeetings()
{
    List<MeetingResult> results = new List<MeetingResult>();
    Player player = _gameWorld.GetPlayer();
    List<MeetingObligation> expiredMeetings = new List<MeetingObligation>();

    // Process each expired meeting
    foreach (MeetingObligation expiredMeeting in expiredMeetings)
    {
        MeetingResult result = ExpireMeeting(expiredMeeting);
        results.Add(result);
    }

    return results;
}

/// <summary>
/// Get meetings by urgency level.
/// </summary>
public List<MeetingObligation> GetMeetingsByUrgency(MeetingUrgency urgency)
{
    return GetActiveMeetingObligations()
        .Where(m => GetMeetingUrgencyLevel(m) == urgency)
        .ToList();
}

/// <summary>
/// Check if the player can meet with a specific NPC right now.
/// </summary>
public bool CanMeetWithNPC(string npcId)
{
    MeetingObligation meeting = GetMeetingWithNPC(npcId);
    if (meeting == null) return false;

    // Check if player is at NPC's location
    return IsPlayerAtNPCLocation(npcId);
}

// Private helper methods

private MeetingResult ValidateMeetingRequest(MeetingObligation meeting)
{
    MeetingResult result = new MeetingResult { Success = true };

    if (string.IsNullOrEmpty(meeting.RequesterId))
    {
        result.Success = false;
        result.ErrorMessage = "Meeting must have a valid requester ID";
        return result;
    }

    NPC npc = _npcRepository.GetById(meeting.RequesterId);
    if (npc == null)
    {
        result.Success = false;
        result.ErrorMessage = "Requester NPC not found";
        return result;
    }
    return result;
}

private MeetingResult ValidateMeetingCompletion(MeetingObligation meeting)
{
    MeetingResult result = new MeetingResult { Success = true };

    // Check if player is at NPC's location
    if (!IsPlayerAtNPCLocation(meeting.RequesterId))
    {
        result.Success = false;
        result.ErrorMessage = $"You must be at {meeting.RequesterName}'s Venue to meet";
        return result;
    }

    return result;
}

private MeetingResult ExpireMeeting(MeetingObligation expiredMeeting)
{
    MeetingResult result = new MeetingResult
    {
        Operation = MeetingOperation.Expire,
        NPCId = expiredMeeting.RequesterId,
        NPCName = expiredMeeting.RequesterName,
        AffectedMeeting = expiredMeeting
    };

    // Remove from player's obligations
    _gameWorld.GetPlayer().MeetingObligations.Remove(expiredMeeting);

    // Apply relationship penalty based on stakes
    ApplyMeetingExpirationPenalty(expiredMeeting);

    result.Success = true;

    _messageSystem.AddSystemMessage(
        $"Meeting with {expiredMeeting.RequesterName} has expired!",
        SystemMessageTypes.Danger
    );

    return result;
}

private void AwardMeetingCompletionTokens(MeetingObligation meeting)
{
    int tokensAwarded = 1; // Base reward
    ConnectionType tokenType = ConnectionType.Trust; // Meetings typically build trust

    _tokenManager.AddTokensToNPC(tokenType, tokensAwarded, meeting.RequesterId);

    _messageSystem.AddSystemMessage(
        $"Gained {tokensAwarded} {tokenType} tokens with {meeting.RequesterName}",
        SystemMessageTypes.Success
    );
}

private void ApplyMeetingExpirationPenalty(MeetingObligation expiredMeeting)
{
    int tokenPenalty = 2; // Meetings are important social commitments
    ConnectionType tokenType = ConnectionType.Trust;

    _tokenManager.RemoveTokensFromNPC(tokenType, tokenPenalty, expiredMeeting.RequesterId);

    _messageSystem.AddSystemMessage(
        $"Lost {tokenPenalty} {tokenType} tokens with {expiredMeeting.RequesterName} for missing meeting!",
        SystemMessageTypes.Danger
    );
}

private MeetingUrgency GetMeetingUrgencyLevel(MeetingObligation meeting)
{
    return MeetingUrgency.Normal;
}

private bool IsPlayerAtNPCLocation(string npcId)
{
    Player player = _gameWorld.GetPlayer();
    if (_gameWorld.GetPlayerCurrentLocation() == null) return false;

    // Get current time block for NPC Venue checking
    TimeBlocks currentTime = GetCurrentTimeBlock();
    List<NPC> npcsAtCurrentSpot = _npcRepository.GetNPCsForLocationAndTime(
        _gameWorld.GetPlayerCurrentLocation().Id,
        currentTime);

    return npcsAtCurrentSpot.Any(npc => npc.ID == npcId);
}

private TimeBlocks GetCurrentTimeBlock()
{
    // This would typically come from TimeManager
    // Simplified for now
    return TimeBlocks.Morning;
}
}

/// <summary>
/// Meeting urgency levels for categorization and UI display.
/// </summary>
public enum MeetingUrgency
{
Normal,   // > 6 hours
Urgent,   // 3-6 hours  
Critical  // < 3 hours
}
