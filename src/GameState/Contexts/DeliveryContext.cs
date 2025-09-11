/// <summary>
/// Context for delivery conversations where letters are completed
/// Contains delivery-specific data without dictionaries
/// </summary>
public class DeliveryContext : ConversationContextBase
{
    public List<DeliveryObligation> LettersCarriedForNpc { get; set; }
    public DeliveryObligation SelectedLetter { get; set; }
    public string LetterId { get; set; }
    public string SenderName { get; set; }
    public string SenderNpcId { get; set; }
    public string LetterDescription { get; set; }
    public ConnectionType TokenType { get; set; }
    public StakeType Stakes { get; set; }
    public EmotionalFocus EmotionalFocus { get; set; }
    public int Payment { get; set; }
    public bool HasDeadline { get; set; }
    public int SegmentsUntilDeadline { get; set; }
    public string UrgencyLevel { get; set; }
    public bool CanDeliverLetter { get; set; }
    public string DeliveryObligationId { get; set; }
    public int TokenReward { get; set; }
    public int FlowBonus { get; set; }

    public DeliveryContext()
    {
        Type = ConversationType.Delivery;
        LettersCarriedForNpc = new List<DeliveryObligation>();
        FlowBonus = 5; // Default flow bonus for successful delivery
    }

    public void SetSelectedLetter(DeliveryObligation obligation)
    {
        SelectedLetter = obligation;
        if (obligation != null)
        {
            LetterId = obligation.Id;
            SenderName = obligation.SenderName;
            SenderNpcId = obligation.SenderId;
            LetterDescription = obligation.Description;
            TokenType = obligation.TokenType;
            Stakes = obligation.Stakes;
            EmotionalFocus = obligation.EmotionalFocus;
            Payment = obligation.Payment;
            HasDeadline = obligation.DeadlineInSegments > 0;
            SegmentsUntilDeadline = obligation.SegmentsUntilDeadline;
            UrgencyLevel = GetUrgencyFromDeadline(obligation.DeadlineInSegments);
            DeliveryObligationId = obligation.Id;
            CanDeliverLetter = true;

            // Calculate token reward based on emotional focus
            TokenReward = obligation.EmotionalFocus switch
            {
                EmotionalFocus.CRITICAL => 3,
                EmotionalFocus.HIGH => 2,
                EmotionalFocus.MEDIUM => 1,
                _ => 1
            };
        }
    }

    public bool HasLettersToDeliver()
    {
        return LettersCarriedForNpc != null && LettersCarriedForNpc.Count > 0;
    }

    public bool IsLetterUrgent()
    {
        if (SelectedLetter == null) return false;
        
        return SelectedLetter.EmotionalFocus == EmotionalFocus.CRITICAL ||
               GetUrgencyFromDeadline(SelectedLetter.DeadlineInSegments) == "HIGH" ||
               (SelectedLetter.DeadlineInSegments > 0 && SelectedLetter.DeadlineInSegments <= 2);
    }

    public string GetDeliveryMessage()
    {
        if (SelectedLetter == null) return "No letter selected for delivery";

        return $"Delivering letter from {SenderName} to {Npc?.Name}";
    }

    public int CalculateTotalRewards()
    {
        return Payment + (TokenReward * 10); // Rough coin equivalent for display
    }

    private string GetUrgencyFromDeadline(int deadlineInSegments)
    {
        if (deadlineInSegments <= 0) return "EXPIRED";
        if (deadlineInSegments <= 2) return "CRITICAL";
        if (deadlineInSegments <= 4) return "HIGH";
        if (deadlineInSegments <= 8) return "MEDIUM";
        return "LOW";
    }
}