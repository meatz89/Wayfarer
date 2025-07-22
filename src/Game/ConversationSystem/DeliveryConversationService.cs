using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Service that generates rich, contextual conversations for letter deliveries.
/// Provides multiple meaningful choices based on letter content, timing, relationships, and circumstances.
/// </summary>
public class DeliveryConversationService
{
    private readonly GameWorld _gameWorld;
    private readonly ConnectionTokenManager _tokenManager;
    private readonly StandingObligationManager _obligationManager;
    private readonly Random _random;
    
    public DeliveryConversationService(
        GameWorld gameWorld,
        ConnectionTokenManager tokenManager,
        StandingObligationManager obligationManager)
    {
        _gameWorld = gameWorld;
        _tokenManager = tokenManager;
        _obligationManager = obligationManager;
        _random = new Random();
    }
    
    public DeliveryConversationContext AnalyzeDeliveryContext(Letter letter, NPC recipient)
    {
        var player = _gameWorld.GetPlayer();
        var isLate = letter.DaysInQueue > 3;
        var isVeryLate = letter.DaysInQueue > 5;
        var isFragile = letter.HasPhysicalProperty(LetterPhysicalProperties.Fragile);
        var isValuable = letter.HasPhysicalProperty(LetterPhysicalProperties.Valuable);
        var isConfidential = letter.HasPhysicalProperty(LetterPhysicalProperties.RequiresProtection);
        var isChainLetter = letter.IsChainLetter;
        
        // Analyze relationship - get tokens for the recipient's token types
        var recipientTokens = 0;
        if (recipient.LetterTokenTypes.Any())
        {
            recipientTokens = recipient.LetterTokenTypes.Sum(tokenType => _tokenManager.GetTokenCount(tokenType));
        }
        var hasStrongRelationship = recipientTokens >= 3;
        var hasWeakRelationship = recipientTokens == 0;
        
        // Check for special circumstances
        var playerIsDesperate = player.Coins < 5;
        var playerIsExhausted = player.Stamina < 2;
        var hasPatronObligation = _obligationManager.GetActiveObligations().Any(o => o.Name == "Patron's Expectation");
        var recipientIsPatron = recipient.Profession == Professions.Noble;
        
        return new DeliveryConversationContext
        {
            Letter = letter,
            Recipient = recipient,
            IsLate = isLate,
            IsVeryLate = isVeryLate,
            IsFragile = isFragile,
            IsValuable = isValuable,
            IsConfidential = isConfidential,
            IsChainLetter = isChainLetter,
            HasStrongRelationship = hasStrongRelationship,
            HasWeakRelationship = hasWeakRelationship,
            PlayerIsDesperate = playerIsDesperate,
            PlayerIsExhausted = playerIsExhausted,
            HasPatronObligation = hasPatronObligation,
            RecipientIsPatron = recipientIsPatron,
            RecipientTokens = recipientTokens
        };
    }
    
    public List<ConversationChoice> GenerateDeliveryChoices(DeliveryConversationContext context)
    {
        var choices = new List<ConversationChoice>();
        var choiceId = 1;
        
        // Always include standard delivery option
        choices.Add(CreateStandardDeliveryChoice(context, choiceId++));
        
        // Add contextual choices based on circumstances
        if (context.IsLate && !context.IsVeryLate)
        {
            choices.Add(CreateApologeticDeliveryChoice(context, choiceId++));
        }
        
        if (context.IsVeryLate)
        {
            choices.Add(CreateExcuseDeliveryChoice(context, choiceId++));
            choices.Add(CreateHonestDeliveryChoice(context, choiceId++));
        }
        
        if (context.IsFragile)
        {
            choices.Add(CreateCarefulDeliveryChoice(context, choiceId++));
        }
        
        if (context.IsValuable && context.HasStrongRelationship)
        {
            choices.Add(CreateDiscreetDeliveryChoice(context, choiceId++));
        }
        
        if (context.IsConfidential)
        {
            choices.Add(CreatePrivateDeliveryChoice(context, choiceId++));
            if (context.HasWeakRelationship)
            {
                choices.Add(CreateGossipDeliveryChoice(context, choiceId++));
            }
        }
        
        if (context.PlayerIsDesperate && !context.RecipientIsPatron)
        {
            choices.Add(CreateBegForTipChoice(context, choiceId++));
        }
        
        if (context.RecipientIsPatron && context.HasPatronObligation)
        {
            choices.Add(CreateReportProgressChoice(context, choiceId++));
        }
        
        // Add chain letter specific choice
        if (context.IsChainLetter)
        {
            choices.Add(CreateChainLetterChoice(context, choiceId++));
        }
        
        // Limit choices to prevent overwhelming the player
        if (choices.Count > 5)
        {
            // Keep standard delivery and 4 most relevant contextual choices
            var standardChoice = choices[0];
            var contextualChoices = choices.Skip(1).OrderBy(c => c.Priority).Take(4).ToList();
            choices = new List<ConversationChoice> { standardChoice };
            choices.AddRange(contextualChoices);
        }
        
        return choices;
    }
    
