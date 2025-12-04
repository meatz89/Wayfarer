/// <summary>
/// Loads card-based systems: social/mental/physical cards, decks, exchanges, NPC deck initialization.
/// COMPOSITION OVER INHERITANCE: Extracted from PackageLoader for single responsibility.
/// </summary>
public class CardSystemLoader
{
    private readonly GameWorld _gameWorld;
    private List<ExchangeCard> _parsedExchangeCards;

    public CardSystemLoader(GameWorld gameWorld)
    {
        _gameWorld = gameWorld;
    }

    /// <summary>
    /// Load all card content from a package.
    /// Called by PackageLoader during package loading.
    /// </summary>
    public void LoadCardContent(
        List<SocialCardDTO> socialCardDtos,
        List<MentalCardDTO> mentalCardDtos,
        List<PhysicalCardDTO> physicalCardDtos,
        List<SocialChallengeDeckDTO> socialDeckDtos,
        List<MentalChallengeDeckDTO> mentalDeckDtos,
        List<PhysicalChallengeDeckDTO> physicalDeckDtos,
        List<ObligationDTO> obligationDtos,
        List<ExchangeDTO> exchangeDtos,
        PackageLoadResult result,
        bool allowSkeletons)
    {
        LoadSocialCards(socialCardDtos, result, allowSkeletons);
        LoadMentalCards(mentalCardDtos, result, allowSkeletons);
        LoadPhysicalCards(physicalCardDtos, result, allowSkeletons);
        LoadSocialChallengeDecks(socialDeckDtos, result, allowSkeletons);
        LoadMentalChallengeDecks(mentalDeckDtos, result, allowSkeletons);
        LoadPhysicalChallengeDecks(physicalDeckDtos, result, allowSkeletons);
        LoadObligations(obligationDtos, allowSkeletons);
        LoadExchanges(exchangeDtos, allowSkeletons);
    }

    /// <summary>
    /// Initialize exchange decks for Mercantile NPCs.
    /// Must be called AFTER LoadCardContent and LoadNPCs.
    /// </summary>
    public void InitializeNPCExchangeDecks(DeckCompositionDTO deckCompositions)
    {
        foreach (NPC npc in _gameWorld.NPCs)
        {
            List<ExchangeCard> npcExchangeCards = new List<ExchangeCard>();

            NpcDeckEntry deckEntry = null;
            if (deckCompositions?.NpcDecks != null)
            {
                deckEntry = deckCompositions.NpcDecks.FirstOrDefault(d => d.NpcId == npc.Name);
            }

            if (deckEntry?.ExchangeDeck != null)
            {
                foreach (CardCountEntry cardCount in deckEntry.ExchangeDeck)
                {
                    string cardId = cardCount.CardId;
                    int count = cardCount.Count;

                    ExchangeCard exchangeCard = _parsedExchangeCards?.FirstOrDefault(e => e.Name == cardId);
                    if (exchangeCard != null)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            npcExchangeCards.Add(exchangeCard);
                        }
                    }
                }
            }
            else if (npc.PersonalityType == PersonalityType.MERCANTILE)
            {
                npcExchangeCards = ExchangeParser.CreateDefaultExchangesForNPC(npc);
            }

