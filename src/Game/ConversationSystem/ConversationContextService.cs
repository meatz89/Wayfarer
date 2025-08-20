using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Service that generates rich, contextual conversations for letter deliveries.
/// Provides multiple meaningful choices based on letter content, timing, relationships, and circumstances.
/// </summary>
public class ConversationContextService
{
    private readonly GameWorld _gameWorld;
    private readonly TokenMechanicsManager _tokenManager;
    private readonly StandingObligationManager _obligationManager;
    private readonly Random _random;

    public ConversationContextService(
        GameWorld gameWorld,
        TokenMechanicsManager tokenManager,
        StandingObligationManager obligationManager)
    {
        _gameWorld = gameWorld;
        _tokenManager = tokenManager;
        _obligationManager = obligationManager;
        _random = new Random();
    }

    public DeliveryConversationContext AnalyzeDeliveryContext(DeliveryObligation letter, NPC recipient)
    {
        Player player = _gameWorld.GetPlayer();
        bool isLate = letter.DaysInQueue > 3;
        bool isVeryLate = letter.DaysInQueue > 5;

        // Analyze relationship - get tokens for the recipient's token types
        int recipientTokens = 0;
        if (recipient.LetterTokenTypes.Any())
        {
            recipientTokens = recipient.LetterTokenTypes.Sum(tokenType => _tokenManager.GetTokenCount(tokenType));
        }
        bool hasStrongRelationship = recipientTokens >= 3;
        bool hasWeakRelationship = recipientTokens == 0;

        // Check for special circumstances
        bool playerIsDesperate = player.Coins < 5;
        bool playerIsExhausted = player.Stamina < 2;
        bool hasPatronObligation = _obligationManager.GetActiveObligations().Any(o => o.Name == "Patron's Expectation");
        bool recipientIsPatron = recipient.Profession == Professions.Noble;

        return new DeliveryConversationContext
        {
            DeliveryObligation = letter,
            Recipient = recipient,
            IsLate = isLate,
            IsVeryLate = isVeryLate,
            HasStrongRelationship = hasStrongRelationship,
            HasWeakRelationship = hasWeakRelationship,
            PlayerIsDesperate = playerIsDesperate,
            PlayerIsExhausted = playerIsExhausted,
            RecipientTokens = recipientTokens
        };
    }

    public List<ConversationChoice> GenerateDeliveryChoices(DeliveryConversationContext context)
    {
        List<ConversationChoice> choices = new List<ConversationChoice>();
        int choiceId = 1;

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

        // Physical properties (fragile, valuable, confidential) don't apply to obligations

        {
            choices.Add(CreateBegForTipChoice(context, choiceId++));
        }

        {
            choices.Add(CreateReportProgressChoice(context, choiceId++));
        }

        {
        }

        // Limit choices to prevent overwhelming the player
        if (choices.Count > 5)
        {
            // Keep standard delivery and 4 most relevant contextual choices
            ConversationChoice standardChoice = choices[0];
            List<ConversationChoice> contextualChoices = choices.Skip(1).OrderBy(c => c.Priority).Take(4).ToList();
            choices = new List<ConversationChoice> { standardChoice };
            choices.AddRange(contextualChoices);
        }

        return choices;
    }