    private ConversationChoice CreateStandardDeliveryChoice(DeliveryConversationContext context, int id)
    {
        var letter = context.Letter;
        var tokenType = context.Recipient.LetterTokenTypes.FirstOrDefault();
        
        return new ConversationChoice
        {
            ChoiceID = id.ToString(),
            NarrativeText = $"Deliver professionally ({letter.Payment} coins + 1 {tokenType} token)",
            FocusCost = 0,
            IsAffordable = true,
            ChoiceType = ConversationChoiceType.DeliverForTokens,
            TemplatePurpose = "Standard delivery with relationship building",
            Priority = 0,
            DeliveryOutcome = new DeliveryOutcome
            {
                BasePayment = letter.Payment,
                BonusPayment = 0,
                TokenReward = true,
                TokenType = tokenType,
                TokenAmount = 1
            }
        };
    }
    
    private ConversationChoice CreateApologeticDeliveryChoice(DeliveryConversationContext context, int id)
    {
        var letter = context.Letter;
        var tokenType = context.Recipient.LetterTokenTypes.FirstOrDefault();
        
        return new ConversationChoice
        {
            ChoiceID = id.ToString(),
            NarrativeText = $"Apologize for delay and offer discount ({letter.Payment - 2} coins + 2 tokens)",
            FocusCost = 1,
            IsAffordable = true,
            ChoiceType = ConversationChoiceType.DeliverApologetic,
            TemplatePurpose = "Build goodwill despite late delivery",
            Priority = 1,
            DeliveryOutcome = new DeliveryOutcome
            {
                BasePayment = letter.Payment - 2,
                BonusPayment = 0,
                TokenReward = true,
                TokenType = tokenType,
                TokenAmount = 2,
                AdditionalEffect = "Preserves relationship despite delay"
            }
        };
    }
    
    private ConversationChoice CreateExcuseDeliveryChoice(DeliveryConversationContext context, int id)
    {
        var letter = context.Letter;
        return new ConversationChoice
        {
            ChoiceID = id.ToString(),
            NarrativeText = $"Make elaborate excuse for delay ({letter.Payment} coins, no token)",
            FocusCost = 2,
            IsAffordable = true,
            ChoiceType = ConversationChoiceType.DeliverWithExcuse,
            TemplatePurpose = "Avoid consequences for very late delivery",
            Priority = 2,
            DeliveryOutcome = new DeliveryOutcome
            {
                BasePayment = letter.Payment,
                BonusPayment = 0,
                TokenReward = false,
                AdditionalEffect = "Avoids penalty but no relationship gain"
            }
        };
    }
    
    private ConversationChoice CreateHonestDeliveryChoice(DeliveryConversationContext context, int id)
    {
        var letter = context.Letter;
        var penalty = Math.Min(letter.Payment / 2, 5);
        var tokenType = context.Recipient.LetterTokenTypes.FirstOrDefault();
        
        return new ConversationChoice
        {
            ChoiceID = id.ToString(),
            NarrativeText = $"Admit to tardiness honestly ({letter.Payment - penalty} coins + respect)",
            FocusCost = 0,
            IsAffordable = true,
            ChoiceType = ConversationChoiceType.DeliverHonest,
            TemplatePurpose = "Take responsibility for late delivery",
            Priority = 1,
            DeliveryOutcome = new DeliveryOutcome
            {
                BasePayment = letter.Payment - penalty,
                BonusPayment = 0,
                TokenReward = true,
                TokenType = tokenType,
                TokenAmount = 1,
                AdditionalEffect = "Gains respect for honesty"
            }
        };
    }
    
