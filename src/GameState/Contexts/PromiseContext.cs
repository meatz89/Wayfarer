/// <summary>
/// Context for promise/letter delivery request conversations
/// Contains promise-specific data without dictionaries
/// </summary>
public class PromiseContext : ConversationContextBase
{
    public PromiseCardData PromiseData { get; set; }
    public LetterDetails LetterDetails { get; set; }
    public TermDetails SuccessTerms { get; set; }
    public TermDetails FailureTerms { get; set; }
    public int NegotiationDifficulty { get; set; }
    public ConnectionType TokenType { get; set; }
    public string Destination { get; set; }
    public string RecipientName { get; set; }
    public string RecipientId { get; set; }
    public StakeType Stakes { get; set; }
    public EmotionalFocus EmotionalFocus { get; set; }
    public int MomentumThreshold { get; set; }
    public bool GeneratesLetterOnSuccess { get; set; }
    public int BaseSuccessRate { get; set; }
    public bool HasDeadline { get; set; }
    public int SegmentsUntilDeadline { get; set; }
    public string UrgencyLevel { get; set; }
    public int DeadlineSegments { get; set; }
    public int QueuePosition { get; set; }
    public int Payment { get; set; }
    public bool ForcesPositionOne { get; set; }
    public PersonalityType NPCPersonality { get; set; }

    public void SetNPCPersonality(PersonalityType personality)
    {
        NPCPersonality = personality;
        // Additional personality-specific logic can be added here
    }

    public PromiseContext()
    {
        ConversationTypeId = "request"; // Handles Request bundles with promise cards
        GeneratesLetterOnSuccess = true;
    }

    public void SetPromiseData(PromiseCardData promiseData)
    {
        PromiseData = promiseData;
        if (promiseData != null)
        {
            SuccessTerms = promiseData.SuccessTerms;
            FailureTerms = promiseData.FailureTerms;
            NegotiationDifficulty = promiseData.NegotiationDifficulty;
            TokenType = promiseData.TokenType;
            Destination = promiseData.Destination;
            RecipientName = promiseData.RecipientName;

            if (SuccessTerms != null)
            {
                DeadlineSegments = SuccessTerms.DeadlineSegments;
                QueuePosition = SuccessTerms.QueuePosition;
                Payment = SuccessTerms.Payment;
                ForcesPositionOne = SuccessTerms.ForcesPositionOne;
            }
        }
    }

    public void SetLetterDetails(LetterDetails letterDetails)
    {
        LetterDetails = letterDetails;
        if (letterDetails != null)
        {
            RecipientId = letterDetails.RecipientId;
            RecipientName = letterDetails.RecipientName;
            TokenType = letterDetails.TokenType;
            Stakes = letterDetails.Stakes;
            EmotionalFocus = letterDetails.EmotionalFocus;
        }
    }

    public bool HasValidTerms()
    {
        return SuccessTerms != null && FailureTerms != null;
    }

    public bool IsUrgent()
    {
        return EmotionalFocus == EmotionalFocus.CRITICAL ||
               UrgencyLevel == "HIGH" ||
               (HasDeadline && SegmentsUntilDeadline < 2); // Less than 2 segments = urgent
    }
}