    private ConversationChoice CreateStandardDeliveryChoice(DeliveryConversationContext context, int id)
    {
        DeliveryObligation letter = context.DeliveryObligation;
        ConnectionType tokenType = context.Recipient.LetterTokenTypes.FirstOrDefault();

        return new ConversationChoice
        {
            ChoiceID = id.ToString(),
            NarrativeText = $"Deliver professionally ({letter.Payment} coins + 1 {tokenType} token)",
            PatienceCost = 0,
            IsAffordable = true,
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
        DeliveryObligation letter = context.DeliveryObligation;
        ConnectionType tokenType = context.Recipient.LetterTokenTypes.FirstOrDefault();

        return new ConversationChoice
        {
            ChoiceID = id.ToString(),
            NarrativeText = $"Apologize for delay and offer discount ({letter.Payment - 2} coins + 2 tokens)",
            PatienceCost = 1,
            IsAffordable = true,
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
        DeliveryObligation letter = context.DeliveryObligation;
        return new ConversationChoice
        {
            ChoiceID = id.ToString(),
            NarrativeText = $"Make elaborate excuse for delay ({letter.Payment} coins, no token)",
            PatienceCost = 2,
            IsAffordable = true,
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
        DeliveryObligation letter = context.DeliveryObligation;
        int penalty = Math.Min(letter.Payment / 2, 5);
        ConnectionType tokenType = context.Recipient.LetterTokenTypes.FirstOrDefault();

        return new ConversationChoice
        {
            ChoiceID = id.ToString(),
            NarrativeText = $"Admit to tardiness honestly ({letter.Payment - penalty} coins + respect)",
            PatienceCost = 0,
            IsAffordable = true,
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
        DeliveryObligation letter = context.DeliveryObligation;
        ConnectionType tokenType = context.Recipient.LetterTokenTypes.FirstOrDefault();

        return new ConversationChoice
        {
            ChoiceID = id.ToString(),
            NarrativeText = $"Emphasize careful handling ({letter.Payment + 2} coins + 1 token)",
            PatienceCost = 0,
            IsAffordable = true,
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
        DeliveryObligation letter = context.DeliveryObligation;
        ConnectionType tokenType = context.Recipient.LetterTokenTypes.FirstOrDefault();

        return new ConversationChoice
        {
            ChoiceID = id.ToString(),
            NarrativeText = $"Deliver with utmost discretion ({letter.Payment} coins + 2 tokens)",
            PatienceCost = 1,
            IsAffordable = true,
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
        DeliveryObligation letter = context.DeliveryObligation;
        ConnectionType tokenType = context.Recipient.LetterTokenTypes.FirstOrDefault();

        return new ConversationChoice
        {
            ChoiceID = id.ToString(),
            NarrativeText = $"Insist on private delivery ({letter.Payment + 3} coins + 1 token)",
            PatienceCost = 1,
            IsAffordable = true,
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
        DeliveryObligation letter = context.DeliveryObligation;
        ConnectionType tokenType = context.Recipient.LetterTokenTypes.FirstOrDefault();

        return new ConversationChoice
        {
            ChoiceID = id.ToString(),
            NarrativeText = $"Hint at juicy contents ({letter.Payment + 5} coins, lose 1 token)",
            PatienceCost = 0,
            IsAffordable = context.RecipientTokens >= 1,
            TemplatePurpose = "Trade relationship for immediate profit",
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
        DeliveryObligation letter = context.DeliveryObligation;
        double tipChance = context.HasStrongRelationship ? 0.7 : 0.3;
        ConnectionType tokenType = context.Recipient.LetterTokenTypes.FirstOrDefault();

        return new ConversationChoice
        {
            ChoiceID = id.ToString(),
            NarrativeText = $"Plead desperate circumstances ({letter.Payment} coins + possible tip)",
            PatienceCost = 2,
            IsAffordable = true,
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
        DeliveryObligation letter = context.DeliveryObligation;
        ConnectionType tokenType = context.Recipient.LetterTokenTypes.FirstOrDefault();

        return new ConversationChoice
        {
            ChoiceID = id.ToString(),
            NarrativeText = $"Report on obligation progress ({letter.Payment} coins + patron approval)",
            PatienceCost = 0,
            IsAffordable = true,
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
}

public class DeliveryConversationContext
{
    public DeliveryObligation DeliveryObligation { get; set; }
    public NPC Recipient { get; set; }
    public bool IsLate { get; set; }
    public bool IsVeryLate { get; set; }
    public bool HasStrongRelationship { get; set; }
    public bool HasWeakRelationship { get; set; }
    public bool PlayerIsDesperate { get; set; }
    public bool PlayerIsExhausted { get; set; }
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
    public bool GeneratesReturnDeliveryObligation { get; set; }
    public double ChanceForTip { get; set; }
    public int TipAmount { get; set; }
    public int ReducesLeverage { get; set; }
    public string AdditionalEffect { get; set; }
}