    private ConversationChoice CreateCarefulDeliveryChoice(DeliveryConversationContext context, int id)
    {
        var letter = context.Letter;
        var tokenType = context.Recipient.LetterTokenTypes.FirstOrDefault();
        
        return new ConversationChoice
        {
            ChoiceID = id.ToString(),
            NarrativeText = $"Emphasize careful handling ({letter.Payment + 2} coins + 1 token)",
            FocusCost = 0,
            IsAffordable = true,
            ChoiceType = ConversationChoiceType.DeliverCarefully,
            TemplatePurpose = "Highlight protection of fragile item",
            Priority = 1,
            DeliveryOutcome = new DeliveryOutcome
            {
                BasePayment = letter.Payment,
                BonusPayment = 2,
                TokenReward = true,
                TokenType = tokenType,
                TokenAmount = 1,
                AdditionalEffect = "Appreciation for careful handling"
            }
        };
    }
    
    private ConversationChoice CreateDiscreetDeliveryChoice(DeliveryConversationContext context, int id)
    {
        var letter = context.Letter;
        var tokenType = context.Recipient.LetterTokenTypes.FirstOrDefault();
        
        return new ConversationChoice
        {
            ChoiceID = id.ToString(),
            NarrativeText = $"Deliver with utmost discretion ({letter.Payment} coins + 2 tokens)",
            FocusCost = 1,
            IsAffordable = true,
            ChoiceType = ConversationChoiceType.DeliverDiscreetly,
            TemplatePurpose = "Show trustworthiness with valuable items",
            Priority = 2,
            DeliveryOutcome = new DeliveryOutcome
            {
                BasePayment = letter.Payment,
                BonusPayment = 0,
                TokenReward = true,
                TokenType = tokenType,
                TokenAmount = 2,
                AdditionalEffect = "Deepens trust for future valuable deliveries"
            }
        };
    }
    
    private ConversationChoice CreatePrivateDeliveryChoice(DeliveryConversationContext context, int id)
    {
        var letter = context.Letter;
        var tokenType = context.Recipient.LetterTokenTypes.FirstOrDefault();
        
        return new ConversationChoice
        {
            ChoiceID = id.ToString(),
            NarrativeText = $"Insist on private delivery ({letter.Payment + 3} coins + 1 token)",
            FocusCost = 1,
            IsAffordable = true,
            ChoiceType = ConversationChoiceType.DeliverPrivately,
            TemplatePurpose = "Protect confidential information",
            Priority = 1,
            DeliveryOutcome = new DeliveryOutcome
            {
                BasePayment = letter.Payment,
                BonusPayment = 3,
                TokenReward = true,
                TokenType = tokenType,
                TokenAmount = 1,
                AdditionalEffect = "Recipient appreciates discretion"
            }
        };
    }
    
    private ConversationChoice CreateGossipDeliveryChoice(DeliveryConversationContext context, int id)
    {
        var letter = context.Letter;
        var tokenType = context.Recipient.LetterTokenTypes.FirstOrDefault();
        
        return new ConversationChoice
        {
            ChoiceID = id.ToString(),
            NarrativeText = $"Hint at juicy contents ({letter.Payment + 5} coins, lose 1 token)",
            FocusCost = 0,
            IsAffordable = context.RecipientTokens >= 1,
            ChoiceType = ConversationChoiceType.DeliverWithGossip,
            TemplatePurpose = "Trade reputation for immediate profit",
            Priority = 3,
            DeliveryOutcome = new DeliveryOutcome
            {
                BasePayment = letter.Payment,
                BonusPayment = 5,
                TokenReward = false,
                TokenPenalty = true,
                TokenType = tokenType,
                TokenAmount = -1,
                AdditionalEffect = "Damages trust but earns extra coins"
            }
        };
    }
    
