// PackageLoadResult: Entity tracking structure for package-round isolation
//
// Purpose: Track entities added during a single package load round to enforce
// package-round principle (initialize ONLY entities from THIS round, never re-process)
//
// Usage Scenarios:
//   Static Loading (Startup):  Accumulate results → aggregate → initialize ONCE
//   Dynamic Loading (Runtime): Use result directly → initialize IMMEDIATELY
//
// Architectural Enforcement:
//   Methods receiving explicit lists cannot re-process existing entities
//   Package isolation guaranteed by construction (each result contains only its entities)
//   No entity state checks needed (if in result, guaranteed uninitialized)

public class PackageLoadResult
{
    // Spatial entities requiring initialization
    public List<Venue> VenuesAdded { get; set; } = new List<Venue>();
    public List<Location> LocationsAdded { get; set; } = new List<Location>();

    // Domain entities
    public List<NPC> NPCsAdded { get; set; } = new List<NPC>();
    public List<Item> ItemsAdded { get; set; } = new List<Item>();

    // Route network
    public List<RouteOption> RoutesAdded { get; set; } = new List<RouteOption>();

    // Three parallel tactical systems - Card templates
    public List<SocialCard> SocialCardsAdded { get; set; } = new List<SocialCard>();
    public List<MentalCard> MentalCardsAdded { get; set; } = new List<MentalCard>();
    public List<PhysicalCard> PhysicalCardsAdded { get; set; } = new List<PhysicalCard>();

    // Three parallel tactical systems - Challenge decks
    public List<SocialChallengeDeck> SocialChallengeDecksAdded { get; set; } = new List<SocialChallengeDeck>();
    public List<MentalChallengeDeck> MentalChallengeDecksAdded { get; set; } = new List<MentalChallengeDeck>();
    public List<PhysicalChallengeDeck> PhysicalChallengeDecksAdded { get; set; } = new List<PhysicalChallengeDeck>();

    // Strategic layer - Scene system
    public List<SceneTemplate> SceneTemplatesAdded { get; set; } = new List<SceneTemplate>();
    public List<Scene> ScenesAdded { get; set; } = new List<Scene>();

    // Supporting systems
    public List<ExchangeCard> ExchangeCardsAdded { get; set; } = new List<ExchangeCard>();

    // Package metadata
    public string PackageId { get; set; } = "unknown";

    // Helper properties
    public int TotalEntityCount =>
        VenuesAdded.Count +
        LocationsAdded.Count +
        NPCsAdded.Count +
        ItemsAdded.Count +
        RoutesAdded.Count +
        SocialCardsAdded.Count +
        MentalCardsAdded.Count +
        PhysicalCardsAdded.Count +
        SocialChallengeDecksAdded.Count +
        MentalChallengeDecksAdded.Count +
        PhysicalChallengeDecksAdded.Count +
        SceneTemplatesAdded.Count +
        ScenesAdded.Count +
        ExchangeCardsAdded.Count;

    public bool HasEntities => TotalEntityCount > 0;

    // Helper methods
    public void Clear()
    {
        VenuesAdded.Clear();
        LocationsAdded.Clear();
        NPCsAdded.Clear();
        ItemsAdded.Clear();
        RoutesAdded.Clear();
        SocialCardsAdded.Clear();
        MentalCardsAdded.Clear();
        PhysicalCardsAdded.Clear();
        SocialChallengeDecksAdded.Clear();
        MentalChallengeDecksAdded.Clear();
        PhysicalChallengeDecksAdded.Clear();
        SceneTemplatesAdded.Clear();
        ScenesAdded.Clear();
        ExchangeCardsAdded.Clear();

        PackageId = "unknown";
    }
}