            npc.InitializeExchangeDeck(npcExchangeCards);
        }
    }

    private void LoadSocialCards(List<SocialCardDTO> cardDtos, PackageLoadResult result, bool allowSkeletons)
    {
        if (cardDtos == null) return;

        foreach (SocialCardDTO dto in cardDtos)
        {
            try
            {
                SocialCard card = SocialCardParser.ParseCard(dto);
                _gameWorld.SocialCards.Add(card);
                result.SocialCardsAdded.Add(card);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"FATAL: Failed to parse social card '{dto.Id}'. " +
                    $"Total cards loaded so far: {_gameWorld.SocialCards.Count}. " +
                    $"Error: {ex.Message}", ex);
            }
        }

        List<SocialCard> allCards = _gameWorld.SocialCards.ToList();
        SocialCardParser.ValidateFoundationCardRules(allCards);
    }

    private void LoadMentalCards(List<MentalCardDTO> mentalCards, PackageLoadResult result, bool allowSkeletons)
    {
        if (mentalCards == null) return;

        MentalCardParser parser = new MentalCardParser();
        foreach (MentalCardDTO dto in mentalCards)
        {
            MentalCard card = parser.ParseCard(dto);
            _gameWorld.MentalCards.Add(card);
            result.MentalCardsAdded.Add(card);
        }
    }

    private void LoadPhysicalCards(List<PhysicalCardDTO> physicalCards, PackageLoadResult result, bool allowSkeletons)
    {
        if (physicalCards == null) return;

        PhysicalCardParser parser = new PhysicalCardParser();
        foreach (PhysicalCardDTO dto in physicalCards)
        {
            PhysicalCard card = parser.ParseCard(dto);
            _gameWorld.PhysicalCards.Add(card);
            result.PhysicalCardsAdded.Add(card);
        }
    }

    private void LoadSocialChallengeDecks(List<SocialChallengeDeckDTO> decks, PackageLoadResult result, bool allowSkeletons)
    {
        if (decks == null) return;

        foreach (SocialChallengeDeckDTO dto in decks)
        {
            SocialChallengeDeck deck = dto.ToDomain();

            foreach (string cardId in deck.CardIds)
            {
                bool cardExists = _gameWorld.SocialCards.Any(c => c.Id == cardId);
                if (!cardExists)
                {
                    int totalCards = _gameWorld.SocialCards.Count;
                    string allCardIds = string.Join(", ", _gameWorld.SocialCards.Select(c => c.Id));
                    throw new InvalidOperationException(
                        $"Social deck '{deck.Id}' references missing card '{cardId}'. " +
                        $"Total cards loaded: {totalCards}. " +
                        $"All loaded card IDs: {allCardIds}");
                }
            }

            _gameWorld.SocialChallengeDecks.Add(deck);
            result.SocialChallengeDecksAdded.Add(deck);
        }
    }

    private void LoadMentalChallengeDecks(List<MentalChallengeDeckDTO> decks, PackageLoadResult result, bool allowSkeletons)
    {
        if (decks == null) return;

        foreach (MentalChallengeDeckDTO dto in decks)
        {
            MentalChallengeDeck deck = dto.ToDomain();

            foreach (string cardId in deck.CardIds)
            {
                bool cardExists = _gameWorld.MentalCards.Any(c => c.Id == cardId);
                if (!cardExists)
                {
                    throw new InvalidOperationException(
                        $"Mental deck '{deck.Id}' references missing card '{cardId}'. " +
                        $"Available cards: {string.Join(", ", _gameWorld.MentalCards.Take(5).Select(c => c.Id))}...");
                }
            }

            _gameWorld.MentalChallengeDecks.Add(deck);
            result.MentalChallengeDecksAdded.Add(deck);
        }
    }

    private void LoadPhysicalChallengeDecks(List<PhysicalChallengeDeckDTO> decks, PackageLoadResult result, bool allowSkeletons)
    {
        if (decks == null) return;

        foreach (PhysicalChallengeDeckDTO dto in decks)
        {
            PhysicalChallengeDeck deck = dto.ToDomain();

            foreach (string cardId in deck.CardIds)
            {
                bool cardExists = _gameWorld.PhysicalCards.Any(c => c.Id == cardId);
                if (!cardExists)
                {
                    throw new InvalidOperationException(
                        $"Physical deck '{deck.Id}' references missing card '{cardId}'. " +
                        $"Available cards: {string.Join(", ", _gameWorld.PhysicalCards.Take(5).Select(c => c.Id))}...");
                }
            }

            _gameWorld.PhysicalChallengeDecks.Add(deck);
            result.PhysicalChallengeDecksAdded.Add(deck);
        }
    }

    private void LoadObligations(List<ObligationDTO> obligations, bool allowSkeletons)
    {
        if (obligations == null) return;

        ObligationParser parser = new ObligationParser(_gameWorld);
        foreach (ObligationDTO dto in obligations)
        {
            Obligation obligation = parser.ParseObligation(dto);
            _gameWorld.Obligations.Add(obligation);
        }
    }

    private void LoadExchanges(List<ExchangeDTO> exchangeDtos, bool allowSkeletons)
    {
        if (exchangeDtos == null) return;

        foreach (ExchangeDTO dto in exchangeDtos)
        {
            _gameWorld.ExchangeDefinitions.Add(dto);
        }

        EntityResolver entityResolver = new EntityResolver(_gameWorld);
        _parsedExchangeCards = new List<ExchangeCard>();
        foreach (ExchangeDTO dto in exchangeDtos)
        {
            ExchangeCard exchangeCard = ExchangeParser.ParseExchange(dto, entityResolver);
            _parsedExchangeCards.Add(exchangeCard);
        }
    }
}