    private ConversationChoice CreateBegForTipChoice(DeliveryConversationContext context, int id)
    {
        var letter = context.Letter;
        var tipChance = context.HasStrongRelationship ? 0.7 : 0.3;
        var tokenType = context.Recipient.LetterTokenTypes.FirstOrDefault();
        
        return new ConversationChoice
        {
            ChoiceID = id.ToString(),
            NarrativeText = $"Plead desperate circumstances ({letter.Payment} coins + possible tip)",
            FocusCost = 2,
            IsAffordable = true,
            ChoiceType = ConversationChoiceType.DeliverDesperate,
            TemplatePurpose = "Appeal for extra help when desperate",
            Priority = 3,
            DeliveryOutcome = new DeliveryOutcome
            {
                BasePayment = letter.Payment,
                BonusPayment = 0,
                TokenReward = true,
                TokenType = tokenType,
                TokenAmount = 1,
                ChanceForTip = tipChance,
                TipAmount = context.HasStrongRelationship ? 5 : 2,
                AdditionalEffect = $"{(int)(tipChance * 100)}% chance of sympathy tip"
            }
        };
    }
    
    private ConversationChoice CreateReportProgressChoice(DeliveryConversationContext context, int id)
    {
        var letter = context.Letter;
        var tokenType = context.Recipient.LetterTokenTypes.FirstOrDefault();
        
        return new ConversationChoice
        {
            ChoiceID = id.ToString(),
            NarrativeText = $"Report on obligation progress ({letter.Payment} coins + patron approval)",
            FocusCost = 0,
            IsAffordable = true,
            ChoiceType = ConversationChoiceType.DeliverWithReport,
            TemplatePurpose = "Update patron on delivery obligations",
            Priority = 1,
            DeliveryOutcome = new DeliveryOutcome
            {
                BasePayment = letter.Payment,
                BonusPayment = 0,
                TokenReward = true,
                TokenType = tokenType,
                TokenAmount = 1,
                ReducesLeverage = 1,
                AdditionalEffect = "Reduces patron leverage by showing diligence"
            }
        };
    }
    
    private ConversationChoice CreateChainLetterChoice(DeliveryConversationContext context, int id)
    {
        var letter = context.Letter;
        var tokenType = context.Recipient.LetterTokenTypes.FirstOrDefault();
        
        return new ConversationChoice
        {
            ChoiceID = id.ToString(),
            NarrativeText = $"Explain chain letter opportunity ({letter.Payment} coins + unlock chain)",
            FocusCost = 1,
            IsAffordable = true,
            ChoiceType = ConversationChoiceType.DeliverChainLetter,
            TemplatePurpose = "Unlock chain letter sequence",
            Priority = 2,
            DeliveryOutcome = new DeliveryOutcome
            {
                BasePayment = letter.Payment,
                BonusPayment = 0,
                TokenReward = true,
                TokenType = tokenType,
                TokenAmount = 1,
                UnlocksChainLetters = true,
                AdditionalEffect = "Unlocks follow-up chain letters"
            }
        };
    }
}

public class DeliveryConversationContext
{
    public Letter Letter { get; set; }
    public NPC Recipient { get; set; }
    public bool IsLate { get; set; }
    public bool IsVeryLate { get; set; }
    public bool IsFragile { get; set; }
    public bool IsValuable { get; set; }
    public bool IsConfidential { get; set; }
    public bool IsChainLetter { get; set; }
    public bool HasStrongRelationship { get; set; }
    public bool HasWeakRelationship { get; set; }
    public bool PlayerIsDesperate { get; set; }
    public bool PlayerIsExhausted { get; set; }
    public bool HasPatronObligation { get; set; }
    public bool RecipientIsPatron { get; set; }
    public int RecipientTokens { get; set; }
}

public class DeliveryOutcome
{
    public int BasePayment { get; set; }
    public int BonusPayment { get; set; }
    public bool TokenReward { get; set; }
    public bool TokenPenalty { get; set; }
    public ConnectionType TokenType { get; set; }
    public int TokenAmount { get; set; }
    public bool GeneratesReturnLetter { get; set; }
    public bool UnlocksChainLetters { get; set; }
    public double ChanceForTip { get; set; }
    public int TipAmount { get; set; }
    public int ReducesLeverage { get; set; }
    public string AdditionalEffect { get; set; }
}